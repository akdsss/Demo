using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public class CombatResolver
{
    public List<ScheduledCombatEffect> PendingScheduledEffectsForNextRound { get; } = new();

    public List<CombatEvent> ResolveRound(
        IEnumerable<PlannedAction> actions,
        int roundIndex,
        IEnumerable<CharacterData> allCharacters = null,
        IEnumerable<ScheduledCombatEffect> carryoverScheduledEffects = null)
    {
        Dictionary<CharacterData, CharacterState> statesByLegacyCharacter = BuildStateDictionary(allCharacters);
        List<PlannedAction> normalizedActions = NormalizeActions(actions, roundIndex, statesByLegacyCharacter);
        List<CharacterState> roundStates = statesByLegacyCharacter.Values.ToList();
        List<CombatEvent> events = new()
        {
            new CombatEvent
            {
                EventType = CombatEventType.RoundStarted,
                RoundIndex = roundIndex,
                Message = $"第 {roundIndex} 回合结算开始"
            }
        };

        CombatAreaRules.ApplyRoundStart(roundStates, roundIndex, events);
        Dictionary<int, List<ScheduledCombatEffect>> priorityScheduledEffectsBySlot = new();
        Dictionary<int, List<ScheduledCombatEffect>> slotEndScheduledEffectsBySlot = new();
        QueueCarryoverScheduledEffects(
            carryoverScheduledEffects,
            roundIndex,
            statesByLegacyCharacter,
            priorityScheduledEffectsBySlot,
            slotEndScheduledEffectsBySlot,
            PendingScheduledEffectsForNextRound);

        for (int slotIndex = Timeline.MinSlotIndex; slotIndex <= Timeline.MaxSlotIndex; slotIndex++)
        {
            List<SlotResolutionItem> slotItems = BuildSlotResolutionItems(
                normalizedActions,
                priorityScheduledEffectsBySlot,
                slotIndex);

            foreach (SlotResolutionItem item in slotItems)
            {
                if (item.ScheduledEffect != null)
                {
                    ResolveScheduledEffect(item.ScheduledEffect, events, roundStates, priorityScheduledEffectsBySlot, slotEndScheduledEffectsBySlot, PendingScheduledEffectsForNextRound);
                }
                else
                {
                    ResolveAction(item.Action, events, roundStates, priorityScheduledEffectsBySlot, slotEndScheduledEffectsBySlot, PendingScheduledEffectsForNextRound);
                }
            }

            ResolveSlotEndScheduledEffects(slotIndex, slotEndScheduledEffectsBySlot, events, roundStates, priorityScheduledEffectsBySlot, PendingScheduledEffectsForNextRound);
            TickSlotStatuses(roundStates, roundIndex, events);
        }

        CombatAreaRules.ApplyRoundEnd(roundStates, roundIndex, events);

        events.Add(new CombatEvent
        {
            EventType = CombatEventType.RoundEnded,
            RoundIndex = roundIndex,
            Message = $"第 {roundIndex} 回合结算结束"
        });

        AssignEventIds(events);
        return events;
    }

    private static Dictionary<CharacterData, CharacterState> BuildStateDictionary(IEnumerable<CharacterData> allCharacters)
    {
        Dictionary<CharacterData, CharacterState> statesByLegacyCharacter = new();
        if (allCharacters == null)
        {
            return statesByLegacyCharacter;
        }

        foreach (CharacterData character in allCharacters)
        {
            if (character == null || statesByLegacyCharacter.ContainsKey(character))
            {
                continue;
            }

            statesByLegacyCharacter[character] = CharacterState.FromCharacterData(character);
        }

        return statesByLegacyCharacter;
    }

    private static List<PlannedAction> NormalizeActions(
        IEnumerable<PlannedAction> actions,
        int roundIndex,
        Dictionary<CharacterData, CharacterState> statesByLegacyCharacter)
    {
        List<PlannedAction> normalizedActions = new();

        if (actions == null)
        {
            return normalizedActions;
        }

        foreach (PlannedAction action in actions)
        {
            if (action == null || action.IsDefault)
            {
                continue;
            }

            action.RoundIndex = roundIndex;
            action.Source = ResolveSharedState(action.Source, statesByLegacyCharacter);
            action.TargetCharacter = ResolveSharedState(action.TargetCharacter, statesByLegacyCharacter);
            normalizedActions.Add(action);
        }

        return normalizedActions;
    }

    private static CharacterState ResolveSharedState(
        CharacterState state,
        Dictionary<CharacterData, CharacterState> statesByLegacyCharacter)
    {
        if (state == null)
        {
            return null;
        }

        if (state.LegacyCharacterData == null)
        {
            return state;
        }

        if (statesByLegacyCharacter.TryGetValue(state.LegacyCharacterData, out CharacterState sharedState))
        {
            return sharedState;
        }

        statesByLegacyCharacter[state.LegacyCharacterData] = state;
        return state;
    }

    private static void ResolveAction(
        PlannedAction action,
        List<CombatEvent> events,
        IReadOnlyList<CharacterState> roundStates,
        Dictionary<int, List<ScheduledCombatEffect>> priorityScheduledEffectsBySlot,
        Dictionary<int, List<ScheduledCombatEffect>> slotEndScheduledEffectsBySlot,
        List<ScheduledCombatEffect> nextRoundScheduledEffects)
    {
        SkillFailReason failReason = GetResolutionFailReason(action);
        if (failReason != SkillFailReason.None)
        {
            events.Add(BuildSkillFailedEvent(action, failReason));
            return;
        }

        events.Add(CombatEvent.ForAction(CombatEventType.ActionStarted, action));
        SpendMp(action, events);

        if (action.Skill.Effects.Count == 0)
        {
            return;
        }

        foreach (SkillEffectDefinition effect in action.Skill.Effects)
        {
            ResolveEffect(action, effect, events, roundStates, priorityScheduledEffectsBySlot, slotEndScheduledEffectsBySlot, nextRoundScheduledEffects);
        }
    }

    private static SkillFailReason GetResolutionFailReason(PlannedAction action)
    {
        if (action == null || action.IsDefault || action.Skill == null)
        {
            return SkillFailReason.MissingSkill;
        }

        if (action.Source == null || action.Source.LegacyCharacterData == null)
        {
            return SkillFailReason.MissingSource;
        }

        if (action.Source.IsDefeated)
        {
            return SkillFailReason.SourceDefeated;
        }

        if (action.Source.HasBlockedTag(action.Skill.Tags))
        {
            return SkillFailReason.BlockedByStatus;
        }

        if (action.Skill.MpCost > 0 && action.Source.CurrentMp < action.Skill.MpCost)
        {
            return SkillFailReason.MpInsufficient;
        }

        bool needsCharacterTarget =
            action.Skill.TargetType == SkillTargetType.Ally ||
            action.Skill.TargetType == SkillTargetType.Enemy ||
            action.Skill.TargetType == SkillTargetType.Character;

        if (needsCharacterTarget && action.TargetCharacter == null)
        {
            return SkillFailReason.MissingTarget;
        }

        if (action.TargetCharacter != null && action.TargetCharacter.IsDefeated)
        {
            return SkillFailReason.TargetDefeated;
        }

        if (action.Skill.HasTag(SkillTag.Melee) &&
            !action.Skill.HasTag(SkillTag.Move) &&
            !action.Skill.HasTag(SkillTag.Area) &&
            IsMeleeTargetInDifferentArea(action))
        {
            return SkillFailReason.MeleeTargetNotInSameArea;
        }

        if (action.Skill.HasTag(SkillTag.Move) &&
            IsMoveTargetSameAsSourceArea(action))
        {
            return SkillFailReason.MoveTargetAlreadyCurrentArea;
        }

        if (action.Skill.RequiresDifferentTargetArea &&
            action.TargetCharacter != null &&
            action.TargetAreaId != CombatAreaId.Unknown &&
            action.TargetAreaId == action.TargetCharacter.CurrentAreaId)
        {
            return SkillFailReason.RushTargetMustBeInDifferentArea;
        }

        if (CombatAreaRules.TryEvade(action))
        {
            return SkillFailReason.TargetEvaded;
        }

        return SkillFailReason.None;
    }

    private static bool IsMeleeTargetInDifferentArea(PlannedAction action)
    {
        CombatAreaId sourceAreaId = action.Source?.CurrentAreaId ?? CombatAreaId.Unknown;
        CombatAreaId targetAreaId = action.TargetCharacter?.CurrentAreaId ?? CombatAreaId.Unknown;
        return sourceAreaId != CombatAreaId.Unknown &&
            targetAreaId != CombatAreaId.Unknown &&
            sourceAreaId != targetAreaId;
    }

    private static bool IsMoveTargetSameAsSourceArea(PlannedAction action)
    {
        if (action?.Source == null)
        {
            return false;
        }

        if (action.TargetAreaId != CombatAreaId.Unknown)
        {
            return action.TargetAreaId == action.Source.CurrentAreaId;
        }

        if (action.TargetArea != null && action.Source.CurrentArea != null)
        {
            return action.TargetArea.IsSameArea(action.Source.CurrentArea);
        }

        if (action.TargetCoord.HasValue &&
            AreaDefinition.GetAreaIdForLegacyCoord(action.TargetCoord.Value) == action.Source.CurrentAreaId)
        {
            return true;
        }

        return false;
    }

    private static CombatEvent BuildSkillFailedEvent(PlannedAction action, SkillFailReason failReason)
    {
        CombatEvent combatEvent = CombatEvent.ForAction(CombatEventType.SkillFailed, action);
        combatEvent.FailReason = failReason;
        combatEvent.Message = failReason.ToString();
        return combatEvent;
    }

    private static void ResolveEffect(
        PlannedAction action,
        SkillEffectDefinition effect,
        List<CombatEvent> events,
        IReadOnlyList<CharacterState> roundStates,
        Dictionary<int, List<ScheduledCombatEffect>> priorityScheduledEffectsBySlot,
        Dictionary<int, List<ScheduledCombatEffect>> slotEndScheduledEffectsBySlot,
        List<ScheduledCombatEffect> nextRoundScheduledEffects)
    {
        if (effect == null)
        {
            return;
        }

        if (effect.DelaySlots > 0)
        {
            ScheduleEffect(action, effect, events, priorityScheduledEffectsBySlot, slotEndScheduledEffectsBySlot, nextRoundScheduledEffects);
            return;
        }

        switch (effect.EffectType)
        {
            case SkillEffectType.Damage:
                if (action.Skill.HasTag(SkillTag.Area))
                {
                    ResolveAreaDamage(action, effect, events, roundStates);
                }
                else
                {
                    ResolveDamage(action, effect, events);
                }
                break;
            case SkillEffectType.Heal:
                if (action.Skill.HasTag(SkillTag.Area) || effect.AffectAllies)
                {
                    ResolveAreaHeal(action, effect, events, roundStates);
                }
                else
                {
                    ResolveHeal(action, effect, events);
                }
                break;
            case SkillEffectType.RestoreMp:
                ResolveRestoreMp(action, effect, events, roundStates);
                break;
            case SkillEffectType.Move:
                ResolveMove(action, events);
                break;
            case SkillEffectType.MoveTarget:
                ResolveMoveTarget(action, events);
                break;
            case SkillEffectType.SwapPosition:
                ResolveSwapPosition(action, events);
                break;
            case SkillEffectType.Defend:
                action.Source?.AddOrRefreshStatus(StatusCatalog.Create(StatusCatalog.Defense), action.Source);
                events.Add(BuildEffectEvent(CombatEventType.DefenseApplied, SkillEffectType.Defend, action));
                events.Add(CombatAreaRules.BuildStatusEvent(CombatEventType.StatusApplied, action.Source, StatusCatalog.Defense, action.RoundIndex));
                break;
            case SkillEffectType.ApplyStatus:
                CharacterState statusTarget = action.Skill.TargetType == SkillTargetType.Self
                    ? action.Source
                    : action.TargetCharacter ?? action.Source;
                statusTarget?.AddOrRefreshStatus(StatusCatalog.Create(effect.StatusId), action.Source);
                CombatEvent statusAppliedEvent = BuildEffectEvent(CombatEventType.StatusApplied, SkillEffectType.ApplyStatus, action, statusTarget);
                statusAppliedEvent.StatusId = effect.StatusId;
                events.Add(statusAppliedEvent);
                break;
            case SkillEffectType.RevealIntent:
                events.Add(new CombatEvent
                {
                    EventType = CombatEventType.IntentRevealed,
                    RoundIndex = action.RoundIndex,
                    SlotIndex = action.SlotIndex,
                    Source = action.Source,
                    SourceLegacyCharacterData = action.Source?.LegacyCharacterData,
                    Skill = action.Skill,
                    Message = $"{action.Source?.DisplayName ?? "未知角色"} 揭示敌方意图：{effect.Message}"
                });
                break;
            case SkillEffectType.RemoveStatus:
                CombatEvent statusRemovedEvent = BuildEffectEvent(CombatEventType.StatusRemoved, SkillEffectType.RemoveStatus, action);
                statusRemovedEvent.StatusId = effect.StatusId;
                events.Add(statusRemovedEvent);
                break;
            case SkillEffectType.ScheduledEffect:
                events.Add(BuildEffectEvent(CombatEventType.ScheduledEffectCreated, SkillEffectType.ScheduledEffect, action));
                break;
        }
    }

    private static void ResolveDamage(PlannedAction action, SkillEffectDefinition effect, List<CombatEvent> events)
    {
        ResolveDamage(action, effect, events, action.TargetCharacter);
    }

    private static void ResolveDamage(
        PlannedAction action,
        SkillEffectDefinition effect,
        List<CombatEvent> events,
        CharacterState target)
    {
        if (target == null)
        {
            events.Add(BuildSkillFailedEvent(action, SkillFailReason.MissingTarget));
            return;
        }

        if (TryResolveCounter(action, target, events))
        {
            return;
        }

        float multiplier = SkillEffectMath.ResolvePowerMultiplier(effect);
        multiplier *= CombatAreaRules.GetDamageMultiplier(action, target);
        float amount = SkillEffectMath.CalculatePowerAmount(action.Source.Attack, multiplier);
        if (target.HasStatus(StatusCatalog.Defense))
        {
            amount *= 0.5f;
        }
        amount = CombatAreaRules.AbsorbShield(target, amount, action.RoundIndex, events);
        target.Hp = Math.Max(0, target.Hp - amount);

        CombatEvent damageEvent = BuildEffectEvent(CombatEventType.DamageApplied, SkillEffectType.Damage, action, target);
        damageEvent.Amount = amount;
        damageEvent.TargetHpAfter = target.Hp;
        events.Add(damageEvent);
        CombatAreaRules.ApplyAfterDamage(action, target, amount, action.RoundIndex, events);

        if (target.Hp <= 0 && target.BattleState != CharacterBattleState.DEAD)
        {
            target.BattleState = CharacterBattleState.DEAD;
            CombatEvent defeatedEvent = BuildEffectEvent(CombatEventType.CharacterDefeated, SkillEffectType.None, action, target);
            defeatedEvent.TargetHpAfter = target.Hp;
            events.Add(defeatedEvent);
        }
    }

    private static void ResolveAreaDamage(
        PlannedAction action,
        SkillEffectDefinition effect,
        List<CombatEvent> events,
        IReadOnlyList<CharacterState> roundStates)
    {
        CombatAreaId targetAreaId = ResolveAreaDamageTargetAreaId(action);
        List<CharacterState> targets = roundStates?
            .Where(state =>
                state != null &&
                !state.IsDefeated &&
                (!effect.ExcludeSource || state != action.Source) &&
                targetAreaId != CombatAreaId.Unknown &&
                state.CurrentAreaId == targetAreaId)
            .ToList() ?? new List<CharacterState>();

        if (targets.Count == 0)
        {
            events.Add(BuildSkillFailedEvent(action, SkillFailReason.MissingTarget));
            return;
        }

        foreach (CharacterState target in targets)
        {
            ResolveDamage(action, effect, events, target);
        }
    }

    private static CombatAreaId ResolveAreaDamageTargetAreaId(PlannedAction action)
    {
        if (action?.Skill != null &&
            action.Skill.HasTag(SkillTag.Melee) &&
            action.Skill.HasTag(SkillTag.Area) &&
            action.Source != null)
        {
            return action.Source.CurrentAreaId;
        }

        CombatAreaId targetAreaId = action?.TargetAreaId ?? CombatAreaId.Unknown;
        if (targetAreaId != CombatAreaId.Unknown)
        {
            return targetAreaId;
        }

        if (action?.TargetArea != null)
        {
            return action.TargetArea.AreaId;
        }

        if (action?.TargetCharacter != null)
        {
            return action.TargetCharacter.CurrentAreaId;
        }

        return action?.Source?.CurrentAreaId ?? CombatAreaId.Unknown;
    }

    private static void ResolveHeal(PlannedAction action, SkillEffectDefinition effect, List<CombatEvent> events)
    {
        CharacterState target = action.TargetCharacter ?? action.Source;
        ResolveHealOnTarget(action, effect, events, target);
    }

    private static void ResolveHealOnTarget(
        PlannedAction action,
        SkillEffectDefinition effect,
        List<CombatEvent> events,
        CharacterState target)
    {
        if (target == null)
        {
            events.Add(BuildSkillFailedEvent(action, SkillFailReason.MissingTarget));
            return;
        }

        float multiplier = SkillEffectMath.ResolvePowerMultiplier(effect);
        multiplier *= CombatAreaRules.GetHealMultiplier(action);
        float amount = SkillEffectMath.CalculatePowerAmount(action.Source.Attack, multiplier);
        target.Hp = Math.Min(target.MaxHp, target.Hp + amount);

        CombatEvent healEvent = BuildEffectEvent(CombatEventType.HealApplied, SkillEffectType.Heal, action, target);
        healEvent.Amount = amount;
        healEvent.TargetHpAfter = target.Hp;
        events.Add(healEvent);
    }

    private static void ResolveAreaHeal(
        PlannedAction action,
        SkillEffectDefinition effect,
        List<CombatEvent> events,
        IReadOnlyList<CharacterState> roundStates)
    {
        CombatAreaId targetAreaId = ResolveTargetAreaId(action);
        List<CharacterState> targets = roundStates?
            .Where(state =>
                state != null &&
                !state.IsDefeated &&
                state.Faction == action.Source.Faction &&
                (effect.AffectAllies ||
                    (targetAreaId != CombatAreaId.Unknown && state.CurrentAreaId == targetAreaId)))
            .ToList() ?? new List<CharacterState>();

        if (targets.Count == 0)
        {
            events.Add(BuildSkillFailedEvent(action, SkillFailReason.MissingTarget));
            return;
        }

        foreach (CharacterState target in targets)
        {
            ResolveHealOnTarget(action, effect, events, target);
        }
    }

    private static void ResolveRestoreMp(
        PlannedAction action,
        SkillEffectDefinition effect,
        List<CombatEvent> events,
        IReadOnlyList<CharacterState> roundStates)
    {
        List<CharacterState> targets = effect.AffectAllies
            ? roundStates?
                .Where(state => state != null && !state.IsDefeated && state.Faction == action.Source.Faction)
                .ToList() ?? new List<CharacterState>()
            : new List<CharacterState> { action.TargetCharacter ?? action.Source };

        foreach (CharacterState target in targets.Where(target => target != null))
        {
            int amount = Mathf.RoundToInt(effect.Value);
            int before = target.CurrentMp;
            target.CurrentMp = Math.Clamp(target.CurrentMp + amount, 0, target.MaxMp);
            int changed = target.CurrentMp - before;
            if (changed == 0)
            {
                continue;
            }

            events.Add(new CombatEvent
            {
                EventType = CombatEventType.MpChanged,
                RoundIndex = action.RoundIndex,
                SlotIndex = action.SlotIndex,
                Source = action.Source,
                Target = target,
                SourceLegacyCharacterData = action.Source?.LegacyCharacterData,
                TargetLegacyCharacterData = target.LegacyCharacterData,
                Skill = action.Skill,
                Amount = changed
            });
        }
    }

    private static void ResolveMove(PlannedAction action, List<CombatEvent> events)
    {
        CombatAreaId targetAreaId = ResolveTargetAreaId(action);
        if (targetAreaId == CombatAreaId.Unknown)
        {
            events.Add(BuildSkillFailedEvent(action, SkillFailReason.MissingTarget));
            return;
        }

        Vector2I fromCoord = action.Source.LegacyCoord;
        AreaDefinition fromArea = action.Source.CurrentArea;
        CombatAreaId fromAreaId = action.Source.CurrentAreaId;
        action.Source.CurrentAreaId = targetAreaId;
        action.Source.LegacyCoord = AreaDefinition.GetLegacyCoordForAreaId(targetAreaId);
        action.Source.CurrentArea = AreaDefinition.FromKnownArea(targetAreaId);

        CombatEvent moveEvent = BuildEffectEvent(CombatEventType.CharacterMoved, SkillEffectType.Move, action);
        moveEvent.FromCoord = fromCoord;
        moveEvent.ToCoord = action.Source.LegacyCoord;
        moveEvent.FromAreaId = fromAreaId;
        moveEvent.ToAreaId = targetAreaId;
        events.Add(moveEvent);
        CombatAreaRules.ApplyAfterMove(action, fromArea, action.Source.CurrentArea, action.RoundIndex, events);
    }

    private static void ResolveMoveTarget(PlannedAction action, List<CombatEvent> events)
    {
        CharacterState target = action.TargetCharacter;
        CombatAreaId targetAreaId = ResolveTargetAreaId(action);
        if (target == null || targetAreaId == CombatAreaId.Unknown)
        {
            events.Add(BuildSkillFailedEvent(action, SkillFailReason.MissingTarget));
            return;
        }

        MoveCharacterState(target, targetAreaId, action, events);
    }

    private static void ResolveSwapPosition(PlannedAction action, List<CombatEvent> events)
    {
        if (action.Source == null || action.TargetCharacter == null)
        {
            events.Add(BuildSkillFailedEvent(action, SkillFailReason.MissingTarget));
            return;
        }

        CombatAreaId sourceAreaId = action.Source.CurrentAreaId;
        CombatAreaId targetAreaId = action.TargetCharacter.CurrentAreaId;
        if (sourceAreaId == CombatAreaId.Unknown || targetAreaId == CombatAreaId.Unknown)
        {
            events.Add(BuildSkillFailedEvent(action, SkillFailReason.MissingTarget));
            return;
        }

        MoveCharacterState(action.Source, targetAreaId, action, events);
        MoveCharacterState(action.TargetCharacter, sourceAreaId, action, events);
    }

    private static void MoveCharacterState(
        CharacterState movingCharacter,
        CombatAreaId targetAreaId,
        PlannedAction action,
        List<CombatEvent> events)
    {
        Vector2I fromCoord = movingCharacter.LegacyCoord;
        AreaDefinition fromArea = movingCharacter.CurrentArea;
        CombatAreaId fromAreaId = movingCharacter.CurrentAreaId;
        movingCharacter.CurrentAreaId = targetAreaId;
        movingCharacter.LegacyCoord = AreaDefinition.GetLegacyCoordForAreaId(targetAreaId);
        movingCharacter.CurrentArea = AreaDefinition.FromKnownArea(targetAreaId);

        CombatEvent moveEvent = BuildEffectEvent(CombatEventType.CharacterMoved, SkillEffectType.Move, action, movingCharacter);
        moveEvent.Source = movingCharacter;
        moveEvent.SourceLegacyCharacterData = movingCharacter.LegacyCharacterData;
        moveEvent.Target = action.TargetCharacter;
        moveEvent.TargetLegacyCharacterData = action.TargetCharacter?.LegacyCharacterData;
        moveEvent.FromCoord = fromCoord;
        moveEvent.ToCoord = movingCharacter.LegacyCoord;
        moveEvent.FromAreaId = fromAreaId;
        moveEvent.ToAreaId = targetAreaId;
        events.Add(moveEvent);
        CombatAreaRules.ApplyAfterMove(action, fromArea, movingCharacter.CurrentArea, action.RoundIndex, events);
    }

    private static CombatAreaId ResolveTargetAreaId(PlannedAction action)
    {
        CombatAreaId targetAreaId = action.TargetAreaId;
        if (targetAreaId == CombatAreaId.Unknown)
        {
            targetAreaId = action.TargetArea?.AreaId ?? CombatAreaId.Unknown;
        }
        if (targetAreaId == CombatAreaId.Unknown && action.TargetCoord.HasValue)
        {
            targetAreaId = AreaDefinition.GetAreaIdForLegacyCoord(action.TargetCoord.Value);
        }
        if (targetAreaId == CombatAreaId.Unknown && action.TargetCharacter != null)
        {
            targetAreaId = action.TargetCharacter.CurrentAreaId;
        }

        return targetAreaId;
    }

    private static void SpendMp(PlannedAction action, List<CombatEvent> events)
    {
        if (action?.Source == null || action.Skill == null || action.Skill.MpCost <= 0)
        {
            return;
        }

        action.Source.CurrentMp = Math.Max(0, action.Source.CurrentMp - action.Skill.MpCost);
        events.Add(new CombatEvent
        {
            EventType = CombatEventType.MpChanged,
            RoundIndex = action.RoundIndex,
            SlotIndex = action.SlotIndex,
            Source = action.Source,
            Target = action.Source,
            SourceLegacyCharacterData = action.Source.LegacyCharacterData,
            TargetLegacyCharacterData = action.Source.LegacyCharacterData,
            Skill = action.Skill,
            Amount = -action.Skill.MpCost
        });
    }

    private static bool TryResolveCounter(PlannedAction action, CharacterState target, List<CombatEvent> events)
    {
        if (action?.Skill == null || action.Source == null || target == null || target.IsDefeated)
        {
            return false;
        }

        bool isMelee = action.Skill.HasTag(SkillTag.Melee);
        bool isSingleMelee = isMelee && action.Skill.HasTag(SkillTag.SingleTarget);
        string counterStatus = string.Empty;
        if (target.HasStatus(StatusCatalog.CounterMelee) && isMelee)
        {
            counterStatus = StatusCatalog.CounterMelee;
        }
        else if (target.HasStatus(StatusCatalog.CounterSingleMelee) && isSingleMelee)
        {
            counterStatus = StatusCatalog.CounterSingleMelee;
        }

        if (string.IsNullOrEmpty(counterStatus))
        {
            return false;
        }

        target.RemoveStatus(counterStatus);
        events.Add(CombatAreaRules.BuildStatusEvent(CombatEventType.StatusRemoved, target, counterStatus, action.RoundIndex));

        float counterAmount = SkillEffectMath.CalculatePowerAmount(target.Attack, ResolveCounterDamageEffect(counterStatus));
        CharacterState attacker = action.Source;
        counterAmount = CombatAreaRules.AbsorbShield(attacker, counterAmount, action.RoundIndex, events);
        attacker.Hp = Math.Max(0, attacker.Hp - counterAmount);
        CombatEvent counterDamageEvent = new()
        {
            EventType = CombatEventType.DamageApplied,
            RoundIndex = action.RoundIndex,
            SlotIndex = action.SlotIndex,
            Source = target,
            Target = attacker,
            SourceLegacyCharacterData = target.LegacyCharacterData,
            TargetLegacyCharacterData = attacker.LegacyCharacterData,
            Skill = action.Skill,
            Amount = counterAmount,
            TargetHpAfter = attacker.Hp,
            StatusId = counterStatus
        };
        events.Add(counterDamageEvent);

        if (attacker.Hp <= 0 && attacker.BattleState != CharacterBattleState.DEAD)
        {
            attacker.BattleState = CharacterBattleState.DEAD;
            CombatEvent defeatedEvent = BuildEffectEvent(CombatEventType.CharacterDefeated, SkillEffectType.None, action, attacker);
            defeatedEvent.TargetHpAfter = attacker.Hp;
            events.Add(defeatedEvent);
        }

        return true;
    }

    private static SkillEffectDefinition ResolveCounterDamageEffect(string counterStatus)
    {
        return StatusCatalog.Create(counterStatus)
            .TriggerEffects
            .FirstOrDefault(effect => effect?.EffectType == SkillEffectType.Damage)
            ?? SkillEffectDefinition.Damage(1.0f);
    }

    private static void ScheduleEffect(
        PlannedAction action,
        SkillEffectDefinition effect,
        List<CombatEvent> events,
        Dictionary<int, List<ScheduledCombatEffect>> priorityScheduledEffectsBySlot,
        Dictionary<int, List<ScheduledCombatEffect>> slotEndScheduledEffectsBySlot,
        List<ScheduledCombatEffect> nextRoundScheduledEffects)
    {
        events.Add(BuildEffectEvent(CombatEventType.ScheduledEffectCreated, SkillEffectType.ScheduledEffect, action));
        ResolveDueTiming(action.RoundIndex, action.SlotIndex, effect.DelaySlots, out int dueRound, out int dueSlot);
        PlannedAction scheduledAction = CloneActionForScheduledEffect(action);
        scheduledAction.RoundIndex = dueRound;
        scheduledAction.SlotIndex = dueSlot;

        if (effect.SnapshotTargetAreaOnSchedule)
        {
            CombatAreaId snapshotAreaId = ResolveScheduledSnapshotAreaId(action);
            if (snapshotAreaId != CombatAreaId.Unknown)
            {
                scheduledAction.TargetAreaId = snapshotAreaId;
                scheduledAction.TargetArea = AreaDefinition.FromKnownArea(snapshotAreaId);
                scheduledAction.TargetCoord = AreaDefinition.GetLegacyCoordForAreaId(snapshotAreaId);
            }
        }

        ScheduledCombatEffect scheduledEffect = new()
        {
            Action = scheduledAction,
            Effect = CloneWithoutDelay(effect),
            DueRoundIndex = dueRound,
            DueSlotIndex = dueSlot,
            Priority = effect.ResolveWithActionPriority ? CombatAreaRules.GetAdjustedPriority(scheduledAction) : 10,
            Faction = scheduledAction.Faction,
            ResolveWithActionPriority = effect.ResolveWithActionPriority,
            RequiresLivingSource = effect.RequiresLivingSourceOnTrigger
        };

        QueueScheduledEffect(scheduledEffect, action.RoundIndex, priorityScheduledEffectsBySlot, slotEndScheduledEffectsBySlot, nextRoundScheduledEffects);
    }

    private static void ResolveSlotEndScheduledEffects(
        int slotIndex,
        Dictionary<int, List<ScheduledCombatEffect>> slotEndScheduledEffectsBySlot,
        List<CombatEvent> events,
        IReadOnlyList<CharacterState> roundStates,
        Dictionary<int, List<ScheduledCombatEffect>> priorityScheduledEffectsBySlot,
        List<ScheduledCombatEffect> nextRoundScheduledEffects)
    {
        if (!slotEndScheduledEffectsBySlot.TryGetValue(slotIndex, out List<ScheduledCombatEffect> effects))
        {
            return;
        }

        foreach (ScheduledCombatEffect scheduledEffect in effects
            .OrderBy(FactionSortValue)
            .ThenBy(effect => effect.Sequence))
        {
            ResolveScheduledEffect(scheduledEffect, events, roundStates, priorityScheduledEffectsBySlot, slotEndScheduledEffectsBySlot, nextRoundScheduledEffects);
        }
    }

    private static void ResolveScheduledEffect(
        ScheduledCombatEffect scheduledEffect,
        List<CombatEvent> events,
        IReadOnlyList<CharacterState> roundStates,
        Dictionary<int, List<ScheduledCombatEffect>> priorityScheduledEffectsBySlot,
        Dictionary<int, List<ScheduledCombatEffect>> slotEndScheduledEffectsBySlot,
        List<ScheduledCombatEffect> nextRoundScheduledEffects)
    {
        if (scheduledEffect?.Action == null || scheduledEffect.Effect == null)
        {
            return;
        }

        PlannedAction action = scheduledEffect.Action;
        action.RoundIndex = scheduledEffect.DueRoundIndex;
        action.SlotIndex = scheduledEffect.DueSlotIndex;
        if (scheduledEffect.RequiresLivingSource)
        {
            SkillFailReason failReason = GetScheduledTriggerFailReason(action);
            if (failReason != SkillFailReason.None)
            {
                events.Add(BuildSkillFailedEvent(action, failReason));
                return;
            }
        }

        ResolveEffect(action, scheduledEffect.Effect, events, roundStates, priorityScheduledEffectsBySlot, slotEndScheduledEffectsBySlot, nextRoundScheduledEffects);
    }

    private static void QueueCarryoverScheduledEffects(
        IEnumerable<ScheduledCombatEffect> carryoverScheduledEffects,
        int roundIndex,
        Dictionary<CharacterData, CharacterState> statesByLegacyCharacter,
        Dictionary<int, List<ScheduledCombatEffect>> priorityScheduledEffectsBySlot,
        Dictionary<int, List<ScheduledCombatEffect>> slotEndScheduledEffectsBySlot,
        List<ScheduledCombatEffect> nextRoundScheduledEffects)
    {
        foreach (ScheduledCombatEffect scheduledEffect in carryoverScheduledEffects ?? Enumerable.Empty<ScheduledCombatEffect>())
        {
            if (scheduledEffect == null)
            {
                continue;
            }

            scheduledEffect.Action = NormalizeScheduledAction(scheduledEffect.Action, statesByLegacyCharacter);
            QueueScheduledEffect(scheduledEffect, roundIndex, priorityScheduledEffectsBySlot, slotEndScheduledEffectsBySlot, nextRoundScheduledEffects);
        }
    }

    private static void QueueScheduledEffect(
        ScheduledCombatEffect scheduledEffect,
        int currentRoundIndex,
        Dictionary<int, List<ScheduledCombatEffect>> priorityScheduledEffectsBySlot,
        Dictionary<int, List<ScheduledCombatEffect>> slotEndScheduledEffectsBySlot,
        List<ScheduledCombatEffect> nextRoundScheduledEffects)
    {
        if (scheduledEffect == null)
        {
            return;
        }

        if (scheduledEffect.DueRoundIndex > currentRoundIndex)
        {
            nextRoundScheduledEffects.Add(scheduledEffect);
            return;
        }

        Dictionary<int, List<ScheduledCombatEffect>> targetDictionary = scheduledEffect.ResolveWithActionPriority
            ? priorityScheduledEffectsBySlot
            : slotEndScheduledEffectsBySlot;
        if (!targetDictionary.TryGetValue(scheduledEffect.DueSlotIndex, out List<ScheduledCombatEffect> effects))
        {
            effects = new List<ScheduledCombatEffect>();
            targetDictionary[scheduledEffect.DueSlotIndex] = effects;
        }

        scheduledEffect.Sequence = effects.Count;
        effects.Add(scheduledEffect);
    }

    private static PlannedAction NormalizeScheduledAction(
        PlannedAction action,
        Dictionary<CharacterData, CharacterState> statesByLegacyCharacter)
    {
        if (action == null)
        {
            return null;
        }

        action.Source = ResolveSharedState(action.Source, statesByLegacyCharacter);
        action.TargetCharacter = ResolveSharedState(action.TargetCharacter, statesByLegacyCharacter);
        return action;
    }

    private static List<SlotResolutionItem> BuildSlotResolutionItems(
        IEnumerable<PlannedAction> normalizedActions,
        Dictionary<int, List<ScheduledCombatEffect>> priorityScheduledEffectsBySlot,
        int slotIndex)
    {
        List<SlotResolutionItem> items = new();
        int sequence = 0;
        foreach (PlannedAction action in normalizedActions
            .Where(action => action.SlotIndex == slotIndex && !action.IsDefault))
        {
            items.Add(new SlotResolutionItem
            {
                Action = action,
                Priority = CombatAreaRules.GetAdjustedPriority(action),
                Faction = action.Faction,
                Sequence = sequence++
            });
        }

        if (priorityScheduledEffectsBySlot.TryGetValue(slotIndex, out List<ScheduledCombatEffect> scheduledEffects))
        {
            foreach (ScheduledCombatEffect scheduledEffect in scheduledEffects)
            {
                items.Add(new SlotResolutionItem
                {
                    ScheduledEffect = scheduledEffect,
                    Priority = scheduledEffect.Priority,
                    Faction = scheduledEffect.Faction,
                    Sequence = sequence++
                });
            }
        }

        return items
            .OrderByDescending(item => item.Priority)
            .ThenBy(FactionSortValue)
            .ThenBy(item => item.Sequence)
            .ToList();
    }

    private static int FactionSortValue(SlotResolutionItem item)
    {
        return item?.Faction == CombatFaction.Player ? 0 : 1;
    }

    private static int FactionSortValue(ScheduledCombatEffect effect)
    {
        return effect?.Faction == CombatFaction.Player ? 0 : 1;
    }

    private static void ResolveDueTiming(int roundIndex, int slotIndex, int delaySlots, out int dueRound, out int dueSlot)
    {
        int zeroBasedDueSlot = slotIndex - Timeline.MinSlotIndex + delaySlots;
        int roundOffset = zeroBasedDueSlot / Timeline.MaxSlotIndex;
        dueRound = roundIndex + roundOffset;
        dueSlot = Timeline.MinSlotIndex + zeroBasedDueSlot % Timeline.MaxSlotIndex;
    }

    private static SkillFailReason GetScheduledTriggerFailReason(PlannedAction action)
    {
        if (action == null || action.IsDefault || action.Skill == null)
        {
            return SkillFailReason.MissingSkill;
        }

        if (action.Source == null || action.Source.LegacyCharacterData == null)
        {
            return SkillFailReason.MissingSource;
        }

        if (action.Source.IsDefeated)
        {
            return SkillFailReason.SourceDefeated;
        }

        if (action.TargetCharacter != null &&
            !action.Skill.HasTag(SkillTag.Area) &&
            action.TargetCharacter.IsDefeated)
        {
            return SkillFailReason.TargetDefeated;
        }

        if (action.Skill.HasTag(SkillTag.Melee) &&
            !action.Skill.HasTag(SkillTag.Move) &&
            !action.Skill.HasTag(SkillTag.Area) &&
            IsMeleeTargetInDifferentArea(action))
        {
            return SkillFailReason.MeleeTargetNotInSameArea;
        }

        if (CombatAreaRules.TryEvade(action))
        {
            return SkillFailReason.TargetEvaded;
        }

        return SkillFailReason.None;
    }

    private static CombatAreaId ResolveScheduledSnapshotAreaId(PlannedAction action)
    {
        if (action?.TargetCharacter != null)
        {
            return action.TargetCharacter.CurrentAreaId;
        }

        return ResolveTargetAreaId(action);
    }

    private static PlannedAction CloneActionForScheduledEffect(PlannedAction action)
    {
        return new PlannedAction
        {
            ActionId = action.ActionId,
            IsDefault = action.IsDefault,
            RoundIndex = action.RoundIndex,
            SlotIndex = action.SlotIndex,
            Faction = action.Faction,
            Source = action.Source,
            Skill = action.Skill,
            TargetCharacter = action.TargetCharacter,
            TargetArea = action.TargetArea,
            TargetAreaId = action.TargetAreaId,
            TargetCoord = action.TargetCoord,
            IsRevealed = action.IsRevealed,
            LegacyCommandExecuteInfo = action.LegacyCommandExecuteInfo
        };
    }

    private static SkillEffectDefinition CloneWithoutDelay(SkillEffectDefinition effect)
    {
        return new SkillEffectDefinition
        {
            EffectType = effect.EffectType,
            Value = effect.Value,
            StatusId = effect.StatusId,
            TargetAreaId = effect.TargetAreaId,
            ResolveWithActionPriority = effect.ResolveWithActionPriority,
            RequiresLivingSourceOnTrigger = effect.RequiresLivingSourceOnTrigger,
            SnapshotTargetAreaOnSchedule = effect.SnapshotTargetAreaOnSchedule,
            ExcludeSource = effect.ExcludeSource,
            AffectAllies = effect.AffectAllies,
            Message = effect.Message
        };
    }

    private static void TickSlotStatuses(IEnumerable<CharacterState> states, int roundIndex, List<CombatEvent> events)
    {
        foreach (CharacterState state in states ?? Enumerable.Empty<CharacterState>())
        {
            List<string> expiredStatusIds = state.Statuses
                .Where(status => status?.Definition?.DurationType == StatusDurationType.Slots)
                .Select(status =>
                {
                    status.TickDuration();
                    return status.IsExpired ? status.Definition.Id : string.Empty;
                })
                .Where(statusId => !string.IsNullOrEmpty(statusId))
                .ToList();

            foreach (string statusId in expiredStatusIds)
            {
                if (state.RemoveStatus(statusId))
                {
                    events.Add(CombatAreaRules.BuildStatusEvent(CombatEventType.StatusRemoved, state, statusId, roundIndex));
                }
            }
        }
    }

    private static CombatEvent BuildEffectEvent(
        CombatEventType eventType,
        SkillEffectType effectType,
        PlannedAction action,
        CharacterState targetOverride = null)
    {
        CombatEvent combatEvent = CombatEvent.ForAction(eventType, action);
        combatEvent.EffectType = effectType;
        if (targetOverride != null)
        {
            combatEvent.Target = targetOverride;
            combatEvent.TargetLegacyCharacterData = targetOverride.LegacyCharacterData;
        }

        combatEvent.SourceHpAfter = action.Source?.Hp ?? 0;
        combatEvent.TargetHpAfter = combatEvent.Target?.Hp ?? 0;
        return combatEvent;
    }

    private static void AssignEventIds(List<CombatEvent> events)
    {
        for (int i = 0; i < events.Count; i++)
        {
            CombatEvent combatEvent = events[i];
            combatEvent.EventId = $"round_{combatEvent.RoundIndex}_slot_{combatEvent.SlotIndex}_{i}_{combatEvent.EventType}";
        }
    }

    private sealed class SlotResolutionItem
    {
        public PlannedAction Action { get; set; }
        public ScheduledCombatEffect ScheduledEffect { get; set; }
        public int Priority { get; set; }
        public CombatFaction Faction { get; set; } = CombatFaction.Unknown;
        public int Sequence { get; set; }
    }
}

public sealed class ScheduledCombatEffect
{
    public PlannedAction Action { get; set; }
    public SkillEffectDefinition Effect { get; set; }
    public int DueRoundIndex { get; set; }
    public int DueSlotIndex { get; set; }
    public int Priority { get; set; }
    public CombatFaction Faction { get; set; } = CombatFaction.Unknown;
    public bool ResolveWithActionPriority { get; set; }
    public bool RequiresLivingSource { get; set; }
    public int Sequence { get; set; }
}

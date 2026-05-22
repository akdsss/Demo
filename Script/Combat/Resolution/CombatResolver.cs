using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public class CombatResolver
{
    public List<CombatEvent> ResolveRound(IEnumerable<PlannedAction> actions, int roundIndex, IEnumerable<CharacterData> allCharacters = null)
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

        for (int slotIndex = Timeline.MinSlotIndex; slotIndex <= Timeline.MaxSlotIndex; slotIndex++)
        {
            IEnumerable<PlannedAction> slotActions = normalizedActions
                .Where(action => action.SlotIndex == slotIndex && !action.IsDefault)
                .OrderByDescending(CombatAreaRules.GetAdjustedPriority);

            foreach (PlannedAction action in slotActions)
            {
                ResolveAction(action, events, roundStates);
            }
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

    public List<CombatEvent> ResolveSingleAction(PlannedAction action)
    {
        return ResolveRound(new[] { action }, action?.RoundIndex ?? 0)
            .Where(combatEvent =>
                combatEvent.EventType != CombatEventType.RoundStarted &&
                combatEvent.EventType != CombatEventType.RoundEnded)
            .ToList();
    }

    public List<CombatEvent> ResolveSingleLegacyCommand(CommandExecuteInfo commandExecuteInfo, int slotIndex = 1)
    {
        PlannedAction action = commandExecuteInfo.ToPlannedAction(slotIndex);
        return ResolveSingleAction(action);
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
        IReadOnlyList<CharacterState> roundStates)
    {
        SkillFailReason failReason = GetResolutionFailReason(action);
        if (failReason != SkillFailReason.None)
        {
            events.Add(BuildSkillFailedEvent(action, failReason));
            return;
        }

        events.Add(CombatEvent.ForAction(CombatEventType.ActionStarted, action));

        if (action.Skill.Effects.Count == 0)
        {
            return;
        }

        foreach (SkillEffectDefinition effect in action.Skill.Effects)
        {
            ResolveEffect(action, effect, events, roundStates);
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

        if (CombatAreaRules.TryEvade(action))
        {
            return SkillFailReason.TargetEvaded;
        }

        return SkillFailReason.None;
    }

    private static bool IsMeleeTargetInDifferentArea(PlannedAction action)
    {
        AreaDefinition sourceArea = action.Source?.CurrentArea;
        AreaDefinition targetArea = action.TargetCharacter?.CurrentArea;
        return sourceArea != null && targetArea != null && !sourceArea.IsSameArea(targetArea);
    }

    private static bool IsMoveTargetSameAsSourceArea(PlannedAction action)
    {
        if (action?.Source == null)
        {
            return false;
        }

        if (action.TargetCoord.HasValue && action.TargetCoord.Value == action.Source.LegacyCoord)
        {
            return true;
        }

        if (action.TargetArea != null && action.Source.CurrentArea != null)
        {
            return action.TargetArea.IsSameArea(action.Source.CurrentArea);
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
        IReadOnlyList<CharacterState> roundStates)
    {
        if (effect == null)
        {
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
                ResolveHeal(action, effect, events);
                break;
            case SkillEffectType.Move:
                ResolveMove(action, events);
                break;
            case SkillEffectType.Defend:
                events.Add(BuildEffectEvent(CombatEventType.DefenseApplied, SkillEffectType.Defend, action));
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

        float multiplier = effect.Value <= 0 ? 1.0f : effect.Value;
        multiplier *= CombatAreaRules.GetDamageMultiplier(action, target);
        float amount = Math.Max(0, action.Source.Attack * multiplier);
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
        AreaDefinition targetArea = action.TargetCharacter?.CurrentArea ??
            action.TargetArea ??
            action.Source?.CurrentArea;
        List<CharacterState> targets = roundStates?
            .Where(state =>
                state != null &&
                !state.IsDefeated &&
                state != action.Source &&
                targetArea != null &&
                state.CurrentArea != null &&
                state.CurrentArea.IsSameArea(targetArea))
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

    private static void ResolveHeal(PlannedAction action, SkillEffectDefinition effect, List<CombatEvent> events)
    {
        CharacterState target = action.TargetCharacter ?? action.Source;
        float multiplier = effect.Value <= 0 ? 1.0f : effect.Value;
        multiplier *= CombatAreaRules.GetHealMultiplier(action);
        float amount = Math.Max(0, action.Source.Attack * multiplier);
        target.Hp = Math.Min(target.MaxHp, target.Hp + amount);

        CombatEvent healEvent = BuildEffectEvent(CombatEventType.HealApplied, SkillEffectType.Heal, action, target);
        healEvent.Amount = amount;
        healEvent.TargetHpAfter = target.Hp;
        events.Add(healEvent);
    }

    private static void ResolveMove(PlannedAction action, List<CombatEvent> events)
    {
        Vector2I? targetCoord = action.TargetCoord ?? action.TargetArea?.LegacyCoord;
        if (!targetCoord.HasValue)
        {
            events.Add(BuildSkillFailedEvent(action, SkillFailReason.MissingTarget));
            return;
        }

        Vector2I fromCoord = action.Source.LegacyCoord;
        AreaDefinition fromArea = action.Source.CurrentArea;
        action.Source.LegacyCoord = targetCoord.Value;
        action.Source.CurrentArea = AreaDefinition.FromLegacyCoord(targetCoord.Value);

        CombatEvent moveEvent = BuildEffectEvent(CombatEventType.CharacterMoved, SkillEffectType.Move, action);
        moveEvent.FromCoord = fromCoord;
        moveEvent.ToCoord = targetCoord.Value;
        events.Add(moveEvent);
        CombatAreaRules.ApplyAfterMove(action, fromArea, action.Source.CurrentArea, action.RoundIndex, events);
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
}

using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public class CombatResolver
{
    public List<CombatEvent> ResolveRound(IEnumerable<PlannedAction> actions, int roundIndex)
    {
        List<PlannedAction> normalizedActions = NormalizeActions(actions, roundIndex);
        List<CombatEvent> events = new()
        {
            new CombatEvent
            {
                EventType = CombatEventType.RoundStarted,
                RoundIndex = roundIndex,
                Message = $"第 {roundIndex} 回合结算开始"
            }
        };

        for (int slotIndex = Timeline.MinSlotIndex; slotIndex <= Timeline.MaxSlotIndex; slotIndex++)
        {
            IEnumerable<PlannedAction> slotActions = normalizedActions
                .Where(action => action.SlotIndex == slotIndex && !action.IsDefault)
                .OrderByDescending(action => action.Skill?.Priority ?? 0);

            foreach (PlannedAction action in slotActions)
            {
                ResolveAction(action, events);
            }
        }

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

    private static List<PlannedAction> NormalizeActions(IEnumerable<PlannedAction> actions, int roundIndex)
    {
        List<PlannedAction> normalizedActions = new();
        Dictionary<CharacterData, CharacterState> statesByLegacyCharacter = new();

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

    private static void ResolveAction(PlannedAction action, List<CombatEvent> events)
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
            ResolveEffect(action, effect, events);
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

        return SkillFailReason.None;
    }

    private static CombatEvent BuildSkillFailedEvent(PlannedAction action, SkillFailReason failReason)
    {
        CombatEvent combatEvent = CombatEvent.ForAction(CombatEventType.SkillFailed, action);
        combatEvent.FailReason = failReason;
        combatEvent.Message = failReason.ToString();
        return combatEvent;
    }

    private static void ResolveEffect(PlannedAction action, SkillEffectDefinition effect, List<CombatEvent> events)
    {
        if (effect == null)
        {
            return;
        }

        switch (effect.EffectType)
        {
            case SkillEffectType.Damage:
                ResolveDamage(action, effect, events);
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
                CombatEvent statusAppliedEvent = BuildEffectEvent(CombatEventType.StatusApplied, SkillEffectType.ApplyStatus, action);
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
        CharacterState target = action.TargetCharacter;
        if (target == null)
        {
            events.Add(BuildSkillFailedEvent(action, SkillFailReason.MissingTarget));
            return;
        }

        float multiplier = effect.Value <= 0 ? 1.0f : effect.Value;
        float amount = Math.Max(0, action.Source.Attack * multiplier);
        target.Hp = Math.Max(0, target.Hp - amount);

        CombatEvent damageEvent = BuildEffectEvent(CombatEventType.DamageApplied, SkillEffectType.Damage, action);
        damageEvent.Amount = amount;
        damageEvent.TargetHpAfter = target.Hp;
        events.Add(damageEvent);

        if (target.Hp <= 0 && target.BattleState != CharacterBattleState.DEAD)
        {
            target.BattleState = CharacterBattleState.DEAD;
            CombatEvent defeatedEvent = BuildEffectEvent(CombatEventType.CharacterDefeated, SkillEffectType.None, action);
            defeatedEvent.TargetHpAfter = target.Hp;
            events.Add(defeatedEvent);
        }
    }

    private static void ResolveHeal(PlannedAction action, SkillEffectDefinition effect, List<CombatEvent> events)
    {
        CharacterState target = action.TargetCharacter ?? action.Source;
        float multiplier = effect.Value <= 0 ? 1.0f : effect.Value;
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
        action.Source.LegacyCoord = targetCoord.Value;
        action.Source.CurrentArea = AreaDefinition.FromLegacyCoord(targetCoord.Value);

        CombatEvent moveEvent = BuildEffectEvent(CombatEventType.CharacterMoved, SkillEffectType.Move, action);
        moveEvent.FromCoord = fromCoord;
        moveEvent.ToCoord = targetCoord.Value;
        events.Add(moveEvent);
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

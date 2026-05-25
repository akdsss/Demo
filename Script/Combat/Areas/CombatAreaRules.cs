using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public static class CombatAreaRules
{
    public const int DesignPriorityStep = 10;

    public static int GetAdjustedPriority(PlannedAction action)
    {
        int priority = action?.Skill?.Priority ?? 0;
        if (action?.Source?.CurrentAreaId == CombatAreaId.Zhen &&
            action.Skill.HasTag(SkillTag.Move))
        {
            priority += DesignPriorityStep;
        }

        return priority;
    }

    public static int ToDisplayPriority(int priority)
    {
        if (priority <= 0)
        {
            return priority;
        }

        return Math.Clamp(priority / DesignPriorityStep, 1, 5);
    }

    public static void ApplyRoundStart(IEnumerable<CharacterState> states, int roundIndex, List<CombatEvent> events)
    {
        foreach (CharacterState state in EnumerateLivingStates(states))
        {
            RemoveStatus(state, StatusCatalog.MoveBlocked, roundIndex, events);

            if (state.CurrentAreaId == CombatAreaId.Xun)
            {
                ApplyStatus(state, StatusCatalog.XunDodge, roundIndex, events);
            }
            else
            {
                RemoveStatus(state, StatusCatalog.XunDodge, roundIndex, events);
            }
        }
    }

    public static void ApplyRoundEnd(IEnumerable<CharacterState> states, int roundIndex, List<CombatEvent> events)
    {
        foreach (CharacterState state in EnumerateLivingStates(states))
        {
            ResolveBurn(state, roundIndex, events);
            ResolveKunEndHeal(state, roundIndex, events);
            ResolveXunGale(state, roundIndex, events);
        }
    }

    public static bool TryEvade(PlannedAction action)
    {
        if (action?.TargetCharacter == null || action.Skill == null)
        {
            return false;
        }

        bool isSingleRanged = action.Skill.HasTag(SkillTag.Ranged) &&
            action.Skill.HasTag(SkillTag.SingleTarget);
        return isSingleRanged &&
            HasActiveDodge(action.TargetCharacter) &&
            !action.TargetCharacter.HasStatus(StatusCatalog.Mark);
    }

    public static float GetDamageMultiplier(PlannedAction action, CharacterState target)
    {
        if (action?.Source == null || action.Skill == null)
        {
            return 1.0f;
        }

        return GetDamageMultiplier(action.Source, action.Skill.Tags, target);
    }

    public static float GetDamageMultiplier(CharacterState source, SkillTag skillTags, CharacterState target)
    {
        if (source == null)
        {
            return 1.0f;
        }

        float multiplier = 1.0f;
        CombatAreaId sourceArea = source.CurrentAreaId;
        CombatAreaId targetArea = target?.CurrentAreaId ?? CombatAreaId.Unknown;

        if (source.HasStatus(StatusCatalog.Rage))
        {
            multiplier *= 1.5f;
        }

        if (source.HasStatus(StatusCatalog.PowerUp))
        {
            multiplier *= 1.5f;
        }

        if (sourceArea == CombatAreaId.Yin)
        {
            multiplier *= 0.85f;
        }
        else if (sourceArea == CombatAreaId.Yang)
        {
            multiplier *= 1.15f;
        }

        multiplier *= GetDamageTakenMultiplier(target);

        if (sourceArea == CombatAreaId.Qian && HasTag(skillTags, SkillTag.Area))
        {
            multiplier *= 1.3f;
        }

        if (sourceArea == CombatAreaId.Dui && HasTag(skillTags, SkillTag.Ranged))
        {
            multiplier *= 1.2f;
        }

        if (sourceArea == CombatAreaId.Li && (targetArea == CombatAreaId.Qian || targetArea == CombatAreaId.Dui))
        {
            multiplier *= 1.5f;
        }

        if (sourceArea == CombatAreaId.Zhen && (targetArea == CombatAreaId.Gen || targetArea == CombatAreaId.Kun))
        {
            multiplier *= 1.5f;
        }

        if (sourceArea == CombatAreaId.Gen && HasTag(skillTags, SkillTag.Melee))
        {
            multiplier *= 1.3f;
        }
        else if (HasTag(skillTags, SkillTag.Melee) && source.HasStatus(StatusCatalog.GenMeleeCarryover))
        {
            multiplier *= 1.3f;
        }

        return multiplier;
    }

    public static float GetDamageTakenMultiplier(CharacterState target)
    {
        if (target == null)
        {
            return 1.0f;
        }

        float multiplier = 1.0f;
        if (target.HasStatus(StatusCatalog.Rage))
        {
            multiplier *= 1.5f;
        }

        if (target.CurrentAreaId == CombatAreaId.Yin)
        {
            multiplier *= 0.85f;
        }
        else if (target.CurrentAreaId == CombatAreaId.Yang)
        {
            multiplier *= 1.15f;
        }
        else if (target.CurrentAreaId == CombatAreaId.Gen)
        {
            multiplier *= 0.9f;
        }

        return multiplier;
    }

    public static float GetHealMultiplier(PlannedAction action)
    {
        if (action?.Source?.CurrentAreaId == CombatAreaId.Kan)
        {
            return 1.4f;
        }

        return 1.0f;
    }

    public static void ApplyAfterDamage(PlannedAction action, CharacterState target, float dealtDamage, int roundIndex, List<CombatEvent> events)
    {
        if (action?.Source == null || target == null || action.Skill == null || dealtDamage <= 0)
        {
            return;
        }

        CombatAreaId sourceArea = action.Source.CurrentAreaId;
        CombatAreaId targetArea = target.CurrentAreaId;

        if (sourceArea == CombatAreaId.Qian)
        {
            ApplyStatus(target, StatusCatalog.Mark, roundIndex, events, action.Source);
        }

        if (sourceArea == CombatAreaId.Li)
        {
            ApplyStatus(target, StatusCatalog.Burn, roundIndex, events, action.Source);
        }

        if (sourceArea == CombatAreaId.Dui && action.Skill.HasTag(SkillTag.Ranged))
        {
            ApplyHpAndMpRestore(action.Source, dealtDamage * 0.1f, roundIndex, events);
        }

        if (action.Skill.HasTag(SkillTag.Melee) &&
            (sourceArea == CombatAreaId.Kun || action.Source.HasStatus(StatusCatalog.KunMeleeDrainCarryover)))
        {
            ApplyHeal(action.Source, dealtDamage * 0.15f, roundIndex, events, action.Source);
        }

        if (action.Skill.HasTag(SkillTag.Melee) && targetArea == CombatAreaId.Kan)
        {
            ApplyStatus(target, StatusCatalog.MoveBlocked, roundIndex, events, target);
        }

    }

    public static void ConsumeMeleeCarryoversAfterDamage(PlannedAction action, int roundIndex, List<CombatEvent> events)
    {
        if (action?.Source == null || action.Skill == null || !action.Skill.HasTag(SkillTag.Melee))
        {
            return;
        }

        RemoveStatus(action.Source, StatusCatalog.GenMeleeCarryover, roundIndex, events);
        RemoveStatus(action.Source, StatusCatalog.KunMeleeDrainCarryover, roundIndex, events);
    }

    public static void ApplyAfterMove(CharacterState movingCharacter, PlannedAction action, AreaDefinition fromArea, AreaDefinition toArea, int roundIndex, List<CombatEvent> events)
    {
        CharacterState mover = movingCharacter ?? action?.Source;
        if (mover == null)
        {
            return;
        }

        if (fromArea?.AreaId == CombatAreaId.Gen && toArea?.AreaId != CombatAreaId.Gen)
        {
            ApplyStatus(mover, StatusCatalog.GenMeleeCarryover, roundIndex, events, mover);
        }

        if (fromArea?.AreaId == CombatAreaId.Kun && toArea?.AreaId != CombatAreaId.Kun)
        {
            ApplyStatus(mover, StatusCatalog.KunMeleeDrainCarryover, roundIndex, events, mover);
        }

        if (fromArea?.AreaId == CombatAreaId.Xun && toArea?.AreaId != CombatAreaId.Xun)
        {
            RemoveStatus(mover, StatusCatalog.XunDodge, roundIndex, events);
        }

        if (toArea?.AreaId == CombatAreaId.Xun)
        {
            ApplyStatus(mover, StatusCatalog.XunDodge, roundIndex, events);
        }
    }

    private static bool HasActiveDodge(CharacterState target)
    {
        if (target == null)
        {
            return false;
        }

        return target.CurrentAreaId == CombatAreaId.Xun ||
            target.HasStatus(StatusCatalog.XunDodge) ||
            target.HasStatus(StatusCatalog.Dodge);
    }

    public static CombatEvent BuildStatusEvent(CombatEventType eventType, CharacterState target, string statusId, int roundIndex)
    {
        return new CombatEvent
        {
            EventType = eventType,
            RoundIndex = roundIndex,
            Target = target,
            TargetLegacyCharacterData = target?.LegacyCharacterData,
            StatusId = statusId,
            TargetHpAfter = target?.Hp ?? 0
        };
    }

    private static void ResolveBurn(CharacterState state, int roundIndex, List<CombatEvent> events)
    {
        if (!state.HasStatus(StatusCatalog.Burn))
        {
            return;
        }

        CombatAreaId areaId = state.CurrentAreaId;
        if (areaId == CombatAreaId.Kan)
        {
            RemoveStatus(state, StatusCatalog.Burn, roundIndex, events);
            return;
        }

        float percent = areaId == CombatAreaId.Qian || areaId == CombatAreaId.Dui ? 0.12f : 0.06f;
        ApplyDamage(state, state.MaxHp * percent, roundIndex, events, StatusCatalog.Burn);
        RemoveStatus(state, StatusCatalog.Burn, roundIndex, events);
    }

    private static void ResolveKunEndHeal(CharacterState state, int roundIndex, List<CombatEvent> events)
    {
        if (state.CurrentAreaId == CombatAreaId.Kun)
        {
            ApplyHeal(state, state.MaxHp * 0.06f, roundIndex, events, state);
        }
    }

    private static void ResolveXunGale(CharacterState state, int roundIndex, List<CombatEvent> events)
    {
        if (state.CurrentAreaId != CombatAreaId.Xun || state.HasStatus(StatusCatalog.GaleImmune))
        {
            return;
        }

        StatusInstance gale = state.AddOrRefreshStatus(StatusCatalog.Create(StatusCatalog.Gale), state);
        events.Add(BuildStatusEvent(CombatEventType.StatusApplied, state, StatusCatalog.Gale, roundIndex));
        if (gale != null && gale.StackCount >= 3)
        {
            RemoveStatus(state, StatusCatalog.Gale, roundIndex, events);
            ApplyDamage(state, state.MaxHp * 0.4f, roundIndex, events, StatusCatalog.Gale);
            ApplyStatus(state, StatusCatalog.GaleImmune, roundIndex, events, state);
        }
    }

    private static void ApplyStatus(CharacterState target, string statusId, int roundIndex, List<CombatEvent> events, CharacterState source = null)
    {
        if (target == null)
        {
            return;
        }

        target.AddOrRefreshStatus(StatusCatalog.Create(statusId), source);
        events.Add(BuildStatusEvent(CombatEventType.StatusApplied, target, statusId, roundIndex));
    }

    private static void RemoveStatus(CharacterState target, string statusId, int roundIndex, List<CombatEvent> events)
    {
        if (target != null && target.RemoveStatus(statusId))
        {
            events.Add(BuildStatusEvent(CombatEventType.StatusRemoved, target, statusId, roundIndex));
        }
    }

    private static void ApplyHeal(CharacterState target, float amount, int roundIndex, List<CombatEvent> events, CharacterState source)
    {
        if (target == null || amount <= 0)
        {
            return;
        }

        target.Hp = Math.Min(target.MaxHp, target.Hp + amount);
        events.Add(new CombatEvent
        {
            EventType = CombatEventType.HealApplied,
            RoundIndex = roundIndex,
            Source = source,
            Target = target,
            SourceLegacyCharacterData = source?.LegacyCharacterData,
            TargetLegacyCharacterData = target.LegacyCharacterData,
            Amount = amount,
            TargetHpAfter = target.Hp
        });
    }

    private static void ApplyHpAndMpRestore(CharacterState target, float amount, int roundIndex, List<CombatEvent> events)
    {
        ApplyHeal(target, amount, roundIndex, events, target);
        target.CurrentMp = Math.Min(target.MaxMp, target.CurrentMp + Mathf.RoundToInt(amount));
        events.Add(new CombatEvent
        {
            EventType = CombatEventType.MpChanged,
            RoundIndex = roundIndex,
            Target = target,
            TargetLegacyCharacterData = target.LegacyCharacterData,
            Amount = Mathf.RoundToInt(amount)
        });
    }

    private static void ApplyDamage(CharacterState target, float amount, int roundIndex, List<CombatEvent> events, string sourceStatus)
    {
        if (target == null || amount <= 0)
        {
            return;
        }

        amount *= GetDamageTakenMultiplier(target);
        target.Hp = Math.Max(0, target.Hp - amount);
        events.Add(new CombatEvent
        {
            EventType = CombatEventType.DamageApplied,
            RoundIndex = roundIndex,
            Target = target,
            TargetLegacyCharacterData = target.LegacyCharacterData,
            Amount = amount,
            TargetHpAfter = target.Hp,
            StatusId = sourceStatus
        });

        if (target.Hp <= 0 && target.BattleState != CharacterBattleState.DEAD)
        {
            target.BattleState = CharacterBattleState.DEAD;
            events.Add(new CombatEvent
            {
                EventType = CombatEventType.CharacterDefeated,
                RoundIndex = roundIndex,
                Target = target,
                TargetLegacyCharacterData = target.LegacyCharacterData,
                TargetHpAfter = target.Hp
            });
        }
    }

    private static IEnumerable<CharacterState> EnumerateLivingStates(IEnumerable<CharacterState> states)
    {
        return states?.Where(state => state != null && !state.IsDefeated) ?? Enumerable.Empty<CharacterState>();
    }

    private static bool HasTag(SkillTag tags, SkillTag tag)
    {
        return (tags & tag) == tag;
    }
}

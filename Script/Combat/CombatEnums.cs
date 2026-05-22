using System;

[Flags]
public enum SkillTag
{
    None = 0,
    Melee = 1 << 0,
    Ranged = 1 << 1,
    Move = 1 << 2,
    Defense = 1 << 3,
    Special = 1 << 4,
    SingleTarget = 1 << 5,
    Area = 1 << 6,
    Heal = 1 << 7
}

public enum CombatFaction
{
    Unknown,
    Player,
    Enemy
}

public enum SkillTargetType
{
    None,
    Self,
    Ally,
    Enemy,
    Character,
    Area,
    TimelineSlot
}

public enum SkillEffectType
{
    None,
    Damage,
    Heal,
    Move,
    ApplyStatus,
    RemoveStatus,
    Defend,
    ScheduledEffect
}

public enum CombatEventType
{
    None,
    RoundStarted,
    RoundEnded,
    ActionStarted,
    SkillFailed,
    CharacterMoved,
    DamageApplied,
    HealApplied,
    DefenseApplied,
    MpChanged,
    StatusApplied,
    StatusRemoved,
    ScheduledEffectCreated,
    CharacterDefeated
}

public enum SkillFailReason
{
    None,
    MissingSkill,
    MissingSource,
    SourceDefeated,
    ActionChanceInsufficient,
    MpInsufficient,
    MissingTarget,
    TargetDefeated,
    MeleeTargetNotInSameArea,
    MoveTargetAlreadyCurrentArea,
    RushTargetMustBeInDifferentArea,
    TimelineSlotUnavailable,
    BlockedByStatus,
    TargetEvaded
}

public enum TimelineSlotState
{
    Empty,
    Occupied,
    Locked
}

public enum CombatAreaId
{
    Unknown,
    Qian,
    Dui,
    Li,
    Zhen,
    Xun,
    Kan,
    Gen,
    Kun,
    Yin,
    Yang,
    LegacyCoord
}

public enum AreaEffectTiming
{
    RoundStart,
    RoundEnd,
    BeforeAction,
    AfterAction,
    BeforeDamage,
    AfterDamage,
    OnMove,
    OnLeaveArea,
    OnEnterArea,
    OnDefeated
}

public enum ModifierOperation
{
    Add,
    Multiply,
    Override
}

public enum StatusStackMode
{
    Unique,
    RefreshDuration,
    StackCount
}

public enum StatusDurationType
{
    Instant,
    Slots,
    Rounds,
    TriggerCount,
    UntilRemoved
}

public enum StatusRemoveCondition
{
    None,
    RoundEnd,
    NextMeleeHit,
    SourceDefeated,
    Manual
}

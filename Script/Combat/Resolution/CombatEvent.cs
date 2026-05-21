using Godot;

public class CombatEvent
{
    public CombatEventType EventType { get; set; } = CombatEventType.None;
    public string EventId { get; set; } = string.Empty;
    public string ActionId { get; set; } = string.Empty;
    public int RoundIndex { get; set; }
    public int SlotIndex { get; set; }
    public int Priority { get; set; }
    public CombatFaction Faction { get; set; } = CombatFaction.Unknown;
    public CharacterState Source { get; set; }
    public CharacterState Target { get; set; }
    public CharacterData SourceLegacyCharacterData { get; set; }
    public CharacterData TargetLegacyCharacterData { get; set; }
    public SkillDefinition Skill { get; set; }
    public SkillEffectType EffectType { get; set; } = SkillEffectType.None;
    public SkillFailReason FailReason { get; set; } = SkillFailReason.None;
    public string FailTextKey { get; set; } = string.Empty;
    public float Amount { get; set; }
    public float SourceHpAfter { get; set; }
    public float TargetHpAfter { get; set; }
    public Vector2I? FromCoord { get; set; }
    public Vector2I? ToCoord { get; set; }
    public string StatusId { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;

    public static CombatEvent ForAction(CombatEventType eventType, PlannedAction action)
    {
        return new CombatEvent
        {
            EventType = eventType,
            ActionId = action?.ActionId ?? string.Empty,
            RoundIndex = action?.RoundIndex ?? 0,
            SlotIndex = action?.SlotIndex ?? 0,
            Priority = action?.Skill?.Priority ?? 0,
            Faction = action?.Faction ?? CombatFaction.Unknown,
            Source = action?.Source,
            Target = action?.TargetCharacter,
            SourceLegacyCharacterData = action?.Source?.LegacyCharacterData,
            TargetLegacyCharacterData = action?.TargetCharacter?.LegacyCharacterData,
            Skill = action?.Skill,
            FailTextKey = action?.Skill?.FailTextKey ?? string.Empty
        };
    }
}

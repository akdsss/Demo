using Godot;

public class PlannedAction
{
    public string ActionId { get; set; } = string.Empty;
    public bool IsDefault { get; set; }
    public int RoundIndex { get; set; }
    public int SlotIndex { get; set; } = 1;
    public CombatFaction Faction { get; set; } = CombatFaction.Unknown;
    public CharacterState Source { get; set; }
    public SkillDefinition Skill { get; set; }
    public CharacterState TargetCharacter { get; set; }
    public AreaDefinition TargetArea { get; set; }
    public Vector2I? TargetCoord { get; set; }
    public bool IsRevealed { get; set; } = true;
    public CommandExecuteInfo LegacyCommandExecuteInfo { get; set; }

    public bool HasTargetCharacter
    {
        get { return TargetCharacter != null; }
    }

    public bool HasTargetArea
    {
        get { return TargetArea != null || TargetCoord.HasValue; }
    }

    public static PlannedAction Empty(int slotIndex, CombatFaction faction = CombatFaction.Unknown)
    {
        return new PlannedAction
        {
            IsDefault = true,
            SlotIndex = slotIndex,
            Faction = faction
        };
    }
}

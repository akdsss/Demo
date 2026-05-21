using Godot;

public class ValidationContext
{
    public CharacterState Source { get; set; }
    public CharacterState TargetCharacter { get; set; }
    public AreaDefinition TargetArea { get; set; }
    public Vector2I? TargetCoord { get; set; }
    public Timeline Timeline { get; set; }
    public int SlotIndex { get; set; } = Timeline.MinSlotIndex;
    public int RequiredActionCost { get; set; } = 1;
    public bool RequiresDifferentTargetArea { get; set; }
}

using Godot;

[GlobalClass]
public partial class PlayerInfoInLevel : Resource
{
    [Export] public PlayerData playerData;
    [Export] public Vector2I coord;
    [Export] public CombatAreaId areaId = CombatAreaId.Unknown;
}

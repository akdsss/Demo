using Godot;

[GlobalClass]
public partial class EnemyInfoInLevel : Resource
{
    [Export] public EnemyData enemyData;
    [Export] public Vector2I coord;
    [Export] public CombatAreaId areaId = CombatAreaId.Unknown;
}

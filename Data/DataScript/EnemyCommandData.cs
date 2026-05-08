using Godot;

[GlobalClass]
public partial class EnemyCommandData : CommandData
{
    [Export] public EnemyCommandName enemyCommandName;
}

public enum EnemyCommandName
{
    Move,
    Attack,
    Skill,
    Dead
}

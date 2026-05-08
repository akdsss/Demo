using Godot;

[GlobalClass]
public partial class EnemyData : CharacterData
{
    [Export] public int hp;
    [Export] public EnemyCommandData[] enemyCommandDataArray;
    [Export(PropertyHint.Enum, "Move,Attack,Skill,Dead")]
    public string[] enemyCommands = new string[0];
}


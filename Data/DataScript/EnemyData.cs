using Godot;

[GlobalClass]
public partial class EnemyData : CharacterData
{
    [Export] public int hp;
    //[Export] public EnemyCommandData[] enemyCommandDataArray;
    [Export(PropertyHint.Enum, "Move,Attack,Skip,Dead")]
    public string[] enemyCommands;
    
}


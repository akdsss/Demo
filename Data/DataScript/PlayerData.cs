using Godot;

[GlobalClass]
public partial class PlayerData : CharacterData
{
    [Export] public int hp;
    [Export] public PlayerCommandData[] playerCommandDataList;
}

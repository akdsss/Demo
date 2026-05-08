using Godot;
using System;

public partial class CharacterData : Resource
{
	[Export] public int characterId;
	[Export] public string characterName;
	[Export] public string characterDescription;
	public Vector2I coord;
    public CharacterBattleState characterBattleState;
}

public enum CharacterBattleState
{
	ALIVE,
	DYING,
    DEAD
}

using Godot;
using System;
using System.Collections.Generic;

public abstract partial class CharacterData : Resource
{
	[Export] public int characterId;
	[Export] public string characterName;
    [Export] public string characterDescription;
	[Export] public CharacterType characterType;
}

public enum CharacterType
{
	Player,
	Enemy
}

[GlobalClass]
public partial class PlayerData : CharacterData
{
    [Export] public int hp;
	[Export] public List<PlayerCommandData> playerCommandDataList;
}

[GlobalClass]
public partial class EnemyData : CharacterData
{
    [Export] public int hp;
	[Export] public List<EnemyCommandData> enemyCommandDataList;
}

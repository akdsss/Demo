using Godot;
using System;

[GlobalClass]
public partial class LevelData : Resource
{
	[Export] public int levelId;
	[Export] public string levelName;
	[Export] public LevelType levelType;
}

public enum LevelType
{
	NORMAL,
	BOSS
}

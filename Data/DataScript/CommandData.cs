using Godot;
using System;
using System.Collections.Generic;

[GlobalClass]
public partial class CommandData : Resource
{
	[Export] public int commandId;
	[Export] public string commandName;
	[Export] public string commandDescription;
	[Export] public CommandType commandType;
}

public class CommandExecuteInfo
{
	public int targetCharacterNum;
	public List<CharacterData> targetCharacterList;
}

public enum CommandType
{
	ATTACK,
	DEFENT,
	MOVE
}

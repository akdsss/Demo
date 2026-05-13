using Godot;
using System;
using System.Collections.Generic;

[GlobalClass]
public partial class CommandData : Resource
{
	[Export] public int commandId;
	[Export] public string commandName;
	[Export] public string commandDescription;
}

public class CommandExecuteInfo
{
	public CommandData commandData;
	public int targetCharacterNum;
	public List<CharacterData> targetCharacterList;
}

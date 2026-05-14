using Godot;
using System;
using System.Collections.Generic;

[GlobalClass]
public partial class CommandData : Resource
{
	[Export] public int commandId;
	[Export] public string commandName;
	[Export] public string commandDescription;
	[Export] public float[] allCommandParams;
}

public class CommandExecuteInfo
{
	public CharacterData sourceCharacterData;
	public CommandData commandData;
	public int targetCharacterNum;
	public Vector2I targetCoord;
	public List<CharacterData> targetCharacterList;
	public void Execute()
	{
		int cmdID = commandData.commandId;
		switch (cmdID)
		{
			case 0:// 攻击
				DamageEventManager dem = new();
				DamageEventInfo dei = new();
                dei.damageSourceCharacter = sourceCharacterData;
				dei.damageValue = sourceCharacterData.atk;
				dei.damageTargetCharacter = targetCharacterList[0];
				dem.Execute(dei);
				break;
			case 1:// 移动
				Autoloads.gd_ChessBoard.MoveCharacter(sourceCharacterData, targetCoord);
				break;
			case 2:// 防御
				break;
			default:
				break;
		}
	}
}

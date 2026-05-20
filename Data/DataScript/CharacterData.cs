using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class CharacterData : Resource
{
	[Export] public int characterId;
	[Export] public string characterName;
	[Export] public string characterDescription;
	[Export] public Texture2D characterHeadImage;
    [Export] public float hp;
    [Export] public float maxHp;
    [Export] public float atk;
    public Vector2I coord;
	public CharacterBattleState characterBattleState;
    [Export] public int turnInitialActionTimes;
    public int currentRestActionTimes;
	public List<CommandExecuteInfo> commandQueue;

	public void CharacterInitialize()
	{
		commandQueue = new List<CommandExecuteInfo>();
		int targetQueueLength = Autoloads.gameQueueLength;
		for (int i = 0; i < targetQueueLength; i++)
		{
            commandQueue.Add(new CommandExecuteInfo());
		}
		hp = maxHp;
		currentRestActionTimes = turnInitialActionTimes;
	}
	public void SetCommand(int actionPointCost, CharacterHeadButtonControl characterHeadButtonControl, int ccmdQueueIdx, CommandExecuteInfo commandExecuteInfo)
	{
		currentRestActionTimes -= actionPointCost;
		characterHeadButtonControl.UpdateUIDisplay();
		characterHeadButtonControl.ChangeToActionOverDisplay();
		commandQueue[ccmdQueueIdx] = commandExecuteInfo;
		CommandItemUIControl.CurrentSelected.SetCommandToQueue(commandExecuteInfo.commandData);
	}
	public void MakeDamage(CharacterData targetCharacterData, float damageValue)
	{
		targetCharacterData.hp = Math.Max(0, targetCharacterData.hp - damageValue);
		Autoloads.sceneSingleton.enemyCharacterHeadListUIControl.UpdateAllUIDisplay();
	}
}

public enum CharacterBattleState
{
	ALIVE,
	DYING,
	DEAD
}

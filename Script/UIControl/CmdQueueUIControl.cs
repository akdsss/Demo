using Godot;
using System;
using System.Collections.Generic;

public partial class CmdQueueUIControl : Node
{
	// [Export] public Texture defaultCharacterHead;
	[Export] public string defaultCmdName;
	// [Export] public TextureRect[] allCharacterHeadArray;
	// [Export] public Label[] allCmdMatrix;
	[Export] public HBoxContainer CommandQueueMatrix;
	[Export] public PackedScene commandListPrefab;
	List<ActionListUIControl> actionListUIControlList = new();
	public List<List<CommandItemUIControl>> commandItemUIControlMatrix;
	public CmdQueueState cmdQueueState = CmdQueueState.NORMAL;
	public override void _Ready()
	{
		// 注册到场景单例
		Autoloads.sceneSingleton.cmdQueueUIControl = this;
		// ResetAll();

		// for (int i = 0; i < allCharacterHeadArray.Length; i++)
		// {
		//     allCharacterHeadArray[i].Texture = (Texture2D)defaultCharacterHead;
		// }
		// Initialize();

	}
	public void Initialize()
	{
		// 初始化UI节点
		PubTool.instance.ClearChildren(CommandQueueMatrix);
		for (int i = 0; i < Autoloads.sceneSingleton.gameQueueLength; i++)
		{
			Node commandItemList = commandListPrefab.Instantiate();
			actionListUIControlList.Add(commandItemList as ActionListUIControl);
			CommandQueueMatrix.AddChild(commandItemList);
		}

		// 初始化命令列表
		commandItemUIControlMatrix = new List<List<CommandItemUIControl>>();
		foreach (ActionListUIControl actionListUIControl in actionListUIControlList)
		{
			commandItemUIControlMatrix.Add(actionListUIControl.actionItemUIControlList);
		}

		// 初始化每个命令控制器
		ResetCmdQueueMatric();
	}
	public void UpdateCmdMatrix(){
		ResetCmdQueueMatric();
	}
	public void ResetCmdQueueMatric()
	{
		int j = 0;
		foreach (var commandItemUIControlList in commandItemUIControlMatrix)
		{
			foreach (var commandItemUIControl in commandItemUIControlList)
			{
				commandItemUIControl.commandItemState = CommandItemState.NORMAL;
				commandItemUIControl.label.Text = defaultCmdName;
				commandItemUIControl.cmdIdxInQueue = j;
			}
			j++;
		}
	}
	// public void ResetAll()
	// {
	//     for (int i = 0; i < allCharacterHeadArray.Length; i++)
	//     {
	//         allCharacterHeadArray[i].Texture = (Texture2D)defaultCharacterHead;
	//     }
	//     for (int i = 0; i < allCmdMatrix.Length; i++)
	//     {
	//         allCmdMatrix[i].Text = defaultCmdName;
	//     }
	// }
	// public void SetCharacterHead(LevelData levelData)
	// {
	//     int idx = 0;
	//     for (int i = 0; i < levelData.playerInfoInLevelArray.Length; i++)
	//     {
	//         PlayerInfoInLevel playerInfoInLevel = levelData.playerInfoInLevelArray[i];
	//         if (idx >= allCharacterHeadArray.Length)
	//         {
	//             GD.PrintErr("头像列表赋值错误，超出最大上限");
	//         }
	//         allCharacterHeadArray[idx].Texture = playerInfoInLevel.playerData.characterHeadImage;
	//         idx++;
	//     }
	//     for (int i = 0; i < levelData.enemyInfoInLevelArray.Length; i++)
	//     {
	//         EnemyInfoInLevel enemyInfoInLevel = levelData.enemyInfoInLevelArray[i];
	//         if (idx >= allCharacterHeadArray.Length)
	//         {
	//             GD.PrintErr("头像列表赋值错误，超出最大上限");
	//         }
	//         allCharacterHeadArray[idx].Texture = enemyInfoInLevel.enemyData.characterHeadImage;
	//         idx++;
	//     }
	// }
	public void SwitchOnPlayerCommandSet()
	{
		int playerIdx = Autoloads.sceneSingleton.battleManager.battlePlayerDataList.IndexOf(Autoloads.sceneSingleton.battleManager.eventManager.currentMainPlayer);
		if (playerIdx < 0)
		{
			GD.PrintErr("SwitchOnPlayerCommandSet: 未找到当前玩家索引");
			return;
		}
		foreach (var commandItemUIControlList in commandItemUIControlMatrix)
		{
			commandItemUIControlList[playerIdx].isOnset = true;
		}
		// foreach (var commandItemUIControlList in commandItemUIControlMatrix)
		// {
		//     if (commandItemUIControlList[playerIdx].commandItemState == CommandItemState.NORMAL)
		//     {
		//         commandItemUIControlList[playerIdx].commandItemState = CommandItemState.HIGHLIGHT;
		//     }
		// }
	}
	public void SwitchOffPlayerCommandSet()
	{
		foreach (var commandItemUIControlList in commandItemUIControlMatrix)
		{
			foreach (var commandItemUIControl in commandItemUIControlList)
			{
				commandItemUIControl.isOnset = false;
				if (commandItemUIControl.commandItemState == CommandItemState.HIGHLIGHT)
				{
					commandItemUIControl.commandItemState = CommandItemState.NORMAL;
				}
			}
		}
	}
}

public enum CmdQueueState
{
	NORMAL,
	CMDSET
}

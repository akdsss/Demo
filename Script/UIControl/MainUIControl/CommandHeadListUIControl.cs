using Godot;
using System;
using System.Collections.Generic;

public partial class CommandHeadListUIControl : VBoxContainer
{
	[Export] public PackedScene characterHeadPrefab;
	public List<TextureRect> allCharacterHeadList;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		// 注册到AutoLoads
		Autoloads.sceneSingleton.commandHeadListUIControl = this;

		allCharacterHeadList = new List<TextureRect>();
		PubTool.instance.ClearChildren(this);
		for (int i = 0; i < Autoloads.sceneSingleton.gameCharacterNum; i++)
		{
			Node characterHeadItem = characterHeadPrefab.Instantiate();
			AddChild(characterHeadItem);
			((TextureRect)characterHeadItem).Texture = Autoloads.sceneSingleton.defaultCharacterImage;
			allCharacterHeadList.Add((TextureRect)characterHeadItem);
		}
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
	public void SetCharacterHead(LevelData levelData)
	{
		int idx = 0;
		for (int i = 0; i < levelData.playerInfoInLevelArray.Length; i++)
		{
			PlayerInfoInLevel playerInfoInLevel = levelData.playerInfoInLevelArray[i];
			if (idx >= allCharacterHeadList.Count)
			{
				GD.PrintErr("头像列表赋值错误，超出最大上限");
				return;
			}
			allCharacterHeadList[idx].Texture = playerInfoInLevel.playerData.characterHeadImage;
			idx++;
		}
		for (int i = 0; i < levelData.enemyInfoInLevelArray.Length; i++)
		{
			EnemyInfoInLevel enemyInfoInLevel = levelData.enemyInfoInLevelArray[i];
			if (idx >= allCharacterHeadList.Count)
			{
				GD.PrintErr("头像列表赋值错误，超出最大上限");
				return;
			}
			allCharacterHeadList[idx].Texture = enemyInfoInLevel.enemyData.characterHeadImage;
			idx++;
		}
	}
}

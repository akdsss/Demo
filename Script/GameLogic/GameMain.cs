using Godot;
using System;

public partial class GameMain : Node
{
	[Export] LevelData levelData;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Autoloads.gd_ChessBoard.BindAreaAnchors(
			Autoloads.gd_ChessBoard.chessBoardUIControl.chessCellUIControlArray);

		// 关闭玩家选项面板
		Autoloads.sceneSingleton.playerActionChoseList.Visible = false;

		// 开始游戏
		StartGame();
    }

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public void StartGame()
	{ 
		PubTool.instance.gameMode = GameMode.Normal;
		levelData.LevelInitialize();
		Autoloads.sceneSingleton.battleManager.BattleStart(levelData);
	}
	public void ExitGame()
	{
		GD.Print("退出游戏");
		GetTree().Quit();
	}
}

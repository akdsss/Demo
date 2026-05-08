using Godot;
using System;

public partial class GameMain : Node
{
	public ChessBoard chessBoard
	{
		get
		{
			return GetTree().Root.GetNode<ChessBoard>("/root/gd_ChessBoard");
		}
	}
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		// 获取棋盘UI控制脚本引用
		chessBoard.chessBoardUIControl = GetNode<ChessBoardUIControl>("ChessBoard");

		// 为棋盘单元赋值UI控制脚本引用
		int ui_ctl_idx = 0;
		for (int i = 0; i < 3; i++)
		{
			for (int j = 0; j < (i == 1 ? 4 : 3); j++)
			{
				chessBoard.chessCellList[i][j].chessCellUIControl = chessBoard.chessBoardUIControl.chessCellUIControlArray[ui_ctl_idx];
			}
		}
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public void StartGame()
	{ 
	}
	public void ExitGame()
	{
		GD.Print("退出游戏");
		GetTree().Quit();
	}

	public void TestLevelLoad(LevelData levelData)
	{
		levelData.LevelInitialize();
	}
}

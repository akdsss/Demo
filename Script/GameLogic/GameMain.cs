using Godot;
using System;

public partial class GameMain : Node
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
        // 初始化棋盘UI引用
        int ui_ctl_idx = 0;
        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < (i == 1 ? 4 : 3); j++)
            {
                Autoloads.gd_ChessBoard.chessCellList[i][j].chessCellUIControl = Autoloads.gd_ChessBoard.chessBoardUIControl.chessCellUIControlArray[ui_ctl_idx];
                ui_ctl_idx++;
            }
        }

		// 关闭玩家选项面板
		Autoloads.sceneSingleton.playerActionChoseList.Visible = false;
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
}

using Godot;
using System;

[GlobalClass]
public partial class LevelData : Resource
{
	[Export] public int levelId;
	[Export] public string levelName;
	[Export] public LevelType levelType;
	[Export] public EnemyInfoInLevel[] enemyInfoInLevelArray;
	[Export] public PlayerInfoInLevel[] playerInfoInLevelArray;
	[Export] public TutorialStepData[] tutorialStepDataArray;
	[Export] public GrowthRewardData growthRewardData;

	public void LevelInitialize()
	{
		Autoloads.sceneSingleton.gameStateLable.Text = $"关卡{levelName}加载中";
		// 加载角色到棋盘
		ChessBoard chessBoard = Autoloads.gd_ChessBoard;
		chessBoard.ResetChessBoard();

        foreach (EnemyInfoInLevel enemyInfo in enemyInfoInLevelArray)
		{
			chessBoard.SetCharacterToChessCell(enemyInfo.enemyData, enemyInfo.coord);
		}
		foreach(PlayerInfoInLevel playerInfo in playerInfoInLevelArray)
		{
			chessBoard.SetCharacterToChessCell(playerInfo.playerData, playerInfo.coord);
		}

        Autoloads.sceneSingleton.gameStateLable.Text = "关卡已加载";
    }
}

public enum LevelType
{
	NORMAL,
	BOSS,
	TUTORIAL
}

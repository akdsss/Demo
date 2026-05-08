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

	public void LevelInitialize()
	{
		ChessBoard chessBoard = Autoloads.gd_ChessBoard;
		foreach (EnemyInfoInLevel enemyInfo in enemyInfoInLevelArray)
		{
			chessBoard.SetCharacterToChessCell(enemyInfo.enemyData, enemyInfo.coord);
		}
		foreach(PlayerInfoInLevel playerInfo in playerInfoInLevelArray)
		{
			chessBoard.SetCharacterToChessCell(playerInfo.playerData, playerInfo.coord);
		}
	}
}

public enum LevelType
{
	NORMAL,
	BOSS
}

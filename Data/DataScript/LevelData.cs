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
			CombatAreaId areaId = ResolveAreaId(enemyInfo.areaId, enemyInfo.coord);
			enemyInfo.enemyData.initialAreaId = areaId;
			chessBoard.SetCharacterToArea(enemyInfo.enemyData, areaId);
		}
		foreach(PlayerInfoInLevel playerInfo in playerInfoInLevelArray)
		{
			CombatAreaId areaId = ResolveAreaId(playerInfo.areaId, playerInfo.coord);
			playerInfo.playerData.initialAreaId = areaId;
			chessBoard.SetCharacterToArea(playerInfo.playerData, areaId);
		}

        Autoloads.sceneSingleton.gameStateLable.Text = "关卡已加载";
    }

	private static CombatAreaId ResolveAreaId(CombatAreaId configuredAreaId, Vector2I legacyCoord)
	{
		return configuredAreaId != CombatAreaId.Unknown
			? configuredAreaId
			: AreaDefinition.GetAreaIdForLegacyCoord(legacyCoord);
	}
}

public enum LevelType
{
	NORMAL,
	BOSS,
	TUTORIAL
}

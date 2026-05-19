using Godot;
using System.Collections.Generic;
using System.Threading.Tasks;

public partial class BattleManager : Node
{
	public BattleState battleState = BattleState.NOT_INITIALIZED;
	//private TurnManager turnManager;
	private List<CharacterData> battleCharacterDataList;
	public List<PlayerData> battlePlayerDataList;
	public List<EnemyData> battleEnemyDataList;

	// 事件管理
	public EventManager eventManager = new();

	[Signal] public delegate void BSEventHandler();
	[Signal] public delegate void MainTSEventHandler();
	[Signal] public delegate void PreTSEventHandler();
	[Signal] public delegate void PlayTSEventHandler();

	public async void BattleStart(LevelData levelData)
	{
		PubTool.instance.PrintToCmdAndTitle("战斗开始");

		BattleInitialize(levelData);
		battleState = BattleState.PLAYING;
		while (battleState != BattleState.OVER)
		{
			await TurnStart();
			if(PubTool.instance.gameMode == GameMode.Test)
			{
				PubTool.instance.PrintToCmdAndTitle("测试：仅进行一回合");
				battleState = BattleState.OVER;
			}
			else
			{
				await ToSignal(this, nameof(MainTS));
			}
		   
		}
		BattleEnd();
	}
	public void BattleInitialize(LevelData levelData)
	{
		PubTool.instance.PrintToCmdAndTitle("战斗初始化");
		battleState = BattleState.INITIALIZED;

		// 玩家、敌人列表初始化
		battlePlayerDataList = new();
		battleEnemyDataList = new();
		foreach (var playerInfo in levelData.playerInfoInLevelArray)
		{
			battlePlayerDataList.Add(playerInfo.playerData);
		}
		foreach (var enemyInfo in levelData.enemyInfoInLevelArray)
		{
			battleEnemyDataList.Add(enemyInfo.enemyData);
		}

		// 回合管理初始化
		battleCharacterDataList = new();

		battleCharacterDataList.AddRange(battlePlayerDataList);
		battleCharacterDataList.AddRange(battleEnemyDataList);

		// 角色初始化
		foreach (var characterData in battleCharacterDataList)
		{
			characterData.CharacterInitialize();
		}

		// 玩家头像列表UI初始化
		Autoloads.sceneSingleton.playerCharacterHeadListUIControl.Initialize();
		// 敌方头像列表UI初始化
		Autoloads.sceneSingleton.enemyCharacterHeadListUIControl.Initialize();
		// 命令队列头像初始化
		Autoloads.sceneSingleton.cmdQueueUIControl.SetCharacterHead(levelData);
	}
	public void BattleEnd()
	{
		PubTool.instance.PrintToCmdAndTitle("战斗结束");
		battleState = BattleState.END;
	}
	//private partial class TurnManager : Node
	//{

	//}
	int turnNum = 0;
	public MainTurnState mainTurnState = MainTurnState.NOT_INITIALIZED;
	public PrepareTurnState prepareTurnState;
	public PlayTurnState playTurnState;
	//public BattleManager bM;

	public async Task TurnStart()
	{
		TurnInitialize();
		await PrepareTurn();
		await PlayTurn();
		TurnEnd();
	}
	public void TurnInitialize()
	{
		turnNum++;
		PubTool.instance.PrintToCmdAndTitle("第" + turnNum + "轮战斗回合开始");
		mainTurnState = MainTurnState.INITIALIZED;

		// 重置角色行动次数
		foreach (var characterData in battleCharacterDataList)
		{
			characterData.currentRestActionTimes = characterData.turnInitialActionTimes;
		}
	}
	public void TurnEnd()
	{
		PubTool.instance.PrintToCmdAndTitle("回合结束");
		mainTurnState = MainTurnState.END;
	}
	public async Task PrepareTurn()
	{
		PubTool.instance.PrintToCmdAndTitle("准备阶段");
		PrepareTurnInitialize();
		//GD.Print("Has PrepareTurn signal: ", HasSignal("PreTS"));
		//foreach (var s in GetSignalList())
		//{
		//	GD.Print("Signal: ", s);
		//}
		while (prepareTurnState != PrepareTurnState.PRE_OVER)
		{
			// 敌人准备
			prepareTurnState = PrepareTurnState.ENEMY_PRE;
			while (prepareTurnState != PrepareTurnState.ENEMY_PRE_OVER)
			{
				PubTool.instance.PrintToCmdAndTitle("敌人准备中……");
				for (int i = 0; i < battleEnemyDataList.Count; i++)
				{
					EnemyData enemyData = battleEnemyDataList[i];
					CommandExecuteInfo commandExecuteInfo = new();
					commandExecuteInfo.sourceCharacterData = enemyData;
					commandExecuteInfo.commandData = enemyData.enemyCommandDataArray[0];
					enemyData.commandQueue[0] = commandExecuteInfo;

				}
				await ToSignal(this, nameof(PreTS));
			}
			// 玩家准备
			prepareTurnState = PrepareTurnState.PLAYER_PRE;
			while (prepareTurnState != PrepareTurnState.PLAYER_PRE_OVER)
			{
				PubTool.instance.PrintToCmdAndTitle("玩家准备中……");
				await ToSignal(this, nameof(PreTS));
			}
			if (PubTool.instance.gameMode == GameMode.Test)
			{
				PubTool.instance.PrintToCmdAndTitle("测试：仅准备一轮");
				prepareTurnState = PrepareTurnState.PRE_OVER;
			}
			else
			{
				await ToSignal(this, nameof(PreTS));
			}
		}
		PrepareTurnEnd();
	}
	public void PrepareTurnInitialize()
	{
		prepareTurnState = PrepareTurnState.BEFORE_PRE;
	}
	public void PrepareTurnEnd()
	{
		prepareTurnState = PrepareTurnState.AFTER_PRE;
	}
	public async Task PlayTurn()
	{
		PubTool.instance.PrintToCmdAndTitle("演出阶段");
		PlayTurnInitialize();
		while (playTurnState != PlayTurnState.PLAY_OVER)
		{
			// 敌人行动
			while (playTurnState != PlayTurnState.ENEMY_PLAY_OVER)
			{
				PubTool.instance.PrintToCmdAndTitle("敌人行动中……");
				if (PubTool.instance.gameMode == GameMode.Test)
				{
					PubTool.instance.PrintToCmdAndTitle("敌人自动行动");
					playTurnState = PlayTurnState.ENEMY_PLAY_OVER;
				}
				else
				{
					await ToSignal(this, nameof(PlayTS));
				}

			}
			// 玩家行动
			while (playTurnState != PlayTurnState.PLAYER_PLAY_OVER)
			{
				PubTool.instance.PrintToCmdAndTitle("玩家行动中……");
				if (PubTool.instance.gameMode == GameMode.Test)
				{
					PubTool.instance.PrintToCmdAndTitle("玩家自动行动");
					playTurnState = PlayTurnState.PLAYER_PLAY_OVER;
				}
				else
				{
					await ToSignal(this, nameof(PlayTS));
				}
			}
			if (PubTool.instance.gameMode == GameMode.Test)
			{
				PubTool.instance.PrintToCmdAndTitle("测试：仅行动一轮");
				playTurnState = PlayTurnState.PLAY_OVER;
			}
			else
			{
				await ToSignal(this, nameof(PlayTS));
			}
				
		}
		PlayTurnEnd();
	}
	public void PlayTurnInitialize()
	{
		playTurnState = PlayTurnState.BEFORE_PLAY;
	}
	public void PlayTurnEnd()
	{
		playTurnState = PlayTurnState.AFTER_PLAY;
	}

	public void SetManagerState(BattleState _battleState)
	{
		battleState = _battleState;
		PubTool.instance.PrintToCmdAndTitle($"已将battleState修改为{_battleState}");
		EmitSignal(nameof(BS));
	}
	public void SetManagerState(MainTurnState _mainTurnState)
	{
		mainTurnState = _mainTurnState;
		PubTool.instance.PrintToCmdAndTitle($"已将mainTurnState修改为{_mainTurnState}");
		EmitSignal(nameof(MainTS));
	}
	public void SetManagerState(PrepareTurnState _prepareTurnState)
	{
		prepareTurnState = _prepareTurnState;
		PubTool.instance.PrintToCmdAndTitle($"已将prepareTurnState修改为{_prepareTurnState}");
		EmitSignal(nameof(PreTS));
	}
	public void SetManagerState(PlayTurnState _playTurnState)
	{
		playTurnState = _playTurnState;
		PubTool.instance.PrintToCmdAndTitle($"已将playTurnState修改为{_playTurnState}");
		EmitSignal(nameof(PlayTS));
	}
}

public enum BattleState
{
	NOT_INITIALIZED,
	INITIALIZED,
	PLAYING,
	OVER,
	END
}

public enum PrepareTurnState
{
	BEFORE_PRE,
	ENEMY_PRE,
	ENEMY_PRE_OVER,
	PLAYER_PRE,
	PLAYER_PRE_OVER,
	PRE_OVER,
	AFTER_PRE,
}

public enum PlayTurnState
{
	BEFORE_PLAY,
	ENEMY_PLAY,
	ENEMY_PLAY_OVER,
	PLAYER_PLAY,
	PLAYER_PLAY_OVER,
	PLAY_OVER,
	AFTER_PLAY,
}

public enum MainTurnState
{
	NOT_INITIALIZED,
	INITIALIZED,
	PRE_TURN,
	PLY_TURN,
	OVER,
	END
}

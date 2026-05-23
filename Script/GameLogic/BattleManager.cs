using Godot;
using System.Collections.Generic;
using System.Threading.Tasks;

public partial class BattleManager : Node
{
	public BattleState battleState = BattleState.NOT_INITIALIZED;
	public GameResult gameResult;
	private List<CharacterData> battleCharacterDataList;
	public List<PlayerData> battlePlayerDataList;
	public List<EnemyData> battleEnemyDataList;
	private LevelData currentLevelData;
	private List<CombatEvent> pendingCombatEvents = new();
	private bool startSettlementRequested;
	private readonly EnemyActionPlanner enemyActionPlanner = new();

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
			if (PubTool.instance.gameMode != GameMode.Test)
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
		currentLevelData = levelData;

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

		battleCharacterDataList = new();

		battleCharacterDataList.AddRange(battlePlayerDataList);
		battleCharacterDataList.AddRange(battleEnemyDataList);
		Autoloads.sceneSingleton.gameCharacterNum = battleCharacterDataList.Count;

		foreach (var characterData in battleCharacterDataList)
		{
			characterData.CharacterInitialize();
		}

		Autoloads.sceneSingleton.enemyCharacterHeadListUIControl.Initialize();
		Autoloads.sceneSingleton.cmdQueueUIControl.Initialize();
		if (levelData.levelType == LevelType.TUTORIAL)
		{
			Autoloads.sceneSingleton.tutorialOverlayControl?.StartTutorial(levelData);
		}
	}
	public void BattleEnd()
	{
		string result = gameResult == GameResult.Win ? "玩家胜利" : "玩家失败";
		PubTool.instance.PrintToCmdAndTitle("战斗结束：" + result);
		battleState = BattleState.END;
	}
	int turnNum = 0;
	public MainTurnState mainTurnState = MainTurnState.NOT_INITIALIZED;
	public PrepareTurnState prepareTurnState;
	public PlayTurnState playTurnState;

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

		foreach (var characterData in battleCharacterDataList)
		{
			characterData.currentRestActionTimes = GetActionTimesForRound(characterData);
		}
	}
	public void TurnEnd()
	{
		PubTool.instance.PrintToCmdAndTitle("回合结束");
		mainTurnState = MainTurnState.END;
		CheckGameEnd();
	}
	public async Task PrepareTurn()
	{
		PubTool.instance.PrintToCmdAndTitle("准备阶段");
		PrepareTurnInitialize();
		while (prepareTurnState != PrepareTurnState.PRE_OVER)
		{
			bool enemyHasRemainingAction = false;
			foreach (var enemyData in battleEnemyDataList)
			{
				if (enemyData.currentRestActionTimes > 0)
				{
					enemyHasRemainingAction = true;
					break;
				}
			}
			if (enemyHasRemainingAction == false)
			{

			}
			else
			{
				prepareTurnState = PrepareTurnState.ENEMY_PRE;
				PubTool.instance.PrintToCmdAndTitle("敌人准备中……");
				Autoloads.sceneSingleton.enemyCharacterHeadListUIControl.ShowAllThinking();
				for (int i = 0; i < battleEnemyDataList.Count; i++)
				{
					EnemyData enemyData = battleEnemyDataList[i];
					if (enemyData.currentRestActionTimes <= 0)
					{
						continue;
					}

					EnemyActionPlan actionPlan = enemyActionPlanner.PlanNextAction(
						enemyData,
						battlePlayerDataList,
						battleEnemyDataList,
						turnNum);
					if (actionPlan == null || !actionPlan.IsValid)
					{
						GD.PrintErr($"敌人 {enemyData.characterName} 没有可生成的行动，跳过剩余行动。");
						enemyData.currentRestActionTimes = 0;
						continue;
					}

					int slotMatrixIndex = actionPlan.SlotIndex - 1;
					int enemyRowIndex = i + battlePlayerDataList.Count;
					if (slotMatrixIndex < 0 ||
						slotMatrixIndex >= Autoloads.sceneSingleton.cmdQueueUIControl.commandItemUIControlMatrix.Count ||
						enemyRowIndex >= Autoloads.sceneSingleton.cmdQueueUIControl.commandItemUIControlMatrix[slotMatrixIndex].Count ||
						i >= Autoloads.sceneSingleton.enemyCharacterHeadListUIControl.enemyHeadButtonList.Count)
					{
						GD.PrintErr($"敌人 {enemyData.characterName} 生成行动后找不到对应时间轴槽位。");
						enemyData.currentRestActionTimes = 0;
						continue;
					}

					CommandItemUIControl cmdItemUIControl = Autoloads.sceneSingleton.cmdQueueUIControl.commandItemUIControlMatrix[slotMatrixIndex][enemyRowIndex];
					CharacterHeadButtonControl enemyHead = Autoloads.sceneSingleton.enemyCharacterHeadListUIControl.enemyHeadButtonList[i];
					enemyData.SetCommand(1, enemyHead, slotMatrixIndex, actionPlan.CommandExecuteInfo, cmdItemUIControl);
				}
				Autoloads.sceneSingleton.enemyCharacterHeadListUIControl.UpdateEnemyPrepareDisplays();
			}

			PlayerPrepareInitialize();
			bool playerHasRemainingAction = false;
			foreach (var characterData in battlePlayerDataList)
			{
				if (characterData.currentRestActionTimes > 0)
				{
					playerHasRemainingAction = true;
					break;
				}
			}
			if (playerHasRemainingAction == false)
			{
				SetManagerState(PrepareTurnState.PLAYER_PRE_OVER);
			}
			else
			{
				SetManagerState(PrepareTurnState.PLAYER_PRE);
				while (prepareTurnState != PrepareTurnState.PLAYER_PRE_OVER)
				{
					await ToSignal(this, nameof(PreTS));
				}
			}

			enemyHasRemainingAction = false;
			foreach (var enemyData in battleEnemyDataList)
			{
				if (enemyData.currentRestActionTimes > 0)
				{
					enemyHasRemainingAction = true;
					break;
				}
			}
			playerHasRemainingAction = false;
			foreach (var characterData in battlePlayerDataList)
			{
				if (characterData.currentRestActionTimes > 0)
				{
					playerHasRemainingAction = true;
					break;
				}
			}
			if (enemyHasRemainingAction == false && playerHasRemainingAction == false)
			{
				SetManagerState(PrepareTurnState.PRE_OVER);
			}
		}
		ResolvePreparedActions();
		Autoloads.sceneSingleton.cmdQueueUIControl.RevealEnemyActions();
		await WaitForStartSettlement();
		PrepareTurnEnd();
	}
	public void PrepareTurnInitialize()
	{
		foreach (var characterData in battleCharacterDataList)
		{
			characterData.hasPrepared = false;
			characterData.currentRestActionTimes = GetActionTimesForRound(characterData);
			characterData.ResetCommandQueue();
		}
		Autoloads.sceneSingleton.cmdQueueUIControl.UpdateCmdMatrix();
		Autoloads.sceneSingleton.enemyCharacterHeadListUIControl.UpdateEnemyPrepareDisplays();
		Autoloads.sceneSingleton.cmdQueueUIControl?.SetPlayerActionBannerVisible(false);
		prepareTurnState = PrepareTurnState.BEFORE_PRE;
	}
	public void PrepareTurnEnd()
	{
		prepareTurnState = PrepareTurnState.AFTER_PRE;
		Autoloads.sceneSingleton.cmdQueueUIControl?.SetPlayerActionBannerVisible(false);
	}
	public void EnemyPrepareInitialize()
	{
		foreach (var enemyData in battleEnemyDataList)
		{
			enemyData.hasPrepared = false;
		}
	}
	public void PlayerPrepareInitialize()
	{
		foreach (var characterData in battlePlayerDataList)
		{
			characterData.hasPrepared = false;
		}
	}
	public async Task PlayTurn()
	{
		PubTool.instance.PrintToCmdAndTitle("演出阶段");
		PlayTurnInitialize();
		if (pendingCombatEvents == null || pendingCombatEvents.Count == 0)
		{
			ResolvePreparedActions();
		}
		foreach (CombatEvent combatEvent in pendingCombatEvents)
		{
			await PlayCombatEvent(combatEvent);
		}
		PlayTurnEnd();
	}
	private void ResolvePreparedActions()
	{
		List<PlannedAction> plannedActions = LegacyCommandAdapter.ToPlannedActions(battleCharacterDataList);
		CombatResolver resolver = new();
		pendingCombatEvents = resolver.ResolveRound(plannedActions, turnNum, battleCharacterDataList);
		PubTool.instance.PrintToCmdAndTitle($"后台结算完成：{pendingCombatEvents.Count} 个事件");
	}
	private async Task WaitForStartSettlement()
	{
		startSettlementRequested = false;
		Autoloads.sceneSingleton.cmdQueueUIControl.ShowStartSettlementButton(true);
		PubTool.instance.PrintToCmdAndTitle("怪物行动已揭示，请点击开始结算");
		while (!startSettlementRequested)
		{
			await ToSignal(this, nameof(PreTS));
		}
		Autoloads.sceneSingleton.cmdQueueUIControl.ShowStartSettlementButton(false);
	}
	public void RequestStartSettlement()
	{
		startSettlementRequested = true;
		EmitSignal(nameof(PreTS));
	}
	private async Task PlayCombatEvent(CombatEvent combatEvent)
	{
		string logText = CombatEventLogFormatter.Format(combatEvent);
		if (!string.IsNullOrEmpty(logText))
		{
			PubTool.instance.PrintToCmdAndTitle(logText);
			Autoloads.sceneSingleton.tutorialOverlayControl?.Notify(TutorialWaitCondition.CombatLogShown);
		}

		await PlayCombatEventPresentationPlaceholder(combatEvent);
		CombatEventApplier.ApplyToLegacyState(combatEvent);
		if (combatEvent?.EventType == CombatEventType.CharacterMoved)
		{
			Autoloads.sceneSingleton.tutorialOverlayControl?.Notify(TutorialWaitCondition.AreaChanged);
		}
		await Task.Delay(IsMetaEvent(combatEvent) ? 10 : 100);
	}
	private Task PlayCombatEventPresentationPlaceholder(CombatEvent combatEvent)
	{
		return Task.CompletedTask;
	}
	private static bool IsMetaEvent(CombatEvent combatEvent)
	{
		return combatEvent == null ||
			combatEvent.EventType == CombatEventType.RoundStarted ||
			combatEvent.EventType == CombatEventType.RoundEnded;
	}
	public void PlayTurnInitialize()
	{
		playTurnState = PlayTurnState.BEFORE_PLAY;
	}
	public void PlayTurnEnd()
	{
		Autoloads.sceneSingleton.cmdQueueUIControl.UpdateCmdMatrix();
		playTurnState = PlayTurnState.AFTER_PLAY;
	}
	public void CheckGameEnd()
	{
		bool allPlayersDead = true;
		foreach (var playerData in battlePlayerDataList)
		{
			if (playerData.characterBattleState == CharacterBattleState.ALIVE)
			{
				allPlayersDead = false;
				break;
			}
		}
		if (allPlayersDead)
		{
			battleState = BattleState.OVER;
			gameResult = GameResult.Lose;
		}
		else
		{
			bool allEnemiesDead = true;
			foreach (var enemyData in battleEnemyDataList)
			{
				if (enemyData.characterBattleState == CharacterBattleState.ALIVE)
				{
					allEnemiesDead = false;
					break;
				}
			}
			if (allEnemiesDead)
			{
				battleState = BattleState.OVER;
				gameResult = GameResult.Win;
				currentLevelData?.growthRewardData?.ApplyToCharacters(battlePlayerDataList);
				Autoloads.sceneSingleton.growthRewardOverlayControl?.ShowReward(
					currentLevelData?.growthRewardData,
					battlePlayerDataList);
				Autoloads.sceneSingleton.tutorialOverlayControl?.Notify(TutorialWaitCondition.VictoryReached);
			}
		}
	}
	public void CheckPlayerReadyOver()
	{
		foreach (var playerData in battlePlayerDataList)
		{
			if (playerData.currentRestActionTimes > 0 && playerData.characterBattleState == CharacterBattleState.ALIVE)
			{
				GD.Print($"玩家{playerData.characterName}仍有{playerData.currentRestActionTimes}次行动机会");
				return;
			}
		}
		GD.Print("所有玩家行动机会已用尽");
		SetManagerState(PrepareTurnState.PLAYER_PRE_OVER);
	}
	private int GetActionTimesForRound(CharacterData characterData)
	{
		return characterData is EnemyData enemyData
			? enemyData.GetActionTimesForRound(turnNum)
			: characterData.turnInitialActionTimes;
	}
	public void SetManagerState(BattleState _battleState)
	{
		battleState = _battleState;
		EmitSignal(nameof(BS));
	}
	public void SetManagerState(MainTurnState _mainTurnState)
	{
		mainTurnState = _mainTurnState;
		EmitSignal(nameof(MainTS));
	}
	public void SetManagerState(PrepareTurnState _prepareTurnState)
	{
		prepareTurnState = _prepareTurnState;
		Autoloads.sceneSingleton.cmdQueueUIControl?.RefreshTimelineUnitInfo();
		Autoloads.sceneSingleton.cmdQueueUIControl?.SetPlayerActionBannerVisible(_prepareTurnState == PrepareTurnState.PLAYER_PRE);
		EmitSignal(nameof(PreTS));
	}
	public void SetManagerState(PlayTurnState _playTurnState)
	{
		playTurnState = _playTurnState;
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

public enum GameResult
{
	Win,
	Lose
}

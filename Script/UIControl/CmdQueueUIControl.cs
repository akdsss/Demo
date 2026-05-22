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
	public bool IsInspectMode { get; private set; }
	private VBoxContainer timelineControlBar;
	private Button setCommandButton;
	private Button inspectButton;
	private Button startSettlementButton;
	private Label timelineDetailLabel;
	public override void _Ready()
	{
		// 注册到场景单例
		Autoloads.sceneSingleton.cmdQueueUIControl = this;
		EnsureTimelineControlBar();
		// ResetAll();

		// for (int i = 0; i < allCharacterHeadArray.Length; i++)
		// {
		//     allCharacterHeadArray[i].Texture = (Texture2D)defaultCharacterHead;
		// }
		// Initialize();

	}
	public void Initialize()
	{
		EnsureTimelineControlBar();
		// 初始化UI节点
		PubTool.instance.ClearChildren(CommandQueueMatrix);
		actionListUIControlList.Clear();
		for (int i = 0; i < Autoloads.sceneSingleton.gameQueueLength; i++)
		{
			Node commandItemList = commandListPrefab.Instantiate();
			ActionListUIControl actionListUIControl = commandItemList as ActionListUIControl;
			actionListUIControl.ExpectedItemCount = GetTimelineRowCount();
			actionListUIControlList.Add(actionListUIControl);
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
		SetControlMode(false);
		ShowStartSettlementButton(false);
	}
	public void UpdateCmdMatrix(){
		ResetCmdQueueMatric();
	}
	public void ResetCmdQueueMatric()
	{
		for (int slotIdx = 0; slotIdx < commandItemUIControlMatrix.Count; slotIdx++)
		{
			List<CommandItemUIControl> commandItemUIControlList = commandItemUIControlMatrix[slotIdx];
			for (int rowIdx = 0; rowIdx < commandItemUIControlList.Count; rowIdx++)
			{
				CommandItemUIControl commandItemUIControl = commandItemUIControlList[rowIdx];
				CharacterData owner = GetTimelineOwner(rowIdx);
				commandItemUIControl.ConfigureTimelineSlot(slotIdx, rowIdx, owner, defaultCmdName);
			}
		}
		ShowCommandDetail("时间轴", "我方与敌方共用六时点矩阵；蓝色为我方，红色为敌方。");
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
		SetControlMode(false);
		foreach (var commandItemUIControlList in commandItemUIControlMatrix)
		{
			if (playerIdx >= commandItemUIControlList.Count)
			{
				continue;
			}

			commandItemUIControlList[playerIdx].EnablePlacement();
		}
		ShowCommandDetail("设置指令", "选择空白时点并长按 2 秒确认。已设置指令不可覆盖。");
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

	public void RevealEnemyActions()
	{
		foreach (List<CommandItemUIControl> commandItemUIControlList in commandItemUIControlMatrix)
		{
			foreach (CommandItemUIControl commandItemUIControl in commandItemUIControlList)
			{
				commandItemUIControl.RevealEnemyAction();
			}
		}
		ShowCommandDetail("怪物行动揭示", "怪物行动已揭示，可检视优先级、描述和目标。");
		Autoloads.sceneSingleton.tutorialOverlayControl?.Notify(TutorialWaitCondition.EnemyActionsRevealed);
	}

	public void ShowStartSettlementButton(bool visible)
	{
		EnsureTimelineControlBar();
		startSettlementButton.Visible = visible;
	}

	public void CancelCurrentCommandSelection()
	{
		SwitchOffPlayerCommandSet();
		if (Autoloads.sceneSingleton == null)
		{
			return;
		}

		Autoloads.sceneSingleton.playerActionChoseList.Visible = false;
		Autoloads.sceneSingleton.enemyCharacterHeadListUIControl?.ChangeToUninteractable();
		Autoloads.sceneSingleton.playerCharacterHeadListUIControl?.ResetUIDisplay();
		if (Autoloads.sceneSingleton.battleManager?.eventManager != null)
		{
			Autoloads.sceneSingleton.battleManager.eventManager.currentMainPlayerCommand = null;
			Autoloads.sceneSingleton.battleManager.eventManager.moveEventInfo = null;
			Autoloads.sceneSingleton.battleManager.eventManager.damageEventInfo = null;
		}
		ShowCommandDetail("返回", "已取消当前指令选择。");
	}

	public void ShowSkillDetail(CommandData commandData, CharacterData source = null, bool revealed = true)
	{
		if (commandData == null)
		{
			ShowCommandDetail("技能详情", "暂无技能。");
			return;
		}

		SkillDefinition skill = SkillDefinition.FromCommandData(commandData);
		string title = revealed ? commandData.commandName : "未揭示怪物行动";
		string areaText = source == null
			? "未知"
			: AreaDefinition.FromLegacyCoord(source.coord).DisplayName;
		string detail = revealed
			? $"来源：{source?.characterName ?? "未知"}\n当前区域：{areaText}\n优先级：{skill.Priority}（区域修正后：{FormatAdjustedPriority(skill, source)}）\nMP：{skill.MpCost}\n目标：{FormatTargetType(skill.TargetType)}\n标签：{FormatSkillTags(skill.Tags)}\n效果：{commandData.commandDescription}"
			: $"来源：{source?.characterName ?? "怪物"}\n当前区域：{areaText}\n标签：{FormatSkillTags(skill.Tags)}\n怪物行动尚未揭示。";
		ShowCommandDetail(title, detail);
	}

	public void ShowCommandDetail(string title, string detail)
	{
		EnsureTimelineControlBar();
		timelineDetailLabel.Text = $"{title}\n{detail}";
	}

	private void EnsureTimelineControlBar()
	{
		if (timelineControlBar != null && timelineControlBar.GetParent() != null)
		{
			return;
		}

		timelineControlBar = GetNodeOrNull<VBoxContainer>("TimelineControlBar");
		if (timelineControlBar == null)
		{
			timelineControlBar = new VBoxContainer
			{
				Name = "TimelineControlBar",
				CustomMinimumSize = new Vector2(150, 0)
			};
			AddChild(timelineControlBar);
		}

		setCommandButton = CreateOrGetButton("SetCommandButton", "设置指令");
		inspectButton = CreateOrGetButton("InspectButton", "检视详情");
		startSettlementButton = CreateOrGetButton("StartSettlementButton", "开始结算");
		timelineDetailLabel = GetNodeOrNull<Label>("TimelineControlBar/DetailLabel");
		if (timelineDetailLabel == null)
		{
			timelineDetailLabel = new Label
			{
				Name = "DetailLabel",
				AutowrapMode = TextServer.AutowrapMode.WordSmart,
				CustomMinimumSize = new Vector2(150, 90)
			};
			timelineControlBar.AddChild(timelineDetailLabel);
		}

		setCommandButton.Pressed -= OnSetCommandPressed;
		inspectButton.Pressed -= OnInspectPressed;
		startSettlementButton.Pressed -= OnStartSettlementPressed;
		setCommandButton.Pressed += OnSetCommandPressed;
		inspectButton.Pressed += OnInspectPressed;
		startSettlementButton.Pressed += OnStartSettlementPressed;
		Autoloads.sceneSingleton?.uiSfxRouter?.RegisterButton(setCommandButton);
		Autoloads.sceneSingleton?.uiSfxRouter?.RegisterButton(inspectButton);
		Autoloads.sceneSingleton?.uiSfxRouter?.RegisterButton(startSettlementButton);
	}

	private Button CreateOrGetButton(string nodeName, string text)
	{
		Button button = GetNodeOrNull<Button>($"TimelineControlBar/{nodeName}");
		if (button != null)
		{
			return button;
		}

		button = new Button
		{
			Name = nodeName,
			Text = text,
			ToggleMode = nodeName != "StartSettlementButton"
		};
		timelineControlBar.AddChild(button);
		return button;
	}

	private void OnSetCommandPressed()
	{
		SetControlMode(false);
		ShowCommandDetail("设置指令", "选择角色、技能和目标后，长按空白时点 2 秒放置。");
	}

	private void OnInspectPressed()
	{
		SetControlMode(true);
		ShowCommandDetail("检视详情", "点击已放置指令查看详情；未揭示怪物行动只显示标签。");
	}

	private void OnStartSettlementPressed()
	{
		Autoloads.sceneSingleton.tutorialOverlayControl?.Notify(TutorialWaitCondition.StartSettlement);
		Autoloads.sceneSingleton.battleManager?.RequestStartSettlement();
		ShowStartSettlementButton(false);
	}

	public Control GetTutorialHighlightControl(TutorialHighlightTarget target)
	{
		return target switch
		{
			TutorialHighlightTarget.InspectButton => inspectButton,
			TutorialHighlightTarget.StartSettlementButton => startSettlementButton,
			TutorialHighlightTarget.BattleLog => timelineDetailLabel,
			_ => null
		};
	}

	private void SetControlMode(bool inspectMode)
	{
		IsInspectMode = inspectMode;
		if (setCommandButton != null)
		{
			setCommandButton.ButtonPressed = !inspectMode;
		}
		if (inspectButton != null)
		{
			inspectButton.ButtonPressed = inspectMode;
		}
	}

	private int GetTimelineRowCount()
	{
		int playerCount = Autoloads.sceneSingleton?.battleManager?.battlePlayerDataList?.Count ?? 0;
		int enemyCount = Autoloads.sceneSingleton?.battleManager?.battleEnemyDataList?.Count ?? 0;
		int count = playerCount + enemyCount;
		return count > 0 ? count : (Autoloads.sceneSingleton?.gameCharacterNum ?? 0);
	}

	private CharacterData GetTimelineOwner(int rowIdx)
	{
		if (Autoloads.sceneSingleton?.battleManager == null)
		{
			return null;
		}

		List<PlayerData> players = Autoloads.sceneSingleton.battleManager.battlePlayerDataList;
		List<EnemyData> enemies = Autoloads.sceneSingleton.battleManager.battleEnemyDataList;
		if (players != null && rowIdx < players.Count)
		{
			return players[rowIdx];
		}

		int enemyIdx = rowIdx - (players?.Count ?? 0);
		if (enemies != null && enemyIdx >= 0 && enemyIdx < enemies.Count)
		{
			return enemies[enemyIdx];
		}

		return null;
	}

	private static string FormatTargetType(SkillTargetType targetType)
	{
		return targetType switch
		{
			SkillTargetType.Self => "自身",
			SkillTargetType.Ally => "友方",
			SkillTargetType.Enemy => "敌方",
			SkillTargetType.Character => "角色",
			SkillTargetType.Area => "区域",
			SkillTargetType.TimelineSlot => "时点",
			_ => "无"
		};
	}

	private static string FormatSkillTags(SkillTag tags)
	{
		if (tags == SkillTag.None)
		{
			return "无";
		}

		List<string> names = new();
		if ((tags & SkillTag.Melee) != 0) names.Add("近战");
		if ((tags & SkillTag.Ranged) != 0) names.Add("远程");
		if ((tags & SkillTag.Move) != 0) names.Add("位移");
		if ((tags & SkillTag.Defense) != 0) names.Add("防御");
		if ((tags & SkillTag.Heal) != 0) names.Add("治疗");
		if ((tags & SkillTag.Area) != 0) names.Add("范围");
		if ((tags & SkillTag.SingleTarget) != 0) names.Add("单体");
		if ((tags & SkillTag.Special) != 0) names.Add("特殊");
		return string.Join(" / ", names);
	}

	private static int FormatAdjustedPriority(SkillDefinition skill, CharacterData source)
	{
		if (source == null || skill == null)
		{
			return skill?.Priority ?? 0;
		}

		return CombatAreaRules.GetAdjustedPriority(new PlannedAction
		{
			Source = CharacterState.FromCharacterData(source),
			Skill = skill
		});
	}
}

public enum CmdQueueState
{
	NORMAL,
	CMDSET
}

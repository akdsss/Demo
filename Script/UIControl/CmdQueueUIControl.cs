using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class CmdQueueUIControl : Node
{
	[Export] public string defaultCmdName;
	public List<List<CommandItemUIControl>> commandItemUIControlMatrix;
	public CmdQueueState cmdQueueState = CmdQueueState.NORMAL;
	public bool IsInspectMode { get; private set; }
	private const float TimelineWidth = 420f;
	private const float TimelineSlotHeight = 22f;
	private const float TimelineInfoWidth = 210f;
	private const float TimelineTailWidth = 78f;
	private const float TimelineRowSeparation = 2f;
	private const float TimelineTrackSeparation = 1f;
	private const float TimelinePanelVerticalPadding = 3f;
	private const int TimelineHostRowSeparation = 1;
	private const float PlayerBannerHeight = 22f;
	private const float TopPanelHeight = 92f;
	private const float DownPanelHeight = 104f;
	private PackedScene commandItemPrefab;
	private VBoxContainer enemyTimelineHost;
	private VBoxContainer playerTimelineHost;
	private Control playerActionBanner;
	private VBoxContainer timelineControlBar;
	private Button setCommandButton;
	private Button inspectButton;
	private Button startSettlementButton;
	private PanelContainer battleInfoPanel;
	private Label battleInfoLabel;
	private ConfirmationDialog startSettlementDialog;
	private readonly Dictionary<CharacterData, TimelineUnitInfo> timelineUnitInfoMap = new();
	private bool timelineControlSignalsConnected;

	private sealed class TimelineUnitInfo
	{
		public Label HpLabel;
		public Label MpLabel;
		public Label ActionLabel;
	}

	private static float GetTimelineTotalWidth()
	{
		return TimelineInfoWidth + TimelineRowSeparation + TimelineWidth + TimelineRowSeparation + TimelineTailWidth;
	}

	private static float GetTimelineSlotWidth(int slotCount)
	{
		if (slotCount <= 0)
		{
			return TimelineWidth;
		}

		return (TimelineWidth - ((slotCount - 1) * TimelineTrackSeparation)) / slotCount;
	}

	public override void _Ready()
	{
		if (Autoloads.sceneSingleton != null)
		{
			Autoloads.sceneSingleton.cmdQueueUIControl = this;
		}
		EnsureTimelineControlBar();
	}
	public void Initialize()
	{
		EnsureGddTimelineLayout();
		EnsureTimelineControlBar();
		EnsureLeftInteractionHost();
		BuildSeparatedTimelines();
		ResetCmdQueueMatric();
		SetControlMode(false);
		ShowStartSettlementButton(false);
		SetPrepareActionControlsVisible(false);
		RefreshTimelineUnitInfo();
	}
	public void UpdateCmdMatrix(){
		BuildSeparatedTimelines();
		ResetCmdQueueMatric();
	}
	public void ResetCmdQueueMatric()
	{
		if (commandItemUIControlMatrix == null)
		{
			return;
		}

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
		RefreshTimelineUnitInfo();
		ShowCommandDetail("时间轴", "敌方时间轴在上方，我方时间轴在下方；左侧设置指令后长按空白时点放置。");
	}

	private void EnsureGddTimelineLayout()
	{
		commandItemPrefab ??= ResourceLoader.Load<PackedScene>("res://Scene/PrefabScene/CommandItem.tscn");
		Control topPanel = GetTree()?.CurrentScene?.GetNodeOrNull<Control>("MainUi/Panel/VBoxContainer/TopPanel");
		if (topPanel != null)
		{
			topPanel.CustomMinimumSize = new Vector2(0, TopPanelHeight);
			topPanel.SizeFlagsVertical = Control.SizeFlags.Fill;
			Label statusLabel = topPanel.GetNodeOrNull<Label>("Label");
			if (statusLabel != null)
			{
				statusLabel.Text = string.Empty;
				statusLabel.Visible = false;
			}

			enemyTimelineHost = topPanel.GetNodeOrNull<VBoxContainer>("EnemyTimelineHost");
			if (enemyTimelineHost == null)
			{
				enemyTimelineHost = new VBoxContainer
				{
					Name = "EnemyTimelineHost",
					CustomMinimumSize = new Vector2(GetTimelineTotalWidth(), 0)
				};
				enemyTimelineHost.AddThemeConstantOverride("separation", 5);
				topPanel.AddChild(enemyTimelineHost);
			}
			enemyTimelineHost.CustomMinimumSize = new Vector2(GetTimelineTotalWidth(), 0);
			enemyTimelineHost.AddThemeConstantOverride("separation", TimelineHostRowSeparation);
			SetCenteredTimelineHost(enemyTimelineHost, TimelinePanelVerticalPadding, -TimelinePanelVerticalPadding);
		}

		Control downPanel = GetTree()?.CurrentScene?.GetNodeOrNull<Control>("MainUi/Panel/VBoxContainer/DownPanel");
		if (downPanel != null)
		{
			downPanel.CustomMinimumSize = new Vector2(0, DownPanelHeight);
			downPanel.SizeFlagsVertical = Control.SizeFlags.Fill;
			playerActionBanner = downPanel.GetNodeOrNull<Control>("PlayerActionBanner");
			if (playerActionBanner == null)
			{
				playerActionBanner = new ColorRect
				{
					Name = "PlayerActionBanner",
					Color = new Color(0.62f, 0.86f, 0.58f, 1f),
					Visible = false
				};
				Label bannerLabel = new()
				{
					Name = "Label",
					Text = "我方行动",
					HorizontalAlignment = HorizontalAlignment.Center,
					VerticalAlignment = VerticalAlignment.Center
				};
				playerActionBanner.AddChild(bannerLabel);
				downPanel.AddChild(playerActionBanner);
			}
			SetAnchoredOffsets(playerActionBanner, 0f, 4f, 0f, 4f + PlayerBannerHeight);
			SetAnchoredRect(playerActionBanner.GetNodeOrNull<Control>("Label"), 0f, 0f, 1f, 1f);

			playerTimelineHost = downPanel.GetNodeOrNull<VBoxContainer>("PlayerTimelineHost");
			if (playerTimelineHost == null)
			{
				playerTimelineHost = new VBoxContainer
				{
					Name = "PlayerTimelineHost",
					CustomMinimumSize = new Vector2(GetTimelineTotalWidth(), 0)
				};
				playerTimelineHost.AddThemeConstantOverride("separation", 5);
				downPanel.AddChild(playerTimelineHost);
			}
			playerTimelineHost.CustomMinimumSize = new Vector2(GetTimelineTotalWidth(), 0);
			playerTimelineHost.AddThemeConstantOverride("separation", TimelineHostRowSeparation);
			SetCenteredTimelineHost(playerTimelineHost, PlayerBannerHeight + 8f, -TimelinePanelVerticalPadding);
		}

		EnsureLeftInteractionHost();
		EnsureRightInfoPanel();
	}

	private void EnsureLeftInteractionHost()
	{
		Control leftRegion = GetTree()?.CurrentScene?.GetNodeOrNull<Control>("MainUi/Panel/VBoxContainer/MidPanel/HBoxContainer/LeftRegion");
		if (leftRegion == null)
		{
			return;
		}

		Control enemyHeadContainer = GetTree()?.CurrentScene?.GetNodeOrNull<Control>("MainUi/Panel/VBoxContainer/MidPanel/HBoxContainer/RightRegion/EnemyCHContent");
		if (enemyHeadContainer != null &&
			Autoloads.sceneSingleton?.battleManager?.eventManager?.damageEventInfo == null)
		{
			enemyHeadContainer.Visible = false;
		}

		VBoxContainer interactionHost = leftRegion.GetNodeOrNull<VBoxContainer>("BattleInteractionPanel");
		if (interactionHost == null)
		{
			interactionHost = new VBoxContainer
			{
				Name = "BattleInteractionPanel"
			};
			interactionHost.AddThemeConstantOverride("separation", 8);
			leftRegion.AddChild(interactionHost);
		}
		SetAnchoredRect(interactionHost, 0.04f, 0.05f, 0.94f, 0.94f);

		if (timelineControlBar != null && timelineControlBar.GetParent() != interactionHost)
		{
			timelineControlBar.GetParent()?.RemoveChild(timelineControlBar);
			interactionHost.AddChild(timelineControlBar);
		}

		Control choicePanel = Autoloads.sceneSingleton?.playerActionChoseList as Control;
		if (choicePanel != null)
		{
			if (choicePanel.GetParent() != interactionHost)
			{
				choicePanel.GetParent()?.RemoveChild(choicePanel);
				interactionHost.AddChild(choicePanel);
			}
			choicePanel.CustomMinimumSize = new Vector2(0, 150);
			choicePanel.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
			choicePanel.SizeFlagsVertical = Control.SizeFlags.Fill;
			choicePanel.Visible = false;
		}
	}

	private void EnsureRightInfoPanel()
	{
		Control rightRegion = GetTree()?.CurrentScene?.GetNodeOrNull<Control>("MainUi/Panel/VBoxContainer/MidPanel/HBoxContainer/RightRegion");
		if (rightRegion == null)
		{
			return;
		}

		battleInfoPanel = rightRegion.GetNodeOrNull<PanelContainer>("BattleInfoPanel");
		if (battleInfoPanel == null)
		{
			battleInfoPanel = new PanelContainer
			{
				Name = "BattleInfoPanel"
			};
			rightRegion.AddChild(battleInfoPanel);
		}
		SetAnchoredRect(battleInfoPanel, 0.06f, 0.08f, 0.94f, 0.92f);

		battleInfoLabel = battleInfoPanel.GetNodeOrNull<Label>("BattleInfoLabel");
		if (battleInfoLabel == null)
		{
			battleInfoLabel = new Label
			{
				Name = "BattleInfoLabel",
				AutowrapMode = TextServer.AutowrapMode.WordSmart
			};
			battleInfoPanel.AddChild(battleInfoLabel);
		}
		battleInfoLabel.AddThemeFontSizeOverride("font_size", 14);
		battleInfoLabel.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
		battleInfoLabel.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
	}

	private void BuildSeparatedTimelines()
	{
		EnsureGddTimelineLayout();
		if (commandItemPrefab == null || enemyTimelineHost == null || playerTimelineHost == null)
		{
			return;
		}

		timelineUnitInfoMap.Clear();
		commandItemUIControlMatrix = new List<List<CommandItemUIControl>>();
		for (int slotIndex = 0; slotIndex < Autoloads.sceneSingleton.gameQueueLength; slotIndex++)
		{
			commandItemUIControlMatrix.Add(new List<CommandItemUIControl>());
		}

		PubTool.instance.ClearChildren(enemyTimelineHost);
		PubTool.instance.ClearChildren(playerTimelineHost);

		List<PlayerData> players = Autoloads.sceneSingleton?.battleManager?.battlePlayerDataList ?? new List<PlayerData>();
		List<EnemyData> enemies = Autoloads.sceneSingleton?.battleManager?.battleEnemyDataList ?? new List<EnemyData>();
		BuildTimelineRows(playerTimelineHost, players.Cast<CharacterData>().ToList(), 0, false);
		BuildTimelineRows(enemyTimelineHost, enemies.Cast<CharacterData>().ToList(), players.Count, true);
		RefreshTimelineUnitInfo();
	}

	private void BuildTimelineRows(VBoxContainer host, List<CharacterData> characters, int rowStartIndex, bool isEnemy)
	{
		host.AddThemeConstantOverride("separation", TimelineHostRowSeparation);
		for (int characterIndex = 0; characterIndex < characters.Count; characterIndex++)
		{
			CharacterData characterData = characters[characterIndex];
			int rowIndex = rowStartIndex + characterIndex;
			HBoxContainer row = new()
			{
				CustomMinimumSize = new Vector2(0, TimelineSlotHeight),
				SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
				SizeFlagsVertical = Control.SizeFlags.ShrinkCenter
			};
			row.AddThemeConstantOverride("separation", (int)TimelineRowSeparation);
			host.AddChild(row);

			row.AddChild(CreateTimelineUnitInfo(characterData, isEnemy));
			HBoxContainer slotRow = new()
			{
				CustomMinimumSize = new Vector2(TimelineWidth, TimelineSlotHeight),
				SizeFlagsHorizontal = Control.SizeFlags.ShrinkCenter,
				SizeFlagsVertical = Control.SizeFlags.ShrinkCenter
			};
			slotRow.AddThemeConstantOverride("separation", (int)TimelineTrackSeparation);
			row.AddChild(slotRow);

			for (int slotIndex = 0; slotIndex < Autoloads.sceneSingleton.gameQueueLength; slotIndex++)
			{
				CommandItemUIControl item = commandItemPrefab.Instantiate<CommandItemUIControl>();
				item.CustomMinimumSize = new Vector2(GetTimelineSlotWidth(Autoloads.sceneSingleton.gameQueueLength), TimelineSlotHeight);
				item.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
				item.SizeFlagsVertical = Control.SizeFlags.ShrinkCenter;
				slotRow.AddChild(item);
				commandItemUIControlMatrix[slotIndex].Add(item);
			}

			if (isEnemy)
			{
				Label actionLabel = new()
				{
					CustomMinimumSize = new Vector2(TimelineTailWidth, TimelineSlotHeight),
					HorizontalAlignment = HorizontalAlignment.Center,
					VerticalAlignment = VerticalAlignment.Center
				};
				row.AddChild(actionLabel);
				if (timelineUnitInfoMap.TryGetValue(characterData, out TimelineUnitInfo info))
				{
					info.ActionLabel = actionLabel;
				}
			}
			else
			{
				row.AddChild(new Control
				{
					CustomMinimumSize = new Vector2(TimelineTailWidth, TimelineSlotHeight)
				});
			}
		}
	}

	private Control CreateTimelineUnitInfo(CharacterData characterData, bool isEnemy)
	{
		HBoxContainer infoRoot = new()
		{
			CustomMinimumSize = new Vector2(TimelineInfoWidth, TimelineSlotHeight),
			SizeFlagsHorizontal = Control.SizeFlags.Fill,
			SizeFlagsVertical = Control.SizeFlags.ShrinkCenter
		};
		infoRoot.AddThemeConstantOverride("separation", (int)TimelineRowSeparation);

		TextureRect head = new()
		{
			Texture = characterData?.characterHeadImage ?? Autoloads.sceneSingleton.defaultCharacterImage,
			CustomMinimumSize = new Vector2(16, 16),
			SizeFlagsHorizontal = Control.SizeFlags.ShrinkCenter,
			SizeFlagsVertical = Control.SizeFlags.ShrinkCenter,
			ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize,
			StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered
		};
		infoRoot.AddChild(head);

		HBoxContainer textColumn = new()
		{
			SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
			SizeFlagsVertical = Control.SizeFlags.ShrinkCenter
		};
		textColumn.AddThemeConstantOverride("separation", (int)TimelineRowSeparation);
		infoRoot.AddChild(textColumn);

		Label nameLabel = new()
		{
			Text = characterData?.characterName ?? "未知",
			AutowrapMode = TextServer.AutowrapMode.Off
		};
		nameLabel.AddThemeFontSizeOverride("font_size", 8);
		nameLabel.HorizontalAlignment = HorizontalAlignment.Left;
		nameLabel.VerticalAlignment = VerticalAlignment.Center;
		nameLabel.ClipText = true;
		nameLabel.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
		textColumn.AddChild(nameLabel);

		Label hpLabel = new()
		{
			AutowrapMode = TextServer.AutowrapMode.Off
		};
		hpLabel.AddThemeFontSizeOverride("font_size", 8);
		hpLabel.CustomMinimumSize = new Vector2(characterData is PlayerData ? 58 : 70, TimelineSlotHeight);
		hpLabel.HorizontalAlignment = HorizontalAlignment.Left;
		hpLabel.VerticalAlignment = VerticalAlignment.Center;
		hpLabel.ClipText = true;
		textColumn.AddChild(hpLabel);

		Label mpLabel = null;
		if (characterData is PlayerData)
		{
			mpLabel = new Label
			{
				CustomMinimumSize = new Vector2(54, TimelineSlotHeight),
				HorizontalAlignment = HorizontalAlignment.Left,
				VerticalAlignment = VerticalAlignment.Center,
				AutowrapMode = TextServer.AutowrapMode.Off,
				ClipText = true
			};
			mpLabel.AddThemeFontSizeOverride("font_size", 8);
			textColumn.AddChild(mpLabel);
		}
		textColumn.MoveChild(nameLabel, textColumn.GetChildCount() - 1);

		timelineUnitInfoMap[characterData] = new TimelineUnitInfo
		{
			HpLabel = hpLabel,
			MpLabel = mpLabel
		};
		return infoRoot;
	}

	public void RefreshTimelineUnitInfo()
	{
		foreach (KeyValuePair<CharacterData, TimelineUnitInfo> entry in timelineUnitInfoMap)
		{
			CharacterData characterData = entry.Key;
			TimelineUnitInfo info = entry.Value;
			if (characterData == null || info == null)
			{
				continue;
			}

			if (info.HpLabel != null)
			{
				string hpText = $"HP {Mathf.RoundToInt(characterData.hp)}/{Mathf.RoundToInt(characterData.maxHp)}";
				info.HpLabel.Text = hpText;
			}
			if (info.MpLabel != null)
			{
				info.MpLabel.Text = $"MP {characterData.mp}/{characterData.maxMp}";
			}
			if (info.ActionLabel != null)
			{
				info.ActionLabel.Text = characterData.currentRestActionTimes <= 0
					? "完成"
					: $"剩余{characterData.currentRestActionTimes}次";
			}
		}
	}

	private static void SetCenteredTimelineHost(Control control, float topOffset, float bottomOffset)
	{
		if (control == null)
		{
			return;
		}

		float halfWidth = GetTimelineTotalWidth() / 2f;
		control.AnchorLeft = 0.5f;
		control.AnchorRight = 0.5f;
		control.AnchorTop = 0f;
		control.AnchorBottom = 1f;
		control.OffsetLeft = -halfWidth;
		control.OffsetRight = halfWidth;
		control.OffsetTop = topOffset;
		control.OffsetBottom = bottomOffset;
	}

	private static void SetAnchoredOffsets(Control control, float left, float top, float right, float bottom)
	{
		if (control == null)
		{
			return;
		}

		control.AnchorLeft = 0f;
		control.AnchorTop = 0f;
		control.AnchorRight = 1f;
		control.AnchorBottom = 0f;
		control.OffsetLeft = left;
		control.OffsetTop = top;
		control.OffsetRight = -right;
		control.OffsetBottom = bottom;
	}

	private static void SetAnchoredRect(Control control, float left, float top, float right, float bottom)
	{
		if (control == null)
		{
			return;
		}

		control.AnchorLeft = left;
		control.AnchorTop = top;
		control.AnchorRight = right;
		control.AnchorBottom = bottom;
		control.OffsetLeft = 0;
		control.OffsetTop = 0;
		control.OffsetRight = 0;
		control.OffsetBottom = 0;
	}
	public void SwitchOnPlayerCommandSet()
	{
		BattleManager battleManager = Autoloads.sceneSingleton?.battleManager;
		PlayerData currentPlayer = battleManager?.eventManager?.currentMainPlayer;
		if (currentPlayer == null || currentPlayer.characterBattleState != CharacterBattleState.ALIVE || currentPlayer.currentRestActionTimes <= 0)
		{
			ResetCommandSelectionContext("无法设置指令", "该角色已无行动次数。", true);
			return;
		}

		if (commandItemUIControlMatrix == null)
		{
			return;
		}

		int playerIdx = battleManager.battlePlayerDataList.IndexOf(currentPlayer);
		if (playerIdx < 0)
		{
			GD.PrintErr("SwitchOnPlayerCommandSet: 未找到当前玩家索引");
			ResetCommandSelectionContext("无法设置指令", "当前角色不在我方行动列表中。", true);
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
		ShowCommandDetail("设置指令", "选择空白时点并长按 1.2 秒确认。已设置指令不可覆盖。");
	}
	public void SwitchOffPlayerCommandSet()
	{
		if (commandItemUIControlMatrix == null)
		{
			return;
		}

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
		if (visible)
		{
			setCommandButton.Visible = false;
			inspectButton.Visible = false;
			SwitchOffPlayerCommandSet();
			if (Autoloads.sceneSingleton?.playerActionChoseList != null)
			{
				Autoloads.sceneSingleton.playerActionChoseList.Visible = false;
			}
			Autoloads.sceneSingleton?.areaTargetMenuControl?.Dismiss();
			Autoloads.sceneSingleton?.enemyCharacterHeadListUIControl?.ChangeToUninteractable();
			ClearControlMode();
			return;
		}

		RefreshPrepareControlVisibility();
	}

	public void SetPlayerActionBannerVisible(bool visible)
	{
		EnsureGddTimelineLayout();
		if (playerActionBanner != null)
		{
			playerActionBanner.Visible = visible;
		}
		SetPrepareActionControlsVisible(visible);
	}

	private void SetPrepareActionControlsVisible(bool visible)
	{
		EnsureTimelineControlBar();
		if (!visible)
		{
			setCommandButton.Visible = false;
			inspectButton.Visible = false;
			if (Autoloads.sceneSingleton?.playerActionChoseList != null)
			{
				Autoloads.sceneSingleton.playerActionChoseList.Visible = false;
			}
			return;
		}

		RefreshPrepareControlVisibility();
	}

	public void CancelCurrentCommandSelection()
	{
		ResetCommandSelectionContext("返回", "已取消当前指令选择。");
	}

	public void ResetCommandSelectionContext(string title = null, string detail = null, bool clearCurrentPlayer = false)
	{
		SwitchOffPlayerCommandSet();
		SceneSingleton sceneSingleton = Autoloads.sceneSingleton;
		if (sceneSingleton == null)
		{
			return;
		}

		if (sceneSingleton.playerActionChoseList != null)
		{
			sceneSingleton.playerActionChoseList.Visible = false;
		}
		sceneSingleton.areaTargetMenuControl?.Dismiss();
		sceneSingleton.enemyCharacterHeadListUIControl?.ChangeToUninteractable();
		ClearPendingCommandRequest(clearCurrentPlayer);

		ClearControlMode();
		RefreshPrepareControlVisibility();
		if (!string.IsNullOrEmpty(title) || !string.IsNullOrEmpty(detail))
		{
			ShowCommandDetail(title ?? string.Empty, detail ?? string.Empty);
		}
	}

	public bool HasAnyPlayerActionRemaining()
	{
		return Autoloads.sceneSingleton?.battleManager?.battlePlayerDataList?.Any(player =>
			player != null &&
			player.characterBattleState == CharacterBattleState.ALIVE &&
			player.currentRestActionTimes > 0) == true;
	}

	public void RefreshPrepareControlVisibility()
	{
		EnsureTimelineControlBar();
		if (startSettlementButton != null && startSettlementButton.Visible)
		{
			setCommandButton.Visible = false;
			inspectButton.Visible = false;
			return;
		}

		bool isPlayerPrepare = Autoloads.sceneSingleton?.battleManager?.prepareTurnState == PrepareTurnState.PLAYER_PRE;
		bool hasPlayerAction = isPlayerPrepare && HasAnyPlayerActionRemaining();
		setCommandButton.Visible = hasPlayerAction;
		inspectButton.Visible = isPlayerPrepare;
		if (!hasPlayerAction && Autoloads.sceneSingleton?.playerActionChoseList != null)
		{
			Autoloads.sceneSingleton.playerActionChoseList.Visible = false;
		}
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
			: AreaDefinition.FormatAreaId(source.ResolveCurrentAreaId());
		string detail = revealed
			? $"来源：{source?.characterName ?? "未知"}\n当前区域：{areaText}\n优先级：{skill.Priority}（区域修正后：{FormatAdjustedPriority(skill, source)}）\nMP：{skill.MpCost}\n目标：{FormatTargetType(skill.TargetType)}\n标签：{FormatSkillTags(skill.Tags)}\n效果：{commandData.commandDescription}"
			: $"来源：{source?.characterName ?? "怪物"}\n当前区域：{areaText}\n标签：{FormatSkillTags(skill.Tags)}\n怪物行动尚未揭示。";
		ShowCommandDetail(title, detail);
	}

	public void ShowCommandDetail(string title, string detail)
	{
		EnsureRightInfoPanel();
		if (battleInfoLabel != null)
		{
			battleInfoLabel.Text = $"{title}\n{detail}";
		}
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
		Label legacyDetailLabel = timelineControlBar.GetNodeOrNull<Label>("DetailLabel");
		if (legacyDetailLabel != null)
		{
			timelineControlBar.RemoveChild(legacyDetailLabel);
			legacyDetailLabel.QueueFree();
		}

		if (!timelineControlSignalsConnected)
		{
			setCommandButton.Pressed += OnSetCommandPressed;
			inspectButton.Pressed += OnInspectPressed;
			startSettlementButton.Pressed += OnStartSettlementPressed;
			timelineControlSignalsConnected = true;
		}
	}

	private Button CreateOrGetButton(string nodeName, string text)
	{
		Button button = timelineControlBar?.GetNodeOrNull<Button>(nodeName);
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
		if (!HasAnyPlayerActionRemaining())
		{
			ResetCommandSelectionContext("设置指令", "当前没有可行动的我方角色。", true);
			return;
		}

		PlayerData playerData = ResolveCurrentPlayerForCommand();
		if (playerData == null)
		{
			ResetCommandSelectionContext("设置指令", "当前没有可行动的我方角色。", true);
			return;
		}

		ClearPendingCommandRequest(false);
		Autoloads.sceneSingleton.battleManager.eventManager.currentMainPlayer = playerData;
		Autoloads.sceneSingleton.playerActionChoseList.Visible = true;
		(Autoloads.sceneSingleton.playerActionChoseList as PlayerChoseListPanelControl)?.ShowCommandCategoryPanel(playerData);
		ShowCommandDetail("设置指令", $"当前角色：{playerData.characterName}\n先选择技能分类，再选择技能和目标。");
	}

	private void OnInspectPressed()
	{
		SetControlMode(true);
		ShowCommandDetail("检视详情", "点击已放置指令查看详情；未揭示怪物行动只显示标签。");
	}

	private void OnStartSettlementPressed()
	{
		EnsureStartSettlementDialog();
		startSettlementDialog.PopupCentered();
	}

	private void EnsureStartSettlementDialog()
	{
		if (startSettlementDialog != null && startSettlementDialog.GetParent() != null)
		{
			return;
		}

		startSettlementDialog = new ConfirmationDialog
		{
			Name = "StartSettlementDialog",
			Title = "开始结算",
			DialogText = "是否进入演出阶段？"
		};
		AddChild(startSettlementDialog);
		startSettlementDialog.Confirmed += ConfirmStartSettlement;
	}

	private void ConfirmStartSettlement()
	{
		Autoloads.sceneSingleton.tutorialOverlayControl?.Notify(TutorialWaitCondition.StartSettlement);
		Autoloads.sceneSingleton.battleManager?.RequestStartSettlement();
		ShowStartSettlementButton(false);
	}

	public Control GetTutorialHighlightControl(TutorialHighlightTarget target)
	{
		return target switch
		{
			TutorialHighlightTarget.PlayerTimeline => playerTimelineHost,
			TutorialHighlightTarget.EnemyTimeline => enemyTimelineHost,
			TutorialHighlightTarget.TimelineSlot => playerTimelineHost,
			TutorialHighlightTarget.InspectButton => inspectButton,
			TutorialHighlightTarget.StartSettlementButton => startSettlementButton,
			TutorialHighlightTarget.BattleLog => battleInfoLabel,
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

	private void ClearControlMode()
	{
		IsInspectMode = false;
		if (setCommandButton != null)
		{
			setCommandButton.ButtonPressed = false;
		}
		if (inspectButton != null)
		{
			inspectButton.ButtonPressed = false;
		}
	}

	private void ClearPendingCommandRequest(bool clearCurrentPlayer)
	{
		EventManager eventManager = Autoloads.sceneSingleton?.battleManager?.eventManager;
		if (eventManager == null)
		{
			return;
		}

		if (clearCurrentPlayer)
		{
			eventManager.currentMainPlayer = null;
		}
		eventManager.currentMainPlayerCommand = null;
		eventManager.currentMainEnemy = null;
		eventManager.currentTargetAreaId = CombatAreaId.Unknown;
		eventManager.moveEventInfo = null;
		eventManager.damageEventInfo = null;
	}

	private PlayerData ResolveCurrentPlayerForCommand()
	{
		BattleManager battleManager = Autoloads.sceneSingleton?.battleManager;
		PlayerData currentPlayer = battleManager?.eventManager?.currentMainPlayer;
		if (currentPlayer != null &&
			currentPlayer.characterBattleState == CharacterBattleState.ALIVE &&
			currentPlayer.currentRestActionTimes > 0)
		{
			return currentPlayer;
		}

		return battleManager?.battlePlayerDataList?
			.FirstOrDefault(player =>
				player != null &&
				player.currentRestActionTimes > 0 &&
				player.characterBattleState == CharacterBattleState.ALIVE);
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

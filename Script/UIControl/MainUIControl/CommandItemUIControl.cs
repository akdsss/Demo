using Godot;
using System;
using System.Collections.Generic;

public partial class CommandItemUIControl : Control
{
	private static CommandItemUIControl currentSelected;
	public static CommandItemUIControl CurrentSelected
	{
		get => currentSelected;
	}
	[Export] public Label label;
	[Export] public ColorRect colorRect;
	public int cmdIdxInQueue;
	public int characterRowIdx;
	public CharacterData ownerCharacterData;
	public CommandData assignedCommandData;
	public bool isEnemyActionRevealed = true;
	private Button slotButton;
	private ProgressBar holdProgressBar;
	private bool isHoldingForPlacement;
	private bool holdCompletedThisPress;
	private double holdElapsedSeconds;
	private const double HoldToPlaceSeconds = 2.0;
	static readonly string playerNormalColor = "c1cfeb";
	static readonly string enemyNormalColor = "ebc1c1";
	static readonly string normalColor = "ffffff";
	static readonly string disableColor = "6d6d6f";
	static readonly string highlightColor = "f1ca8e";
	static readonly string lockedColor = "76a1b1";
	static readonly string hiddenEnemyColor = "8d7f99";
	static readonly Dictionary<CommandItemState, string> colorDict = new()
	{
		{CommandItemState.NORMAL, normalColor},
		{CommandItemState.DISABLE, disableColor},
		{CommandItemState.HIGHLIGHT, highlightColor},
		{CommandItemState.LOCKED, lockedColor}
	};
	public CommandItemState commandItemState = CommandItemState.NORMAL;
	public bool isOnset = false;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		slotButton = GetNodeOrNull<Button>("Button");
		if (slotButton != null)
		{
			slotButton.ButtonDown += OnSlotButtonDown;
			slotButton.ButtonUp += OnSlotButtonUp;
		}
		EnsureHoldProgressBar();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (isHoldingForPlacement)
		{
			holdElapsedSeconds += delta;
			UpdateHoldProgressBar();
			if (holdElapsedSeconds >= HoldToPlaceSeconds)
			{
				CompleteHoldPlacement();
			}
		}

		// HIGHLIGHT模式下闪烁
		if (commandItemState == CommandItemState.HIGHLIGHT)
		{
			// 获取时间
			double time = (double)Time.GetTimeDictFromSystem()["second"];
			time *= 2;
			float t = (float)(Math.Sin(time * Math.PI) * 0.5 + 0.5); // 计算0到1之间的正弦波插值

			// 解析高亮颜色和正常颜色
			Color highlight = new(highlightColor);
			Color normal = GetNormalColor();

			// 颜色插值并赋值给节点
			Color interpolatedColor = normal.Lerp(highlight, t);
			colorRect.Color = interpolatedColor;
		}
		else
		{
			colorRect.Color = GetStateColor();
		}
	}
	// public void ResetCmdItem()
	// {
	// 	commandItemState = CommandItemState.NORMAL;
	// 	label.Text = Autoloads.sceneSingleton.cmdQueueUIControl.defaultCmdName;
	// 	cmdIdxInQueue = 0;
	// }
	public void MouseEnter()
	{
		if (isOnset == true && CanAcceptPlacement())
		{
			commandItemState = CommandItemState.HIGHLIGHT;
			// GD.Print("鼠标进入区域");
		}
		else if (assignedCommandData != null)
		{
			ShowDetail();
		}

	}
	// 鼠标退出区域时，只有在CMDSET模式状态下才执行退出操作
	public void MouseExit()
	{
		if (isOnset == true && commandItemState == CommandItemState.HIGHLIGHT)
		{
			commandItemState = CommandItemState.NORMAL;
			// GD.Print("鼠标退出区域");
		}
	}

	public void Click()
	{
		if (holdCompletedThisPress)
		{
			holdCompletedThisPress = false;
			return;
		}

		if (assignedCommandData != null || commandItemState == CommandItemState.LOCKED)
		{
			ShowDetail();
			return;
		}

		if (Autoloads.sceneSingleton.cmdQueueUIControl != null && Autoloads.sceneSingleton.cmdQueueUIControl.IsInspectMode)
		{
			Autoloads.sceneSingleton.cmdQueueUIControl.ShowCommandDetail("空白时点", "该时点尚未放置指令。");
			return;
		}

		if (isOnset == true)
		{
			Autoloads.sceneSingleton.cmdQueueUIControl?.ShowCommandDetail("放置指令", "请长按空白时点 2 秒确认指令。");
		}

	}
	public void SetCommandToQueue(CommandData commandData)
	{
		assignedCommandData = commandData;
		commandItemState = CommandItemState.LOCKED;
		isOnset = false;

		if (commandData is EnemyCommandData)
		{
			SetEnemyActionRevealed(false);
			return;
		}

		isEnemyActionRevealed = true;
		label.Text = commandData.commandName;
		Autoloads.sceneSingleton.cmdQueueUIControl?.SwitchOffPlayerCommandSet();
		Autoloads.sceneSingleton.playerCharacterHeadListUIControl?.ResetUIDisplay();
		Autoloads.sceneSingleton.enemyCharacterHeadListUIControl?.ResetUIDisplay();
	}

	public void ConfigureTimelineSlot(int slotIndex, int rowIndex, CharacterData owner, string defaultText)
	{
		cmdIdxInQueue = slotIndex;
		characterRowIdx = rowIndex;
		ownerCharacterData = owner;
		assignedCommandData = null;
		isEnemyActionRevealed = true;
		isHoldingForPlacement = false;
		holdCompletedThisPress = false;
		holdElapsedSeconds = 0;
		isOnset = false;
		commandItemState = CommandItemState.NORMAL;
		label.Text = defaultText;
		UpdateHoldProgressBar();
	}

	public void SetEnemyActionRevealed(bool revealed)
	{
		if (assignedCommandData == null)
		{
			return;
		}

		isEnemyActionRevealed = revealed;
		label.Text = revealed ? BuildRevealedCommandText(assignedCommandData) : BuildHiddenEnemyText(assignedCommandData);
	}

	public void RevealEnemyAction()
	{
		if (assignedCommandData is EnemyCommandData)
		{
			SetEnemyActionRevealed(true);
		}
	}

	public void EnablePlacement()
	{
		if (!CanAcceptPlacement())
		{
			return;
		}

		isOnset = true;
		commandItemState = CommandItemState.HIGHLIGHT;
	}

	public bool CanAcceptPlacement()
	{
		return assignedCommandData == null && commandItemState != CommandItemState.LOCKED;
	}

	private void OnSlotButtonDown()
	{
		if (!isOnset || !CanAcceptPlacement())
		{
			return;
		}

		if (Autoloads.sceneSingleton.cmdQueueUIControl != null && Autoloads.sceneSingleton.cmdQueueUIControl.IsInspectMode)
		{
			return;
		}

		isHoldingForPlacement = true;
		holdCompletedThisPress = false;
		holdElapsedSeconds = 0;
		UpdateHoldProgressBar();
	}

	private void OnSlotButtonUp()
	{
		if (!isHoldingForPlacement)
		{
			return;
		}

		isHoldingForPlacement = false;
		holdElapsedSeconds = 0;
		UpdateHoldProgressBar();
	}

	private void CompleteHoldPlacement()
	{
		isHoldingForPlacement = false;
		holdCompletedThisPress = true;
		holdElapsedSeconds = 0;
		UpdateHoldProgressBar();
		TryPlaceCurrentCommand();
	}

	private void TryPlaceCurrentCommand()
	{
		if (!CanAcceptPlacement())
		{
			Autoloads.sceneSingleton.cmdQueueUIControl?.ShowCommandDetail("不可覆盖", "已设置指令的时点不可覆盖。");
			return;
		}

		currentSelected = this;
		SceneSingleton sS = Autoloads.sceneSingleton;
		if (sS?.battleManager?.eventManager?.currentMainPlayer == null || sS.battleManager.eventManager.currentMainPlayerCommand == null)
		{
			sS?.cmdQueueUIControl?.ShowCommandDetail("无法放置", "请先选择角色、技能和目标。");
			return;
		}

		CommandExecuteInfo cei = new()
		{
			isDefault = false,
			commandData = sS.battleManager.eventManager.currentMainPlayerCommand,
			sourceCharacterData = sS.battleManager.eventManager.currentMainPlayer,
			targetCoord = sS.battleManager.eventManager.moveEventInfo?.moveTargetCoord ?? new Vector2I(),
			targetCharacterData = sS.battleManager.eventManager.damageEventInfo?.damageTargetCharacter,
		};
		sS.battleManager.eventManager.currentMainPlayer.SetCommand(1, CharacterHeadButtonControl.CurrentSelectedPlayerHead, cmdIdxInQueue, cei);
		sS.enemyCharacterHeadListUIControl.ChangeToUninteractable();
		sS.cmdQueueUIControl?.ShowSkillDetail(cei.commandData, cei.sourceCharacterData);
		sS.battleManager.CheckPlayerReadyOver();
	}

	private void ShowDetail()
	{
		if (assignedCommandData == null)
		{
			return;
		}

		if (assignedCommandData is EnemyCommandData && !isEnemyActionRevealed)
		{
			Autoloads.sceneSingleton.cmdQueueUIControl?.ShowSkillDetail(assignedCommandData, ownerCharacterData, false);
			return;
		}

		Autoloads.sceneSingleton.cmdQueueUIControl?.ShowSkillDetail(assignedCommandData, ownerCharacterData);
	}

	private void EnsureHoldProgressBar()
	{
		holdProgressBar = GetNodeOrNull<ProgressBar>("HoldProgressBar");
		if (holdProgressBar != null)
		{
			return;
		}

		holdProgressBar = new ProgressBar
		{
			Name = "HoldProgressBar",
			MinValue = 0,
			MaxValue = 1,
			Value = 0,
			ShowPercentage = false,
			MouseFilter = MouseFilterEnum.Ignore
		};
		holdProgressBar.AnchorLeft = 0;
		holdProgressBar.AnchorTop = 1;
		holdProgressBar.AnchorRight = 1;
		holdProgressBar.AnchorBottom = 1;
		holdProgressBar.OffsetTop = -6;
		holdProgressBar.OffsetBottom = 0;
		AddChild(holdProgressBar);
	}

	private void UpdateHoldProgressBar()
	{
		if (holdProgressBar == null)
		{
			return;
		}

		double progress = isHoldingForPlacement ? holdElapsedSeconds / HoldToPlaceSeconds : 0;
		holdProgressBar.Value = Mathf.Clamp((float)progress, 0, 1);
		holdProgressBar.Visible = holdProgressBar.Value > 0;
	}

	private Color GetNormalColor()
	{
		if (ownerCharacterData is PlayerData)
		{
			return new Color(playerNormalColor);
		}

		if (ownerCharacterData is EnemyData)
		{
			return new Color(enemyNormalColor);
		}

		return new Color(normalColor);
	}

	private Color GetStateColor()
	{
		if (assignedCommandData is EnemyCommandData && !isEnemyActionRevealed)
		{
			return new Color(hiddenEnemyColor);
		}

		if (commandItemState == CommandItemState.NORMAL)
		{
			return GetNormalColor();
		}

		return new Color(colorDict[commandItemState]);
	}

	private static string BuildHiddenEnemyText(CommandData commandData)
	{
		SkillDefinition skill = SkillDefinition.FromCommandData(commandData);
		string tagText = FormatSkillTags(skill.Tags);
		return string.IsNullOrEmpty(tagText) ? "意图？" : tagText;
	}

	private static string BuildRevealedCommandText(CommandData commandData)
	{
		SkillDefinition skill = SkillDefinition.FromCommandData(commandData);
		return $"{commandData.commandName}\nP{skill.Priority}";
	}

	private static string FormatSkillTags(SkillTag tags)
	{
		if (tags == SkillTag.None)
		{
			return string.Empty;
		}

		List<string> names = new();
		if ((tags & SkillTag.Melee) != 0) names.Add("近战");
		if ((tags & SkillTag.Ranged) != 0) names.Add("远程");
		if ((tags & SkillTag.Move) != 0) names.Add("位移");
		if ((tags & SkillTag.Defense) != 0) names.Add("防御");
		if ((tags & SkillTag.Heal) != 0) names.Add("治疗");
		if ((tags & SkillTag.Area) != 0) names.Add("范围");
		if ((tags & SkillTag.Special) != 0) names.Add("特殊");
		if (names.Count == 0 && (tags & SkillTag.SingleTarget) != 0) names.Add("单体");
		return string.Join("/", names);
	}
}

public enum CommandItemState
{
	NORMAL,
	DISABLE,
	HIGHLIGHT,
	LOCKED
}

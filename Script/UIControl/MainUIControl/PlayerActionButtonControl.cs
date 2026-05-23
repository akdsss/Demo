using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class PlayerActionButtonControl : Button
{
	public PlayerCommandData playerCommandData;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		MouseEntered += OnSkillMouseEntered;
		MouseExited += OnSkillMouseExited;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
	public void PlayerActionButtonClick()
	{
		EventManager eventManager = Autoloads.sceneSingleton.battleManager.eventManager;
		eventManager.currentMainPlayerCommand = playerCommandData;
		eventManager.currentTargetAreaId = CombatAreaId.Unknown;
		eventManager.moveEventInfo = null;
		eventManager.damageEventInfo = null;

		PlayerData source = eventManager.currentMainPlayer;
		SkillDefinition skill = SkillDefinition.FromCommandData(playerCommandData);
		PlayerChoseListPanelControl choicePanel = Autoloads.sceneSingleton.playerActionChoseList as PlayerChoseListPanelControl;
		if (skill.TargetType == SkillTargetType.Area || skill.HasTag(SkillTag.Move))
		{
			Autoloads.sceneSingleton.areaTargetMenuControl?.ShowForCommand(playerCommandData, source);
		}
		else if (skill.TargetType == SkillTargetType.Enemy)
		{
			choicePanel?.ShowCharacterTargetList(
				playerCommandData,
				Autoloads.sceneSingleton.battleManager.battleEnemyDataList.Cast<CharacterData>().ToList(),
				"选择敌方目标");
		}
		else if (skill.TargetType == SkillTargetType.Ally)
		{
			choicePanel?.ShowCharacterTargetList(
				playerCommandData,
				Autoloads.sceneSingleton.battleManager.battlePlayerDataList.Cast<CharacterData>().ToList(),
				"选择友方目标");
		}
		else if (skill.TargetType == SkillTargetType.Character)
		{
			List<CharacterData> targets = new();
			targets.AddRange(Autoloads.sceneSingleton.battleManager.battlePlayerDataList);
			targets.AddRange(Autoloads.sceneSingleton.battleManager.battleEnemyDataList);
			choicePanel?.ShowCharacterTargetList(playerCommandData, targets, "选择目标");
		}
		else
		{
			eventManager.damageEventInfo = new DamageEventInfo
			{
				damageSourceCharacter = source,
				damageTargetCharacter = skill.TargetType == SkillTargetType.Self ? source : null
			};
			Autoloads.sceneSingleton.cmdQueueUIControl?.SwitchOnPlayerCommandSet();
		}

		Autoloads.sceneSingleton.cmdQueueUIControl?.ShowSkillDetail(playerCommandData, eventManager.currentMainPlayer);
		choicePanel?.ShowSkillDetail(playerCommandData);
		Autoloads.sceneSingleton.tutorialOverlayControl?.Notify(TutorialWaitCondition.SelectSkill);
	}

	private void OnSkillMouseEntered()
	{
		Scale = new Vector2(1.04f, 1.04f);
		Autoloads.sceneSingleton.cmdQueueUIControl?.ShowSkillDetail(playerCommandData, Autoloads.sceneSingleton.battleManager.eventManager.currentMainPlayer);
		(Autoloads.sceneSingleton.playerActionChoseList as PlayerChoseListPanelControl)?.ShowSkillDetail(playerCommandData);
	}

	private void OnSkillMouseExited()
	{
		Scale = Vector2.One;
	}
}

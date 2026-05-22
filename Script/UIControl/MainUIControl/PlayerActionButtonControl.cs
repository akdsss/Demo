using Godot;
using System;

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
		playerCommandData.UIButtonClick();
		Autoloads.sceneSingleton.battleManager.eventManager.currentMainPlayerCommand = playerCommandData;
		Autoloads.sceneSingleton.cmdQueueUIControl?.ShowSkillDetail(playerCommandData, Autoloads.sceneSingleton.battleManager.eventManager.currentMainPlayer);
		Autoloads.sceneSingleton.tutorialOverlayControl?.Notify(TutorialWaitCondition.SelectSkill);
	}

	private void OnSkillMouseEntered()
	{
		Scale = new Vector2(1.04f, 1.04f);
		Autoloads.sceneSingleton.cmdQueueUIControl?.ShowSkillDetail(playerCommandData, Autoloads.sceneSingleton.battleManager.eventManager.currentMainPlayer);
	}

	private void OnSkillMouseExited()
	{
		Scale = Vector2.One;
	}
}

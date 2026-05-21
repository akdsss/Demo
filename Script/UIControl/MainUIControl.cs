using Godot;
using System;

public partial class MainUIControl : CanvasLayer
{
	[Export] public Panel SetPanel;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		EnsureBattleInputMap();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public void SetPanelCloseButtonClicked()
	{
		SetPanel.Visible = false;
	}

	public void SetPanelOpenButtonClicked()
	{
		SetPanel.Visible = true;
	}

	public override void _UnhandledInput(InputEvent @event)
	{
		if (@event.IsActionPressed("back"))
		{
			Autoloads.sceneSingleton?.cmdQueueUIControl?.CancelCurrentCommandSelection();
			GetViewport().SetInputAsHandled();
		}
	}

	private static void EnsureBattleInputMap()
	{
		EnsureMouseAction("confirm", MouseButton.Left);
		EnsureMouseAction("place_action_hold", MouseButton.Left);
		EnsureMouseAction("back", MouseButton.Right);
		EnsureMouseAction("inspect", MouseButton.Middle);
	}

	private static void EnsureMouseAction(string actionName, MouseButton button)
	{
		if (!InputMap.HasAction(actionName))
		{
			InputMap.AddAction(actionName);
		}

		foreach (InputEvent inputEvent in InputMap.ActionGetEvents(actionName))
		{
			if (inputEvent is InputEventMouseButton mouseButton && mouseButton.ButtonIndex == button)
			{
				return;
			}
		}

		InputMap.ActionAddEvent(actionName, new InputEventMouseButton { ButtonIndex = button });
	}
}

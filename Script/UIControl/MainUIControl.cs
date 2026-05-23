using Godot;
using System;

public partial class MainUIControl : CanvasLayer
{
	[Export] public Panel SetPanel;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		if (Autoloads.sceneSingleton != null)
		{
			Autoloads.sceneSingleton.mainUIControl = this;
		}
		CallDeferred(nameof(RegisterMainUIControl));
		EnsureBattleInputMap();
		EnsureEncyclopediaOverlay();
		EnsureGrowthRewardOverlay();
		EnsureAreaTargetMenu();
		EnsureTutorialOverlay();
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

	public Control GetTutorialHighlightControl(TutorialHighlightTarget target)
	{
		if (target != TutorialHighlightTarget.EncyclopediaButton)
		{
			return null;
		}

		Button encyclopediaButton = SetPanel?.GetNodeOrNull<Button>("VBoxContainer/EncyclopediaButton");
		if (SetPanel != null && SetPanel.Visible && encyclopediaButton != null)
		{
			return encyclopediaButton;
		}

		return GetNodeOrNull<Control>("Panel/VBoxContainer/TopPanel/Button");
	}

	public void EncyclopediaButtonClicked()
	{
		SetPanel.Visible = false;
		Autoloads.sceneSingleton?.encyclopediaOverlayControl?.OpenEncyclopedia();
	}

	public void ExitGameButtonClicked()
	{
		GetTree().Quit();
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

	private void EnsureTutorialOverlay()
	{
		TutorialOverlayControl overlay = GetNodeOrNull<TutorialOverlayControl>("TutorialOverlay");
		if (overlay == null)
		{
			overlay = new TutorialOverlayControl
			{
				Name = "TutorialOverlay"
			};
			AddChild(overlay);
		}

		if (Autoloads.sceneSingleton != null)
		{
			Autoloads.sceneSingleton.tutorialOverlayControl = overlay;
		}
		CallDeferred(nameof(RegisterTutorialOverlay));
	}

	private void EnsureEncyclopediaOverlay()
	{
		EncyclopediaOverlayControl overlay = GetNodeOrNull<EncyclopediaOverlayControl>("EncyclopediaOverlay");
		if (overlay == null)
		{
			overlay = new EncyclopediaOverlayControl
			{
				Name = "EncyclopediaOverlay"
			};
			AddChild(overlay);
		}

		if (Autoloads.sceneSingleton != null)
		{
			Autoloads.sceneSingleton.encyclopediaOverlayControl = overlay;
		}
		CallDeferred(nameof(RegisterEncyclopediaOverlay));
	}

	private void EnsureGrowthRewardOverlay()
	{
		GrowthRewardOverlayControl overlay = GetNodeOrNull<GrowthRewardOverlayControl>("GrowthRewardOverlay");
		if (overlay == null)
		{
			overlay = new GrowthRewardOverlayControl
			{
				Name = "GrowthRewardOverlay"
			};
			AddChild(overlay);
		}

		if (Autoloads.sceneSingleton != null)
		{
			Autoloads.sceneSingleton.growthRewardOverlayControl = overlay;
		}
		CallDeferred(nameof(RegisterGrowthRewardOverlay));
	}

	private void EnsureAreaTargetMenu()
	{
		AreaTargetMenuControl menu = GetNodeOrNull<AreaTargetMenuControl>("AreaTargetMenu");
		if (menu == null)
		{
			menu = new AreaTargetMenuControl
			{
				Name = "AreaTargetMenu"
			};
			AddChild(menu);
		}

		if (Autoloads.sceneSingleton != null)
		{
			Autoloads.sceneSingleton.areaTargetMenuControl = menu;
		}
		CallDeferred(nameof(RegisterAreaTargetMenu));
	}

	public void RegisterTutorialOverlay()
	{
		if (Autoloads.sceneSingleton != null)
		{
			Autoloads.sceneSingleton.tutorialOverlayControl = GetNodeOrNull<TutorialOverlayControl>("TutorialOverlay");
		}
	}

	public void RegisterMainUIControl()
	{
		if (Autoloads.sceneSingleton != null)
		{
			Autoloads.sceneSingleton.mainUIControl = this;
		}
	}

	public void RegisterEncyclopediaOverlay()
	{
		if (Autoloads.sceneSingleton != null)
		{
			Autoloads.sceneSingleton.encyclopediaOverlayControl = GetNodeOrNull<EncyclopediaOverlayControl>("EncyclopediaOverlay");
			Autoloads.sceneSingleton.encyclopediaOverlayControl?.SetExternalOpenButton(
				SetPanel?.GetNodeOrNull<Button>("VBoxContainer/EncyclopediaButton"));
		}
	}

	public void RegisterGrowthRewardOverlay()
	{
		if (Autoloads.sceneSingleton != null)
		{
			Autoloads.sceneSingleton.growthRewardOverlayControl = GetNodeOrNull<GrowthRewardOverlayControl>("GrowthRewardOverlay");
		}
	}

	public void RegisterAreaTargetMenu()
	{
		if (Autoloads.sceneSingleton != null)
		{
			Autoloads.sceneSingleton.areaTargetMenuControl = GetNodeOrNull<AreaTargetMenuControl>("AreaTargetMenu");
		}
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

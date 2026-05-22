using Godot;
using System;

public partial class MainUIControl : CanvasLayer
{
	[Export] public Panel SetPanel;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		EnsureBattleInputMap();
		EnsureUiSfxRouter();
		EnsureEncyclopediaOverlay();
		EnsureGrowthRewardOverlay();
		EnsureTutorialOverlay();
		EnsureBattlePresentationPlaceholder();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public void SetPanelCloseButtonClicked()
	{
		Autoloads.sceneSingleton?.uiSfxRouter?.PlayClick();
		SetPanel.Visible = false;
	}

	public void SetPanelOpenButtonClicked()
	{
		Autoloads.sceneSingleton?.uiSfxRouter?.PlayClick();
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

	private void EnsureUiSfxRouter()
	{
		UISfxRouter router = GetNodeOrNull<UISfxRouter>("UISfxRouter");
		if (router == null)
		{
			router = new UISfxRouter
			{
				Name = "UISfxRouter"
			};
			AddChild(router);
		}

		if (Autoloads.sceneSingleton != null)
		{
			Autoloads.sceneSingleton.uiSfxRouter = router;
		}
		CallDeferred(nameof(RegisterUiSfxRouter));
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

	private void EnsureBattlePresentationPlaceholder()
	{
		BattlePresentationPlaceholderControl overlay = GetNodeOrNull<BattlePresentationPlaceholderControl>("BattlePresentationPlaceholder");
		if (overlay == null)
		{
			overlay = new BattlePresentationPlaceholderControl
			{
				Name = "BattlePresentationPlaceholder"
			};
			AddChild(overlay);
		}

		if (Autoloads.sceneSingleton != null)
		{
			Autoloads.sceneSingleton.battlePresentationPlaceholderControl = overlay;
		}
		CallDeferred(nameof(RegisterBattlePresentationPlaceholder));
	}

	public void RegisterUiSfxRouter()
	{
		if (Autoloads.sceneSingleton != null)
		{
			Autoloads.sceneSingleton.uiSfxRouter = GetNodeOrNull<UISfxRouter>("UISfxRouter");
		}
	}

	public void RegisterTutorialOverlay()
	{
		if (Autoloads.sceneSingleton != null)
		{
			Autoloads.sceneSingleton.tutorialOverlayControl = GetNodeOrNull<TutorialOverlayControl>("TutorialOverlay");
		}
	}

	public void RegisterEncyclopediaOverlay()
	{
		if (Autoloads.sceneSingleton != null)
		{
			Autoloads.sceneSingleton.encyclopediaOverlayControl = GetNodeOrNull<EncyclopediaOverlayControl>("EncyclopediaOverlay");
		}
	}

	public void RegisterGrowthRewardOverlay()
	{
		if (Autoloads.sceneSingleton != null)
		{
			Autoloads.sceneSingleton.growthRewardOverlayControl = GetNodeOrNull<GrowthRewardOverlayControl>("GrowthRewardOverlay");
		}
	}

	public void RegisterBattlePresentationPlaceholder()
	{
		if (Autoloads.sceneSingleton != null)
		{
			Autoloads.sceneSingleton.battlePresentationPlaceholderControl = GetNodeOrNull<BattlePresentationPlaceholderControl>("BattlePresentationPlaceholder");
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

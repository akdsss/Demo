using Godot;
using System;

public partial class MainUIControl : CanvasLayer
{
	[Export] public Panel SetPanel;
	private Button settingsOpenButton;
	private Button battleLogButton;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		if (Autoloads.sceneSingleton != null)
		{
			Autoloads.sceneSingleton.mainUIControl = this;
		}
		SetProcessInput(true);
		EnsureSettingsClickTargets();
		EnsureBattleLogButton();
		CallDeferred(nameof(EnsureSettingsClickTargets));
		CallDeferred(nameof(EnsureBattleLogButton));
		CallDeferred(nameof(RegisterMainUIControl));
		EnsureBattleInputMap();
		EnsureBattleLogOverlay();
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
		if (SetPanel != null)
		{
			SetPanel.Visible = false;
		}
	}

	public void SetPanelOpenButtonClicked()
	{
		ToggleSettingsPanel();
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
		if (SetPanel != null)
		{
			SetPanel.Visible = false;
		}
		Autoloads.sceneSingleton?.encyclopediaOverlayControl?.OpenEncyclopedia();
	}

	public void BattleLogButtonClicked()
	{
		EnsureBattleLogOverlay();
		Autoloads.sceneSingleton?.battleLogOverlayControl?.OpenLog();
	}

	public void ExitGameButtonClicked()
	{
		GetTree().Quit();
	}

	public override void _Input(InputEvent @event)
	{
		BattleLogOverlayControl battleLogOverlay = Autoloads.sceneSingleton?.battleLogOverlayControl;
		bool battleLogOpen = battleLogOverlay != null && battleLogOverlay.IsOpen;

		if (IsEscapePressed(@event))
		{
			if (battleLogOpen)
			{
				battleLogOverlay.CloseLog();
			}
			else
			{
				ToggleSettingsPanel();
			}
			GetViewport().SetInputAsHandled();
			return;
		}

		if (@event.IsActionPressed("back") && battleLogOpen)
		{
			battleLogOverlay.CloseLog();
			GetViewport().SetInputAsHandled();
			return;
		}

		if (!battleLogOpen &&
			@event is InputEventMouseButton mouseButton &&
			mouseButton.ButtonIndex == MouseButton.Left &&
			mouseButton.Pressed)
		{
			EnsureSettingsClickTargets();
			EnsureBattleLogButton();
			if (IsMouseInsideControl(battleLogButton, mouseButton.Position))
			{
				BattleLogButtonClicked();
				GetViewport().SetInputAsHandled();
				return;
			}

			if (IsMouseInsideControl(settingsOpenButton, mouseButton.Position))
			{
				SetPanelOpenButtonClicked();
				GetViewport().SetInputAsHandled();
				return;
			}
		}

		if (@event.IsActionPressed("back"))
		{
			if (SetPanel != null && SetPanel.Visible)
			{
				SetPanel.Visible = false;
			}
			else
			{
				Autoloads.sceneSingleton?.cmdQueueUIControl?.CancelCurrentCommandSelection();
			}
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

	private void EnsureBattleLogOverlay()
	{
		BattleLogOverlayControl overlay = GetNodeOrNull<BattleLogOverlayControl>("BattleLogOverlay");
		if (overlay == null)
		{
			overlay = new BattleLogOverlayControl
			{
				Name = "BattleLogOverlay"
			};
			AddChild(overlay);
		}

		if (Autoloads.sceneSingleton != null)
		{
			Autoloads.sceneSingleton.battleLogOverlayControl = overlay;
		}
		CallDeferred(nameof(RegisterBattleLogOverlay));
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

	public void RegisterBattleLogOverlay()
	{
		if (Autoloads.sceneSingleton != null)
		{
			Autoloads.sceneSingleton.battleLogOverlayControl = GetNodeOrNull<BattleLogOverlayControl>("BattleLogOverlay");
		}
	}

	public void EnsureSettingsClickTargets()
	{
		settingsOpenButton = GetNodeOrNull<Button>("Panel/VBoxContainer/TopPanel/Button");
		if (settingsOpenButton != null)
		{
			settingsOpenButton.ZIndex = 100;
			settingsOpenButton.MouseFilter = Control.MouseFilterEnum.Stop;
			settingsOpenButton.MoveToFront();
			SetDecorativeChildrenMouseFilter(settingsOpenButton, Control.MouseFilterEnum.Ignore);
		}

		if (SetPanel != null)
		{
			SetPanel.ZIndex = 100;
			SetPanel.MouseFilter = Control.MouseFilterEnum.Stop;
			SetPanel.MoveToFront();
		}
	}

	public void EnsureBattleLogButton()
	{
		Control topPanel = GetNodeOrNull<Control>("Panel/VBoxContainer/TopPanel");
		if (topPanel == null)
		{
			return;
		}

		settingsOpenButton ??= GetNodeOrNull<Button>("Panel/VBoxContainer/TopPanel/Button");
		battleLogButton = topPanel.GetNodeOrNull<Button>("BattleLogButton");
		if (battleLogButton == null)
		{
			battleLogButton = new Button
			{
				Name = "BattleLogButton",
				MouseFilter = Control.MouseFilterEnum.Stop
			};
			topPanel.AddChild(battleLogButton);
		}

		if (settingsOpenButton != null)
		{
			float buttonWidth = settingsOpenButton.OffsetRight - settingsOpenButton.OffsetLeft;
			float buttonGap = Mathf.Max(0f, -settingsOpenButton.OffsetRight);
			float logButtonRight = settingsOpenButton.OffsetLeft - buttonGap;

			battleLogButton.AnchorLeft = settingsOpenButton.AnchorLeft;
			battleLogButton.AnchorRight = settingsOpenButton.AnchorRight;
			battleLogButton.AnchorTop = settingsOpenButton.AnchorTop;
			battleLogButton.AnchorBottom = settingsOpenButton.AnchorBottom;
			battleLogButton.OffsetLeft = logButtonRight - buttonWidth;
			battleLogButton.OffsetTop = settingsOpenButton.OffsetTop;
			battleLogButton.OffsetRight = logButtonRight;
			battleLogButton.OffsetBottom = settingsOpenButton.OffsetBottom;
			battleLogButton.ZIndex = settingsOpenButton.ZIndex;
		}
		else
		{
			battleLogButton.AnchorLeft = 1f;
			battleLogButton.AnchorRight = 1f;
			battleLogButton.AnchorTop = 0f;
			battleLogButton.AnchorBottom = 0f;
			battleLogButton.OffsetLeft = -144f;
			battleLogButton.OffsetTop = 6f;
			battleLogButton.OffsetRight = -78f;
			battleLogButton.OffsetBottom = 72f;
			battleLogButton.ZIndex = 100;
		}

		battleLogButton.MouseFilter = Control.MouseFilterEnum.Stop;
		battleLogButton.MoveToFront();
		battleLogButton.Pressed -= BattleLogButtonClicked;
		battleLogButton.Pressed += BattleLogButtonClicked;

		TextureRect icon = battleLogButton.GetNodeOrNull<TextureRect>("TextureRect");
		if (icon == null)
		{
			icon = new TextureRect
			{
				Name = "TextureRect",
				MouseFilter = Control.MouseFilterEnum.Ignore
			};
			battleLogButton.AddChild(icon);
		}

		icon.SetAnchorsPreset(Control.LayoutPreset.FullRect);
		icon.OffsetLeft = 0f;
		icon.OffsetTop = 0f;
		icon.OffsetRight = 0f;
		icon.OffsetBottom = 0f;
		icon.ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize;
		icon.StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered;
		icon.Texture = ResourceLoader.Load<Texture2D>("res://Asset/ui image/log.png");
		icon.MouseFilter = Control.MouseFilterEnum.Ignore;
	}

	private void ToggleSettingsPanel()
	{
		EnsureSettingsClickTargets();
		if (SetPanel != null)
		{
			SetPanel.Visible = !SetPanel.Visible;
			if (SetPanel.Visible)
			{
				SetPanel.MoveToFront();
			}
		}
	}

	private static bool IsEscapePressed(InputEvent @event)
	{
		return @event is InputEventKey keyEvent &&
			keyEvent.Pressed &&
			!keyEvent.Echo &&
			keyEvent.Keycode == Key.Escape;
	}

	private static bool IsMouseInsideControl(Control control, Vector2 viewportPosition)
	{
		return control != null && control.Visible && control.GetGlobalRect().HasPoint(viewportPosition);
	}

	private static void SetDecorativeChildrenMouseFilter(Control control, Control.MouseFilterEnum mouseFilter)
	{
		foreach (Node child in control.GetChildren())
		{
			if (child is Control childControl)
			{
				childControl.MouseFilter = mouseFilter;
				SetDecorativeChildrenMouseFilter(childControl, mouseFilter);
			}
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

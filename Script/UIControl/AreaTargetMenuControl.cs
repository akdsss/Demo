using Godot;

public partial class AreaTargetMenuControl : Control
{
    private Panel panel;
    private GridContainer buttonGrid;
    private Label titleLabel;
    private PlayerCommandData pendingCommandData;
    private CharacterData pendingSource;

    public override void _Ready()
    {
        MouseFilter = MouseFilterEnum.Stop;
        SetAnchorsPreset(LayoutPreset.FullRect);
        Visible = false;
        BuildUi();
    }

    public override void _GuiInput(InputEvent @event)
    {
        if (!Visible)
        {
            return;
        }

        if (@event is InputEventMouseButton mouseButton &&
            mouseButton.ButtonIndex == MouseButton.Left &&
            mouseButton.Pressed)
        {
            CancelSelection();
            AcceptEvent();
            GetViewport().SetInputAsHandled();
        }
    }

    public void ShowForCommand(PlayerCommandData commandData, CharacterData source)
    {
        pendingCommandData = commandData;
        pendingSource = source;
        titleLabel.Text = commandData == null
            ? "选择目标区域"
            : $"选择 {commandData.commandName} 的目标区域";
        Visible = true;
        MoveToFront();
    }

    public void Dismiss()
    {
        HideAndClear();
    }

    private void BuildUi()
    {
        ColorRect dimRect = new()
        {
            Name = "AreaTargetMenuInputBlocker",
            Color = new Color(0, 0, 0, 0.01f),
            MouseFilter = MouseFilterEnum.Stop
        };
        dimRect.GuiInput += OnCancelSurfaceGuiInput;
        dimRect.SetAnchorsPreset(LayoutPreset.FullRect);
        AddChild(dimRect);

        panel = new Panel
        {
            Name = "AreaTargetPanel",
            MouseFilter = MouseFilterEnum.Stop
        };
        panel.GuiInput += OnCancelSurfaceGuiInput;
        panel.AnchorLeft = 0.24f;
        panel.AnchorRight = 0.76f;
        panel.AnchorTop = 0.24f;
        panel.AnchorBottom = 0.70f;
        AddChild(panel);

        VBoxContainer content = new()
        {
            Name = "Content",
            MouseFilter = MouseFilterEnum.Pass
        };
        content.SetAnchorsPreset(LayoutPreset.FullRect);
        content.OffsetLeft = 18;
        content.OffsetRight = -18;
        content.OffsetTop = 16;
        content.OffsetBottom = -16;
        panel.AddChild(content);

        titleLabel = new Label
        {
            Name = "TitleLabel",
            Text = "选择目标区域",
            HorizontalAlignment = HorizontalAlignment.Center
        };
        content.AddChild(titleLabel);

        buttonGrid = new GridContainer
        {
            Name = "AreaButtonGrid",
            Columns = 5,
            MouseFilter = MouseFilterEnum.Pass,
            SizeFlagsVertical = SizeFlags.ExpandFill
        };
        content.AddChild(buttonGrid);

        foreach (CombatAreaId areaId in AreaDefinition.GetDefaultAreaOrder())
        {
            Button button = new()
            {
                Name = $"{areaId}Button",
                Text = AreaDefinition.GetDisplayName(areaId),
                CustomMinimumSize = new Vector2(96, 44),
                MouseFilter = MouseFilterEnum.Stop
            };
            button.Pressed += () => ConfirmArea(areaId);
            buttonGrid.AddChild(button);
        }
    }

    private void OnCancelSurfaceGuiInput(InputEvent @event)
    {
        if (!Visible)
        {
            return;
        }

        if (@event is InputEventMouseButton mouseButton &&
            mouseButton.ButtonIndex == MouseButton.Left &&
            mouseButton.Pressed)
        {
            CancelSelection();
            AcceptEvent();
            GetViewport().SetInputAsHandled();
        }
    }

    private void ConfirmArea(CombatAreaId areaId)
    {
        SceneSingleton sceneSingleton = Autoloads.sceneSingleton;
        EventManager eventManager = sceneSingleton?.battleManager?.eventManager;
        if (eventManager == null)
        {
            HideAndClear();
            return;
        }

        eventManager.currentTargetAreaId = areaId;
        eventManager.currentMainPlayerCommand = pendingCommandData;
        SkillDefinition skill = SkillDefinition.FromCommandData(pendingCommandData);
        eventManager.moveEventInfo = skill.HasTag(SkillTag.Move)
            ? new MoveEventInfo
        {
            moveSourceCharacter = pendingSource,
            moveTargetAreaId = areaId,
            moveTargetCoord = AreaDefinition.GetLegacyCoordForAreaId(areaId)
        }
            : null;

        sceneSingleton.cmdQueueUIControl?.SwitchOnPlayerCommandSet();
        sceneSingleton.cmdQueueUIControl?.ShowCommandDetail("目标区域", $"已选择 {AreaDefinition.GetDisplayName(areaId)}。请选择空白时点并长按 1.2 秒确认。");
        HideAndClear();
    }

    private void CancelSelection()
    {
        EventManager eventManager = Autoloads.sceneSingleton?.battleManager?.eventManager;
        if (eventManager != null)
        {
            eventManager.currentTargetAreaId = CombatAreaId.Unknown;
            eventManager.moveEventInfo = null;
        }

        Autoloads.sceneSingleton?.cmdQueueUIControl?.ShowCommandDetail("返回", "已返回上一级技能选择。");
        HideAndClear();
    }

    private void HideAndClear()
    {
        Visible = false;
        pendingCommandData = null;
        pendingSource = null;
    }
}

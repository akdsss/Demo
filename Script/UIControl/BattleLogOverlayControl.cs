using Godot;

public partial class BattleLogOverlayControl : Control
{
    private const float PanelWidth = 760f;
    private const string EmptyLogText = "暂无战斗记录";
    private Panel panel;
    private RichTextLabel logTextLabel;

    public bool IsOpen
    {
        get { return Visible; }
    }

    public override void _Ready()
    {
        BuildOverlay();
        Visible = false;
        RefreshLogText();
    }

    public override void _Notification(int what)
    {
        if (what == NotificationResized)
        {
            FitToViewport();
        }
    }

    public override void _GuiInput(InputEvent @event)
    {
        if (!Visible)
        {
            return;
        }

        if (@event is InputEventMouseButton mouseButton &&
            mouseButton.ButtonIndex == MouseButton.Right &&
            mouseButton.Pressed)
        {
            CloseLog();
            AcceptEvent();
            GetViewport().SetInputAsHandled();
        }
    }

    public void OpenLog()
    {
        FitToViewport();
        RefreshLogText();
        Visible = true;
        MoveToFront();
    }

    public void CloseLog()
    {
        Visible = false;
    }

    public void RefreshLogText()
    {
        if (logTextLabel == null)
        {
            return;
        }

        string logText = PubTool.instance.GetPrintHistoryText();
        logTextLabel.Text = string.IsNullOrWhiteSpace(logText) ? EmptyLogText : logText;
    }

    private void BuildOverlay()
    {
        SetAnchorsPreset(LayoutPreset.FullRect);
        FitToViewport();
        MouseFilter = MouseFilterEnum.Stop;
        ZIndex = 220;

        ColorRect inputBlocker = new()
        {
            Name = "BattleLogInputBlocker",
            Color = new Color(0, 0, 0, 0.01f),
            MouseFilter = MouseFilterEnum.Stop
        };
        inputBlocker.SetAnchorsPreset(LayoutPreset.FullRect);
        AddChild(inputBlocker);

        panel = new Panel
        {
            Name = "BattleLogPanel",
            MouseFilter = MouseFilterEnum.Stop
        };
        panel.AnchorLeft = 0.5f;
        panel.AnchorRight = 0.5f;
        panel.AnchorTop = 0.14f;
        panel.AnchorBottom = 0.86f;
        panel.OffsetLeft = -PanelWidth * 0.5f;
        panel.OffsetRight = PanelWidth * 0.5f;
        panel.OffsetTop = 0f;
        panel.OffsetBottom = 0f;
        AddChild(panel);

        ColorRect background = new()
        {
            Name = "BattleLogBackground",
            Color = new Color(0, 0, 0, 0.76f),
            MouseFilter = MouseFilterEnum.Ignore
        };
        background.SetAnchorsPreset(LayoutPreset.FullRect);
        panel.AddChild(background);

        MarginContainer margin = new()
        {
            Name = "BattleLogMargin",
            MouseFilter = MouseFilterEnum.Pass
        };
        margin.SetAnchorsPreset(LayoutPreset.FullRect);
        margin.AddThemeConstantOverride("margin_left", 18);
        margin.AddThemeConstantOverride("margin_right", 18);
        margin.AddThemeConstantOverride("margin_top", 14);
        margin.AddThemeConstantOverride("margin_bottom", 16);
        panel.AddChild(margin);

        VBoxContainer content = new()
        {
            Name = "BattleLogContent",
            MouseFilter = MouseFilterEnum.Pass
        };
        margin.AddChild(content);

        HBoxContainer header = new()
        {
            Name = "BattleLogHeader",
            MouseFilter = MouseFilterEnum.Pass
        };
        content.AddChild(header);

        Label titleLabel = new()
        {
            Name = "BattleLogTitle",
            Text = "战斗记录",
            SizeFlagsHorizontal = SizeFlags.ExpandFill
        };
        titleLabel.AddThemeColorOverride("font_color", new Color(1, 1, 1, 0.96f));
        header.AddChild(titleLabel);

        Button closeButton = new()
        {
            Name = "CloseButton",
            Text = "关闭",
            CustomMinimumSize = new Vector2(72, 34),
            MouseFilter = MouseFilterEnum.Stop
        };
        closeButton.Pressed += CloseLog;
        header.AddChild(closeButton);

        logTextLabel = new RichTextLabel
        {
            Name = "BattleLogText",
            FitContent = false,
            ScrollActive = true,
            SelectionEnabled = true,
            AutowrapMode = TextServer.AutowrapMode.WordSmart,
            SizeFlagsHorizontal = SizeFlags.ExpandFill,
            SizeFlagsVertical = SizeFlags.ExpandFill,
            MouseFilter = MouseFilterEnum.Stop
        };
        logTextLabel.AddThemeColorOverride("default_color", new Color(1, 1, 1, 0.92f));
        content.AddChild(logTextLabel);
    }

    private void FitToViewport()
    {
        AnchorLeft = 0f;
        AnchorTop = 0f;
        AnchorRight = 1f;
        AnchorBottom = 1f;
        OffsetLeft = 0f;
        OffsetTop = 0f;
        OffsetRight = 0f;
        OffsetBottom = 0f;
        Size = GetViewportRect().Size;
    }
}

using Godot;
using System.Collections.Generic;

public partial class EncyclopediaOverlayControl : Control
{
    private const float PanelWidth = 1040f;
    private const float LeftPaneWidth = 220f;
    private const float PaneMinimumHeight = 520f;
    private const float ListTopInset = 112f;
    private const float DetailTopInset = 118f;
    private const string LeftBackgroundPath = "res://Asset/ui image/Encyclopedia_left.png";
    private const string RightBackgroundPath = "res://Asset/ui image/Encyclopedia_right.png";
    private static readonly Color TextColor = new(0f, 0f, 0f, 1f);
    private static readonly Color TransparentColor = new(0f, 0f, 0f, 0f);
    private Control externalOpenButton;
    private ColorRect dimRect;
    private PanelContainer panel;
    private VBoxContainer entryList;
    private Label titleLabel;
    private Label bodyLabel;
    private readonly List<EncyclopediaEntry> entries = BuildEntries();

    public override void _Ready()
    {
        BuildOverlay();
        titleLabel.Text = string.Empty;
        bodyLabel.Text = string.Empty;
        dimRect.Visible = false;
        panel.Visible = false;
    }

    public override void _Notification(int what)
    {
        if (what == NotificationResized)
        {
            FitToViewport();
        }
    }

    public void SetExternalOpenButton(Control control)
    {
        externalOpenButton = control;
    }

    public Control GetTutorialHighlightControl(TutorialHighlightTarget target)
    {
        return target == TutorialHighlightTarget.EncyclopediaButton ? externalOpenButton : null;
    }

    private void BuildOverlay()
    {
        SetAnchorsPreset(LayoutPreset.FullRect);
        FitToViewport();
        MouseFilter = MouseFilterEnum.Ignore;

        dimRect = new ColorRect
        {
            Name = "EncyclopediaDim",
            Color = new Color(0, 0, 0, 0.42f),
            MouseFilter = MouseFilterEnum.Ignore
        };
        dimRect.SetAnchorsPreset(LayoutPreset.FullRect);
        AddChild(dimRect);

        panel = new PanelContainer
        {
            Name = "EncyclopediaPanel",
            MouseFilter = MouseFilterEnum.Stop
        };
        panel.AddThemeStyleboxOverride("panel", new StyleBoxFlat
        {
            BgColor = TransparentColor
        });
        panel.AnchorLeft = 0.5f;
        panel.AnchorRight = 0.5f;
        panel.AnchorTop = 0.10f;
        panel.AnchorBottom = 0.90f;
        panel.OffsetTop = 0f;
        panel.OffsetBottom = 0f;
        AddChild(panel);
        ApplyPanelWidth();

        VBoxContainer root = new()
        {
            Name = "Root",
            SizeFlagsHorizontal = SizeFlags.ExpandFill,
            SizeFlagsVertical = SizeFlags.ExpandFill
        };
        panel.AddChild(root);

        HBoxContainer header = new()
        {
            Name = "Header",
            SizeFlagsHorizontal = SizeFlags.ExpandFill
        };
        header.Visible = false;
        header.AddThemeConstantOverride("separation", 12);
        root.AddChild(header);

        Label headerLabel = new()
        {
            Text = "百科",
            SizeFlagsHorizontal = SizeFlags.ExpandFill
        };
        headerLabel.AddThemeFontSizeOverride("font_size", 24);
        header.AddChild(headerLabel);

        Button closeButton = new()
        {
            Text = "关闭"
        };
        closeButton.CustomMinimumSize = new Vector2(110, 44);
        closeButton.AddThemeFontSizeOverride("font_size", 18);
        closeButton.Pressed += CloseEncyclopedia;
        header.AddChild(closeButton);

        HBoxContainer body = new()
        {
            Name = "Body",
            SizeFlagsHorizontal = SizeFlags.ExpandFill,
            SizeFlagsVertical = SizeFlags.ExpandFill
        };
        body.AddThemeConstantOverride("separation", 0);
        root.AddChild(body);

        Control leftPane = new()
        {
            Name = "EntryListPane",
            CustomMinimumSize = new Vector2(LeftPaneWidth, PaneMinimumHeight),
            SizeFlagsVertical = SizeFlags.ExpandFill,
            MouseFilter = MouseFilterEnum.Pass
        };
        body.AddChild(leftPane);
        leftPane.AddChild(CreatePaneBackground("EntryListBackground", LeftBackgroundPath));

        Label leftTitleLabel = new()
        {
            Name = "LeftTitle",
            Text = "百科",
            MouseFilter = MouseFilterEnum.Ignore
        };
        leftTitleLabel.AnchorLeft = 0f;
        leftTitleLabel.AnchorTop = 0f;
        leftTitleLabel.AnchorRight = 1f;
        leftTitleLabel.AnchorBottom = 0f;
        leftTitleLabel.OffsetLeft = 18f;
        leftTitleLabel.OffsetTop = 14f;
        leftTitleLabel.OffsetRight = -14f;
        leftTitleLabel.OffsetBottom = 58f;
        leftTitleLabel.AddThemeFontSizeOverride("font_size", 24);
        ApplyLabelTextColor(leftTitleLabel);
        leftPane.AddChild(leftTitleLabel);

        MarginContainer listMargin = new()
        {
            Name = "EntryListMargin",
            MouseFilter = MouseFilterEnum.Pass
        };
        listMargin.SetAnchorsPreset(LayoutPreset.FullRect);
        listMargin.AddThemeConstantOverride("margin_left", 16);
        listMargin.AddThemeConstantOverride("margin_right", 14);
        listMargin.AddThemeConstantOverride("margin_top", (int)ListTopInset);
        listMargin.AddThemeConstantOverride("margin_bottom", 18);
        leftPane.AddChild(listMargin);

        ScrollContainer listScroll = new()
        {
            Name = "EntryListScroll",
            SizeFlagsHorizontal = SizeFlags.ExpandFill,
            SizeFlagsVertical = SizeFlags.ExpandFill,
            MouseFilter = MouseFilterEnum.Stop
        };
        listMargin.AddChild(listScroll);

        entryList = new VBoxContainer
        {
            Name = "EntryList",
            SizeFlagsHorizontal = SizeFlags.ExpandFill
        };
        listScroll.AddChild(entryList);

        foreach (EncyclopediaEntry entry in entries)
        {
            Button entryButton = new()
            {
                Text = $"{entry.Collection} / {entry.Title}",
                MouseFilter = MouseFilterEnum.Stop
            };
            entryButton.CustomMinimumSize = new Vector2(0, 42);
            entryButton.SizeFlagsHorizontal = SizeFlags.ExpandFill;
            entryButton.AddThemeFontSizeOverride("font_size", 16);
            ApplyButtonTextStyle(entryButton);
            entryButton.Pressed += () => ShowEntry(entry);
            entryList.AddChild(entryButton);
        }

        Control rightPane = new()
        {
            Name = "DetailPane",
            CustomMinimumSize = new Vector2(0, PaneMinimumHeight),
            SizeFlagsHorizontal = SizeFlags.ExpandFill,
            SizeFlagsVertical = SizeFlags.ExpandFill,
            MouseFilter = MouseFilterEnum.Pass
        };
        body.AddChild(rightPane);
        rightPane.AddChild(CreatePaneBackground("DetailBackground", RightBackgroundPath));

        Button paneCloseButton = new()
        {
            Name = "PaneCloseButton",
            Text = "关闭",
            MouseFilter = MouseFilterEnum.Stop
        };
        paneCloseButton.AnchorLeft = 1f;
        paneCloseButton.AnchorTop = 0f;
        paneCloseButton.AnchorRight = 1f;
        paneCloseButton.AnchorBottom = 0f;
        paneCloseButton.OffsetLeft = -128f;
        paneCloseButton.OffsetTop = 16f;
        paneCloseButton.OffsetRight = -18f;
        paneCloseButton.OffsetBottom = 60f;
        paneCloseButton.AddThemeFontSizeOverride("font_size", 18);
        ApplyButtonTextStyle(paneCloseButton);
        paneCloseButton.Pressed += CloseEncyclopedia;
        rightPane.AddChild(paneCloseButton);

        MarginContainer detailMargin = new()
        {
            Name = "DetailMargin",
            MouseFilter = MouseFilterEnum.Pass
        };
        detailMargin.SetAnchorsPreset(LayoutPreset.FullRect);
        detailMargin.AddThemeConstantOverride("margin_left", 28);
        detailMargin.AddThemeConstantOverride("margin_right", 28);
        detailMargin.AddThemeConstantOverride("margin_top", (int)DetailTopInset);
        detailMargin.AddThemeConstantOverride("margin_bottom", 24);
        rightPane.AddChild(detailMargin);

        PanelContainer detailTextPanel = new()
        {
            Name = "DetailTextPanel",
            SizeFlagsHorizontal = SizeFlags.ExpandFill,
            SizeFlagsVertical = SizeFlags.ExpandFill,
            MouseFilter = MouseFilterEnum.Pass
        };
        detailTextPanel.AddThemeStyleboxOverride("panel", CreateTranslucentTextStyle(0.70f));
        detailMargin.AddChild(detailTextPanel);

        MarginContainer detailTextMargin = new()
        {
            Name = "DetailTextMargin",
            MouseFilter = MouseFilterEnum.Pass
        };
        detailTextMargin.AddThemeConstantOverride("margin_left", 18);
        detailTextMargin.AddThemeConstantOverride("margin_right", 18);
        detailTextMargin.AddThemeConstantOverride("margin_top", 14);
        detailTextMargin.AddThemeConstantOverride("margin_bottom", 14);
        detailTextPanel.AddChild(detailTextMargin);

        VBoxContainer detail = new()
        {
            Name = "Detail",
            SizeFlagsHorizontal = SizeFlags.ExpandFill,
            SizeFlagsVertical = SizeFlags.ExpandFill
        };
        detailTextMargin.AddChild(detail);

        titleLabel = new Label
        {
            Name = "Title",
            AutowrapMode = TextServer.AutowrapMode.WordSmart
        };
        titleLabel.AddThemeFontSizeOverride("font_size", 22);
        ApplyLabelTextColor(titleLabel);
        detail.AddChild(titleLabel);

        ScrollContainer detailScroll = new()
        {
            Name = "DetailScroll",
            SizeFlagsHorizontal = SizeFlags.ExpandFill,
            SizeFlagsVertical = SizeFlags.ExpandFill
        };
        detail.AddChild(detailScroll);

        bodyLabel = new Label
        {
            Name = "Body",
            AutowrapMode = TextServer.AutowrapMode.WordSmart,
            SizeFlagsHorizontal = SizeFlags.ExpandFill
        };
        bodyLabel.AddThemeFontSizeOverride("font_size", 18);
        ApplyLabelTextColor(bodyLabel);
        detailScroll.AddChild(bodyLabel);

        paneCloseButton.MoveToFront();
    }

    public void OpenEncyclopedia()
    {
        FitToViewport();
        dimRect.Visible = true;
        dimRect.MoveToFront();
        panel.Visible = true;
        panel.MoveToFront();
        Autoloads.sceneSingleton.tutorialOverlayControl?.Notify(TutorialWaitCondition.OpenEncyclopedia);
    }

    public bool HandleBackPressed(Vector2 viewportPosition)
    {
        if (panel == null || !panel.Visible || !panel.GetGlobalRect().HasPoint(viewportPosition))
        {
            return false;
        }

        CloseEncyclopedia();
        return true;
    }

    private void CloseEncyclopedia()
    {
        if (dimRect != null)
        {
            dimRect.Visible = false;
        }

        if (panel != null)
        {
            panel.Visible = false;
        }
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
        ApplyPanelWidth();
    }

    private void ApplyPanelWidth()
    {
        if (panel == null)
        {
            return;
        }

        float availableWidth = Mathf.Max(0f, GetViewportRect().Size.X - 80f);
        float panelWidth = availableWidth > 0f ? Mathf.Min(PanelWidth, availableWidth) : PanelWidth;
        panel.OffsetLeft = -panelWidth * 0.5f;
        panel.OffsetRight = panelWidth * 0.5f;
    }

    private void ShowEntry(EncyclopediaEntry entry)
    {
        titleLabel.Text = $"{entry.Collection}：{entry.Title}";
        bodyLabel.Text = entry.Body;
    }

    private static TextureRect CreatePaneBackground(string name, string texturePath)
    {
        TextureRect background = new()
        {
            Name = name,
            Texture = ResourceLoader.Load<Texture2D>(texturePath),
            ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize,
            StretchMode = TextureRect.StretchModeEnum.Scale,
            MouseFilter = MouseFilterEnum.Ignore
        };
        background.SetAnchorsPreset(LayoutPreset.FullRect);
        return background;
    }

    private static void ApplyLabelTextColor(Label label)
    {
        label.AddThemeColorOverride("font_color", TextColor);
        label.AddThemeColorOverride("font_shadow_color", TransparentColor);
    }

    private static void ApplyButtonTextStyle(Button button)
    {
        button.AddThemeColorOverride("font_color", TextColor);
        button.AddThemeColorOverride("font_hover_color", TextColor);
        button.AddThemeColorOverride("font_pressed_color", TextColor);
        button.AddThemeColorOverride("font_hover_pressed_color", TextColor);
        button.AddThemeColorOverride("font_disabled_color", new Color(0f, 0f, 0f, 0.55f));
        button.AddThemeStyleboxOverride("normal", CreateTranslucentTextStyle(0.62f));
        button.AddThemeStyleboxOverride("hover", CreateTranslucentTextStyle(0.78f));
        button.AddThemeStyleboxOverride("pressed", CreateTranslucentTextStyle(0.88f));
        button.AddThemeStyleboxOverride("disabled", CreateTranslucentTextStyle(0.42f));
    }

    private static StyleBoxFlat CreateTranslucentTextStyle(float alpha)
    {
        return new StyleBoxFlat
        {
            BgColor = new Color(1f, 1f, 1f, alpha),
            BorderColor = new Color(1f, 1f, 1f, Mathf.Min(1f, alpha + 0.16f)),
            BorderWidthLeft = 1,
            BorderWidthTop = 1,
            BorderWidthRight = 1,
            BorderWidthBottom = 1
        };
    }

    private readonly struct EncyclopediaEntry
    {
        public string Collection { get; }
        public string Title { get; }
        public string Body { get; }

        public EncyclopediaEntry(string collection, string title, string body)
        {
            Collection = collection;
            Title = title;
            Body = body;
        }
    }

    private static List<EncyclopediaEntry> BuildEntries()
    {
        List<EncyclopediaEntry> builtEntries = new();

        foreach (CombatAreaId areaId in new[]
        {
            CombatAreaId.Yang,
            CombatAreaId.Yin,
            CombatAreaId.Qian,
            CombatAreaId.Dui,
            CombatAreaId.Li,
            CombatAreaId.Zhen,
            CombatAreaId.Xun,
            CombatAreaId.Kan,
            CombatAreaId.Gen,
            CombatAreaId.Kun
        })
        {
            builtEntries.Add(new EncyclopediaEntry(
                "区域修正",
                AreaDefinition.GetDisplayName(areaId),
                AreaDefinition.GetEncyclopediaText(areaId)));
        }

        foreach (string statusId in new[]
        {
            StatusCatalog.Dodge,
            StatusCatalog.Mark,
            StatusCatalog.Burn,
            StatusCatalog.Gale
        })
        {
            StatusDefinition status = StatusCatalog.Create(statusId);
            builtEntries.Add(new EncyclopediaEntry("状态效果", status.DisplayName, status.Description));
        }

        return builtEntries;
    }
}

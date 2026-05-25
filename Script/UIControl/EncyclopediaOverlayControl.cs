using Godot;
using System.Collections.Generic;

public partial class EncyclopediaOverlayControl : Control
{
    private const float PanelWidth = 1180f;
    private const float LeftPaneWidth = 360f;
    private const string LeftBackgroundPath = "res://Asset/ui image/Encyclopedia_left.png";
    private const string RightBackgroundPath = "res://Asset/ui image/Encyclopedia_right.png";
    private Control externalOpenButton;
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

        panel = new PanelContainer
        {
            Name = "EncyclopediaPanel",
            MouseFilter = MouseFilterEnum.Stop
        };
        panel.AddThemeStyleboxOverride("panel", new StyleBoxFlat
        {
            BgColor = new Color(0.94f, 0.95f, 0.97f, 1f),
            BorderColor = new Color(0.24f, 0.27f, 0.32f, 1f),
            BorderWidthLeft = 2,
            BorderWidthTop = 2,
            BorderWidthRight = 2,
            BorderWidthBottom = 2
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
            CustomMinimumSize = new Vector2(LeftPaneWidth, 520),
            SizeFlagsVertical = SizeFlags.ExpandFill,
            MouseFilter = MouseFilterEnum.Pass
        };
        body.AddChild(leftPane);
        leftPane.AddChild(CreatePaneBackground("EntryListBackground", LeftBackgroundPath));

        MarginContainer listMargin = new()
        {
            Name = "EntryListMargin",
            MouseFilter = MouseFilterEnum.Pass
        };
        listMargin.SetAnchorsPreset(LayoutPreset.FullRect);
        listMargin.AddThemeConstantOverride("margin_left", 16);
        listMargin.AddThemeConstantOverride("margin_right", 14);
        listMargin.AddThemeConstantOverride("margin_top", 18);
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
            entryButton.Pressed += () => ShowEntry(entry);
            entryList.AddChild(entryButton);
        }

        Control rightPane = new()
        {
            Name = "DetailPane",
            CustomMinimumSize = new Vector2(0, 520),
            SizeFlagsHorizontal = SizeFlags.ExpandFill,
            SizeFlagsVertical = SizeFlags.ExpandFill,
            MouseFilter = MouseFilterEnum.Pass
        };
        body.AddChild(rightPane);
        rightPane.AddChild(CreatePaneBackground("DetailBackground", RightBackgroundPath));

        MarginContainer detailMargin = new()
        {
            Name = "DetailMargin",
            MouseFilter = MouseFilterEnum.Pass
        };
        detailMargin.SetAnchorsPreset(LayoutPreset.FullRect);
        detailMargin.AddThemeConstantOverride("margin_left", 28);
        detailMargin.AddThemeConstantOverride("margin_right", 28);
        detailMargin.AddThemeConstantOverride("margin_top", 22);
        detailMargin.AddThemeConstantOverride("margin_bottom", 24);
        rightPane.AddChild(detailMargin);

        VBoxContainer detail = new()
        {
            Name = "Detail",
            SizeFlagsHorizontal = SizeFlags.ExpandFill,
            SizeFlagsVertical = SizeFlags.ExpandFill
        };
        detailMargin.AddChild(detail);

        titleLabel = new Label
        {
            Name = "Title",
            AutowrapMode = TextServer.AutowrapMode.WordSmart
        };
        titleLabel.AddThemeFontSizeOverride("font_size", 22);
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
        detailScroll.AddChild(bodyLabel);
    }

    public void OpenEncyclopedia()
    {
        FitToViewport();
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
            StatusCatalog.Shield,
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

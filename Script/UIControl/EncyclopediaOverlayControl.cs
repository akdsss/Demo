using Godot;
using System.Collections.Generic;

public partial class EncyclopediaOverlayControl : Control
{
    private const float PanelWidth = 720f;
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
        panel.AnchorLeft = 0.5f;
        panel.AnchorRight = 0.5f;
        panel.AnchorTop = 0.10f;
        panel.AnchorBottom = 0.90f;
        panel.OffsetLeft = -PanelWidth * 0.5f;
        panel.OffsetRight = PanelWidth * 0.5f;
        panel.OffsetTop = 0f;
        panel.OffsetBottom = 0f;
        AddChild(panel);

        VBoxContainer root = new()
        {
            Name = "Root"
        };
        panel.AddChild(root);

        HBoxContainer header = new()
        {
            Name = "Header"
        };
        root.AddChild(header);

        Label headerLabel = new()
        {
            Text = "百科"
        };
        header.AddChild(headerLabel);

        Button closeButton = new()
        {
            Text = "关闭"
        };
        closeButton.Pressed += () => panel.Visible = false;
        header.AddChild(closeButton);

        HBoxContainer body = new()
        {
            Name = "Body"
        };
        root.AddChild(body);

        ScrollContainer listScroll = new()
        {
            Name = "EntryListScroll",
            CustomMinimumSize = new Vector2(150, 260)
        };
        body.AddChild(listScroll);

        entryList = new VBoxContainer
        {
            Name = "EntryList"
        };
        listScroll.AddChild(entryList);

        foreach (EncyclopediaEntry entry in entries)
        {
            Button entryButton = new()
            {
                Text = $"{entry.Collection} / {entry.Title}",
                MouseFilter = MouseFilterEnum.Stop
            };
            entryButton.Pressed += () => ShowEntry(entry);
            entryList.AddChild(entryButton);
        }

        VBoxContainer detail = new()
        {
            Name = "Detail",
            CustomMinimumSize = new Vector2(260, 260)
        };
        body.AddChild(detail);

        titleLabel = new Label
        {
            Name = "Title",
            AutowrapMode = TextServer.AutowrapMode.WordSmart
        };
        detail.AddChild(titleLabel);

        ScrollContainer detailScroll = new()
        {
            Name = "DetailScroll",
            CustomMinimumSize = new Vector2(260, 220),
            SizeFlagsVertical = SizeFlags.ExpandFill
        };
        detail.AddChild(detailScroll);

        bodyLabel = new Label
        {
            Name = "Body",
            AutowrapMode = TextServer.AutowrapMode.WordSmart,
            SizeFlagsHorizontal = SizeFlags.ExpandFill
        };
        detailScroll.AddChild(bodyLabel);
    }

    public void OpenEncyclopedia()
    {
        FitToViewport();
        panel.Visible = true;
        panel.MoveToFront();
        Autoloads.sceneSingleton.tutorialOverlayControl?.Notify(TutorialWaitCondition.OpenEncyclopedia);
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

    private void ShowEntry(EncyclopediaEntry entry)
    {
        titleLabel.Text = $"{entry.Collection}：{entry.Title}";
        bodyLabel.Text = entry.Body;
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

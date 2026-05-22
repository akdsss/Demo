using Godot;
using System.Collections.Generic;

public partial class EncyclopediaOverlayControl : Control
{
    private Button openButton;
    private PanelContainer panel;
    private VBoxContainer entryList;
    private TextureRect detailIcon;
    private Label titleLabel;
    private Label bodyLabel;
    private readonly List<EncyclopediaEntry> entries = BuildEntries();

    public override void _Ready()
    {
        BuildOverlay();
        ShowEntry(entries[0]);
        panel.Visible = false;
    }

    public Control GetTutorialHighlightControl(TutorialHighlightTarget target)
    {
        return target == TutorialHighlightTarget.EncyclopediaButton ? openButton : null;
    }

    private void BuildOverlay()
    {
        SetAnchorsPreset(LayoutPreset.FullRect);
        MouseFilter = MouseFilterEnum.Ignore;

        openButton = new Button
        {
            Name = "EncyclopediaButton",
            Text = "百科",
            MouseFilter = MouseFilterEnum.Stop
        };
        openButton.AnchorLeft = 1;
        openButton.AnchorRight = 1;
        openButton.AnchorTop = 0;
        openButton.AnchorBottom = 0;
        openButton.OffsetLeft = -92;
        openButton.OffsetRight = -12;
        openButton.OffsetTop = 12;
        openButton.OffsetBottom = 44;
        openButton.Pressed += OpenEncyclopedia;
        AddChild(openButton);
        Autoloads.sceneSingleton?.uiSfxRouter?.RegisterButton(openButton);

        panel = new PanelContainer
        {
            Name = "EncyclopediaPanel",
            MouseFilter = MouseFilterEnum.Stop
        };
        panel.AnchorLeft = 0.54f;
        panel.AnchorRight = 0.96f;
        panel.AnchorTop = 0.12f;
        panel.AnchorBottom = 0.88f;
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
        Autoloads.sceneSingleton?.uiSfxRouter?.RegisterButton(closeButton);

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
            Autoloads.sceneSingleton?.uiSfxRouter?.RegisterButton(entryButton);
        }

        VBoxContainer detail = new()
        {
            Name = "Detail",
            CustomMinimumSize = new Vector2(260, 260)
        };
        body.AddChild(detail);

        detailIcon = new TextureRect
        {
            Name = "Icon",
            CustomMinimumSize = new Vector2(72, 72),
            StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered
        };
        detail.AddChild(detailIcon);

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

    private void OpenEncyclopedia()
    {
        panel.Visible = true;
        Autoloads.sceneSingleton.tutorialOverlayControl?.Notify(TutorialWaitCondition.OpenEncyclopedia);
    }

    private void ShowEntry(EncyclopediaEntry entry)
    {
        titleLabel.Text = $"{entry.Collection}：{entry.Title}";
        bodyLabel.Text = entry.Body;
        detailIcon.Texture = string.IsNullOrEmpty(entry.IconPath) ? null : ResourceLoader.Load<Texture2D>(entry.IconPath);
        detailIcon.Visible = detailIcon.Texture != null;
    }

    private readonly struct EncyclopediaEntry
    {
        public string Collection { get; }
        public string Title { get; }
        public string Body { get; }
        public string IconPath { get; }

        public EncyclopediaEntry(string collection, string title, string body, string iconPath = "")
        {
            Collection = collection;
            Title = title;
            Body = body;
            IconPath = iconPath;
        }
    }

    private static List<EncyclopediaEntry> BuildEntries()
    {
        List<EncyclopediaEntry> builtEntries = new()
        {
            new("区域修正", "十区域映射", "当前 Demo 使用现有 10 个棋盘格承载十区域：乾、兑、离、震、巽、坎、艮、坤、阴、阳。移动到不同格子即进入对应区域。")
        };

        foreach (AreaDefinition area in AreaDefinition.CreateBaguaDefaults())
        {
            builtEntries.Add(new EncyclopediaEntry("区域修正", area.DisplayName, area.EncyclopediaText, BattleAssetCatalog.GetAreaIconPath(area.AreaId)));
        }

        string[] statusIds =
        {
            StatusCatalog.Dodge,
            StatusCatalog.Mark,
            StatusCatalog.Shield,
            StatusCatalog.Burn,
            StatusCatalog.Gale,
            StatusCatalog.MoveBlocked,
            StatusCatalog.GenMeleeCarryover,
            StatusCatalog.KunMeleeDrainCarryover,
            StatusCatalog.Rage
        };

        foreach (string statusId in statusIds)
        {
            StatusDefinition status = StatusCatalog.Create(statusId);
            builtEntries.Add(new EncyclopediaEntry("状态效果", status.DisplayName, status.Description, BattleAssetCatalog.GetStatusIconPath(status.Id)));
        }

        return builtEntries;
    }
}

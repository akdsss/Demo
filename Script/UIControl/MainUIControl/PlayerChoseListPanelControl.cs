using Godot;
using System.Collections.Generic;
using System.Linq;

public partial class PlayerChoseListPanelControl : Panel
{
    private enum ChoiceMenuView
    {
        Hidden,
        PlayerSelect,
        CommandCategory,
        SkillList,
        CharacterTargetList
    }

    [Export] public PackedScene choseButtonPrefab;
    [Export] public VBoxContainer allChoseContent;
    private PlayerData currentPlayerData;
    private Label titleLabel;
    private Label detailLabel;
    private ChoiceMenuView currentView = ChoiceMenuView.Hidden;
    private List<PlayerCommandData> lastSkillList = new();
    private string lastSkillListTitle = string.Empty;
    private const float MenuButtonHeight = 42f;
    private const float PlayerSelectImageHeight = 500f;
    private const float PlayerSelectImageSeparation = 0f;
    private const float PlayerSelectFallbackImageWidth = 210f;
    private const string PlayerSelectOverlayName = "PlayerSelectOverlay";
    // Change this value to move character0A_SHOW.png; the other two images stay attached to its right edge.
    private static readonly Vector2 PlayerSelectFirstImagePosition = new(24f, 380f);
    private static readonly string[] PlayerSelectImagePaths =
    {
        "res://Asset/character/character0A_SHOW.png",
        "res://Asset/character/character0B_SHOW.png",
        "res://Asset/character/character0C_SHOW.png"
    };
    private static readonly Color ChoicePanelBackgroundColor = new(0.6958008f, 0.7204437f, 0.7421875f, 1f);
    private static readonly Color TransparentPanelColor = new(0f, 0f, 0f, 0f);
    private static readonly Color PlayerImageNormalColor = Colors.White;
    private static readonly Color PlayerImageHoverColor = new(1.35f, 1.35f, 1.35f, 1f);
    private static readonly Color DisabledTargetColor = new(0.45f, 0.45f, 0.45f, 1f);
    private static readonly Vector2 TargetPreviewSize = new(300f, 420f);
    private static readonly Vector2 TargetPreviewBottomRightMargin = new(28f, 190f);
    private Control playerSelectOverlay;
    private TextureRect targetPreviewImage;

    public void SetChoseListPanel(List<PlayerCommandData> playerCommandDataList)
    {
        NotifyCommandSubmenuOpen();
        currentPlayerData = Autoloads.sceneSingleton?.battleManager?.eventManager?.currentMainPlayer;
        ShowSkillList(playerCommandDataList ?? new List<PlayerCommandData>(), "技能");
    }

    public void ShowPlayerSelectPanel(List<PlayerData> playerList)
    {
        NotifyCommandSubmenuOpen();
        HideTargetPreview();
        List<Texture2D> textures = LoadPlayerSelectTextures();
        PreparePlayerSelectOverlay();
        if (playerSelectOverlay == null)
        {
            return;
        }

        PubTool.instance.ClearChildren(allChoseContent);
        Visible = false;
        currentPlayerData = null;
        currentView = ChoiceMenuView.PlayerSelect;
        ClearCachedSkillList();
        PubTool.instance.ClearChildren(playerSelectOverlay);

        HBoxContainer imageRow = new()
        {
            CustomMinimumSize = new Vector2(CalculatePlayerSelectPanelWidth(textures), PlayerSelectImageHeight),
            SizeFlagsHorizontal = Control.SizeFlags.ShrinkBegin,
            SizeFlagsVertical = Control.SizeFlags.ShrinkBegin,
            MouseFilter = Control.MouseFilterEnum.Pass
        };
        imageRow.AnchorLeft = 0f;
        imageRow.AnchorTop = 0f;
        imageRow.AnchorRight = 0f;
        imageRow.AnchorBottom = 0f;
        imageRow.OffsetLeft = PlayerSelectFirstImagePosition.X;
        imageRow.OffsetTop = PlayerSelectFirstImagePosition.Y;
        imageRow.OffsetRight = PlayerSelectFirstImagePosition.X + CalculatePlayerSelectPanelWidth(textures);
        imageRow.OffsetBottom = PlayerSelectFirstImagePosition.Y + PlayerSelectImageHeight;
        imageRow.AddThemeConstantOverride("separation", (int)PlayerSelectImageSeparation);
        playerSelectOverlay.AddChild(imageRow);

        List<PlayerData> players = playerList ?? new List<PlayerData>();
        for (int index = 0; index < PlayerSelectImagePaths.Length; index++)
        {
            PlayerData player = index < players.Count ? players[index] : null;
            Texture2D texture = index < textures.Count ? textures[index] : null;
            bool selectable = IsSelectablePlayer(player);
            imageRow.AddChild(CreatePlayerImageButton(player, texture, selectable, index));
        }
    }

    public void ShowCommandCategoryPanel(PlayerData playerData)
    {
        NotifyCommandSubmenuOpen();
        HideTargetPreview();
        HidePlayerSelectOverlay();
        Autoloads.sceneSingleton?.cmdQueueUIControl?.ClearHoveredTimelineRow();
        PrepareCompactPanel();
        PubTool.instance.ClearChildren(allChoseContent);
        currentPlayerData = playerData;
        currentView = ChoiceMenuView.CommandCategory;
        ClearCachedSkillList();
        Visible = true;
        AddTitle($"当前角色：{playerData?.characterName ?? "未选择"}");

        List<PlayerCommandData> commands = playerData?.playerCommandDataList?
            .Where(command => command != null)
            .ToList() ?? new List<PlayerCommandData>();

        string[] categoryOrder = { "近战", "位移", "远程", "特殊" };
        foreach (string category in categoryOrder)
        {
            List<PlayerCommandData> categoryCommands = commands
                .Where(command => GetCategory(command) == category)
                .ToList();
            if (categoryCommands.Count == 0)
            {
                continue;
            }

            Button categoryButton = new()
            {
                Text = category,
                CustomMinimumSize = new Vector2(0, MenuButtonHeight),
                SizeFlagsHorizontal = Control.SizeFlags.ExpandFill
            };
            categoryButton.Pressed += () => ShowSkillList(categoryCommands, category);
            allChoseContent.AddChild(categoryButton);
        }

        AddDetail("点击分类选择技能。", 72);
    }

    public void ShowSkillDetail(PlayerCommandData playerCommandData)
    {
        if (detailLabel == null || playerCommandData == null)
        {
            return;
        }

        SkillDefinition skill = SkillDefinition.FromCommandData(playerCommandData);
        CharacterData source = currentPlayerData ?? Autoloads.sceneSingleton?.battleManager?.eventManager?.currentMainPlayer;
        string description = SkillDescriptionFormatter.Format(skill, source);
        detailLabel.Text = $"优先级：{CombatAreaRules.ToDisplayPriority(skill.Priority)}\nMP：{skill.MpCost}\n效果：{description}";
    }

    public void ShowCharacterTargetList(PlayerCommandData playerCommandData, List<CharacterData> targetList, string title)
    {
        NotifyCommandSubmenuOpen();
        HideTargetPreview();
        HidePlayerSelectOverlay();
        PrepareCompactPanel();
        PubTool.instance.ClearChildren(allChoseContent);
        currentView = ChoiceMenuView.CharacterTargetList;
        Visible = true;
        AddTitle($"{title}：{playerCommandData?.commandName ?? "未选择技能"}");

        foreach (CharacterData target in targetList ?? new List<CharacterData>())
        {
            Button targetButton = new()
            {
                Text = BuildTargetButtonText(target),
                CustomMinimumSize = new Vector2(0, MenuButtonHeight),
                SizeFlagsHorizontal = Control.SizeFlags.ExpandFill
            };

            bool selectable = IsSelectableTarget(target);
            targetButton.Disabled = !selectable;
            if (!selectable)
            {
                targetButton.Modulate = DisabledTargetColor;
            }
            else
            {
                targetButton.Pressed += () => ConfirmCharacterTarget(playerCommandData, target);
                targetButton.MouseEntered += () => ShowTargetPreview(target);
                targetButton.MouseExited += HideTargetPreview;
            }

            allChoseContent.AddChild(targetButton);
        }

        Button backButton = new()
        {
            Text = "返回技能分类",
            CustomMinimumSize = new Vector2(0, MenuButtonHeight),
            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill
        };
        backButton.Pressed += () => ShowCommandCategoryPanel(currentPlayerData);
        allChoseContent.AddChild(backButton);

        AddDetail("选择目标后，再长按时间轴空白时点。已战败目标不可选。", 90);
    }

    public bool HandleBackPressed()
    {
        if (!Visible && !IsPlayerSelectOverlayVisible())
        {
            return false;
        }

        Autoloads.sceneSingleton?.cmdQueueUIControl?.SwitchOffPlayerCommandSet();
        switch (currentView)
        {
            case ChoiceMenuView.CharacterTargetList:
                ClearPendingCommandSelection(false);
                ShowSkillList(lastSkillList, lastSkillListTitle);
                Autoloads.sceneSingleton?.cmdQueueUIControl?.ShowCommandDetail("返回", "已返回技能列表。");
                return true;
            case ChoiceMenuView.SkillList:
                ClearPendingCommandSelection(false);
                ShowCommandCategoryPanel(currentPlayerData);
                Autoloads.sceneSingleton?.cmdQueueUIControl?.ShowCommandDetail("返回", "已返回技能分类。");
                return true;
            case ChoiceMenuView.CommandCategory:
                ClearPendingCommandSelection(true);
                ShowPlayerSelectPanel(Autoloads.sceneSingleton?.battleManager?.battlePlayerDataList);
                Autoloads.sceneSingleton?.cmdQueueUIControl?.ShowCommandDetail("返回", "已返回角色选择。");
                return true;
            case ChoiceMenuView.PlayerSelect:
                ClearPendingCommandSelection(true);
                HidePanel();
                Autoloads.sceneSingleton?.cmdQueueUIControl?.ShowCommandDetail("返回", "已关闭指令菜单。");
                return true;
            default:
                HidePanel();
                return true;
        }
    }

    private void ShowSkillList(List<PlayerCommandData> playerCommandDataList, string title)
    {
        NotifyCommandSubmenuOpen();
        HideTargetPreview();
        HidePlayerSelectOverlay();
        PrepareCompactPanel();
        PubTool.instance.ClearChildren(allChoseContent);
        currentView = ChoiceMenuView.SkillList;
        lastSkillList = playerCommandDataList?
            .Where(command => command != null)
            .ToList() ?? new List<PlayerCommandData>();
        lastSkillListTitle = title ?? string.Empty;
        Visible = true;
        AddTitle($"{title}技能");

        foreach (PlayerCommandData playerCommandData in playerCommandDataList)
        {
            Button choseButton = (Button)choseButtonPrefab.Instantiate();
            allChoseContent.AddChild(choseButton);
            choseButton.Text = playerCommandData.commandName;
            choseButton.CustomMinimumSize = new Vector2(0, MenuButtonHeight);
            choseButton.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
            ((PlayerActionButtonControl)choseButton).playerCommandData = playerCommandData;
        }

        AddDetail("悬停查看详情；单击后选择目标，再长按时间轴空白时点。", 90);
    }

    private void AddTitle(string text)
    {
        titleLabel = new Label
        {
            Text = text,
            AutowrapMode = TextServer.AutowrapMode.WordSmart,
            CustomMinimumSize = new Vector2(0, 34)
		};
        titleLabel.AddThemeFontSizeOverride("font_size", 18);
        allChoseContent.AddChild(titleLabel);
    }

    private void AddDetail(string text, float height)
    {
        detailLabel = new Label
        {
            Text = text,
            AutowrapMode = TextServer.AutowrapMode.WordSmart,
            CustomMinimumSize = new Vector2(0, height)
        };
        detailLabel.AddThemeFontSizeOverride("font_size", 16);
        allChoseContent.AddChild(detailLabel);
    }

    private void PrepareCompactPanel()
    {
        HidePlayerSelectOverlay();
        SelfModulate = Colors.White;
        SetChoicePanelBackgroundVisible(true);
        ResetCompactPanelRect();
        SetAllChoiceContentAnchors(0.1f, 0.025f, 0.9f, 1f);
        CustomMinimumSize = new Vector2(0, 260);
        SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        SizeFlagsVertical = Control.SizeFlags.Fill;
        MouseFilter = Control.MouseFilterEnum.Pass;
        if (allChoseContent != null)
        {
            allChoseContent.AddThemeConstantOverride("separation", 8);
        }
    }

    private void PreparePlayerSelectOverlay()
    {
        EnsurePlayerSelectOverlay();
        if (playerSelectOverlay == null)
        {
            return;
        }

        playerSelectOverlay.Visible = true;
        playerSelectOverlay.MoveToFront();
    }

    private TextureButton CreatePlayerImageButton(PlayerData player, Texture2D texture, bool selectable, int imageIndex)
    {
        Vector2 imageSize = GetPlayerSelectImageSize(texture);
        TextureButton imageButton = new()
        {
            Name = $"PlayerImageButton{imageIndex}",
            TextureNormal = texture,
            TexturePressed = texture,
            TextureHover = texture,
            TextureDisabled = texture,
            IgnoreTextureSize = true,
            StretchMode = TextureButton.StretchModeEnum.KeepAspectCentered,
            CustomMinimumSize = imageSize,
            SizeFlagsHorizontal = Control.SizeFlags.ShrinkBegin,
            SizeFlagsVertical = Control.SizeFlags.ShrinkBegin,
            Disabled = !selectable,
            TooltipText = BuildPlayerButtonText(player),
            FocusMode = Control.FocusModeEnum.None,
            Modulate = selectable ? PlayerImageNormalColor : DisabledTargetColor
        };

        if (selectable)
        {
            imageButton.Pressed += () => ConfirmPlayerSelection(player);
            imageButton.MouseEntered += () => OnPlayerImageMouseEntered(imageButton, player);
            imageButton.MouseExited += () => OnPlayerImageMouseExited(imageButton, player);
        }

        return imageButton;
    }

    private static void OnPlayerImageMouseEntered(TextureButton imageButton, PlayerData player)
    {
        if (imageButton != null)
        {
            imageButton.Modulate = PlayerImageHoverColor;
        }

        Autoloads.sceneSingleton?.cmdQueueUIControl?.SetHoveredTimelineRow(player);
    }

    private static void OnPlayerImageMouseExited(TextureButton imageButton, PlayerData player)
    {
        if (imageButton != null)
        {
            imageButton.Modulate = PlayerImageNormalColor;
        }

        Autoloads.sceneSingleton?.cmdQueueUIControl?.ClearHoveredTimelineRow(player);
    }

    private static List<Texture2D> LoadPlayerSelectTextures()
    {
        List<Texture2D> textures = new();
        foreach (string imagePath in PlayerSelectImagePaths)
        {
            textures.Add(LoadTexture(imagePath));
        }

        return textures;
    }

    private static Texture2D LoadTexture(string resourcePath)
    {
        Texture2D texture = ResourceLoader.Load<Texture2D>(resourcePath);
        if (texture != null)
        {
            return texture;
        }

        Image image = Image.LoadFromFile(ProjectSettings.GlobalizePath(resourcePath));
        return image == null ? null : ImageTexture.CreateFromImage(image);
    }

    private static float CalculatePlayerSelectPanelWidth(List<Texture2D> textures)
    {
        float width = 0f;
        int imageCount = PlayerSelectImagePaths.Length;
        for (int index = 0; index < imageCount; index++)
        {
            Texture2D texture = index < textures.Count ? textures[index] : null;
            width += GetPlayerSelectImageSize(texture).X;
            if (index < imageCount - 1)
            {
                width += PlayerSelectImageSeparation;
            }
        }

        return width;
    }

    private static Vector2 GetPlayerSelectImageSize(Texture2D texture)
    {
        if (texture == null || texture.GetHeight() <= 0)
        {
            return new Vector2(PlayerSelectFallbackImageWidth, PlayerSelectImageHeight);
        }

        float width = PlayerSelectImageHeight * texture.GetWidth() / texture.GetHeight();
        return new Vector2(width, PlayerSelectImageHeight);
    }

    private void ResetCompactPanelRect()
    {
        AnchorLeft = 0f;
        AnchorTop = 0f;
        AnchorRight = 1f;
        AnchorBottom = 0f;
        OffsetLeft = 0f;
        OffsetTop = 0f;
        OffsetRight = 0f;
        OffsetBottom = 260f;
    }

    private void SetChoicePanelBackgroundVisible(bool visible)
    {
        ColorRect background = GetNodeOrNull<ColorRect>("ColorRect");
        if (background != null)
        {
            background.Color = visible ? Colors.White : TransparentPanelColor;
            background.SelfModulate = visible ? ChoicePanelBackgroundColor : Colors.White;
        }
    }

    private void SetAllChoiceContentAnchors(float left, float top, float right, float bottom)
    {
        if (allChoseContent == null)
        {
            return;
        }

        allChoseContent.AnchorLeft = left;
        allChoseContent.AnchorTop = top;
        allChoseContent.AnchorRight = right;
        allChoseContent.AnchorBottom = bottom;
        allChoseContent.OffsetLeft = 0f;
        allChoseContent.OffsetTop = 0f;
        allChoseContent.OffsetRight = 0f;
        allChoseContent.OffsetBottom = 0f;
    }

    private void EnsurePlayerSelectOverlay()
    {
        if (playerSelectOverlay != null)
        {
            return;
        }

        Control screenPanel = GetTree()?.CurrentScene?.GetNodeOrNull<Control>("MainUi/Panel");
        if (screenPanel == null)
        {
            return;
        }

        playerSelectOverlay = screenPanel.GetNodeOrNull<Control>(PlayerSelectOverlayName);
        if (playerSelectOverlay == null)
        {
            playerSelectOverlay = new Control
            {
                Name = PlayerSelectOverlayName,
                MouseFilter = Control.MouseFilterEnum.Pass,
                Visible = false
            };
            screenPanel.AddChild(playerSelectOverlay);
        }

        playerSelectOverlay.SetAnchorsPreset(LayoutPreset.FullRect);
        playerSelectOverlay.OffsetLeft = 0f;
        playerSelectOverlay.OffsetTop = 0f;
        playerSelectOverlay.OffsetRight = 0f;
        playerSelectOverlay.OffsetBottom = 0f;
    }

    private bool IsPlayerSelectOverlayVisible()
    {
        return currentView == ChoiceMenuView.PlayerSelect &&
            playerSelectOverlay != null &&
            playerSelectOverlay.Visible;
    }

    private void HidePlayerSelectOverlay()
    {
        if (playerSelectOverlay != null)
        {
            PubTool.instance.ClearChildren(playerSelectOverlay);
            playerSelectOverlay.Visible = false;
        }
    }

    private static bool IsSelectablePlayer(PlayerData player)
    {
        return player != null &&
            player.CanPrepareActionThisRound();
    }

    private static string BuildPlayerButtonText(PlayerData player)
    {
        if (player == null)
        {
            return "未知角色（不可选）";
        }

        string suffix = IsSelectablePlayer(player) ? string.Empty : "（不可行动）";
        return $"{player.characterName}  行动 {player.currentRestActionTimes}  HP {Mathf.RoundToInt(player.hp)}/{Mathf.RoundToInt(player.maxHp)}{suffix}";
    }

    private void ConfirmPlayerSelection(PlayerData player)
    {
        if (!IsSelectablePlayer(player))
        {
            return;
        }

        EventManager eventManager = Autoloads.sceneSingleton?.battleManager?.eventManager;
        if (eventManager == null)
        {
            return;
        }

        Autoloads.sceneSingleton?.cmdQueueUIControl?.ClearHoveredTimelineRow(player);
        eventManager.currentMainPlayer = player;
        eventManager.currentMainPlayerCommand = null;
        eventManager.currentTargetAreaId = CombatAreaId.Unknown;
        eventManager.moveEventInfo = null;
        eventManager.damageEventInfo = null;
        ShowCommandCategoryPanel(player);
        Autoloads.sceneSingleton?.cmdQueueUIControl?.ShowCommandDetail(
            "设置指令",
            $"当前角色：{player.characterName}\n先选择技能分类，再选择技能和目标。");
    }

    private static bool IsSelectableTarget(CharacterData target)
    {
        return target != null &&
            target.characterBattleState == CharacterBattleState.ALIVE &&
            target.hp > 0;
    }

    private static string BuildTargetButtonText(CharacterData target)
    {
        if (target == null)
        {
            return "未知目标（不可选）";
        }

        string suffix = IsSelectableTarget(target) ? string.Empty : "（战败）";
        return $"{target.characterName}  HP {Mathf.RoundToInt(target.hp)}/{Mathf.RoundToInt(target.maxHp)}{suffix}";
    }

    private void ConfirmCharacterTarget(PlayerCommandData playerCommandData, CharacterData target)
    {
        HideTargetPreview();
        SceneSingleton sceneSingleton = Autoloads.sceneSingleton;
        EventManager eventManager = sceneSingleton?.battleManager?.eventManager;
        if (eventManager == null || playerCommandData == null || target == null)
        {
            return;
        }

        eventManager.currentMainPlayerCommand = playerCommandData;
        eventManager.currentTargetAreaId = target.ResolveCurrentAreaId();
        eventManager.moveEventInfo = null;
        eventManager.damageEventInfo = new DamageEventInfo
        {
            damageSourceCharacter = eventManager.currentMainPlayer,
            damageTargetCharacter = target
        };
        eventManager.currentMainEnemy = target as EnemyData;

        SkillDefinition skill = SkillDefinition.FromCommandData(playerCommandData);
        if (skill.RequiresSecondaryAreaTargetSelection)
        {
            sceneSingleton.areaTargetMenuControl?.ShowForCommand(
                playerCommandData,
                eventManager.currentMainPlayer,
                target);
            sceneSingleton.cmdQueueUIControl?.ShowCommandDetail(
                "目标对象",
                $"已选择 {target.characterName}。请选择技能的目标区域。");
            return;
        }

        sceneSingleton.enemyCharacterHeadListUIControl?.ChangeToUninteractable();
        sceneSingleton.cmdQueueUIControl?.SwitchOnPlayerCommandSet();
        sceneSingleton.cmdQueueUIControl?.ShowCommandDetail(
            "目标对象",
            $"已选择 {target.characterName}。请选择空白时点并长按 0.5 秒确认。");
    }

    private static string GetCategory(PlayerCommandData commandData)
    {
        SkillDefinition skill = SkillDefinition.FromCommandData(commandData);
        if (skill.HasTag(SkillTag.Melee))
        {
            return "近战";
        }
        if (skill.HasTag(SkillTag.Move))
        {
            return "位移";
        }
        if (skill.HasTag(SkillTag.Ranged))
        {
            return "远程";
        }

        return "特殊";
    }

    private void HidePanel()
    {
        HideTargetPreview();
        HidePlayerSelectOverlay();
        Autoloads.sceneSingleton?.cmdQueueUIControl?.ClearHoveredTimelineRow();
        NotifyCommandSubmenuClosed();
        Visible = false;
        currentView = ChoiceMenuView.Hidden;
        currentPlayerData = null;
        ClearCachedSkillList();
    }

    public void DismissPanel()
    {
        HidePanel();
    }

    private void ShowTargetPreview(CharacterData target)
    {
        Texture2D previewTexture = target?.characterHeadImage ?? Autoloads.sceneSingleton?.defaultCharacterImage;
        if (previewTexture == null)
        {
            HideTargetPreview();
            return;
        }

        EnsureTargetPreviewImage();
        if (targetPreviewImage == null)
        {
            return;
        }

        targetPreviewImage.Texture = previewTexture;
        targetPreviewImage.Visible = true;
        targetPreviewImage.MoveToFront();
    }

    private void HideTargetPreview()
    {
        if (targetPreviewImage != null)
        {
            targetPreviewImage.Visible = false;
        }
    }

    private void EnsureTargetPreviewImage()
    {
        if (targetPreviewImage != null)
        {
            return;
        }

        Control screenPanel = GetTree()?.CurrentScene?.GetNodeOrNull<Control>("MainUi/Panel");
        if (screenPanel == null)
        {
            return;
        }

        targetPreviewImage = screenPanel.GetNodeOrNull<TextureRect>("TargetHoverPreviewImage");
        if (targetPreviewImage == null)
        {
            targetPreviewImage = new TextureRect
            {
                Name = "TargetHoverPreviewImage",
                ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize,
                StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered,
                MouseFilter = Control.MouseFilterEnum.Ignore,
                Visible = false
            };
            screenPanel.AddChild(targetPreviewImage);
        }

        targetPreviewImage.CustomMinimumSize = TargetPreviewSize;
        targetPreviewImage.AnchorLeft = 1f;
        targetPreviewImage.AnchorTop = 1f;
        targetPreviewImage.AnchorRight = 1f;
        targetPreviewImage.AnchorBottom = 1f;
        targetPreviewImage.OffsetLeft = -TargetPreviewBottomRightMargin.X - TargetPreviewSize.X;
        targetPreviewImage.OffsetTop = -TargetPreviewBottomRightMargin.Y - TargetPreviewSize.Y;
        targetPreviewImage.OffsetRight = -TargetPreviewBottomRightMargin.X;
        targetPreviewImage.OffsetBottom = -TargetPreviewBottomRightMargin.Y;
    }

    private static void NotifyCommandSubmenuOpen()
    {
        Autoloads.sceneSingleton?.cmdQueueUIControl?.SetCommandSubmenuOpen(true);
    }

    private static void NotifyCommandSubmenuClosed()
    {
        Autoloads.sceneSingleton?.cmdQueueUIControl?.SetCommandSubmenuOpen(false);
    }

    private void ClearCachedSkillList()
    {
        lastSkillList.Clear();
        lastSkillListTitle = string.Empty;
    }

    private static void ClearPendingCommandSelection(bool clearCurrentPlayer)
    {
        EventManager eventManager = Autoloads.sceneSingleton?.battleManager?.eventManager;
        if (eventManager == null)
        {
            return;
        }

        if (clearCurrentPlayer)
        {
            eventManager.currentMainPlayer = null;
        }

        eventManager.currentMainPlayerCommand = null;
        eventManager.currentMainEnemy = null;
        eventManager.currentTargetAreaId = CombatAreaId.Unknown;
        eventManager.moveEventInfo = null;
        eventManager.damageEventInfo = null;
    }
}

using Godot;
using System.Collections.Generic;
using System.Linq;

public partial class PlayerChoseListPanelControl : Panel
{
    [Export] public PackedScene choseButtonPrefab;
    [Export] public VBoxContainer allChoseContent;
    private PlayerData currentPlayerData;
    private Label titleLabel;
    private Label detailLabel;
    private const float MenuButtonHeight = 26f;
    private static readonly Color DisabledTargetColor = new(0.45f, 0.45f, 0.45f, 1f);

    public void SetChoseListPanel(List<PlayerCommandData> playerCommandDataList)
    {
        currentPlayerData = Autoloads.sceneSingleton?.battleManager?.eventManager?.currentMainPlayer;
        ShowSkillList(playerCommandDataList ?? new List<PlayerCommandData>(), "技能");
    }

    public void ShowCommandCategoryPanel(PlayerData playerData)
    {
        PrepareCompactPanel();
        PubTool.instance.ClearChildren(allChoseContent);
        currentPlayerData = playerData;
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

        AddDetail("点击分类选择技能。", 42);
    }

    public void ShowSkillDetail(PlayerCommandData playerCommandData)
    {
        if (detailLabel == null || playerCommandData == null)
        {
            return;
        }

        SkillDefinition skill = SkillDefinition.FromCommandData(playerCommandData);
        detailLabel.Text = $"优先级：{skill.Priority}\nMP：{skill.MpCost}\n效果：{playerCommandData.commandDescription}";
    }

    public void ShowCharacterTargetList(PlayerCommandData playerCommandData, List<CharacterData> targetList, string title)
    {
        PrepareCompactPanel();
        PubTool.instance.ClearChildren(allChoseContent);
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

        AddDetail("选择目标后，再长按时间轴空白时点。已战败目标不可选。", 54);
    }

    private void ShowSkillList(List<PlayerCommandData> playerCommandDataList, string title)
    {
        PrepareCompactPanel();
        PubTool.instance.ClearChildren(allChoseContent);
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

        AddDetail("悬停查看详情；单击后选择目标，再长按时间轴空白时点。", 54);
    }

    private void AddTitle(string text)
    {
        titleLabel = new Label
        {
            Text = text,
            AutowrapMode = TextServer.AutowrapMode.WordSmart,
            CustomMinimumSize = new Vector2(0, 20)
        };
        titleLabel.AddThemeFontSizeOverride("font_size", 13);
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
        detailLabel.AddThemeFontSizeOverride("font_size", 12);
        allChoseContent.AddChild(detailLabel);
    }

    private void PrepareCompactPanel()
    {
        CustomMinimumSize = new Vector2(0, 150);
        SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        SizeFlagsVertical = Control.SizeFlags.Fill;
        if (allChoseContent != null)
        {
            allChoseContent.AddThemeConstantOverride("separation", 5);
        }
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

        sceneSingleton.enemyCharacterHeadListUIControl?.ChangeToUninteractable();
        sceneSingleton.cmdQueueUIControl?.SwitchOnPlayerCommandSet();
        sceneSingleton.cmdQueueUIControl?.ShowCommandDetail(
            "目标对象",
            $"已选择 {target.characterName}。请选择空白时点并长按 1.2 秒确认。");
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
}

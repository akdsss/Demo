using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class GrowthRewardOverlayControl : Control
{
    private ColorRect dimRect;
    private PanelContainer panel;
    private Label summaryLabel;
    private VBoxContainer rewardList;
    private Button continueButton;

    public override void _Ready()
    {
        BuildOverlay();
        HideReward();
    }

    public void ShowReward(GrowthRewardData rewardData, IEnumerable<CharacterData> fallbackCharacters = null)
    {
        ClearRewardRows();

        List<CharacterData> fallbackList = fallbackCharacters?
            .Where(character => character != null)
            .ToList() ?? new List<CharacterData>();
        int rewardEntryCount = rewardData?.EntryCount ?? 0;
        int entryCount = Math.Max(rewardEntryCount, fallbackList.Count);

        summaryLabel.Text = string.IsNullOrEmpty(rewardData?.uiSummaryText)
            ? "教学关已完成。后续正式奖励会从成长配置读取。"
            : rewardData.uiSummaryText;

        for (int i = 0; i < entryCount; i++)
        {
            CharacterData character = rewardData?.GetCharacter(i) ?? (i < fallbackList.Count ? fallbackList[i] : null);
            string statChange = rewardData?.GetStatChangeText(i);
            string unlockSkill = rewardData?.GetUnlockSkillText(i);
            AddRewardRow(character, statChange, unlockSkill);
        }

        Visible = true;
        MoveToFront();
    }

    public Control GetTutorialHighlightControl(TutorialHighlightTarget target)
    {
        return target == TutorialHighlightTarget.GrowthPanel ? panel : null;
    }

    private void BuildOverlay()
    {
        SetAnchorsPreset(LayoutPreset.FullRect);
        MouseFilter = MouseFilterEnum.Stop;

        dimRect = new ColorRect
        {
            Name = "GrowthDim",
            Color = new Color(0, 0, 0, 0.50f),
            MouseFilter = MouseFilterEnum.Stop
        };
        dimRect.SetAnchorsPreset(LayoutPreset.FullRect);
        AddChild(dimRect);

        panel = new PanelContainer
        {
            Name = "GrowthPanel",
            MouseFilter = MouseFilterEnum.Stop
        };
        panel.AnchorLeft = 0.22f;
        panel.AnchorRight = 0.78f;
        panel.AnchorTop = 0.12f;
        panel.AnchorBottom = 0.88f;
        AddChild(panel);

        MarginContainer margin = new()
        {
            Name = "GrowthMargin"
        };
        margin.AddThemeConstantOverride("margin_left", 18);
        margin.AddThemeConstantOverride("margin_right", 18);
        margin.AddThemeConstantOverride("margin_top", 16);
        margin.AddThemeConstantOverride("margin_bottom", 16);
        panel.AddChild(margin);

        VBoxContainer content = new()
        {
            Name = "GrowthContent"
        };
        margin.AddChild(content);

        Label titleLabel = new()
        {
            Name = "TitleLabel",
            Text = "胜利结算",
            HorizontalAlignment = HorizontalAlignment.Center
        };
        content.AddChild(titleLabel);

        summaryLabel = new Label
        {
            Name = "SummaryLabel",
            AutowrapMode = TextServer.AutowrapMode.WordSmart
        };
        content.AddChild(summaryLabel);

        ScrollContainer scrollContainer = new()
        {
            Name = "RewardScroll",
            SizeFlagsVertical = SizeFlags.ExpandFill
        };
        content.AddChild(scrollContainer);

        rewardList = new VBoxContainer
        {
            Name = "RewardList"
        };
        scrollContainer.AddChild(rewardList);

        continueButton = new Button
        {
            Name = "ContinueButton",
            Text = "继续"
        };
        continueButton.Pressed += HideReward;
        content.AddChild(continueButton);
    }

    private void AddRewardRow(CharacterData character, string statChange, string unlockSkill)
    {
        PanelContainer rowPanel = new()
        {
            Name = "RewardRow"
        };
        rewardList.AddChild(rowPanel);

        HBoxContainer row = new()
        {
            Name = "RewardRowContent"
        };
        rowPanel.AddChild(row);

        TextureRect portrait = new()
        {
            Name = "Portrait",
            CustomMinimumSize = new Vector2(56, 56),
            Texture = character?.characterHeadImage ?? Autoloads.sceneSingleton?.defaultCharacterImage
        };
        row.AddChild(portrait);

        VBoxContainer texts = new()
        {
            Name = "RewardTexts",
            SizeFlagsHorizontal = SizeFlags.ExpandFill
        };
        row.AddChild(texts);

        Label nameLabel = new()
        {
            Name = "NameLabel",
            Text = character?.characterName ?? "未指定角色"
        };
        texts.AddChild(nameLabel);

        Label statLabel = new()
        {
            Name = "StatLabel",
            Text = string.IsNullOrEmpty(statChange) ? "属性变化：暂无" : $"属性变化：{statChange}",
            AutowrapMode = TextServer.AutowrapMode.WordSmart
        };
        texts.AddChild(statLabel);

        Label skillLabel = new()
        {
            Name = "SkillLabel",
            Text = string.IsNullOrEmpty(unlockSkill) ? "新技能：暂无" : $"新技能：{unlockSkill}",
            AutowrapMode = TextServer.AutowrapMode.WordSmart
        };
        texts.AddChild(skillLabel);
    }

    private void ClearRewardRows()
    {
        if (rewardList == null)
        {
            return;
        }

        foreach (Node child in rewardList.GetChildren())
        {
            child.QueueFree();
        }
    }

    private void HideReward()
    {
        Visible = false;
    }
}

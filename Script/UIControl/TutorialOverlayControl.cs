using Godot;
using System.Collections.Generic;
using System.Linq;

public partial class TutorialOverlayControl : Control
{
    private ColorRect dimRect;
    private ColorRect highlightRect;
    private PanelContainer promptPanel;
    private Label titleLabel;
    private Label promptLabel;
    private Label waitLabel;
    private Button continueButton;
    private Button skipButton;
    private List<TutorialStepData> steps = new();
    private int currentStepIndex = -1;
    private TutorialStepData currentStep;

    public bool IsRunning { get; private set; }

    public override void _Ready()
    {
        BuildOverlay();
        HideOverlay();
    }

    public override void _Process(double delta)
    {
        if (IsRunning)
        {
            UpdateHighlight();
        }
    }

    public void StartTutorial(LevelData levelData)
    {
        steps = levelData?.tutorialStepDataArray?
            .Where(step => step != null)
            .OrderBy(step => step.orderIndex)
            .ToList() ?? new List<TutorialStepData>();

        if (steps.Count == 0)
        {
            HideOverlay();
            return;
        }

        IsRunning = true;
        currentStepIndex = -1;
        AdvanceToNextStep();
    }

    public void Notify(TutorialWaitCondition waitCondition)
    {
        if (!IsRunning || currentStep == null || currentStep.waitCondition != waitCondition)
        {
            return;
        }

        AdvanceToNextStep();
    }

    private void BuildOverlay()
    {
        SetAnchorsPreset(LayoutPreset.FullRect);
        MouseFilter = MouseFilterEnum.Ignore;

        dimRect = new ColorRect
        {
            Name = "TutorialDim",
            Color = new Color(0, 0, 0, 0.42f),
            MouseFilter = MouseFilterEnum.Ignore
        };
        dimRect.SetAnchorsPreset(LayoutPreset.FullRect);
        AddChild(dimRect);

        highlightRect = new ColorRect
        {
            Name = "TutorialHighlight",
            Color = new Color(1.0f, 0.86f, 0.24f, 0.32f),
            MouseFilter = MouseFilterEnum.Ignore,
            Visible = false
        };
        AddChild(highlightRect);

        promptPanel = new PanelContainer
        {
            Name = "TutorialPrompt",
            MouseFilter = MouseFilterEnum.Stop
        };
        promptPanel.AnchorLeft = 0.18f;
        promptPanel.AnchorRight = 0.82f;
        promptPanel.AnchorTop = 0.70f;
        promptPanel.AnchorBottom = 0.96f;
        AddChild(promptPanel);

        VBoxContainer content = new()
        {
            Name = "PromptContent"
        };
        promptPanel.AddChild(content);

        titleLabel = new Label
        {
            Name = "TitleLabel",
            AutowrapMode = TextServer.AutowrapMode.WordSmart
        };
        content.AddChild(titleLabel);

        promptLabel = new Label
        {
            Name = "PromptLabel",
            AutowrapMode = TextServer.AutowrapMode.WordSmart
        };
        content.AddChild(promptLabel);

        waitLabel = new Label
        {
            Name = "WaitLabel",
            AutowrapMode = TextServer.AutowrapMode.WordSmart
        };
        content.AddChild(waitLabel);

        HBoxContainer buttonRow = new()
        {
            Name = "ButtonRow"
        };
        content.AddChild(buttonRow);

        continueButton = new Button
        {
            Name = "ContinueButton",
            Text = "继续"
        };
        continueButton.Pressed += OnContinuePressed;
        buttonRow.AddChild(continueButton);

        skipButton = new Button
        {
            Name = "SkipButton",
            Text = "跳过教学"
        };
        skipButton.Pressed += HideOverlay;
        buttonRow.AddChild(skipButton);
    }

    private void AdvanceToNextStep()
    {
        currentStepIndex++;
        if (currentStepIndex >= steps.Count)
        {
            HideOverlay();
            return;
        }

        currentStep = steps[currentStepIndex];
        Visible = true;
        titleLabel.Text = $"{currentStep.orderIndex}. {currentStep.title}";
        promptLabel.Text = currentStep.promptText;
        continueButton.Visible = currentStep.waitCondition == TutorialWaitCondition.None;
        waitLabel.Text = currentStep.waitCondition == TutorialWaitCondition.None
            ? "阅读后点击继续。"
            : $"等待操作：{FormatWaitCondition(currentStep.waitCondition)}";
        UpdateHighlight();
    }

    private void HideOverlay()
    {
        IsRunning = false;
        currentStep = null;
        Visible = false;
        if (highlightRect != null)
        {
            highlightRect.Visible = false;
        }
    }

    private void OnContinuePressed()
    {
        if (currentStep?.waitCondition == TutorialWaitCondition.None)
        {
            AdvanceToNextStep();
        }
    }

    private void UpdateHighlight()
    {
        if (currentStep == null || highlightRect == null)
        {
            return;
        }

        if (!TryResolveHighlightRect(currentStep.highlightTarget, out Rect2 rect))
        {
            highlightRect.Visible = false;
            return;
        }

        highlightRect.Visible = true;
        highlightRect.GlobalPosition = rect.Position - new Vector2(6, 6);
        highlightRect.Size = rect.Size + new Vector2(12, 12);
    }

    private static bool TryResolveHighlightRect(TutorialHighlightTarget target, out Rect2 rect)
    {
        rect = new Rect2();
        Control control = ResolveHighlightControl(target);
        if (control != null && control.IsInsideTree() && control.Visible)
        {
            rect = control.GetGlobalRect();
            return true;
        }

        if (target == TutorialHighlightTarget.BattleArea &&
            Autoloads.gd_ChessBoard?.chessBoardUIControl != null)
        {
            Node2D board = Autoloads.gd_ChessBoard.chessBoardUIControl;
            rect = new Rect2(board.GlobalPosition - new Vector2(180, 120), new Vector2(360, 240));
            return true;
        }

        return false;
    }

    private static Control ResolveHighlightControl(TutorialHighlightTarget target)
    {
        SceneSingleton sceneSingleton = Autoloads.sceneSingleton;
        if (sceneSingleton == null)
        {
            return null;
        }

        return target switch
        {
            TutorialHighlightTarget.PlayerTimeline => sceneSingleton.cmdQueueUIControl?.CommandQueueMatrix,
            TutorialHighlightTarget.EnemyTimeline => sceneSingleton.cmdQueueUIControl?.CommandQueueMatrix,
            TutorialHighlightTarget.TimelineSlot => sceneSingleton.cmdQueueUIControl?.CommandQueueMatrix,
            TutorialHighlightTarget.SkillList => sceneSingleton.playerActionChoseList,
            TutorialHighlightTarget.InspectButton => sceneSingleton.cmdQueueUIControl?.GetTutorialHighlightControl(target),
            TutorialHighlightTarget.StartSettlementButton => sceneSingleton.cmdQueueUIControl?.GetTutorialHighlightControl(target),
            TutorialHighlightTarget.EncyclopediaButton => sceneSingleton.encyclopediaOverlayControl?.GetTutorialHighlightControl(target),
            TutorialHighlightTarget.BattleLog => sceneSingleton.cmdQueueUIControl?.GetTutorialHighlightControl(target),
            TutorialHighlightTarget.GrowthPanel => sceneSingleton.growthRewardOverlayControl?.GetTutorialHighlightControl(target),
            _ => null
        };
    }

    private static string FormatWaitCondition(TutorialWaitCondition waitCondition)
    {
        return waitCondition switch
        {
            TutorialWaitCondition.OpenEncyclopedia => "打开百科",
            TutorialWaitCondition.SelectSkill => "选择技能",
            TutorialWaitCondition.HoldPlaceCommand => "长按放置指令",
            TutorialWaitCondition.InspectCommand => "检视指令详情",
            TutorialWaitCondition.EnemyActionsRevealed => "怪物行动揭示",
            TutorialWaitCondition.StartSettlement => "点击开始结算",
            TutorialWaitCondition.CombatLogShown => "查看战斗记录",
            TutorialWaitCondition.AreaChanged => "角色区域变化",
            TutorialWaitCondition.VictoryReached => "教学关胜利",
            _ => "继续"
        };
    }
}

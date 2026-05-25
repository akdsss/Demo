using Godot;
using System.Collections.Generic;
using System.Linq;

public partial class TutorialOverlayControl : Control
{
    private const float PromptPanelWidth = 1024.0f;
    private const float PromptPanelHeight = 280.0f;
    private const float PromptVerticalCenter = 0.50f;
    private const float PromptCenterX = 0.50f;
    private const float PromptLeftCenterX = 0.32f;
    private const float PromptRightCenterX = 0.68f;
    private const float PromptEdgeMargin = 48.0f;
    private const float HighlightPadding = 6.0f;

    private static readonly Color HighlightColor = new(1.0f, 0.86f, 0.24f, 0.32f);
    private static readonly Color WaitTextColor = new(1.0f, 0.88f, 0.12f);
    private static readonly Color NormalTextColor = new(1.0f, 1.0f, 1.0f);
    private static readonly Color ContinueButtonTextColor = new(0.25f, 1.0f, 0.34f);
    private static readonly Color PreviousButtonTextColor = new(1.0f, 1.0f, 1.0f);
    private static readonly Color SkipButtonTextColor = new(1.0f, 0.25f, 0.2f);

    private ColorRect dimRect;
    private TutorialHighlightMarker highlightMarker;
    private PanelContainer promptPanel;
    private Label titleLabel;
    private Label promptLabel;
    private Label waitLabel;
    private Button previousButton;
    private Button continueButton;
    private Button skipButton;
    private List<TutorialStepData> steps = new();
    private readonly HashSet<TutorialWaitCondition> completedWaitConditions = new();
    private int currentStepIndex = -1;
    private TutorialStepData currentStep;
    private LevelData activeLevelData;

    public bool IsRunning { get; private set; }

    public override void _Ready()
    {
        BuildOverlay();
        HideOverlay();
    }

    public override void _Notification(int what)
    {
        if (what == NotificationResized)
        {
            FitToViewport();
            ApplyPromptLayout(currentStep?.promptHorizontalPosition ?? TutorialPromptHorizontalPosition.Center);
        }
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
        FitToViewport();
        activeLevelData = levelData ?? activeLevelData;
        steps = activeLevelData?.tutorialStepDataArray?
            .Where(step => step != null)
            .OrderBy(step => step.orderIndex)
            .ToList() ?? new List<TutorialStepData>();

        if (steps.Count == 0)
        {
            HideOverlay();
            return;
        }

        IsRunning = true;
        completedWaitConditions.Clear();
        currentStepIndex = -1;
        AdvanceToNextStep();
    }

    public void RestartTutorial(LevelData levelData = null)
    {
        StartTutorial(levelData ?? activeLevelData);
    }

    public void Notify(TutorialWaitCondition waitCondition)
    {
        if (!IsRunning || waitCondition == TutorialWaitCondition.None)
        {
            return;
        }

        completedWaitConditions.Add(waitCondition);
        if (currentStep == null || currentStep.waitCondition != waitCondition)
        {
            return;
        }

        AdvanceToNextStep();
    }

    private void BuildOverlay()
    {
        SetAnchorsPreset(LayoutPreset.FullRect);
        FitToViewport();
        ZIndex = 300;
        MouseFilter = MouseFilterEnum.Ignore;

        dimRect = new ColorRect
        {
            Name = "TutorialDim",
            Color = new Color(0, 0, 0, 0f),
            MouseFilter = MouseFilterEnum.Ignore
        };
        dimRect.SetAnchorsPreset(LayoutPreset.FullRect);
        AddChild(dimRect);

        highlightMarker = new TutorialHighlightMarker
        {
            Name = "TutorialHighlight",
            FillColor = HighlightColor,
            MouseFilter = MouseFilterEnum.Ignore,
            Visible = false
        };
        AddChild(highlightMarker);

        promptPanel = new PanelContainer
        {
            Name = "TutorialPrompt",
            MouseFilter = MouseFilterEnum.Stop
        };
        ApplyPromptLayout(TutorialPromptHorizontalPosition.Center);
        AddChild(promptPanel);

        VBoxContainer content = new()
        {
            Name = "PromptContent",
            SizeFlagsHorizontal = SizeFlags.ExpandFill,
            SizeFlagsVertical = SizeFlags.ExpandFill
        };
        content.AddThemeConstantOverride("separation", 8);
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

        Control buttonSpacer = new()
        {
            Name = "ButtonSpacer",
            SizeFlagsVertical = SizeFlags.ExpandFill
        };
        content.AddChild(buttonSpacer);

        HBoxContainer buttonRow = new()
        {
            Name = "ButtonRow",
            Alignment = BoxContainer.AlignmentMode.End,
            SizeFlagsHorizontal = SizeFlags.ExpandFill
        };
        buttonRow.AddThemeConstantOverride("separation", 8);
        content.AddChild(buttonRow);

        previousButton = new Button
        {
            Name = "PreviousButton",
            Text = "上一个"
        };
        ApplyButtonTextColor(previousButton, PreviousButtonTextColor);
        previousButton.Pressed += OnPreviousPressed;
        buttonRow.AddChild(previousButton);

        continueButton = new Button
        {
            Name = "ContinueButton",
            Text = "继续"
        };
        ApplyButtonTextColor(continueButton, ContinueButtonTextColor);
        continueButton.Pressed += OnContinuePressed;
        buttonRow.AddChild(continueButton);

        skipButton = new Button
        {
            Name = "SkipButton",
            Text = "跳过教学"
        };
        ApplyButtonTextColor(skipButton, SkipButtonTextColor);
        skipButton.Pressed += HideOverlay;
        buttonRow.AddChild(skipButton);
    }

    private void AdvanceToNextStep()
    {
        ShowStep(currentStepIndex + 1, true);
    }

    private void ShowStep(int stepIndex, bool advanceCompletedWaitConditions)
    {
        if (stepIndex < 0)
        {
            return;
        }

        if (stepIndex >= steps.Count)
        {
            HideOverlay();
            return;
        }

        currentStepIndex = stepIndex;
        currentStep = steps[currentStepIndex];
        Visible = true;
        FitToViewport();
        MoveToFront();
        ApplyPromptLayout(currentStep.promptHorizontalPosition);
        titleLabel.Text = $"{currentStep.orderIndex}. {currentStep.title}";
        promptLabel.Text = currentStep.promptText;
        continueButton.Visible = CanContinueCurrentStep();
        previousButton.Visible = steps.Count > 1;
        previousButton.Disabled = currentStepIndex <= 0;
        waitLabel.Text = currentStep.waitCondition == TutorialWaitCondition.None
            ? "阅读后点击继续。"
            : $"等待操作：{FormatWaitCondition(currentStep.waitCondition)}";
        waitLabel.AddThemeColorOverride(
            "font_color",
            currentStep.waitCondition == TutorialWaitCondition.None ? NormalTextColor : WaitTextColor);
        UpdateHighlight();
        if (advanceCompletedWaitConditions)
        {
            AdvanceIfWaitConditionAlreadyCompleted();
        }
    }

    private void AdvanceIfWaitConditionAlreadyCompleted()
    {
        while (IsRunning &&
            currentStep != null &&
            currentStep.waitCondition != TutorialWaitCondition.None &&
            completedWaitConditions.Contains(currentStep.waitCondition))
        {
            AdvanceToNextStep();
        }
    }

    private void HideOverlay()
    {
        IsRunning = false;
        currentStep = null;
        Visible = false;
        if (highlightMarker != null)
        {
            highlightMarker.Visible = false;
        }
    }

    private void OnContinuePressed()
    {
        if (CanContinueCurrentStep())
        {
            AdvanceToNextStep();
        }
    }

    private void OnPreviousPressed()
    {
        if (currentStepIndex > 0)
        {
            ShowStep(currentStepIndex - 1, false);
        }
    }

    private bool CanContinueCurrentStep()
    {
        return currentStep != null &&
            (currentStep.waitCondition == TutorialWaitCondition.None ||
            completedWaitConditions.Contains(currentStep.waitCondition));
    }

    private void UpdateHighlight()
    {
        if (currentStep == null || highlightMarker == null)
        {
            return;
        }

        if (!TryResolveHighlightRect(currentStep.highlightTarget, out Rect2 rect))
        {
            highlightMarker.Visible = false;
            return;
        }

        Vector2 paddedSize = rect.Size + new Vector2(HighlightPadding * 2.0f, HighlightPadding * 2.0f);
        Vector2 highlightSize = paddedSize * Mathf.Max(0.1f, currentStep.highlightScale);
        if (currentStep.highlightShape == TutorialHighlightShape.Circle)
        {
            float diameter = Mathf.Max(highlightSize.X, highlightSize.Y);
            highlightSize = new Vector2(diameter, diameter);
        }

        highlightMarker.Visible = true;
        highlightMarker.Shape = currentStep.highlightShape;
        highlightMarker.GlobalPosition = rect.GetCenter() - (highlightSize / 2.0f);
        highlightMarker.Size = highlightSize;
        highlightMarker.QueueRedraw();
    }

    private static bool TryResolveHighlightRect(TutorialHighlightTarget target, out Rect2 rect)
    {
        rect = new Rect2();
        Control control = ResolveHighlightControl(target);
        if (control != null && control.IsInsideTree() && control.IsVisibleInTree())
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
            TutorialHighlightTarget.PlayerTimeline => sceneSingleton.cmdQueueUIControl?.GetTutorialHighlightControl(target),
            TutorialHighlightTarget.EnemyTimeline => sceneSingleton.cmdQueueUIControl?.GetTutorialHighlightControl(target),
            TutorialHighlightTarget.TimelineSlot => sceneSingleton.cmdQueueUIControl?.GetTutorialHighlightControl(target),
            TutorialHighlightTarget.SkillList => sceneSingleton.playerActionChoseList,
            TutorialHighlightTarget.SetCommandButton => sceneSingleton.cmdQueueUIControl?.GetTutorialHighlightControl(target),
            TutorialHighlightTarget.InspectButton => sceneSingleton.cmdQueueUIControl?.GetTutorialHighlightControl(target),
            TutorialHighlightTarget.StartSettlementButton => sceneSingleton.cmdQueueUIControl?.GetTutorialHighlightControl(target),
            TutorialHighlightTarget.EncyclopediaButton => sceneSingleton.mainUIControl?.GetTutorialHighlightControl(target)
                ?? sceneSingleton.encyclopediaOverlayControl?.GetTutorialHighlightControl(target),
            TutorialHighlightTarget.BattleLog => sceneSingleton.mainUIControl?.GetTutorialHighlightControl(target)
                ?? sceneSingleton.battleLogOverlayControl?.GetTutorialHighlightControl(target)
                ?? sceneSingleton.cmdQueueUIControl?.GetTutorialHighlightControl(target),
            TutorialHighlightTarget.GrowthPanel => sceneSingleton.growthRewardOverlayControl?.GetTutorialHighlightControl(target),
            _ => null
        };
    }

    private void ApplyPromptLayout(TutorialPromptHorizontalPosition horizontalPosition)
    {
        if (promptPanel == null)
        {
            return;
        }

        float centerX = horizontalPosition switch
        {
            TutorialPromptHorizontalPosition.Left => PromptLeftCenterX,
            TutorialPromptHorizontalPosition.Right => PromptRightCenterX,
            _ => PromptCenterX
        };

        Vector2 viewportSize = GetViewportRect().Size;
        float viewportWidth = Mathf.Max(1.0f, viewportSize.X);
        float viewportHeight = Mathf.Max(1.0f, viewportSize.Y);
        float promptWidth = Mathf.Min(PromptPanelWidth, Mathf.Max(320.0f, viewportWidth - (PromptEdgeMargin * 2.0f)));
        float promptHeight = Mathf.Min(PromptPanelHeight, Mathf.Max(180.0f, viewportHeight - (PromptEdgeMargin * 2.0f)));
        float halfWidth = promptWidth * 0.5f;
        float halfHeight = promptHeight * 0.5f;
        float centerPixelX = Mathf.Clamp(
            viewportWidth * centerX,
            PromptEdgeMargin + halfWidth,
            viewportWidth - PromptEdgeMargin - halfWidth);
        float centerPixelY = Mathf.Clamp(
            viewportHeight * PromptVerticalCenter,
            PromptEdgeMargin + halfHeight,
            viewportHeight - PromptEdgeMargin - halfHeight);
        float anchorX = centerPixelX / viewportWidth;
        float anchorY = centerPixelY / viewportHeight;

        promptPanel.AnchorLeft = anchorX;
        promptPanel.AnchorRight = anchorX;
        promptPanel.AnchorTop = anchorY;
        promptPanel.AnchorBottom = anchorY;
        promptPanel.OffsetLeft = -halfWidth;
        promptPanel.OffsetRight = halfWidth;
        promptPanel.OffsetTop = -halfHeight;
        promptPanel.OffsetBottom = halfHeight;
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

    private static void ApplyButtonTextColor(Button button, Color color)
    {
        button.AddThemeColorOverride("font_color", color);
        button.AddThemeColorOverride("font_hover_color", color);
        button.AddThemeColorOverride("font_pressed_color", color);
        button.AddThemeColorOverride("font_focus_color", color);
        button.AddThemeColorOverride("font_disabled_color", new Color(color.R, color.G, color.B, 0.45f));
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
            TutorialWaitCondition.OpenBattleLog => "打开战斗记录",
            TutorialWaitCondition.EnterTimelinePlacement => "进入时间轴放置阶段",
            _ => "继续"
        };
    }

    private partial class TutorialHighlightMarker : Control
    {
        public Color FillColor { get; set; } = HighlightColor;
        public TutorialHighlightShape Shape { get; set; } = TutorialHighlightShape.Rectangle;

        public override void _Draw()
        {
            if (Shape == TutorialHighlightShape.Circle)
            {
                DrawCircle(Size / 2.0f, Mathf.Min(Size.X, Size.Y) / 2.0f, FillColor);
                return;
            }

            DrawRect(new Rect2(Vector2.Zero, Size), FillColor);
        }
    }
}

using Godot;

[GlobalClass]
public partial class TutorialStepData : Resource
{
    [Export] public string stepId = string.Empty;
    [Export] public int orderIndex;
    [Export] public string title = string.Empty;
    [Export(PropertyHint.MultilineText)] public string promptText = string.Empty;
    [Export] public TutorialPromptHorizontalPosition promptHorizontalPosition = TutorialPromptHorizontalPosition.Center;
    [Export] public TutorialHighlightTarget highlightTarget = TutorialHighlightTarget.None;
    [Export] public TutorialHighlightShape highlightShape = TutorialHighlightShape.Rectangle;
    [Export] public float highlightScale = 1.0f;
    [Export] public TutorialWaitCondition waitCondition = TutorialWaitCondition.None;
    [Export] public string requiredTargetId = string.Empty;
    [Export] public bool blocksOtherInput = true;
}

public enum TutorialPromptHorizontalPosition
{
    Center = 0,
    Left = 1,
    Right = 2
}

public enum TutorialHighlightShape
{
    Rectangle = 0,
    Circle = 1
}

public enum TutorialHighlightTarget
{
    None = 0,
    BattleArea = 1,
    PlayerTimeline = 2,
    EnemyTimeline = 3,
    TimelineSlot = 4,
    SkillList = 5,
    InspectButton = 6,
    StartSettlementButton = 7,
    EncyclopediaButton = 8,
    BattleLog = 9,
    GrowthPanel = 10,
    SetCommandButton = 11
}

public enum TutorialWaitCondition
{
    None = 0,
    OpenEncyclopedia = 1,
    SelectSkill = 2,
    HoldPlaceCommand = 3,
    InspectCommand = 4,
    EnemyActionsRevealed = 5,
    StartSettlement = 6,
    CombatLogShown = 7,
    AreaChanged = 8,
    VictoryReached = 9,
    OpenBattleLog = 10,
    EnterTimelinePlacement = 11
}

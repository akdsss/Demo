using Godot;

[GlobalClass]
public partial class TutorialStepData : Resource
{
    [Export] public string stepId = string.Empty;
    [Export] public int orderIndex;
    [Export] public string title = string.Empty;
    [Export(PropertyHint.MultilineText)] public string promptText = string.Empty;
    [Export] public TutorialHighlightTarget highlightTarget = TutorialHighlightTarget.None;
    [Export] public TutorialWaitCondition waitCondition = TutorialWaitCondition.None;
    [Export] public string requiredTargetId = string.Empty;
    [Export] public bool blocksOtherInput = true;
}

public enum TutorialHighlightTarget
{
    None,
    BattleArea,
    PlayerTimeline,
    EnemyTimeline,
    TimelineSlot,
    SkillList,
    InspectButton,
    StartSettlementButton,
    EncyclopediaButton,
    BattleLog,
    GrowthPanel
}

public enum TutorialWaitCondition
{
    None,
    OpenEncyclopedia,
    SelectSkill,
    HoldPlaceCommand,
    InspectCommand,
    EnemyActionsRevealed,
    StartSettlement,
    CombatLogShown,
    AreaChanged,
    VictoryReached
}

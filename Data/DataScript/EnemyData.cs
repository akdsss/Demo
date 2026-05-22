using Godot;

[GlobalClass]
public partial class EnemyData : CharacterData
{
    //[Export] public int hp;
    //[Export] public int atk;
    [Export] public EnemyCommandData[] enemyCommandDataArray;
    [Export] public EnemyAiProfile aiProfile = EnemyAiProfile.Basic;
    [Export] public bool canEnterRage;
    [Export] public bool useOddEvenActionPattern;
    [Export] public int oddRoundActionTimes;
    [Export] public int evenRoundActionTimes;
    public bool rageTriggered;
    //[Export(PropertyHint.Enum, "Move,Attack,Skip,Dead")]
    //public string[] enemyCommands;

    public override void CharacterInitialize()
    {
        base.CharacterInitialize();
        rageTriggered = false;
    }

    public int GetBaseActionTimesForRound(int roundIndex)
    {
        if (useOddEvenActionPattern)
        {
            int configured = roundIndex % 2 == 1 ? oddRoundActionTimes : evenRoundActionTimes;
            if (configured > 0)
            {
                return configured;
            }
        }

        return turnInitialActionTimes;
    }

    public int GetActionTimesForRound(int roundIndex)
    {
        int actionTimes = GetBaseActionTimesForRound(roundIndex);
        if (runtimeStatusIds != null && runtimeStatusIds.Contains(StatusCatalog.Rage))
        {
            actionTimes += 1;
        }

        return actionTimes;
    }
}

public enum EnemyAiProfile
{
    Basic,
    Melee,
    Ranged,
    Assassin,
    Support,
    EliteMelee,
    EliteSupport
}

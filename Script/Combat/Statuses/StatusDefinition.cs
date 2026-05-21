using System.Collections.Generic;

public class StatusDefinition
{
    public string Id { get; set; } = "unknown_status";
    public string DisplayName { get; set; } = "Unknown Status";
    public string Description { get; set; } = string.Empty;
    public StatusStackMode StackMode { get; set; } = StatusStackMode.Unique;
    public StatusDurationType DurationType { get; set; } = StatusDurationType.Rounds;
    public int DurationValue { get; set; } = 1;
    public SkillTag TagsBlocked { get; set; } = SkillTag.None;
    public StatusRemoveCondition RemoveCondition { get; set; } = StatusRemoveCondition.None;
    public List<AreaEffectTiming> TimingTriggers { get; } = new();
    public List<AreaModifier> Modifiers { get; } = new();
    public List<SkillEffectDefinition> TriggerEffects { get; } = new();

    public StatusInstance CreateInstance(CharacterState owner, CharacterState source = null)
    {
        return new StatusInstance
        {
            Definition = this,
            Owner = owner,
            Source = source,
            RemainingDuration = DurationValue,
            StackCount = 1
        };
    }
}

public class StatusInstance
{
    public StatusDefinition Definition { get; set; }
    public CharacterState Owner { get; set; }
    public CharacterState Source { get; set; }
    public int RemainingDuration { get; set; }
    public int StackCount { get; set; } = 1;

    public bool IsExpired
    {
        get
        {
            if (Definition == null)
            {
                return true;
            }

            if (Definition.DurationType == StatusDurationType.Instant)
            {
                return true;
            }

            if (Definition.DurationType == StatusDurationType.UntilRemoved)
            {
                return false;
            }

            return RemainingDuration <= 0;
        }
    }

    public void TickDuration()
    {
        if (Definition == null)
        {
            return;
        }

        if (Definition.DurationType == StatusDurationType.UntilRemoved)
        {
            return;
        }

        RemainingDuration--;
    }
}

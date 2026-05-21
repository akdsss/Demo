using Godot;
using System.Collections.Generic;

public class SkillDefinition
{
    public string Id { get; set; } = "unknown_skill";
    public string DisplayName { get; set; } = "Unknown Skill";
    public string Description { get; set; } = string.Empty;
    public SkillTag Tags { get; set; } = SkillTag.None;
    public int Priority { get; set; }
    public int MpCost { get; set; }
    public SkillTargetType TargetType { get; set; } = SkillTargetType.None;
    public int DurationSlots { get; set; } = 1;
    public List<SkillEffectDefinition> Effects { get; } = new();
    public string FailTextKey { get; set; } = string.Empty;
    public int LegacyCommandId { get; set; } = -1;
    public string LegacyCommandType { get; set; } = string.Empty;
    public CommandData LegacyCommandData { get; set; }

    public bool HasTag(SkillTag tag)
    {
        return (Tags & tag) == tag;
    }

    public static SkillDefinition FromCommandData(CommandData commandData)
    {
        SkillDefinition definition = new();
        if (commandData == null)
        {
            return definition;
        }

        definition.LegacyCommandData = commandData;
        definition.LegacyCommandId = commandData.commandId;
        definition.LegacyCommandType = commandData.GetType().Name;
        definition.Id = BuildLegacyId(commandData);
        definition.DisplayName = string.IsNullOrEmpty(commandData.commandName)
            ? definition.Id
            : commandData.commandName;
        definition.Description = commandData.commandDescription ?? string.Empty;
        definition.Priority = commandData.priority;
        definition.MpCost = 0;
        definition.DurationSlots = 1;
        definition.FailTextKey = $"legacy_command_{commandData.commandId}_failed";

        ApplyLegacyDefaults(definition, commandData);
        return definition;
    }

    private static string BuildLegacyId(CommandData commandData)
    {
        string prefix = commandData switch
        {
            PlayerCommandData => "player",
            EnemyCommandData => "enemy",
            _ => "command"
        };

        return $"{prefix}_command_{commandData.commandId}";
    }

    private static void ApplyLegacyDefaults(SkillDefinition definition, CommandData commandData)
    {
        if (commandData is PlayerCommandData)
        {
            ApplyLegacyPlayerDefaults(definition, commandData.commandId);
            return;
        }

        if (commandData is EnemyCommandData)
        {
            ApplyLegacyEnemyDefaults(definition, commandData.commandId);
            return;
        }

        definition.TargetType = SkillTargetType.None;
        definition.Tags = SkillTag.Special;
    }

    private static void ApplyLegacyPlayerDefaults(SkillDefinition definition, int commandId)
    {
        switch (commandId)
        {
            case 1:
                definition.TargetType = SkillTargetType.Enemy;
                definition.Tags = SkillTag.Melee | SkillTag.SingleTarget;
                definition.Effects.Add(SkillEffectDefinition.Damage(1.0f));
                break;
            case 2:
                definition.TargetType = SkillTargetType.Area;
                definition.Tags = SkillTag.Move;
                definition.Effects.Add(SkillEffectDefinition.Move());
                break;
            case 3:
                definition.TargetType = SkillTargetType.Self;
                definition.Tags = SkillTag.Defense;
                definition.Effects.Add(SkillEffectDefinition.Defend());
                break;
            default:
                definition.TargetType = SkillTargetType.None;
                definition.Tags = SkillTag.Special;
                break;
        }
    }

    private static void ApplyLegacyEnemyDefaults(SkillDefinition definition, int commandId)
    {
        switch (commandId)
        {
            case 1:
                definition.TargetType = SkillTargetType.Area;
                definition.Tags = SkillTag.Move;
                definition.Effects.Add(SkillEffectDefinition.Move());
                break;
            case 3:
                definition.TargetType = SkillTargetType.Enemy;
                definition.Tags = SkillTag.Melee | SkillTag.SingleTarget;
                definition.Effects.Add(SkillEffectDefinition.Damage(1.0f));
                break;
            default:
                definition.TargetType = SkillTargetType.None;
                definition.Tags = SkillTag.Special;
                break;
        }
    }
}

public class SkillEffectDefinition
{
    public SkillEffectType EffectType { get; set; } = SkillEffectType.None;
    public float Value { get; set; }
    public string StatusId { get; set; } = string.Empty;
    public int DelaySlots { get; set; }
    public CombatAreaId TargetAreaId { get; set; } = CombatAreaId.Unknown;

    public static SkillEffectDefinition Damage(float multiplier)
    {
        return new SkillEffectDefinition
        {
            EffectType = SkillEffectType.Damage,
            Value = multiplier
        };
    }

    public static SkillEffectDefinition Heal(float multiplier)
    {
        return new SkillEffectDefinition
        {
            EffectType = SkillEffectType.Heal,
            Value = multiplier
        };
    }

    public static SkillEffectDefinition Move()
    {
        return new SkillEffectDefinition
        {
            EffectType = SkillEffectType.Move
        };
    }

    public static SkillEffectDefinition Defend()
    {
        return new SkillEffectDefinition
        {
            EffectType = SkillEffectType.Defend
        };
    }
}

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
    public bool RequiresSecondaryAreaTargetSelection { get; set; }
    public bool RequiresDifferentTargetArea { get; set; }

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
                definition.MpCost = 10;
                definition.Effects.Add(SkillEffectDefinition.Damage(1.0f));
                break;
            case 2:
                definition.TargetType = SkillTargetType.Area;
                definition.Tags = SkillTag.Move;
                definition.MpCost = 5;
                definition.Effects.Add(SkillEffectDefinition.Move());
                break;
            case 3:
                definition.TargetType = SkillTargetType.Self;
                definition.Tags = SkillTag.Defense;
                definition.Effects.Add(SkillEffectDefinition.Defend());
                break;
            case 4:
                definition.TargetType = SkillTargetType.Enemy;
                definition.Tags = SkillTag.Ranged | SkillTag.SingleTarget;
                definition.MpCost = 10;
                definition.Effects.Add(SkillEffectDefinition.Damage(1.0f));
                break;
            case 5:
                definition.TargetType = SkillTargetType.Ally;
                definition.Tags = SkillTag.Heal | SkillTag.Special;
                definition.MpCost = 20;
                definition.Effects.Add(SkillEffectDefinition.Heal(1.0f));
                break;
            case 6:
                definition.TargetType = SkillTargetType.Enemy;
                definition.Tags = SkillTag.Melee | SkillTag.SingleTarget;
                definition.MpCost = 10;
                definition.Effects.Add(SkillEffectDefinition.Damage(1.5f));
                break;
            case 7:
                definition.TargetType = SkillTargetType.Self;
                definition.Tags = SkillTag.Heal | SkillTag.Special;
                definition.Effects.Add(SkillEffectDefinition.RestoreMp(50));
                break;
            case 8:
                definition.TargetType = SkillTargetType.Self;
                definition.Tags = SkillTag.Defense | SkillTag.Melee | SkillTag.SingleTarget;
                definition.MpCost = 10;
                definition.Effects.Add(SkillEffectDefinition.ApplyStatus(StatusCatalog.CounterSingleMelee));
                break;
            case 9:
                definition.TargetType = SkillTargetType.Self;
                definition.Tags = SkillTag.Melee | SkillTag.Area;
                definition.MpCost = 30;
                definition.DurationSlots = 2;
                definition.Effects.Add(SkillEffectDefinition.Damage(1.0f, excludeSource: true, delaySlots: 1));
                break;
            case 10:
                definition.TargetType = SkillTargetType.Enemy;
                definition.Tags = SkillTag.Move | SkillTag.Special;
                definition.MpCost = 5;
                definition.Effects.Add(SkillEffectDefinition.Move());
                definition.Effects.Add(SkillEffectDefinition.ApplyStatus(StatusCatalog.Dodge));
                break;
            case 11:
                definition.TargetType = SkillTargetType.Self;
                definition.Tags = SkillTag.Defense | SkillTag.Melee;
                definition.MpCost = 10;
                definition.Effects.Add(SkillEffectDefinition.ApplyStatus(StatusCatalog.CounterMelee));
                break;
            case 12:
                definition.TargetType = SkillTargetType.Enemy;
                definition.Tags = SkillTag.Move | SkillTag.Melee | SkillTag.SingleTarget;
                definition.MpCost = 10;
                definition.RequiresSecondaryAreaTargetSelection = true;
                definition.RequiresDifferentTargetArea = true;
                definition.Effects.Add(SkillEffectDefinition.Damage(0.5f));
                definition.Effects.Add(SkillEffectDefinition.MoveTarget());
                break;
            case 13:
                definition.TargetType = SkillTargetType.Self;
                definition.Tags = SkillTag.Defense;
                definition.Effects.Add(SkillEffectDefinition.ApplyStatus(StatusCatalog.Dodge));
                break;
            case 14:
                definition.TargetType = SkillTargetType.Area;
                definition.Tags = SkillTag.Ranged | SkillTag.Area | SkillTag.Special;
                definition.MpCost = 30;
                definition.DurationSlots = 4;
                definition.Effects.Add(SkillEffectDefinition.Damage(1.0f));
                break;
            case 15:
                definition.TargetType = SkillTargetType.Enemy;
                definition.Tags = SkillTag.Ranged | SkillTag.Area | SkillTag.Special;
                definition.MpCost = 30;
                definition.DurationSlots = 4;
                definition.Effects.Add(SkillEffectDefinition.Damage(1.0f));
                break;
            case 16:
                definition.TargetType = SkillTargetType.Enemy;
                definition.Tags = SkillTag.Special | SkillTag.SingleTarget;
                definition.MpCost = 10;
                definition.Effects.Add(SkillEffectDefinition.ApplyStatus(StatusCatalog.Mark));
                break;
            case 17:
                definition.TargetType = SkillTargetType.Enemy;
                definition.Tags = SkillTag.Ranged | SkillTag.SingleTarget;
                definition.MpCost = 30;
                definition.DurationSlots = 2;
                definition.Effects.Add(SkillEffectDefinition.Damage(1.0f));
                definition.Effects.Add(SkillEffectDefinition.Damage(1.0f, delaySlots: 1));
                break;
            case 18:
                definition.TargetType = SkillTargetType.Self;
                definition.Tags = SkillTag.Heal | SkillTag.Special;
                definition.Effects.Add(SkillEffectDefinition.RestoreMp(30, affectAllies: true));
                break;
            case 19:
                definition.TargetType = SkillTargetType.Enemy;
                definition.Tags = SkillTag.Ranged | SkillTag.SingleTarget;
                definition.MpCost = 10;
                definition.Effects.Add(SkillEffectDefinition.Damage(0.5f));
                break;
            case 20:
                definition.TargetType = SkillTargetType.Ally;
                definition.Tags = SkillTag.Heal | SkillTag.Area | SkillTag.Special;
                definition.MpCost = 30;
                definition.DurationSlots = 4;
                definition.Effects.Add(SkillEffectDefinition.Heal(1.0f));
                break;
            case 21:
                definition.TargetType = SkillTargetType.Self;
                definition.Tags = SkillTag.Special;
                definition.MpCost = 5;
                definition.Effects.Add(SkillEffectDefinition.RevealIntent("揭示所选时点与前4个时点的所有敌方意图。"));
                break;
            case 22:
                definition.TargetType = SkillTargetType.Self;
                definition.Tags = SkillTag.Special;
                definition.MpCost = 5;
                definition.Effects.Add(SkillEffectDefinition.RevealIntent("揭示所选时点与前后2个时点的所有敌方意图。"));
                break;
            case 23:
                definition.TargetType = SkillTargetType.Self;
                definition.Tags = SkillTag.Special;
                definition.MpCost = 5;
                definition.Effects.Add(SkillEffectDefinition.RevealIntent("揭示所选时点与后4个时点的所有敌方意图。"));
                break;
            case 24:
                definition.TargetType = SkillTargetType.Ally;
                definition.Tags = SkillTag.Special;
                definition.MpCost = 10;
                definition.Effects.Add(SkillEffectDefinition.ApplyStatus(StatusCatalog.PowerUp));
                break;
            case 25:
                definition.TargetType = SkillTargetType.Character;
                definition.Tags = SkillTag.Move | SkillTag.Special;
                definition.MpCost = 5;
                definition.Effects.Add(SkillEffectDefinition.SwapPosition());
                break;
            case 26:
                definition.TargetType = SkillTargetType.Self;
                definition.Tags = SkillTag.Heal | SkillTag.Special;
                definition.Effects.Add(SkillEffectDefinition.RestoreMp(50, affectAllies: true));
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
            case 4:
                definition.TargetType = SkillTargetType.Enemy;
                definition.Tags = SkillTag.Melee | SkillTag.SingleTarget;
                definition.DurationSlots = 2;
                definition.Effects.Add(SkillEffectDefinition.Damage(2.0f));
                break;
            case 5:
                definition.TargetType = SkillTargetType.Enemy;
                definition.Tags = SkillTag.Ranged | SkillTag.SingleTarget;
                definition.Effects.Add(SkillEffectDefinition.Damage(1.0f));
                break;
            case 6:
                definition.TargetType = SkillTargetType.Area;
                definition.Tags = SkillTag.Move | SkillTag.Special;
                definition.Effects.Add(SkillEffectDefinition.Move());
                break;
            case 7:
                definition.TargetType = SkillTargetType.Ally;
                definition.Tags = SkillTag.Heal | SkillTag.Special;
                definition.Effects.Add(SkillEffectDefinition.Heal(1.0f));
                break;
            case 8:
                definition.TargetType = SkillTargetType.Enemy;
                definition.Tags = SkillTag.Melee | SkillTag.Area;
                definition.DurationSlots = 2;
                definition.Effects.Add(SkillEffectDefinition.Damage(1.5f, excludeSource: true));
                break;
            case 9:
                definition.TargetType = SkillTargetType.Enemy;
                definition.Tags = SkillTag.Melee | SkillTag.SingleTarget;
                definition.Effects.Add(SkillEffectDefinition.Damage(1.0f));
                break;
            case 10:
                definition.TargetType = SkillTargetType.Self;
                definition.Tags = SkillTag.Special;
                definition.Effects.Add(SkillEffectDefinition.ApplyStatus(StatusCatalog.Rage));
                break;
            case 11:
                definition.TargetType = SkillTargetType.Enemy;
                definition.Tags = SkillTag.Move | SkillTag.Melee | SkillTag.SingleTarget;
                definition.Effects.Add(SkillEffectDefinition.Move());
                definition.Effects.Add(SkillEffectDefinition.Damage(0.5f));
                break;
            case 12:
                definition.TargetType = SkillTargetType.Enemy;
                definition.Tags = SkillTag.Melee | SkillTag.Area | SkillTag.Special;
                definition.DurationSlots = 4;
                definition.Effects.Add(SkillEffectDefinition.Damage(1.0f, excludeSource: true));
                break;
            case 13:
                definition.TargetType = SkillTargetType.Enemy;
                definition.Tags = SkillTag.Melee | SkillTag.Area | SkillTag.Special;
                definition.DurationSlots = 4;
                definition.Effects.Add(SkillEffectDefinition.Damage(1.0f, excludeSource: true));
                break;
            case 14:
                definition.TargetType = SkillTargetType.Enemy;
                definition.Tags = SkillTag.Ranged | SkillTag.Area | SkillTag.Special;
                definition.DurationSlots = 4;
                definition.Effects.Add(SkillEffectDefinition.Damage(1.0f, excludeSource: true));
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
    public bool ExcludeSource { get; set; }
    public bool AffectAllies { get; set; }
    public string Message { get; set; } = string.Empty;

    public static SkillEffectDefinition Damage(float multiplier, bool excludeSource = false, int delaySlots = 0)
    {
        return new SkillEffectDefinition
        {
            EffectType = SkillEffectType.Damage,
            Value = multiplier,
            ExcludeSource = excludeSource,
            DelaySlots = delaySlots
        };
    }

    public static SkillEffectDefinition Heal(float multiplier, bool affectAllies = false, int delaySlots = 0)
    {
        return new SkillEffectDefinition
        {
            EffectType = SkillEffectType.Heal,
            Value = multiplier,
            AffectAllies = affectAllies,
            DelaySlots = delaySlots
        };
    }

    public static SkillEffectDefinition Move()
    {
        return new SkillEffectDefinition
        {
            EffectType = SkillEffectType.Move
        };
    }

    public static SkillEffectDefinition MoveTarget()
    {
        return new SkillEffectDefinition
        {
            EffectType = SkillEffectType.MoveTarget
        };
    }

    public static SkillEffectDefinition SwapPosition()
    {
        return new SkillEffectDefinition
        {
            EffectType = SkillEffectType.SwapPosition
        };
    }

    public static SkillEffectDefinition RestoreMp(float amount, bool affectAllies = false)
    {
        return new SkillEffectDefinition
        {
            EffectType = SkillEffectType.RestoreMp,
            Value = amount,
            AffectAllies = affectAllies
        };
    }

    public static SkillEffectDefinition Defend()
    {
        return new SkillEffectDefinition
        {
            EffectType = SkillEffectType.Defend
        };
    }

    public static SkillEffectDefinition ApplyStatus(string statusId)
    {
        return new SkillEffectDefinition
        {
            EffectType = SkillEffectType.ApplyStatus,
            StatusId = statusId
        };
    }

    public static SkillEffectDefinition RevealIntent(string message)
    {
        return new SkillEffectDefinition
        {
            EffectType = SkillEffectType.RevealIntent,
            Message = message
        };
    }
}

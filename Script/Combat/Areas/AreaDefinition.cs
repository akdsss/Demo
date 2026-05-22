using Godot;
using System.Collections.Generic;
using System.Linq;

public class AreaDefinition
{
    public string Id { get; set; } = "unknown_area";
    public CombatAreaId AreaId { get; set; } = CombatAreaId.Unknown;
    public string DisplayName { get; set; } = "Unknown Area";
    public string Description { get; set; } = string.Empty;
    public string EncyclopediaText { get; set; } = string.Empty;
    public Vector2I? LegacyCoord { get; set; }
    public List<AreaModifier> Modifiers { get; } = new();
    public List<AreaTrigger> Triggers { get; } = new();

    public bool IsSameArea(AreaDefinition other)
    {
        if (other == null)
        {
            return false;
        }

        if (AreaId == CombatAreaId.LegacyCoord || other.AreaId == CombatAreaId.LegacyCoord)
        {
            return LegacyCoord.HasValue && other.LegacyCoord.HasValue && LegacyCoord.Value == other.LegacyCoord.Value;
        }

        return AreaId != CombatAreaId.Unknown && AreaId == other.AreaId;
    }

    public static AreaDefinition FromKnownArea(CombatAreaId areaId, string displayName = "")
    {
        string id = areaId.ToString().ToLowerInvariant();
        AreaDefinition definition = new()
        {
            Id = id,
            AreaId = areaId,
            DisplayName = string.IsNullOrEmpty(displayName) ? GetDisplayName(areaId) : displayName,
            Description = GetShortDescription(areaId),
            EncyclopediaText = GetEncyclopediaText(areaId)
        };
        AddDefaultModifiers(definition);
        return definition;
    }

    public static List<AreaDefinition> CreateBaguaDefaults()
    {
        return new List<AreaDefinition>
        {
            FromKnownArea(CombatAreaId.Qian),
            FromKnownArea(CombatAreaId.Dui),
            FromKnownArea(CombatAreaId.Li),
            FromKnownArea(CombatAreaId.Zhen),
            FromKnownArea(CombatAreaId.Xun),
            FromKnownArea(CombatAreaId.Kan),
            FromKnownArea(CombatAreaId.Gen),
            FromKnownArea(CombatAreaId.Kun),
            FromKnownArea(CombatAreaId.Yin),
            FromKnownArea(CombatAreaId.Yang)
        };
    }

    public static AreaDefinition FromLegacyCoord(Vector2I coord)
    {
        if (TryGetAreaIdForLegacyCoord(coord, out CombatAreaId mappedAreaId))
        {
            AreaDefinition mappedArea = FromKnownArea(mappedAreaId);
            mappedArea.LegacyCoord = coord;
            return mappedArea;
        }

        return new AreaDefinition
        {
            Id = $"legacy_coord_{coord.X}_{coord.Y}",
            AreaId = CombatAreaId.LegacyCoord,
            DisplayName = $"Legacy Coord ({coord.X},{coord.Y})",
            Description = "Compatibility placeholder for the current board coordinate.",
            LegacyCoord = coord
        };
    }

    public static bool TryGetAreaIdForLegacyCoord(Vector2I coord, out CombatAreaId areaId)
    {
        areaId = coord switch
        {
            { X: 0, Y: 0 } => CombatAreaId.Qian,
            { X: 0, Y: 1 } => CombatAreaId.Dui,
            { X: 0, Y: 2 } => CombatAreaId.Li,
            { X: 1, Y: 0 } => CombatAreaId.Zhen,
            { X: 1, Y: 1 } => CombatAreaId.Xun,
            { X: 1, Y: 2 } => CombatAreaId.Kan,
            { X: 1, Y: 3 } => CombatAreaId.Gen,
            { X: 2, Y: 0 } => CombatAreaId.Kun,
            { X: 2, Y: 1 } => CombatAreaId.Yin,
            { X: 2, Y: 2 } => CombatAreaId.Yang,
            _ => CombatAreaId.Unknown
        };

        return areaId != CombatAreaId.Unknown;
    }

    public static string FormatLegacyCoord(Vector2I? coord)
    {
        if (!coord.HasValue)
        {
            return "未知位置";
        }

        AreaDefinition area = FromLegacyCoord(coord.Value);
        return area.AreaId == CombatAreaId.LegacyCoord
            ? $"({coord.Value.X},{coord.Value.Y})"
            : $"{area.DisplayName}({coord.Value.X},{coord.Value.Y})";
    }

    public static string GetDisplayName(CombatAreaId areaId)
    {
        return areaId switch
        {
            CombatAreaId.Qian => "乾",
            CombatAreaId.Dui => "兑",
            CombatAreaId.Li => "离",
            CombatAreaId.Zhen => "震",
            CombatAreaId.Xun => "巽",
            CombatAreaId.Kan => "坎",
            CombatAreaId.Gen => "艮",
            CombatAreaId.Kun => "坤",
            CombatAreaId.Yin => "阴",
            CombatAreaId.Yang => "阳",
            _ => areaId.ToString()
        };
    }

    public static string GetShortDescription(CombatAreaId areaId)
    {
        return areaId switch
        {
            CombatAreaId.Qian => "霄痕：造成伤害时加刻印；乾覆：范围伤害+30%。",
            CombatAreaId.Dui => "潭蓁：远程吸血回蓝；利镞：远程伤害+20%。",
            CombatAreaId.Li => "焚炙：造成伤害时施加燃烧；熔锋：对乾、兑伤害+50%。",
            CombatAreaId.Zhen => "电掣：移动优先级+1；惊蛰：对艮、坤伤害+50%。",
            CombatAreaId.Xun => "飒踏：获得闪避；罡风：回合结束获得1层罡风。",
            CombatAreaId.Kan => "甘霖：治疗+40%；湍扼：受近战伤害后本回合移动失效。",
            CombatAreaId.Gen => "峦障：进入或回合开始获得护盾；压顶：近战伤害+30%。",
            CombatAreaId.Kun => "厚德：回合结束回血；丰壤：近战伤害吸血。",
            CombatAreaId.Yin => "阴区域：当前作为至阴/至阳路径与教程区域使用。",
            CombatAreaId.Yang => "阳区域：当前作为至阴/至阳路径与教程区域使用。",
            _ => string.Empty
        };
    }

    public static string GetEncyclopediaText(CombatAreaId areaId)
    {
        return areaId switch
        {
            CombatAreaId.Qian => "霄痕：造成伤害时加刻印，持续6个时点（刻印：克制闪避）。乾覆：造成的范围伤害+30%。",
            CombatAreaId.Dui => "潭蓁：远程攻击恢复伤害量10%的生命值与MP。利镞：远程攻击+20%伤害。",
            CombatAreaId.Li => "焚炙：造成伤害时额外施加燃烧。熔锋：对乾、兑区域造成的伤害+50%。",
            CombatAreaId.Zhen => "电掣：移动类技能的优先级+1。惊蛰：对艮、坤区域造成的伤害+50%。",
            CombatAreaId.Xun => "飒踏：获得闪避。罡风：回合结束时，获得1层罡风；叠加至3层时受到最大生命值40%的伤害，触发后免疫罡风。",
            CombatAreaId.Kan => "甘霖：位于该区域时，施加的治疗增加40%。湍扼：若受到近战伤害，则本回合移动类技能失效。",
            CombatAreaId.Gen => "峦障：进入该区域或回合开始时，获得12%最大生命值的护盾。压顶：近战攻击+30%伤害。离开该区域后，该加成可持续一次近战攻击。",
            CombatAreaId.Kun => "厚德：回合结束时，回复6%生命值。丰壤：近战攻击恢复伤害量15%的生命值。离开该区域后，该加成可持续一次近战攻击。",
            CombatAreaId.Yin => "阴区域当前用于完整十区域站位和至阴路径，后续随至阴/至阳技能补齐特殊规则。",
            CombatAreaId.Yang => "阳区域当前用于完整十区域站位和至阳路径，后续随至阴/至阳技能补齐特殊规则。",
            _ => string.Empty
        };
    }

    public static AreaDefinition GetById(CombatAreaId areaId)
    {
        return CreateBaguaDefaults().FirstOrDefault(area => area.AreaId == areaId) ?? FromKnownArea(areaId);
    }

    private static void AddDefaultModifiers(AreaDefinition definition)
    {
        switch (definition.AreaId)
        {
            case CombatAreaId.Qian:
                definition.Modifiers.Add(new AreaModifier { Condition = "乾覆", TargetTags = SkillTag.Area, Operation = ModifierOperation.Multiply, Value = 1.3f });
                break;
            case CombatAreaId.Dui:
                definition.Modifiers.Add(new AreaModifier { Condition = "利镞", TargetTags = SkillTag.Ranged, Operation = ModifierOperation.Multiply, Value = 1.2f });
                break;
            case CombatAreaId.Kan:
                definition.Modifiers.Add(new AreaModifier { Condition = "甘霖", TargetTags = SkillTag.Heal, Operation = ModifierOperation.Multiply, Value = 1.4f });
                break;
            case CombatAreaId.Gen:
                definition.Modifiers.Add(new AreaModifier { Condition = "压顶", TargetTags = SkillTag.Melee, Operation = ModifierOperation.Multiply, Value = 1.3f, DurationPolicy = StatusDurationType.TriggerCount });
                break;
        }
    }
}

public class AreaModifier
{
    public string Condition { get; set; } = string.Empty;
    public SkillTag TargetTags { get; set; } = SkillTag.None;
    public ModifierOperation Operation { get; set; } = ModifierOperation.Add;
    public float Value { get; set; }
    public StatusDurationType DurationPolicy { get; set; } = StatusDurationType.Instant;
}

public class AreaTrigger
{
    public AreaEffectTiming Timing { get; set; } = AreaEffectTiming.RoundStart;
    public string Condition { get; set; } = string.Empty;
    public SkillEffectDefinition Effect { get; set; }
}

using Godot;
using System.Collections.Generic;

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
        return new AreaDefinition
        {
            Id = id,
            AreaId = areaId,
            DisplayName = string.IsNullOrEmpty(displayName) ? areaId.ToString() : displayName
        };
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
            FromKnownArea(CombatAreaId.Kun)
        };
    }

    public static AreaDefinition FromLegacyCoord(Vector2I coord)
    {
        return new AreaDefinition
        {
            Id = $"legacy_coord_{coord.X}_{coord.Y}",
            AreaId = CombatAreaId.LegacyCoord,
            DisplayName = $"Legacy Coord ({coord.X},{coord.Y})",
            Description = "Compatibility placeholder for the current board coordinate.",
            LegacyCoord = coord
        };
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

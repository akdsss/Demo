using Godot;
using System.Collections.Generic;

public static class BattleAssetCatalog
{
    private const string AreaIconRoot = "res://Asset/Generated/AreaIcons";
    private const string StatusIconRoot = "res://Asset/Generated/StatusIcons";
    private const string PresentationRoot = "res://Asset/Generated/CutscenePlaceholders";

    private static readonly Dictionary<CombatAreaId, string> areaIconPaths = new()
    {
        { CombatAreaId.Qian, $"{AreaIconRoot}/area_qian.png" },
        { CombatAreaId.Dui, $"{AreaIconRoot}/area_dui.png" },
        { CombatAreaId.Li, $"{AreaIconRoot}/area_li.png" },
        { CombatAreaId.Zhen, $"{AreaIconRoot}/area_zhen.png" },
        { CombatAreaId.Xun, $"{AreaIconRoot}/area_xun.png" },
        { CombatAreaId.Kan, $"{AreaIconRoot}/area_kan.png" },
        { CombatAreaId.Gen, $"{AreaIconRoot}/area_gen.png" },
        { CombatAreaId.Kun, $"{AreaIconRoot}/area_kun.png" },
        { CombatAreaId.Yin, $"{AreaIconRoot}/area_yin.png" },
        { CombatAreaId.Yang, $"{AreaIconRoot}/area_yang.png" },
    };

    private static readonly Dictionary<string, string> statusIconPaths = new()
    {
        { StatusCatalog.Dodge, $"{StatusIconRoot}/status_dodge.png" },
        { StatusCatalog.Mark, $"{StatusIconRoot}/status_mark.png" },
        { StatusCatalog.Shield, $"{StatusIconRoot}/status_shield.png" },
        { StatusCatalog.Burn, $"{StatusIconRoot}/status_burn.png" },
        { StatusCatalog.Gale, $"{StatusIconRoot}/status_gale.png" },
        { StatusCatalog.MoveBlocked, $"{StatusIconRoot}/status_move_blocked.png" },
        { StatusCatalog.GenMeleeCarryover, $"{StatusIconRoot}/status_gen_carry.png" },
        { StatusCatalog.KunMeleeDrainCarryover, $"{StatusIconRoot}/status_kun_carry.png" },
        { StatusCatalog.Rage, $"{StatusIconRoot}/status_rage.png" },
    };

    public static string GetAreaIconPath(CombatAreaId areaId)
    {
        return areaIconPaths.TryGetValue(areaId, out string path) ? path : string.Empty;
    }

    public static Texture2D GetAreaIconTexture(CombatAreaId areaId)
    {
        return LoadTexture(GetAreaIconPath(areaId));
    }

    public static string GetStatusIconPath(string statusId)
    {
        return !string.IsNullOrEmpty(statusId) && statusIconPaths.TryGetValue(statusId, out string path)
            ? path
            : string.Empty;
    }

    public static Texture2D GetStatusIconTexture(string statusId)
    {
        return LoadTexture(GetStatusIconPath(statusId));
    }

    public static Texture2D GetPresentationTexture(CombatEventType eventType)
    {
        string path = eventType switch
        {
            CombatEventType.CharacterMoved => $"{PresentationRoot}/presentation_move.png",
            CombatEventType.DamageApplied => $"{PresentationRoot}/presentation_damage.png",
            CombatEventType.HealApplied => $"{PresentationRoot}/presentation_heal.png",
            CombatEventType.StatusApplied or CombatEventType.StatusRemoved => $"{PresentationRoot}/presentation_status.png",
            _ => $"{PresentationRoot}/presentation_default.png"
        };

        return LoadTexture(path);
    }

    private static Texture2D LoadTexture(string path)
    {
        return string.IsNullOrEmpty(path) ? null : ResourceLoader.Load<Texture2D>(path);
    }
}

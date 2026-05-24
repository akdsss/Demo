public static class CombatEventLogFormatter
{
    public static string Format(CombatEvent combatEvent)
    {
        if (combatEvent == null)
        {
            return string.Empty;
        }

        string sourceName = combatEvent.Source?.DisplayName
            ?? combatEvent.SourceLegacyCharacterData?.characterName
            ?? "未知角色";
        string targetName = combatEvent.Target?.DisplayName
            ?? combatEvent.TargetLegacyCharacterData?.characterName
            ?? "未知目标";
        string skillName = combatEvent.Skill?.DisplayName ?? "未知指令";

        return combatEvent.EventType switch
        {
            CombatEventType.RoundStarted => combatEvent.Message,
            CombatEventType.RoundEnded => combatEvent.Message,
            CombatEventType.ActionStarted => $"{combatEvent.SlotIndex} 时点：{sourceName} 使用 {skillName}",
            CombatEventType.SkillFailed => FormatSkillFailed(combatEvent, sourceName, skillName),
            CombatEventType.CharacterMoved => $"{sourceName} 从 {FormatArea(combatEvent.FromAreaId, combatEvent.FromCoord)} 移动到 {FormatArea(combatEvent.ToAreaId, combatEvent.ToCoord)}",
            CombatEventType.DamageApplied => FormatDamage(combatEvent, sourceName, targetName),
            CombatEventType.HealApplied => $"{sourceName} 为 {targetName} 恢复 {combatEvent.Amount} 点生命",
            CombatEventType.MpChanged => combatEvent.Amount < 0
                ? $"{targetName} 消耗 {-combatEvent.Amount} 点 MP"
                : $"{targetName} 恢复 {combatEvent.Amount} 点 MP",
            CombatEventType.IntentRevealed => combatEvent.Message,
            CombatEventType.DefenseApplied => $"{sourceName} 进入防御",
            CombatEventType.CharacterDefeated => $"{targetName} 失去战斗能力",
            CombatEventType.StatusApplied => $"{targetName} 获得状态 {combatEvent.StatusId}",
            CombatEventType.StatusRemoved => $"{targetName} 移除状态 {combatEvent.StatusId}",
            CombatEventType.ScheduledEffectCreated => $"{sourceName} 生成延迟效果",
            _ => combatEvent.Message
        };
    }

    private static string FormatSkillFailed(CombatEvent combatEvent, string sourceName, string skillName)
    {
        string prefix = combatEvent.SlotIndex > 0
            ? $"{combatEvent.SlotIndex} 时点："
            : string.Empty;
        return $"{prefix}{sourceName} 使用 {skillName} 释放失败：{FormatFailReason(combatEvent)}";
    }

    private static string FormatFailReason(CombatEvent combatEvent)
    {
        string targetName = combatEvent.Target?.DisplayName
            ?? combatEvent.TargetLegacyCharacterData?.characterName
            ?? "未知目标";

        return combatEvent.FailReason switch
        {
            SkillFailReason.MissingSkill => "指令数据缺失",
            SkillFailReason.MissingSource => "释放者不存在",
            SkillFailReason.SourceDefeated => "释放者已战败",
            SkillFailReason.ActionChanceInsufficient => "行动次数不足",
            SkillFailReason.MpInsufficient => FormatMpInsufficient(combatEvent),
            SkillFailReason.MissingTarget => FormatMissingTarget(combatEvent),
            SkillFailReason.TargetDefeated => $"目标已战败（{targetName}）",
            SkillFailReason.MeleeTargetNotInSameArea => $"攻击目标在不同区域（释放者：{FormatCharacterArea(combatEvent.Source, combatEvent.SourceLegacyCharacterData)}，目标：{FormatCharacterArea(combatEvent.Target, combatEvent.TargetLegacyCharacterData)}）",
            SkillFailReason.MoveTargetAlreadyCurrentArea => $"移动目标为相同区域（当前位置：{FormatCharacterArea(combatEvent.Source, combatEvent.SourceLegacyCharacterData)}）",
            SkillFailReason.RushTargetMustBeInDifferentArea => $"移动目标为相同区域（{targetName} 已处于 {FormatTargetArea(combatEvent)}）",
            SkillFailReason.TimelineSlotUnavailable => "目标时点不可用",
            SkillFailReason.BlockedByStatus => "当前状态限制，无法释放",
            SkillFailReason.TargetEvaded => $"目标闪避（{targetName}）",
            _ => string.IsNullOrEmpty(combatEvent.Message) ? combatEvent.FailReason.ToString() : combatEvent.Message
        };
    }

    private static string FormatMpInsufficient(CombatEvent combatEvent)
    {
        int currentMp = combatEvent.Source?.CurrentMp
            ?? combatEvent.SourceLegacyCharacterData?.mp
            ?? 0;
        int requiredMp = combatEvent.Skill?.MpCost ?? 0;
        return $"MP不足（当前 {currentMp} / 需要 {requiredMp}）";
    }

    private static string FormatMissingTarget(CombatEvent combatEvent)
    {
        if (combatEvent.Skill != null &&
            (combatEvent.Skill.TargetType == SkillTargetType.Area || combatEvent.Skill.HasTag(SkillTag.Area)))
        {
            return "目标区域内没有有效目标";
        }

        return "缺少有效目标";
    }

    private static string FormatCharacterArea(CharacterState characterState, CharacterData legacyCharacterData)
    {
        CombatAreaId areaId = characterState?.CurrentAreaId
            ?? legacyCharacterData?.ResolveCurrentAreaId()
            ?? CombatAreaId.Unknown;
        Godot.Vector2I? fallbackCoord = characterState?.LegacyCoord;
        if (!fallbackCoord.HasValue && legacyCharacterData != null)
        {
            fallbackCoord = legacyCharacterData.coord;
        }

        return FormatArea(areaId, fallbackCoord);
    }

    private static string FormatTargetArea(CombatEvent combatEvent)
    {
        CombatAreaId areaId = combatEvent.TargetAreaId;
        if (areaId == CombatAreaId.Unknown)
        {
            areaId = combatEvent.ToAreaId;
        }
        if (areaId == CombatAreaId.Unknown)
        {
            areaId = combatEvent.Target?.CurrentAreaId ?? CombatAreaId.Unknown;
        }
        if (areaId == CombatAreaId.Unknown && combatEvent.TargetLegacyCharacterData != null)
        {
            areaId = combatEvent.TargetLegacyCharacterData.ResolveCurrentAreaId();
        }

        return FormatArea(areaId, combatEvent.ToCoord);
    }

    private static string FormatCoord(Godot.Vector2I? coord)
    {
        return AreaDefinition.FormatLegacyCoord(coord);
    }

    private static string FormatArea(CombatAreaId areaId, Godot.Vector2I? fallbackCoord)
    {
        return areaId == CombatAreaId.Unknown
            ? FormatCoord(fallbackCoord)
            : AreaDefinition.FormatAreaId(areaId);
    }

    private static string FormatDamage(CombatEvent combatEvent, string sourceName, string targetName)
    {
        string cause = string.IsNullOrEmpty(combatEvent.StatusId) ? string.Empty : $"（{combatEvent.StatusId}）";
        return string.IsNullOrEmpty(combatEvent.Source?.DisplayName) && !string.IsNullOrEmpty(combatEvent.StatusId)
            ? $"{targetName} 受到 {combatEvent.Amount} 点{cause}伤害"
            : $"{sourceName} 对 {targetName} 造成 {combatEvent.Amount} 点{cause}伤害";
    }
}

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
            CombatEventType.SkillFailed => $"{combatEvent.SlotIndex} 时点：{sourceName} 的 {skillName} 失败（{combatEvent.FailReason}）",
            CombatEventType.CharacterMoved => $"{sourceName} 从 {FormatCoord(combatEvent.FromCoord)} 移动到 {FormatCoord(combatEvent.ToCoord)}",
            CombatEventType.DamageApplied => $"{sourceName} 对 {targetName} 造成 {combatEvent.Amount} 点伤害",
            CombatEventType.HealApplied => $"{sourceName} 为 {targetName} 恢复 {combatEvent.Amount} 点生命",
            CombatEventType.DefenseApplied => $"{sourceName} 进入防御",
            CombatEventType.CharacterDefeated => $"{targetName} 失去战斗能力",
            CombatEventType.StatusApplied => $"{targetName} 获得状态 {combatEvent.StatusId}",
            CombatEventType.StatusRemoved => $"{targetName} 移除状态 {combatEvent.StatusId}",
            CombatEventType.ScheduledEffectCreated => $"{sourceName} 生成延迟效果",
            _ => combatEvent.Message
        };
    }

    private static string FormatCoord(Godot.Vector2I? coord)
    {
        return coord.HasValue ? $"({coord.Value.X},{coord.Value.Y})" : "未知位置";
    }
}

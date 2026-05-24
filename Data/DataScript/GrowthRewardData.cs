using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

[GlobalClass]
public partial class GrowthRewardData : Resource
{
    [Export] public string rewardId = string.Empty;
    [Export(PropertyHint.MultilineText)] public string uiSummaryText = string.Empty;
    [Export] public CharacterData[] characterDataArray = Array.Empty<CharacterData>();
    [Export] public string[] statChangeTextArray = Array.Empty<string>();
    [Export] public string[] unlockSkillTextArray = Array.Empty<string>();
    [Export] public int[] maxHpChangeArray = Array.Empty<int>();
    [Export] public int[] maxMpChangeArray = Array.Empty<int>();
    [Export] public int[] actionTimesChangeArray = Array.Empty<int>();
    [Export] public float[] attackChangeArray = Array.Empty<float>();
    [Export] public PlayerCommandData[] unlockSkillDataArray = Array.Empty<PlayerCommandData>();
    [Export] public PlayerCommandData[] upgradeSkillDataArray = Array.Empty<PlayerCommandData>();
    [Export] public int[] upgradePriorityDeltaArray = Array.Empty<int>();
    [Export] public string[] upgradedDescriptionArray = Array.Empty<string>();
    private bool hasApplied;

    public int EntryCount
    {
        get
        {
            return Math.Max(
                characterDataArray?.Length ?? 0,
                Math.Max(
                    Math.Max(statChangeTextArray?.Length ?? 0, unlockSkillTextArray?.Length ?? 0),
                    Math.Max(
                        Math.Max(
                            Math.Max(maxHpChangeArray?.Length ?? 0, maxMpChangeArray?.Length ?? 0),
                            Math.Max(actionTimesChangeArray?.Length ?? 0, attackChangeArray?.Length ?? 0)),
                        Math.Max(unlockSkillDataArray?.Length ?? 0, upgradeSkillDataArray?.Length ?? 0))));
        }
    }

    public CharacterData GetCharacter(int index)
    {
        return index >= 0 && index < (characterDataArray?.Length ?? 0)
            ? characterDataArray[index]
            : null;
    }

    public string GetStatChangeText(int index)
    {
        string explicitText = index >= 0 && index < (statChangeTextArray?.Length ?? 0)
            ? statChangeTextArray[index]
            : string.Empty;
        if (!string.IsNullOrEmpty(explicitText))
        {
            return explicitText;
        }

        List<string> parts = new();
        int maxHpChange = GetMaxHpChange(index);
        int maxMpChange = GetMaxMpChange(index);
        int actionTimesChange = GetActionTimesChange(index);
        float attackChange = GetAttackChange(index);
        if (maxHpChange != 0)
        {
            parts.Add($"最大生命 {(maxHpChange > 0 ? "+" : string.Empty)}{maxHpChange}");
        }
        if (Math.Abs(attackChange) > 0.001f)
        {
            parts.Add($"攻击 {(attackChange > 0 ? "+" : string.Empty)}{attackChange}");
        }
        if (maxMpChange != 0)
        {
            parts.Add($"MP {(maxMpChange > 0 ? "+" : string.Empty)}{maxMpChange}");
        }
        if (actionTimesChange != 0)
        {
            parts.Add($"行动点 {(actionTimesChange > 0 ? "+" : string.Empty)}{actionTimesChange}");
        }

        return parts.Count > 0 ? string.Join("，", parts) : string.Empty;
    }

    public string GetUnlockSkillText(int index)
    {
        string explicitText = index >= 0 && index < (unlockSkillTextArray?.Length ?? 0)
            ? unlockSkillTextArray[index]
            : string.Empty;
        if (!string.IsNullOrEmpty(explicitText))
        {
            return explicitText;
        }

        List<string> parts = new();
        PlayerCommandData unlockSkill = GetUnlockSkill(index);
        PlayerCommandData upgradeSkill = GetUpgradeSkill(index);
        if (unlockSkill != null)
        {
            parts.Add($"解锁 {unlockSkill.commandName}");
        }
        if (upgradeSkill != null)
        {
            parts.Add($"升级 {upgradeSkill.commandName}");
        }

        return parts.Count > 0 ? string.Join("，", parts) : string.Empty;
    }

    public void ApplyToCharacters(IEnumerable<PlayerData> fallbackCharacters)
    {
        if (hasApplied)
        {
            return;
        }

        List<PlayerData> fallbackList = fallbackCharacters?
            .Where(character => character != null)
            .ToList() ?? new List<PlayerData>();
        int entryCount = Math.Max(EntryCount, fallbackList.Count);
        for (int i = 0; i < entryCount; i++)
        {
            PlayerData target = GetCharacter(i) as PlayerData ?? (i < fallbackList.Count ? fallbackList[i] : null);
            if (target == null)
            {
                continue;
            }

            ApplyStats(target, i);
            ApplySkillUnlock(target, GetUnlockSkill(i));
            ApplySkillUpgrade(target, GetUpgradeSkill(i), GetUpgradePriorityDelta(i), GetUpgradedDescription(i));
        }

        hasApplied = true;
    }

    private int GetMaxHpChange(int index)
    {
        return index >= 0 && index < (maxHpChangeArray?.Length ?? 0)
            ? maxHpChangeArray[index]
            : 0;
    }

    private int GetMaxMpChange(int index)
    {
        return index >= 0 && index < (maxMpChangeArray?.Length ?? 0)
            ? maxMpChangeArray[index]
            : 0;
    }

    private int GetActionTimesChange(int index)
    {
        return index >= 0 && index < (actionTimesChangeArray?.Length ?? 0)
            ? actionTimesChangeArray[index]
            : 0;
    }

    private float GetAttackChange(int index)
    {
        return index >= 0 && index < (attackChangeArray?.Length ?? 0)
            ? attackChangeArray[index]
            : 0;
    }

    private PlayerCommandData GetUnlockSkill(int index)
    {
        return index >= 0 && index < (unlockSkillDataArray?.Length ?? 0)
            ? unlockSkillDataArray[index]
            : null;
    }

    private PlayerCommandData GetUpgradeSkill(int index)
    {
        return index >= 0 && index < (upgradeSkillDataArray?.Length ?? 0)
            ? upgradeSkillDataArray[index]
            : null;
    }

    private int GetUpgradePriorityDelta(int index)
    {
        return index >= 0 && index < (upgradePriorityDeltaArray?.Length ?? 0)
            ? upgradePriorityDeltaArray[index]
            : 0;
    }

    private string GetUpgradedDescription(int index)
    {
        return index >= 0 && index < (upgradedDescriptionArray?.Length ?? 0)
            ? upgradedDescriptionArray[index]
            : string.Empty;
    }

    private void ApplyStats(PlayerData target, int index)
    {
        int maxHpChange = GetMaxHpChange(index);
        int maxMpChange = GetMaxMpChange(index);
        int actionTimesChange = GetActionTimesChange(index);
        float attackChange = GetAttackChange(index);
        if (maxHpChange != 0)
        {
            target.maxHp = Math.Max(1, target.maxHp + maxHpChange);
            target.hp = Math.Min(target.maxHp, target.hp + Math.Max(0, maxHpChange));
        }

        if (Math.Abs(attackChange) > 0.001f)
        {
            target.atk = Math.Max(0, target.atk + attackChange);
        }

        if (maxMpChange != 0)
        {
            target.maxMp = Math.Max(0, target.maxMp + maxMpChange);
            target.mp = Math.Min(target.maxMp, target.mp + Math.Max(0, maxMpChange));
        }

        if (actionTimesChange != 0)
        {
            target.turnInitialActionTimes = Math.Max(0, target.turnInitialActionTimes + actionTimesChange);
        }
    }

    private static void ApplySkillUnlock(PlayerData target, PlayerCommandData skill)
    {
        if (target == null || skill == null)
        {
            return;
        }

        List<PlayerCommandData> skills = target.playerCommandDataList?
            .Where(command => command != null)
            .ToList() ?? new List<PlayerCommandData>();
        if (skills.Any(command => command.commandId == skill.commandId))
        {
            return;
        }

        skills.Add(skill);
        target.playerCommandDataList = skills.ToArray();
    }

    private static void ApplySkillUpgrade(
        PlayerData target,
        PlayerCommandData skill,
        int priorityDelta,
        string upgradedDescription)
    {
        if (target == null || skill == null)
        {
            return;
        }

        ApplySkillUnlock(target, skill);
        PlayerCommandData existing = target.playerCommandDataList?
            .FirstOrDefault(command => command != null && command.commandId == skill.commandId);
        if (existing == null)
        {
            return;
        }

        existing.priority += priorityDelta;
        if (!string.IsNullOrEmpty(upgradedDescription))
        {
            existing.commandDescription = upgradedDescription;
        }
    }
}

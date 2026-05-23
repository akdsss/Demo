using Godot;
using System;
using System.Collections.Generic;

public class CharacterState
{
    public int LegacyCharacterId { get; set; } = -1;
    public string StableId { get; set; } = "unknown_character";
    public string DisplayName { get; set; } = "Unknown Character";
    public string Description { get; set; } = string.Empty;
    public CombatFaction Faction { get; set; } = CombatFaction.Unknown;
    public float Hp { get; set; }
    public float MaxHp { get; set; }
    public float Attack { get; set; }
    public int CurrentMp { get; set; }
    public int MaxMp { get; set; }
    public int ActionsPerRound { get; set; }
    public int RemainingActions { get; set; }
    public float ShieldValue { get; set; }
    public Vector2I LegacyCoord { get; set; }
    public CombatAreaId CurrentAreaId { get; set; } = CombatAreaId.Unknown;
    public AreaDefinition CurrentArea { get; set; }
    public CharacterBattleState BattleState { get; set; } = CharacterBattleState.ALIVE;
    public CharacterData LegacyCharacterData { get; set; }
    public List<StatusInstance> Statuses { get; } = new();

    public bool IsDefeated
    {
        get
        {
            return Hp <= 0 || BattleState == CharacterBattleState.DEAD || BattleState == CharacterBattleState.DYING;
        }
    }

    public static CharacterState FromCharacterData(CharacterData characterData)
    {
        CharacterState state = new();
        if (characterData == null)
        {
            return state;
        }

        state.LegacyCharacterData = characterData;
        state.LegacyCharacterId = characterData.characterId;
        state.StableId = $"legacy_character_{characterData.characterId}";
        state.DisplayName = string.IsNullOrEmpty(characterData.characterName)
            ? state.StableId
            : characterData.characterName;
        state.Description = characterData.characterDescription ?? string.Empty;
        state.Faction = InferFaction(characterData);
        state.Hp = characterData.hp;
        state.MaxHp = characterData.maxHp;
        state.Attack = characterData.atk;
        state.CurrentMp = characterData.mp;
        state.MaxMp = characterData.maxMp;
        state.ActionsPerRound = characterData.turnInitialActionTimes;
        state.RemainingActions = characterData.currentRestActionTimes;
        state.ShieldValue = characterData.runtimeShieldValue;
        state.CurrentAreaId = characterData.ResolveCurrentAreaId();
        state.LegacyCoord = AreaDefinition.GetLegacyCoordForAreaId(state.CurrentAreaId);
        state.CurrentArea = AreaDefinition.FromKnownArea(state.CurrentAreaId);
        state.BattleState = characterData.characterBattleState;
        state.LoadRuntimeStatuses(characterData);
        return state;
    }

    public void RefreshFromLegacyData()
    {
        if (LegacyCharacterData == null)
        {
            return;
        }

        CharacterState refreshed = FromCharacterData(LegacyCharacterData);
        LegacyCharacterId = refreshed.LegacyCharacterId;
        StableId = refreshed.StableId;
        DisplayName = refreshed.DisplayName;
        Description = refreshed.Description;
        Faction = refreshed.Faction;
        Hp = refreshed.Hp;
        MaxHp = refreshed.MaxHp;
        Attack = refreshed.Attack;
        ActionsPerRound = refreshed.ActionsPerRound;
        RemainingActions = refreshed.RemainingActions;
        ShieldValue = refreshed.ShieldValue;
        LegacyCoord = refreshed.LegacyCoord;
        CurrentAreaId = refreshed.CurrentAreaId;
        CurrentArea = refreshed.CurrentArea;
        BattleState = refreshed.BattleState;
        Statuses.Clear();
        LoadRuntimeStatuses(LegacyCharacterData);
    }

    public bool HasBlockedTag(SkillTag skillTags)
    {
        foreach (StatusInstance status in Statuses)
        {
            if (status?.Definition == null)
            {
                continue;
            }

            if ((status.Definition.TagsBlocked & skillTags) != SkillTag.None)
            {
                return true;
            }
        }

        return false;
    }

    public bool HasStatus(string statusId)
    {
        return GetStatus(statusId) != null;
    }

    public StatusInstance GetStatus(string statusId)
    {
        if (string.IsNullOrEmpty(statusId))
        {
            return null;
        }

        foreach (StatusInstance status in Statuses)
        {
            if (status?.Definition?.Id == statusId && !status.IsExpired)
            {
                return status;
            }
        }

        return null;
    }

    public StatusInstance AddOrRefreshStatus(StatusDefinition definition, CharacterState source = null)
    {
        if (definition == null)
        {
            return null;
        }

        StatusInstance existing = GetStatus(definition.Id);
        if (existing != null)
        {
            switch (definition.StackMode)
            {
                case StatusStackMode.RefreshDuration:
                    existing.RemainingDuration = definition.DurationValue;
                    break;
                case StatusStackMode.StackCount:
                    existing.StackCount++;
                    existing.RemainingDuration = definition.DurationValue;
                    break;
            }
            return existing;
        }

        StatusInstance status = definition.CreateInstance(this, source);
        Statuses.Add(status);
        return status;
    }

    public bool RemoveStatus(string statusId)
    {
        StatusInstance status = GetStatus(statusId);
        if (status == null)
        {
            return false;
        }

        Statuses.Remove(status);
        return true;
    }

    private static CombatFaction InferFaction(CharacterData characterData)
    {
        return characterData switch
        {
            PlayerData => CombatFaction.Player,
            EnemyData => CombatFaction.Enemy,
            _ => CombatFaction.Unknown
        };
    }

    private void LoadRuntimeStatuses(CharacterData characterData)
    {
        if (characterData.runtimeStatusIds == null)
        {
            characterData.runtimeStatusIds = new List<string>();
        }

        if (characterData.runtimeStatusStacks == null)
        {
            characterData.runtimeStatusStacks = new List<int>();
        }

        for (int i = 0; i < characterData.runtimeStatusIds.Count; i++)
        {
            string statusId = characterData.runtimeStatusIds[i];
            if (string.IsNullOrEmpty(statusId))
            {
                continue;
            }

            StatusInstance status = StatusCatalog.Create(statusId).CreateInstance(this);
            if (i < characterData.runtimeStatusStacks.Count)
            {
                status.StackCount = Math.Max(1, characterData.runtimeStatusStacks[i]);
            }
            Statuses.Add(status);
        }
    }
}

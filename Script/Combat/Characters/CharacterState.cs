using Godot;
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
    public Vector2I LegacyCoord { get; set; }
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
        state.ActionsPerRound = characterData.turnInitialActionTimes;
        state.RemainingActions = characterData.currentRestActionTimes;
        state.LegacyCoord = characterData.coord;
        state.CurrentArea = AreaDefinition.FromLegacyCoord(characterData.coord);
        state.BattleState = characterData.characterBattleState;
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
        LegacyCoord = refreshed.LegacyCoord;
        CurrentArea = refreshed.CurrentArea;
        BattleState = refreshed.BattleState;
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

    private static CombatFaction InferFaction(CharacterData characterData)
    {
        return characterData switch
        {
            PlayerData => CombatFaction.Player,
            EnemyData => CombatFaction.Enemy,
            _ => CombatFaction.Unknown
        };
    }
}

using Godot;

public static class CombatEventApplier
{
    public static void ApplyToLegacyState(CombatEvent combatEvent)
    {
        if (combatEvent == null)
        {
            return;
        }

        switch (combatEvent.EventType)
        {
            case CombatEventType.CharacterMoved:
                ApplyMove(combatEvent);
                break;
            case CombatEventType.DamageApplied:
            case CombatEventType.HealApplied:
                ApplyHpChange(combatEvent);
                break;
            case CombatEventType.CharacterDefeated:
                ApplyDefeated(combatEvent);
                break;
        }
    }

    private static void ApplyMove(CombatEvent combatEvent)
    {
        if (combatEvent.SourceLegacyCharacterData == null || !combatEvent.ToCoord.HasValue)
        {
            return;
        }

        ChessBoard chessBoard = Autoloads.gd_ChessBoard;
        if (chessBoard == null)
        {
            combatEvent.SourceLegacyCharacterData.coord = combatEvent.ToCoord.Value;
            return;
        }

        chessBoard.MoveCharacter(combatEvent.SourceLegacyCharacterData, combatEvent.ToCoord.Value);
    }

    private static void ApplyHpChange(CombatEvent combatEvent)
    {
        if (combatEvent.TargetLegacyCharacterData == null)
        {
            return;
        }

        combatEvent.TargetLegacyCharacterData.hp = combatEvent.TargetHpAfter;
        UpdateCharacterDisplays();
    }

    private static void ApplyDefeated(CombatEvent combatEvent)
    {
        if (combatEvent.TargetLegacyCharacterData == null)
        {
            return;
        }

        combatEvent.TargetLegacyCharacterData.hp = combatEvent.TargetHpAfter;
        combatEvent.TargetLegacyCharacterData.characterBattleState = CharacterBattleState.DEAD;
        UpdateCharacterDisplays();
    }

    private static void UpdateCharacterDisplays()
    {
        if (Autoloads.sceneSingleton == null)
        {
            return;
        }

        Autoloads.sceneSingleton.playerCharacterHeadListUIControl?.UpdateAllUIDisplay();
        Autoloads.sceneSingleton.enemyCharacterHeadListUIControl?.UpdateAllUIDisplay();
    }
}

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
            case CombatEventType.StatusApplied:
                ApplyStatus(combatEvent);
                break;
            case CombatEventType.StatusRemoved:
                RemoveStatus(combatEvent);
                break;
            case CombatEventType.MpChanged:
                UpdateCharacterDisplays();
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
        if (combatEvent.Target != null)
        {
            combatEvent.TargetLegacyCharacterData.runtimeShieldValue = combatEvent.Target.ShieldValue;
        }
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

    private static void ApplyStatus(CombatEvent combatEvent)
    {
        CharacterData target = combatEvent.TargetLegacyCharacterData;
        if (target == null || string.IsNullOrEmpty(combatEvent.StatusId))
        {
            return;
        }

        EnsureRuntimeStatusLists(target);
        int index = target.runtimeStatusIds.IndexOf(combatEvent.StatusId);
        int stackCount = combatEvent.Target?.GetStatus(combatEvent.StatusId)?.StackCount ?? 1;
        if (index >= 0)
        {
            target.runtimeStatusStacks[index] = stackCount;
        }
        else
        {
            target.runtimeStatusIds.Add(combatEvent.StatusId);
            target.runtimeStatusStacks.Add(stackCount);
        }

        if (combatEvent.StatusId == StatusCatalog.Shield && combatEvent.Target != null)
        {
            target.runtimeShieldValue = combatEvent.Target.ShieldValue;
        }
        UpdateCharacterDisplays();
    }

    private static void RemoveStatus(CombatEvent combatEvent)
    {
        CharacterData target = combatEvent.TargetLegacyCharacterData;
        if (target == null || string.IsNullOrEmpty(combatEvent.StatusId))
        {
            return;
        }

        EnsureRuntimeStatusLists(target);
        int index = target.runtimeStatusIds.IndexOf(combatEvent.StatusId);
        if (index >= 0)
        {
            target.runtimeStatusIds.RemoveAt(index);
            target.runtimeStatusStacks.RemoveAt(index);
        }

        if (combatEvent.StatusId == StatusCatalog.Shield)
        {
            target.runtimeShieldValue = 0;
        }
        UpdateCharacterDisplays();
    }

    private static void EnsureRuntimeStatusLists(CharacterData target)
    {
        target.runtimeStatusIds ??= new System.Collections.Generic.List<string>();
        target.runtimeStatusStacks ??= new System.Collections.Generic.List<int>();
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

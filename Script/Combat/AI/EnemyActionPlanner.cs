using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public class EnemyActionPlan
{
    public int SlotIndex { get; set; } = Timeline.MinSlotIndex;
    public CommandExecuteInfo CommandExecuteInfo { get; set; }
    public bool IsValid => CommandExecuteInfo != null && !CommandExecuteInfo.isDefault;
}

public class EnemyActionPlanner
{
    private static readonly Vector2I[] FallbackMoveCoords =
    {
        new(1, 1),
        new(1, 2),
        new(0, 1),
        new(2, 1),
        new(1, 0),
        new(1, 3)
    };
    private static readonly Vector2I YangCoord = new(2, 2);
    private static readonly Vector2I YinCoord = new(2, 1);

    public EnemyActionPlan PlanNextAction(
        EnemyData enemyData,
        IReadOnlyList<PlayerData> playerDataList,
        IReadOnlyList<EnemyData> enemyDataList,
        int roundIndex)
    {
        if (enemyData == null || enemyData.currentRestActionTimes <= 0)
        {
            return null;
        }

        int slotIndex = FindFirstFreeSlot(enemyData);
        if (slotIndex < Timeline.MinSlotIndex)
        {
            return null;
        }

        List<EnemyCommandData> commands = enemyData.enemyCommandDataArray?
            .Where(command => command != null)
            .ToList() ?? new List<EnemyCommandData>();
        if (commands.Count == 0)
        {
            return null;
        }

        return enemyData.aiProfile switch
        {
            EnemyAiProfile.Melee => PlanMelee(enemyData, commands, playerDataList),
            EnemyAiProfile.Ranged => PlanRanged(enemyData, commands, playerDataList),
            EnemyAiProfile.Assassin => PlanAssassin(enemyData, commands, playerDataList),
            EnemyAiProfile.Support => PlanSupport(enemyData, commands, playerDataList, enemyDataList),
            EnemyAiProfile.EliteMelee => PlanEliteMelee(enemyData, commands, playerDataList, roundIndex),
            EnemyAiProfile.EliteSupport => PlanEliteSupport(enemyData, commands, playerDataList, enemyDataList),
            _ => PlanBasic(enemyData, commands, playerDataList)
        };
    }

    private static EnemyActionPlan PlanBasic(
        EnemyData enemyData,
        List<EnemyCommandData> commands,
        IReadOnlyList<PlayerData> playerDataList)
    {
        int slotIndex = FindFirstFreeSlot(enemyData);
        Vector2I projectedCoord = GetProjectedCoord(enemyData);
        CombatAreaId projectedAreaId = GetProjectedAreaId(enemyData);
        List<PlayerData> livingPlayers = playerDataList?
            .Where(IsAlive)
            .ToList() ?? new List<PlayerData>();

        PlayerData sameAreaTarget = livingPlayers
            .Where(player => IsInArea(player, projectedAreaId))
            .OrderBy(player => player.hp)
            .FirstOrDefault();
        PlayerData nearestTarget = livingPlayers
            .OrderBy(player => Distance(projectedCoord, player.coord))
            .ThenBy(player => player.hp)
            .FirstOrDefault();

        EnemyCommandData attackCommand = FindBestCommand(commands, IsAttackCommand);
        EnemyCommandData moveCommand = FindBestCommand(commands, IsMoveCommand);
        EnemyCommandData skipCommand = FindBestCommand(commands, IsSkipCommand);

        EnemyCommandData selectedCommand = null;
        CharacterData targetCharacter = null;
        Vector2I targetCoord = projectedCoord;

        if (sameAreaTarget != null && attackCommand != null)
        {
            selectedCommand = attackCommand;
            targetCharacter = sameAreaTarget;
        }
        else if (nearestTarget != null && moveCommand != null && !IsInArea(nearestTarget, projectedAreaId))
        {
            selectedCommand = moveCommand;
            targetCoord = AreaDefinition.GetLegacyCoordForAreaId(nearestTarget.ResolveCurrentAreaId());
        }
        else if (nearestTarget != null && attackCommand != null)
        {
            selectedCommand = attackCommand;
            targetCharacter = nearestTarget;
        }
        else if (moveCommand != null)
        {
            selectedCommand = moveCommand;
            targetCoord = FindFallbackMoveCoord(projectedCoord);
        }
        else
        {
            selectedCommand = skipCommand ?? commands
                .OrderByDescending(command => SkillDefinition.FromCommandData(command).Priority)
                .ThenBy(command => command.commandId)
                .FirstOrDefault();
        }

        if (selectedCommand == null)
        {
            return null;
        }

        return BuildPlan(enemyData, selectedCommand, slotIndex, targetCharacter, targetCoord);
    }

    private static EnemyActionPlan PlanMelee(
        EnemyData enemyData,
        List<EnemyCommandData> commands,
        IReadOnlyList<PlayerData> playerDataList)
    {
        int slotIndex = FindFirstFreeSlot(enemyData);
        Vector2I projectedCoord = GetProjectedCoord(enemyData);
        CombatAreaId projectedAreaId = GetProjectedAreaId(enemyData);
        List<PlayerData> livingPlayers = GetLivingPlayers(playerDataList);
        PlayerData sameAreaTarget = GetLowestHpPlayer(livingPlayers.Where(player => IsInArea(player, projectedAreaId)));
        PlayerData nearestTarget = GetNearestPlayer(livingPlayers, projectedCoord);
        EnemyCommandData chargeCommand = FindCommandById(commands, 4) ?? FindCommandById(commands, 8);
        EnemyCommandData meleeCommand = FindCommandById(commands, 3) ?? FindCommandById(commands, 9);
        EnemyCommandData moveCommand = FindBestCommand(commands, IsMoveCommand);

        if (sameAreaTarget != null)
        {
            int sameAreaCount = livingPlayers.Count(player => IsInArea(player, projectedAreaId));
            EnemyCommandData selectedAttack = sameAreaCount > 1 && chargeCommand != null
                ? chargeCommand
                : meleeCommand ?? chargeCommand;
            return BuildPlan(enemyData, selectedAttack, slotIndex, sameAreaTarget, projectedCoord);
        }

        if (nearestTarget != null && moveCommand != null)
        {
            return BuildPlan(enemyData, moveCommand, slotIndex, null, AreaDefinition.GetLegacyCoordForAreaId(nearestTarget.ResolveCurrentAreaId()));
        }

        return BuildSkipOrFallback(enemyData, commands, slotIndex);
    }

    private static EnemyActionPlan PlanRanged(
        EnemyData enemyData,
        List<EnemyCommandData> commands,
        IReadOnlyList<PlayerData> playerDataList)
    {
        int slotIndex = FindFirstFreeSlot(enemyData);
        Vector2I projectedCoord = GetProjectedCoord(enemyData);
        CombatAreaId projectedAreaId = GetProjectedAreaId(enemyData);
        List<PlayerData> livingPlayers = GetLivingPlayers(playerDataList);
        bool hasSameAreaPlayer = livingPlayers.Any(player => IsInArea(player, projectedAreaId));
        EnemyCommandData moveCommand = FindBestCommand(commands, IsMoveCommand);
        EnemyCommandData rangedCommand = FindCommandById(commands, 5) ?? FindBestCommand(commands, IsRangedCommand);
        PlayerData target = GetLowestHpPlayer(livingPlayers);

        if (hasSameAreaPlayer && moveCommand != null)
        {
            return BuildPlan(enemyData, moveCommand, slotIndex, null, FindSafeCoord(projectedCoord, livingPlayers));
        }

        if (target != null && rangedCommand != null)
        {
            return BuildPlan(enemyData, rangedCommand, slotIndex, target, projectedCoord);
        }

        return BuildSkipOrFallback(enemyData, commands, slotIndex);
    }

    private static EnemyActionPlan PlanAssassin(
        EnemyData enemyData,
        List<EnemyCommandData> commands,
        IReadOnlyList<PlayerData> playerDataList)
    {
        int slotIndex = FindFirstFreeSlot(enemyData);
        Vector2I projectedCoord = GetProjectedCoord(enemyData);
        CombatAreaId projectedAreaId = GetProjectedAreaId(enemyData);
        List<PlayerData> livingPlayers = GetLivingPlayers(playerDataList);
        PlayerData sameAreaTarget = GetLowestHpPlayer(livingPlayers.Where(player => IsInArea(player, projectedAreaId)));
        PlayerData lowestHpTarget = GetLowestHpPlayer(livingPlayers);
        EnemyCommandData ambushCommand = FindCommandById(commands, 6);
        EnemyCommandData meleeCommand = FindCommandById(commands, 3) ?? FindCommandById(commands, 9);

        if (sameAreaTarget == null && lowestHpTarget != null && ambushCommand != null)
        {
            return BuildPlan(enemyData, ambushCommand, slotIndex, lowestHpTarget, AreaDefinition.GetLegacyCoordForAreaId(lowestHpTarget.ResolveCurrentAreaId()));
        }

        if (sameAreaTarget != null && meleeCommand != null)
        {
            return BuildPlan(enemyData, meleeCommand, slotIndex, sameAreaTarget, projectedCoord);
        }

        return BuildSkipOrFallback(enemyData, commands, slotIndex);
    }

    private static EnemyActionPlan PlanSupport(
        EnemyData enemyData,
        List<EnemyCommandData> commands,
        IReadOnlyList<PlayerData> playerDataList,
        IReadOnlyList<EnemyData> enemyDataList)
    {
        int slotIndex = FindFirstFreeSlot(enemyData);
        Vector2I projectedCoord = GetProjectedCoord(enemyData);
        CombatAreaId projectedAreaId = GetProjectedAreaId(enemyData);
        List<PlayerData> livingPlayers = GetLivingPlayers(playerDataList);
        List<EnemyData> livingAllies = GetLivingEnemies(enemyDataList);
        EnemyData woundedAlly = livingAllies
            .Where(ally => ally.hp < ally.maxHp)
            .OrderBy(HealthPercent)
            .FirstOrDefault();
        EnemyCommandData healCommand = FindCommandById(commands, 7);
        EnemyCommandData moveCommand = FindBestCommand(commands, IsMoveCommand);
        EnemyCommandData rangedCommand = FindCommandById(commands, 5) ?? FindBestCommand(commands, IsRangedCommand);

        if (woundedAlly != null && healCommand != null)
        {
            return BuildPlan(enemyData, healCommand, slotIndex, woundedAlly, AreaDefinition.GetLegacyCoordForAreaId(woundedAlly.ResolveCurrentAreaId()));
        }

        if (livingPlayers.Any(player => IsInArea(player, projectedAreaId)) && moveCommand != null)
        {
            return BuildPlan(enemyData, moveCommand, slotIndex, null, FindSupportCoord(livingPlayers));
        }

        PlayerData target = GetLowestHpPlayer(livingPlayers);
        if (target != null && rangedCommand != null)
        {
            return BuildPlan(enemyData, rangedCommand, slotIndex, target, projectedCoord);
        }

        return BuildSkipOrFallback(enemyData, commands, slotIndex);
    }

    private static EnemyActionPlan PlanEliteMelee(
        EnemyData enemyData,
        List<EnemyCommandData> commands,
        IReadOnlyList<PlayerData> playerDataList,
        int roundIndex)
    {
        EnemyCommandData rageCommand = FindCommandById(commands, 10);
        if (enemyData.canEnterRage &&
            !enemyData.rageTriggered &&
            enemyData.hp <= enemyData.maxHp * 0.5f &&
            rageCommand != null)
        {
            int rageSlot = IsSlotFree(enemyData, Timeline.MaxSlotIndex)
                ? Timeline.MaxSlotIndex
                : FindFirstFreeSlot(enemyData);
            enemyData.rageTriggered = true;
            return BuildPlan(enemyData, rageCommand, rageSlot, enemyData, AreaDefinition.GetLegacyCoordForAreaId(enemyData.ResolveCurrentAreaId()));
        }

        if (enemyData.runtimeStatusIds != null && enemyData.runtimeStatusIds.Contains(StatusCatalog.Rage))
        {
            return PlanRageAction(enemyData, commands, playerDataList, roundIndex);
        }

        return PlanMelee(enemyData, commands, playerDataList);
    }

    private static EnemyActionPlan PlanRageAction(
        EnemyData enemyData,
        List<EnemyCommandData> commands,
        IReadOnlyList<PlayerData> playerDataList,
        int roundIndex)
    {
        int option = roundIndex % 3;
        if (option == 1)
        {
            return PlanRagePathAction(enemyData, commands, playerDataList, YangCoord, 12);
        }

        if (option == 2)
        {
            return PlanRagePathAction(enemyData, commands, playerDataList, YinCoord, 13);
        }

        int slotIndex = FindFirstFreeSlot(enemyData);
        Vector2I projectedCoord = GetProjectedCoord(enemyData);
        CombatAreaId projectedAreaId = GetProjectedAreaId(enemyData);
        List<PlayerData> livingPlayers = GetLivingPlayers(playerDataList);
        PlayerData sameAreaTarget = GetLowestHpPlayer(livingPlayers.Where(player => IsInArea(player, projectedAreaId)));
        PlayerData lowestHpTarget = GetLowestHpPlayer(livingPlayers);
        EnemyCommandData rushCommand = FindCommandById(commands, 11);
        EnemyCommandData chargeCommand = FindCommandById(commands, 8);
        EnemyCommandData singleCommand = FindCommandById(commands, 9) ?? FindCommandById(commands, 3);

        if (sameAreaTarget == null && lowestHpTarget != null && rushCommand != null && slotIndex <= 3)
        {
            return BuildPlan(enemyData, rushCommand, slotIndex, lowestHpTarget, AreaDefinition.GetLegacyCoordForAreaId(lowestHpTarget.ResolveCurrentAreaId()));
        }

        if (sameAreaTarget != null && chargeCommand != null && slotIndex <= 4)
        {
            return BuildPlan(enemyData, chargeCommand, slotIndex, sameAreaTarget, projectedCoord);
        }

        if ((sameAreaTarget ?? lowestHpTarget) != null && singleCommand != null)
        {
            return BuildPlan(enemyData, singleCommand, slotIndex, sameAreaTarget ?? lowestHpTarget, projectedCoord);
        }

        return BuildSkipOrFallback(enemyData, commands, slotIndex);
    }

    private static EnemyActionPlan PlanRagePathAction(
        EnemyData enemyData,
        List<EnemyCommandData> commands,
        IReadOnlyList<PlayerData> playerDataList,
        Vector2I setupCoord,
        int pathCommandId)
    {
        int slotIndex = FindFirstFreeSlot(enemyData);
        EnemyCommandData moveCommand = FindBestCommand(commands, IsMoveCommand);
        EnemyCommandData pathCommand = FindCommandById(commands, pathCommandId);
        PlayerData target = GetLowestHpPlayer(GetLivingPlayers(playerDataList));
        Vector2I projectedCoord = GetProjectedCoord(enemyData);

        if (projectedCoord != setupCoord && moveCommand != null && slotIndex == 1)
        {
            return BuildPlan(enemyData, moveCommand, slotIndex, null, setupCoord);
        }

        if (pathCommand != null && target != null)
        {
            return BuildPlan(enemyData, pathCommand, slotIndex, target, AreaDefinition.GetLegacyCoordForAreaId(target.ResolveCurrentAreaId()));
        }

        return PlanRageAction(enemyData, commands, playerDataList, 0);
    }

    private static EnemyActionPlan PlanEliteSupport(
        EnemyData enemyData,
        List<EnemyCommandData> commands,
        IReadOnlyList<PlayerData> playerDataList,
        IReadOnlyList<EnemyData> enemyDataList)
    {
        int slotIndex = FindFirstFreeSlot(enemyData);
        Vector2I projectedCoord = GetProjectedCoord(enemyData);
        CombatAreaId projectedAreaId = GetProjectedAreaId(enemyData);
        List<PlayerData> livingPlayers = GetLivingPlayers(playerDataList);
        List<EnemyData> livingAllies = GetLivingEnemies(enemyDataList);
        EnemyData woundedAlly = livingAllies
            .Where(ally => ally.hp < ally.maxHp)
            .OrderBy(HealthPercent)
            .FirstOrDefault();
        EnemyCommandData healCommand = FindCommandById(commands, 7);
        EnemyCommandData areaRangedCommand = FindCommandById(commands, 14);
        EnemyCommandData rangedCommand = FindCommandById(commands, 5) ?? areaRangedCommand;
        EnemyCommandData moveCommand = FindBestCommand(commands, IsMoveCommand);

        if (woundedAlly != null && healCommand != null && projectedAreaId == CombatAreaId.Kan)
        {
            return BuildPlan(enemyData, healCommand, slotIndex, woundedAlly, AreaDefinition.GetLegacyCoordForAreaId(woundedAlly.ResolveCurrentAreaId()));
        }

        if (woundedAlly != null && healCommand != null && moveCommand != null && !livingPlayers.Any(player => IsInArea(player, CombatAreaId.Kan)))
        {
            return BuildPlan(enemyData, moveCommand, slotIndex, null, new Vector2I(1, 2));
        }

        PlayerData target = GetLowestHpPlayer(livingPlayers);
        if (target != null && projectedAreaId == CombatAreaId.Dui && areaRangedCommand != null)
        {
            return BuildPlan(enemyData, areaRangedCommand, slotIndex, target, AreaDefinition.GetLegacyCoordForAreaId(target.ResolveCurrentAreaId()));
        }

        if (target != null && rangedCommand != null)
        {
            return BuildPlan(enemyData, rangedCommand, slotIndex, target, projectedCoord);
        }

        return PlanSupport(enemyData, commands, playerDataList, enemyDataList);
    }

    private static EnemyActionPlan BuildPlan(
        EnemyData enemyData,
        EnemyCommandData selectedCommand,
        int slotIndex,
        CharacterData targetCharacter,
        Vector2I targetCoord)
    {
        if (selectedCommand == null || slotIndex < Timeline.MinSlotIndex)
        {
            return null;
        }

        CombatAreaId targetAreaId = ResolveTargetAreaId(targetCharacter, targetCoord);
        return new EnemyActionPlan
        {
            SlotIndex = slotIndex,
            CommandExecuteInfo = new CommandExecuteInfo
            {
                isDefault = false,
                sourceCharacterData = enemyData,
                commandData = selectedCommand,
                targetCharacterData = targetCharacter,
                targetAreaId = targetAreaId,
                targetCoord = targetAreaId == CombatAreaId.Unknown
                    ? targetCoord
                    : AreaDefinition.GetLegacyCoordForAreaId(targetAreaId)
            }
        };
    }

    private static EnemyActionPlan BuildSkipOrFallback(
        EnemyData enemyData,
        List<EnemyCommandData> commands,
        int slotIndex)
    {
        EnemyCommandData selectedCommand = FindBestCommand(commands, IsSkipCommand) ?? commands
            .OrderByDescending(command => SkillDefinition.FromCommandData(command).Priority)
            .ThenBy(command => command.commandId)
            .FirstOrDefault();
        return selectedCommand == null
            ? null
            : BuildPlan(enemyData, selectedCommand, slotIndex, null, GetProjectedCoord(enemyData));
    }

    private static int FindFirstFreeSlot(EnemyData enemyData)
    {
        if (enemyData.commandQueue == null)
        {
            return -1;
        }

        for (int i = 0; i < enemyData.commandQueue.Count && i < Timeline.MaxSlotIndex; i++)
        {
            if (enemyData.commandQueue[i] == null || enemyData.commandQueue[i].isDefault)
            {
                return i + 1;
            }
        }

        return -1;
    }

    private static bool IsSlotFree(EnemyData enemyData, int slotIndex)
    {
        int queueIndex = slotIndex - 1;
        return enemyData?.commandQueue != null &&
            queueIndex >= 0 &&
            queueIndex < enemyData.commandQueue.Count &&
            (enemyData.commandQueue[queueIndex] == null || enemyData.commandQueue[queueIndex].isDefault);
    }

    private static Vector2I GetProjectedCoord(EnemyData enemyData)
    {
        Vector2I projectedCoord = AreaDefinition.GetLegacyCoordForAreaId(enemyData.ResolveCurrentAreaId());
        if (enemyData.commandQueue == null)
        {
            return projectedCoord;
        }

        foreach (CommandExecuteInfo command in enemyData.commandQueue)
        {
            if (command == null || command.isDefault || command.commandData == null)
            {
                continue;
            }

            SkillDefinition skill = SkillDefinition.FromCommandData(command.commandData);
            if (skill.HasTag(SkillTag.Move))
            {
                CombatAreaId projectedAreaId = command.targetAreaId != CombatAreaId.Unknown
                    ? command.targetAreaId
                    : AreaDefinition.GetAreaIdForLegacyCoord(command.targetCoord);
                projectedCoord = AreaDefinition.GetLegacyCoordForAreaId(projectedAreaId);
            }
        }

        return projectedCoord;
    }

    private static CombatAreaId GetProjectedAreaId(EnemyData enemyData)
    {
        CombatAreaId projectedAreaId = enemyData.ResolveCurrentAreaId();
        if (enemyData.commandQueue == null)
        {
            return projectedAreaId;
        }

        foreach (CommandExecuteInfo command in enemyData.commandQueue)
        {
            if (command == null || command.isDefault || command.commandData == null)
            {
                continue;
            }

            SkillDefinition skill = SkillDefinition.FromCommandData(command.commandData);
            if (skill.HasTag(SkillTag.Move))
            {
                projectedAreaId = command.targetAreaId != CombatAreaId.Unknown
                    ? command.targetAreaId
                    : AreaDefinition.GetAreaIdForLegacyCoord(command.targetCoord);
            }
        }

        return projectedAreaId;
    }

    private static CombatAreaId ResolveTargetAreaId(CharacterData targetCharacter, Vector2I targetCoord)
    {
        if (targetCharacter != null)
        {
            CombatAreaId characterAreaId = targetCharacter.ResolveCurrentAreaId();
            if (characterAreaId != CombatAreaId.Unknown)
            {
                return characterAreaId;
            }
        }

        return AreaDefinition.GetAreaIdForLegacyCoord(targetCoord);
    }

    private static bool IsInArea(CharacterData characterData, CombatAreaId areaId)
    {
        return characterData != null &&
            areaId != CombatAreaId.Unknown &&
            characterData.ResolveCurrentAreaId() == areaId;
    }

    private static EnemyCommandData FindBestCommand(
        IEnumerable<EnemyCommandData> commands,
        Func<EnemyCommandData, bool> predicate)
    {
        return commands
            .Where(predicate)
            .OrderByDescending(command => SkillDefinition.FromCommandData(command).Priority)
            .ThenBy(command => command.commandId)
            .FirstOrDefault();
    }

    private static EnemyCommandData FindCommandById(IEnumerable<EnemyCommandData> commands, int commandId)
    {
        return commands?.FirstOrDefault(command => command != null && command.commandId == commandId);
    }

    private static bool IsAttackCommand(EnemyCommandData command)
    {
        SkillDefinition skill = SkillDefinition.FromCommandData(command);
        return skill.TargetType == SkillTargetType.Enemy ||
            skill.Effects.Any(effect => effect.EffectType == SkillEffectType.Damage);
    }

    private static bool IsMoveCommand(EnemyCommandData command)
    {
        return SkillDefinition.FromCommandData(command).HasTag(SkillTag.Move);
    }

    private static bool IsRangedCommand(EnemyCommandData command)
    {
        return SkillDefinition.FromCommandData(command).HasTag(SkillTag.Ranged);
    }

    private static bool IsSkipCommand(EnemyCommandData command)
    {
        SkillDefinition skill = SkillDefinition.FromCommandData(command);
        return skill.TargetType == SkillTargetType.None && skill.Effects.Count == 0;
    }

    private static bool IsAlive(PlayerData playerData)
    {
        return playerData != null &&
            playerData.hp > 0 &&
            playerData.characterBattleState == CharacterBattleState.ALIVE;
    }

    private static bool IsAlive(EnemyData enemyData)
    {
        return enemyData != null &&
            enemyData.hp > 0 &&
            enemyData.characterBattleState == CharacterBattleState.ALIVE;
    }

    private static List<PlayerData> GetLivingPlayers(IReadOnlyList<PlayerData> playerDataList)
    {
        return playerDataList?.Where(IsAlive).ToList() ?? new List<PlayerData>();
    }

    private static List<EnemyData> GetLivingEnemies(IReadOnlyList<EnemyData> enemyDataList)
    {
        return enemyDataList?.Where(IsAlive).ToList() ?? new List<EnemyData>();
    }

    private static PlayerData GetLowestHpPlayer(IEnumerable<PlayerData> players)
    {
        return players?
            .Where(IsAlive)
            .OrderBy(HealthPercent)
            .ThenBy(player => player.hp)
            .FirstOrDefault();
    }

    private static PlayerData GetNearestPlayer(IEnumerable<PlayerData> players, Vector2I origin)
    {
        return players?
            .Where(IsAlive)
            .OrderBy(player => Distance(origin, player.coord))
            .ThenBy(HealthPercent)
            .FirstOrDefault();
    }

    private static float HealthPercent(CharacterData characterData)
    {
        return characterData == null || characterData.maxHp <= 0
            ? 1
            : characterData.hp / characterData.maxHp;
    }

    private static Vector2I FindFallbackMoveCoord(Vector2I currentCoord)
    {
        return FallbackMoveCoords.FirstOrDefault(coord => coord != currentCoord);
    }

    private static Vector2I FindSafeCoord(Vector2I currentCoord, IReadOnlyList<PlayerData> livingPlayers)
    {
        return FallbackMoveCoords.FirstOrDefault(coord =>
            coord != currentCoord &&
            (livingPlayers == null || !livingPlayers.Any(player => player.coord == coord)));
    }

    private static Vector2I FindSupportCoord(IReadOnlyList<PlayerData> livingPlayers)
    {
        Vector2I kanCoord = new(1, 2);
        if (livingPlayers == null || !livingPlayers.Any(player => player.coord == kanCoord))
        {
            return kanCoord;
        }

        Vector2I duiCoord = new(0, 1);
        if (!livingPlayers.Any(player => player.coord == duiCoord))
        {
            return duiCoord;
        }

        Vector2I qianCoord = new(0, 0);
        if (!livingPlayers.Any(player => player.coord == qianCoord))
        {
            return qianCoord;
        }

        return FindSafeCoord(kanCoord, livingPlayers);
    }

    private static int Distance(Vector2I a, Vector2I b)
    {
        return Math.Abs(a.X - b.X) + Math.Abs(a.Y - b.Y);
    }
}

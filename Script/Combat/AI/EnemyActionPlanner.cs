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
    private static readonly Random Rng = new();
    private static readonly CombatAreaId[] AllAreas = AreaDefinition.GetDefaultAreaOrder();
    private static readonly CombatAreaId[] YinPath =
    {
        CombatAreaId.Qian,
        CombatAreaId.Dui,
        CombatAreaId.Li,
        CombatAreaId.Zhen,
        CombatAreaId.Xun,
        CombatAreaId.Kan,
        CombatAreaId.Gen,
        CombatAreaId.Kun,
        CombatAreaId.Yin,
        CombatAreaId.Yang
    };
    private static readonly CombatAreaId[] YangPath =
    {
        CombatAreaId.Kun,
        CombatAreaId.Gen,
        CombatAreaId.Kan,
        CombatAreaId.Xun,
        CombatAreaId.Zhen,
        CombatAreaId.Li,
        CombatAreaId.Dui,
        CombatAreaId.Qian,
        CombatAreaId.Yang,
        CombatAreaId.Yin
    };

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

        List<EnemyCommandData> commands = enemyData.enemyCommandDataArray?
            .Where(command => command != null)
            .ToList() ?? new List<EnemyCommandData>();
        if (commands.Count == 0)
        {
            return null;
        }

        PlanningContext context = BuildPlanningContext(enemyData, commands, playerDataList, enemyDataList);
        if (context.FreeSlots.Count == 0)
        {
            return null;
        }

        return enemyData.aiProfile switch
        {
            EnemyAiProfile.Melee => PlanMeleeByGdd(context),
            EnemyAiProfile.Ranged => PlanRangedByGdd(context),
            EnemyAiProfile.Assassin => PlanAssassinByGdd(context),
            EnemyAiProfile.Support => PlanSupportByGdd(context),
            EnemyAiProfile.EliteMelee => PlanEliteMeleeByGdd(context),
            EnemyAiProfile.EliteSupport => PlanEliteSupportByGdd(context),
            _ => PlanBasicByGdd(context)
        };
    }

    private static EnemyActionPlan PlanBasicByGdd(PlanningContext context)
    {
        List<ReleaseCandidate> candidates = new();
        EnemyCommandData moveCommand = FindCommandById(context.Commands, 1) ?? FindBestCommand(context.Commands, IsMoveCommand);
        EnemyCommandData attackCommand = FindBestCommand(context.Commands, IsAttackCommand);

        AddMoveToPlayerAreaCandidate(candidates, context, moveCommand, maxSlot: Timeline.MaxSlotIndex - 1);
        AddSameAreaAttackCandidate(candidates, context, attackCommand, randomTargetWhenMultiple: true);
        return PickCandidateOrSkip(candidates, context);
    }

    private static EnemyActionPlan PlanMeleeByGdd(PlanningContext context)
    {
        List<ReleaseCandidate> candidates = new();
        EnemyCommandData moveCommand = FindCommandById(context.Commands, 1);
        EnemyCommandData meleeCommand = FindCommandById(context.Commands, 3) ?? FindCommandById(context.Commands, 9);
        EnemyCommandData chargeCommand = FindCommandById(context.Commands, 4);

        AddMoveToPlayerAreaCandidate(candidates, context, moveCommand, maxSlot: Timeline.MaxSlotIndex - 1);
        AddSameAreaAttackCandidate(candidates, context, meleeCommand, randomTargetWhenMultiple: true);
        AddSameAreaAttackCandidate(
            candidates,
            context,
            chargeCommand,
            randomTargetWhenMultiple: true,
            maxSlot: Timeline.MaxSlotIndex - 1);

        return PickCandidateOrSkip(candidates, context);
    }

    private static EnemyActionPlan PlanRangedByGdd(PlanningContext context)
    {
        List<ReleaseCandidate> candidates = new();
        EnemyCommandData moveCommand = FindCommandById(context.Commands, 1);
        EnemyCommandData rangedCommand = FindCommandById(context.Commands, 5) ?? FindBestCommand(context.Commands, IsRangedCommand);
        bool isTrainingRanged = context.Enemy.characterId == 2;
        int moveMaxSlot = isTrainingRanged ? Timeline.MaxSlotIndex : Timeline.MaxSlotIndex - 1;

        AddMoveToEmptyAreaCandidate(candidates, context, moveCommand, maxSlot: moveMaxSlot);

        List<int> rangedSlots = isTrainingRanged
            ? GetFreeSlots(context)
            : GetFreeSlots(context, slot => !slot.HasSameAreaPlayer);
        AddCandidate(candidates, rangedCommand, rangedSlots, slot =>
        {
            PlayerData target = PickRandom(context.LivingPlayers);
            return target == null
                ? null
                : BuildPlan(context.Enemy, rangedCommand, slot, target, AreaDefinition.GetLegacyCoordForAreaId(target.ResolveCurrentAreaId()));
        });

        return PickCandidateOrSkip(candidates, context);
    }

    private static EnemyActionPlan PlanAssassinByGdd(PlanningContext context)
    {
        List<ReleaseCandidate> candidates = new();
        EnemyCommandData ambushCommand = FindCommandById(context.Commands, 6);
        EnemyCommandData meleeCommand = FindCommandById(context.Commands, 3) ?? FindCommandById(context.Commands, 9);

        AddCandidate(
            candidates,
            ambushCommand,
            GetFreeSlots(context, slot => !slot.HasSameAreaPlayer, maxSlot: Timeline.MaxSlotIndex - 1),
            slot =>
            {
                PlayerData target = PickLowestHpPlayer(context.LivingPlayers);
                return target == null
                    ? null
                    : BuildPlan(context.Enemy, ambushCommand, slot, target, AreaDefinition.GetLegacyCoordForAreaId(target.ResolveCurrentAreaId()));
            });

        AddCandidate(
            candidates,
            meleeCommand,
            GetFreeSlots(context, slot => slot.HasSameAreaPlayer),
            slot =>
            {
                PlayerData target = GetQueuedAmbushTarget(context);
                if (target == null || !IsAlive(target))
                {
                    target = PickLowestHpPlayer(GetSlot(context, slot).SameAreaPlayers);
                }

                return target == null
                    ? null
                    : BuildPlan(context.Enemy, meleeCommand, slot, target, AreaDefinition.GetLegacyCoordForAreaId(target.ResolveCurrentAreaId()));
            });

        return PickCandidateOrSkip(candidates, context);
    }

    private static EnemyActionPlan PlanSupportByGdd(PlanningContext context)
    {
        List<ReleaseCandidate> candidates = new();
        EnemyCommandData moveCommand = FindCommandById(context.Commands, 1);
        EnemyCommandData healCommand = FindCommandById(context.Commands, 7);
        EnemyCommandData rangedCommand = FindCommandById(context.Commands, 5) ?? FindBestCommand(context.Commands, IsRangedCommand);

        AddHealCandidate(candidates, context, healCommand, GetFreeSlots(context));
        AddMoveToEmptyAreaCandidate(candidates, context, moveCommand, maxSlot: Timeline.MaxSlotIndex - 1);
        AddCandidate(
            candidates,
            rangedCommand,
            GetFreeSlots(context, slot => !slot.HasSameAreaPlayer),
            slot =>
            {
                PlayerData target = PickRandom(context.LivingPlayers);
                return target == null
                    ? null
                    : BuildPlan(context.Enemy, rangedCommand, slot, target, AreaDefinition.GetLegacyCoordForAreaId(target.ResolveCurrentAreaId()));
            });

        return PickCandidateOrSkip(candidates, context);
    }

    private static EnemyActionPlan PlanEliteMeleeByGdd(PlanningContext context)
    {
        EnemyCommandData rageCommand = FindCommandById(context.Commands, 10);
        if (context.Enemy.canEnterRage &&
            !context.Enemy.rageTriggered &&
            context.Enemy.hp <= context.Enemy.maxHp * 0.5f &&
            rageCommand != null)
        {
            int rageSlot = context.FreeSlots.Contains(Timeline.MaxSlotIndex)
                ? Timeline.MaxSlotIndex
                : PickRandom(context.FreeSlots);
            context.Enemy.rageTriggered = true;
            return BuildPlan(
                context.Enemy,
                rageCommand,
                rageSlot,
                context.Enemy,
                AreaDefinition.GetLegacyCoordForAreaId(GetSlot(context, rageSlot).EnemyAreaId));
        }

        if (context.Enemy.runtimeStatusIds != null && context.Enemy.runtimeStatusIds.Contains(StatusCatalog.Rage))
        {
            return PlanRageByGdd(context);
        }

        List<ReleaseCandidate> candidates = new();
        EnemyCommandData moveCommand = FindCommandById(context.Commands, 1);
        EnemyCommandData chargeCommand = FindCommandById(context.Commands, 8);
        EnemyCommandData singleCommand = FindCommandById(context.Commands, 9) ?? FindCommandById(context.Commands, 3);

        List<int> priorityChargeSlots = GetFreeSlots(
            context,
            slot => slot.SameAreaPlayers.Count > 1,
            maxSlot: Timeline.MaxSlotIndex - 1);
        if (chargeCommand != null && priorityChargeSlots.Count > 0)
        {
            AddCandidate(
                candidates,
                chargeCommand,
                priorityChargeSlots,
                slot => BuildPlanForSameAreaAttack(context, chargeCommand, slot, randomTargetWhenMultiple: true));
            return PickCandidateOrSkip(candidates, context);
        }

        AddMoveToPlayerAreaCandidate(candidates, context, moveCommand, maxSlot: 3);
        AddCandidate(
            candidates,
            chargeCommand,
            GetFreeSlots(context, slot => slot.HasSameAreaPlayer, maxSlot: Timeline.MaxSlotIndex - 1),
            slot => BuildPlanForSameAreaAttack(context, chargeCommand, slot, randomTargetWhenMultiple: true));
        AddCandidate(
            candidates,
            singleCommand,
            GetFreeSlots(context, slot => slot.HasSameAreaPlayer),
            slot => BuildPlanForSameAreaAttack(context, singleCommand, slot, randomTargetWhenMultiple: false));

        return PickCandidateOrSkip(candidates, context);
    }

    private static EnemyActionPlan PlanRageByGdd(PlanningContext context)
    {
        if (context.HasQueuedCommand(12) || context.HasQueuedCommand(13))
        {
            return BuildSkipOrFallback(context);
        }

        if (context.HasQueuedCommand(1))
        {
            return ContinueRagePathPlan(context);
        }

        if (context.HasQueuedCommand(11) || context.HasQueuedCommand(8) || context.HasQueuedCommand(9))
        {
            return ContinueRageAttackPlan(context);
        }

        for (int attempts = 0; attempts < 3; attempts++)
        {
            int option = Rng.Next(3);
            EnemyActionPlan plan = option switch
            {
                0 => ContinueRageAttackPlan(context),
                1 => StartRagePathPlan(context, YinPath, 12),
                2 => StartRagePathPlan(context, YangPath, 13),
                _ => null
            };
            if (plan != null && plan.IsValid)
            {
                return plan;
            }
        }

        return ContinueRageAttackPlan(context) ??
            StartRagePathPlan(context, YinPath, 12) ??
            StartRagePathPlan(context, YangPath, 13) ??
            BuildSkipOrFallback(context);
    }

    private static EnemyActionPlan ContinueRageAttackPlan(PlanningContext context)
    {
        EnemyCommandData rushCommand = FindCommandById(context.Commands, 11);
        EnemyCommandData chargeCommand = FindCommandById(context.Commands, 8);
        EnemyCommandData singleCommand = FindCommandById(context.Commands, 9) ?? FindCommandById(context.Commands, 3);
        QueuedAction rushAction = context.GetFirstQueuedAction(11);
        QueuedAction chargeAction = context.GetFirstQueuedAction(8);

        if (rushAction == null && !context.HasQueuedCommand(8) && !context.HasQueuedCommand(9))
        {
            List<int> rushSlots = GetFreeSlots(context, slot => !slot.HasSameAreaPlayer, minSlot: 1, maxSlot: 3);
            if (rushCommand != null && rushSlots.Count > 0)
            {
                PlayerData target = PickLowestHpPlayer(context.LivingPlayers);
                if (target != null)
                {
                    int slot = PickRandom(rushSlots);
                    return BuildPlan(context.Enemy, rushCommand, slot, target, AreaDefinition.GetLegacyCoordForAreaId(target.ResolveCurrentAreaId()));
                }
            }
        }

        if (chargeAction == null && chargeCommand != null)
        {
            int minChargeSlot = rushAction == null ? 1 : rushAction.SlotIndex + 1;
            int maxChargeSlot = rushAction == null ? 3 : 4;
            List<int> chargeSlots = GetFreeSlots(
                context,
                slot => slot.HasSameAreaPlayer,
                minSlot: minChargeSlot,
                maxSlot: maxChargeSlot);
            if (chargeSlots.Count > 0)
            {
                int slot = PickRandom(chargeSlots);
                return BuildPlanForSameAreaAttack(context, chargeCommand, slot, randomTargetWhenMultiple: true);
            }
        }

        int queuedSingleCount = context.CountQueuedCommand(9);
        int maxSingleCount = rushAction == null ? 2 : 1;
        if (singleCommand != null && queuedSingleCount < maxSingleCount)
        {
            int minSingleSlot = rushAction == null
                ? 4
                : Math.Max(rushAction.SlotIndex + 1, (chargeAction?.SlotIndex ?? rushAction.SlotIndex) + 1);
            List<int> singleSlots = GetFreeSlots(context, minSlot: minSingleSlot, maxSlot: Timeline.MaxSlotIndex);
            if (singleSlots.Count > 0)
            {
                int slot = PickRandom(singleSlots);
                PlayerData target = rushAction?.CommandInfo?.targetCharacterData as PlayerData;
                if (target == null || !IsAlive(target))
                {
                    SlotProjection projection = GetSlot(context, slot);
                    target = projection.HasSameAreaPlayer
                        ? PickRandom(projection.SameAreaPlayers)
                        : PickRandom(context.LivingPlayers);
                }

                return target == null
                    ? null
                    : BuildPlan(context.Enemy, singleCommand, slot, target, AreaDefinition.GetLegacyCoordForAreaId(target.ResolveCurrentAreaId()));
            }
        }

        return BuildSkipOrFallback(context);
    }

    private static EnemyActionPlan StartRagePathPlan(PlanningContext context, CombatAreaId[] path, int pathCommandId)
    {
        EnemyCommandData moveCommand = FindCommandById(context.Commands, 1);
        PlayerData target = PickLowestHpPlayer(context.LivingPlayers);
        if (target == null)
        {
            return null;
        }

        CombatAreaId setupArea = GetPreviousAreaInPath(path, target.ResolveCurrentAreaId());
        if (setupArea == CombatAreaId.Unknown)
        {
            return null;
        }

        if (GetSlot(context, 1)?.IsFree == true &&
            moveCommand != null &&
            GetSlot(context, 1).EnemyAreaId != setupArea)
        {
            return BuildPlan(context.Enemy, moveCommand, 1, null, AreaDefinition.GetLegacyCoordForAreaId(setupArea));
        }

        return BuildRagePathAttack(context, pathCommandId);
    }

    private static EnemyActionPlan ContinueRagePathPlan(PlanningContext context)
    {
        PlayerData target = PickLowestHpPlayer(context.LivingPlayers);
        CombatAreaId targetAreaId = target?.ResolveCurrentAreaId() ?? CombatAreaId.Unknown;
        QueuedAction setupMove = context.GetFirstQueuedAction(1);
        CombatAreaId setupAreaId = setupMove == null
            ? CombatAreaId.Unknown
            : ResolveCommandTargetArea(setupMove.CommandInfo);

        int pathCommandId = 0;
        if (targetAreaId != CombatAreaId.Unknown && setupAreaId == GetPreviousAreaInPath(YinPath, targetAreaId))
        {
            pathCommandId = 12;
        }
        else if (targetAreaId != CombatAreaId.Unknown && setupAreaId == GetPreviousAreaInPath(YangPath, targetAreaId))
        {
            pathCommandId = 13;
        }
        else
        {
            pathCommandId = PickRandom(new[] { 12, 13 });
        }

        return BuildRagePathAttack(context, pathCommandId);
    }

    private static EnemyActionPlan BuildRagePathAttack(PlanningContext context, int pathCommandId)
    {
        EnemyCommandData pathCommand = FindCommandById(context.Commands, pathCommandId);
        List<int> slots = GetFreeSlots(context, minSlot: 2, maxSlot: 3);
        PlayerData target = PickLowestHpPlayer(context.LivingPlayers);
        if (pathCommand == null || slots.Count == 0 || target == null)
        {
            return null;
        }

        int slot = PickRandom(slots);
        return BuildPlan(context.Enemy, pathCommand, slot, target, AreaDefinition.GetLegacyCoordForAreaId(target.ResolveCurrentAreaId()));
    }

    private static EnemyActionPlan PlanEliteSupportByGdd(PlanningContext context)
    {
        EnemyCommandData moveCommand = FindCommandById(context.Commands, 1);
        EnemyCommandData healCommand = FindCommandById(context.Commands, 7);
        EnemyCommandData rangedCommand = FindCommandById(context.Commands, 5);
        EnemyCommandData areaRangedCommand = FindCommandById(context.Commands, 14);

        List<ReleaseCandidate> priorityCandidates = new();
        AddHealCandidate(
            priorityCandidates,
            context,
            healCommand,
            GetFreeSlots(context, slot => slot.EnemyAreaId == CombatAreaId.Kan));
        AddRangedTargetCandidate(
            priorityCandidates,
            context,
            rangedCommand,
            GetFreeSlots(context, slot => slot.EnemyAreaId == CombatAreaId.Dui));
        AddRangedTargetCandidate(
            priorityCandidates,
            context,
            areaRangedCommand,
            GetFreeSlots(context, slot => slot.EnemyAreaId == CombatAreaId.Dui));
        if (priorityCandidates.Count > 0)
        {
            return PickCandidateOrSkip(priorityCandidates, context);
        }

        List<ReleaseCandidate> candidates = new();
        AddHealCandidate(candidates, context, healCommand, GetFreeSlots(context));
        AddCandidate(
            candidates,
            moveCommand,
            GetFreeSlots(context, slot => slot.HasSameAreaPlayer),
            slot =>
            {
                CombatAreaId targetAreaId = PickEliteSupportMoveTarget(context);
                return targetAreaId == CombatAreaId.Unknown
                    ? null
                    : BuildPlan(context.Enemy, moveCommand, slot, null, AreaDefinition.GetLegacyCoordForAreaId(targetAreaId));
            });
        AddRangedTargetCandidate(candidates, context, rangedCommand, GetFreeSlots(context));
        AddRangedTargetCandidate(candidates, context, areaRangedCommand, GetFreeSlots(context));

        return PickCandidateOrSkip(candidates, context);
    }

    private static void AddMoveToPlayerAreaCandidate(
        List<ReleaseCandidate> candidates,
        PlanningContext context,
        EnemyCommandData moveCommand,
        int maxSlot)
    {
        AddCandidate(
            candidates,
            moveCommand,
            GetFreeSlots(context, slot => !slot.HasSameAreaPlayer, maxSlot: maxSlot),
            slot =>
            {
                CombatAreaId targetAreaId = PickRandomPlayerArea(context);
                return targetAreaId == CombatAreaId.Unknown
                    ? null
                    : BuildPlan(context.Enemy, moveCommand, slot, null, AreaDefinition.GetLegacyCoordForAreaId(targetAreaId));
            });
    }

    private static void AddMoveToEmptyAreaCandidate(
        List<ReleaseCandidate> candidates,
        PlanningContext context,
        EnemyCommandData moveCommand,
        int maxSlot)
    {
        AddCandidate(
            candidates,
            moveCommand,
            GetFreeSlots(context, slot => slot.HasSameAreaPlayer, maxSlot: maxSlot),
            slot =>
            {
                CombatAreaId targetAreaId = PickRandomAreaWithoutPlayers(context);
                return targetAreaId == CombatAreaId.Unknown
                    ? null
                    : BuildPlan(context.Enemy, moveCommand, slot, null, AreaDefinition.GetLegacyCoordForAreaId(targetAreaId));
            });
    }

    private static void AddSameAreaAttackCandidate(
        List<ReleaseCandidate> candidates,
        PlanningContext context,
        EnemyCommandData attackCommand,
        bool randomTargetWhenMultiple,
        int maxSlot = Timeline.MaxSlotIndex)
    {
        AddCandidate(
            candidates,
            attackCommand,
            GetFreeSlots(context, slot => slot.HasSameAreaPlayer, maxSlot: maxSlot),
            slot => BuildPlanForSameAreaAttack(context, attackCommand, slot, randomTargetWhenMultiple));
    }

    private static void AddHealCandidate(
        List<ReleaseCandidate> candidates,
        PlanningContext context,
        EnemyCommandData healCommand,
        List<int> slots)
    {
        if (context.HasQueuedCommand(7))
        {
            return;
        }

        AddCandidate(candidates, healCommand, slots, slot =>
        {
            EnemyData target = PickLowestHealthPercentEnemy(context.LivingEnemies.Where(enemy => enemy.hp < enemy.maxHp));
            return target == null
                ? null
                : BuildPlan(context.Enemy, healCommand, slot, target, AreaDefinition.GetLegacyCoordForAreaId(target.ResolveCurrentAreaId()));
        });
    }

    private static void AddRangedTargetCandidate(
        List<ReleaseCandidate> candidates,
        PlanningContext context,
        EnemyCommandData rangedCommand,
        List<int> slots)
    {
        AddCandidate(candidates, rangedCommand, slots, slot =>
        {
            PlayerData target = PickRandom(context.LivingPlayers);
            return target == null
                ? null
                : BuildPlan(context.Enemy, rangedCommand, slot, target, AreaDefinition.GetLegacyCoordForAreaId(target.ResolveCurrentAreaId()));
        });
    }

    private static EnemyActionPlan BuildPlanForSameAreaAttack(
        PlanningContext context,
        EnemyCommandData attackCommand,
        int slot,
        bool randomTargetWhenMultiple)
    {
        SlotProjection projection = GetSlot(context, slot);
        PlayerData target = randomTargetWhenMultiple
            ? PickRandom(projection.SameAreaPlayers)
            : PickLowestHpPlayer(projection.SameAreaPlayers);
        return target == null
            ? null
            : BuildPlan(context.Enemy, attackCommand, slot, target, AreaDefinition.GetLegacyCoordForAreaId(target.ResolveCurrentAreaId()));
    }

    private static void AddCandidate(
        List<ReleaseCandidate> candidates,
        EnemyCommandData command,
        List<int> slotIndexes,
        Func<int, EnemyActionPlan> buildPlan)
    {
        if (command == null || slotIndexes == null || slotIndexes.Count == 0 || buildPlan == null)
        {
            return;
        }

        candidates.Add(new ReleaseCandidate(command, slotIndexes, buildPlan));
    }

    private static EnemyActionPlan PickCandidateOrSkip(List<ReleaseCandidate> candidates, PlanningContext context)
    {
        EnemyActionPlan plan = PickCandidate(candidates);
        return plan != null && plan.IsValid
            ? plan
            : BuildSkipOrFallback(context);
    }

    private static EnemyActionPlan PickCandidate(List<ReleaseCandidate> candidates)
    {
        List<ReleaseCandidate> remaining = candidates?
            .Where(candidate => candidate?.SlotIndexes?.Count > 0)
            .ToList() ?? new List<ReleaseCandidate>();

        while (remaining.Count > 0)
        {
            int index = Rng.Next(remaining.Count);
            ReleaseCandidate candidate = remaining[index];
            EnemyActionPlan plan = candidate.BuildRandomPlan();
            if (plan != null && plan.IsValid)
            {
                return plan;
            }

            remaining.RemoveAt(index);
        }

        return null;
    }

    private static EnemyActionPlan BuildPlan(
        EnemyData enemyData,
        EnemyCommandData selectedCommand,
        int slotIndex,
        CharacterData targetCharacter,
        Vector2I targetCoord)
    {
        if (selectedCommand == null || slotIndex < Timeline.MinSlotIndex || slotIndex > Timeline.MaxSlotIndex)
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

    private static EnemyActionPlan BuildSkipOrFallback(PlanningContext context)
    {
        EnemyCommandData skipCommand = FindBestCommand(context.Commands, IsSkipCommand);
        if (skipCommand == null || context.FreeSlots.Count == 0)
        {
            return null;
        }

        int slot = PickRandom(context.FreeSlots);
        CombatAreaId areaId = GetSlot(context, slot)?.EnemyAreaId ?? context.Enemy.ResolveCurrentAreaId();
        return BuildPlan(context.Enemy, skipCommand, slot, null, AreaDefinition.GetLegacyCoordForAreaId(areaId));
    }

    private static PlanningContext BuildPlanningContext(
        EnemyData enemyData,
        List<EnemyCommandData> commands,
        IReadOnlyList<PlayerData> playerDataList,
        IReadOnlyList<EnemyData> enemyDataList)
    {
        PlanningContext context = new()
        {
            Enemy = enemyData,
            Commands = commands,
            LivingPlayers = GetLivingPlayers(playerDataList),
            LivingEnemies = GetLivingEnemies(enemyDataList)
        };

        CombatAreaId projectedAreaId = enemyData.ResolveCurrentAreaId();
        if (projectedAreaId == CombatAreaId.Unknown)
        {
            projectedAreaId = CombatAreaId.Yang;
        }

        for (int slotIndex = Timeline.MinSlotIndex; slotIndex <= Timeline.MaxSlotIndex; slotIndex++)
        {
            CommandExecuteInfo queuedCommand = GetQueuedCommand(enemyData, slotIndex);
            bool isFree = queuedCommand == null || queuedCommand.isDefault;
            List<PlayerData> sameAreaPlayers = context.LivingPlayers
                .Where(player => player.ResolveCurrentAreaId() == projectedAreaId)
                .ToList();

            context.Slots.Add(new SlotProjection
            {
                SlotIndex = slotIndex,
                EnemyAreaId = projectedAreaId,
                SameAreaPlayers = sameAreaPlayers,
                IsFree = isFree
            });

            if (isFree)
            {
                context.FreeSlots.Add(slotIndex);
            }
            else
            {
                context.QueuedActions.Add(new QueuedAction
                {
                    SlotIndex = slotIndex,
                    CommandInfo = queuedCommand
                });

                SkillDefinition skill = SkillDefinition.FromCommandData(queuedCommand.commandData);
                if (skill.HasTag(SkillTag.Move))
                {
                    CombatAreaId targetAreaId = ResolveCommandTargetArea(queuedCommand);
                    if (targetAreaId != CombatAreaId.Unknown)
                    {
                        projectedAreaId = targetAreaId;
                    }
                }
            }
        }

        return context;
    }

    private static CommandExecuteInfo GetQueuedCommand(EnemyData enemyData, int slotIndex)
    {
        int queueIndex = slotIndex - 1;
        if (enemyData?.commandQueue == null ||
            queueIndex < 0 ||
            queueIndex >= enemyData.commandQueue.Count)
        {
            return null;
        }

        return enemyData.commandQueue[queueIndex];
    }

    private static List<int> GetFreeSlots(
        PlanningContext context,
        Func<SlotProjection, bool> predicate = null,
        int minSlot = Timeline.MinSlotIndex,
        int maxSlot = Timeline.MaxSlotIndex,
        IEnumerable<int> excludedSlots = null)
    {
        HashSet<int> excluded = new(excludedSlots ?? Enumerable.Empty<int>());
        return context.Slots
            .Where(slot =>
                slot.IsFree &&
                slot.SlotIndex >= minSlot &&
                slot.SlotIndex <= maxSlot &&
                !excluded.Contains(slot.SlotIndex) &&
                (predicate == null || predicate(slot)))
            .Select(slot => slot.SlotIndex)
            .ToList();
    }

    private static SlotProjection GetSlot(PlanningContext context, int slotIndex)
    {
        return context?.Slots?.FirstOrDefault(slot => slot.SlotIndex == slotIndex);
    }

    private static CombatAreaId PickRandomPlayerArea(PlanningContext context)
    {
        List<CombatAreaId> areas = context.LivingPlayers
            .Select(player => player.ResolveCurrentAreaId())
            .Where(area => area != CombatAreaId.Unknown)
            .Distinct()
            .ToList();
        return PickRandom(areas);
    }

    private static CombatAreaId PickRandomAreaWithoutPlayers(PlanningContext context)
    {
        List<CombatAreaId> areas = AllAreas
            .Where(area => !AreaHasLivingPlayer(context, area))
            .ToList();
        return PickRandom(areas);
    }

    private static CombatAreaId PickEliteSupportMoveTarget(PlanningContext context)
    {
        bool hasWoundedBelowHalf = context.LivingEnemies.Any(enemy => enemy.hp < enemy.maxHp * 0.5f);
        if (hasWoundedBelowHalf && !AreaHasLivingPlayer(context, CombatAreaId.Kan))
        {
            return CombatAreaId.Kan;
        }

        List<CombatAreaId> preferredAreas = new[] { CombatAreaId.Qian, CombatAreaId.Dui }
            .Where(area => !AreaHasLivingPlayer(context, area))
            .ToList();
        if (preferredAreas.Count > 0)
        {
            return PickRandom(preferredAreas);
        }

        return PickRandomAreaWithoutPlayers(context);
    }

    private static bool AreaHasLivingPlayer(PlanningContext context, CombatAreaId areaId)
    {
        return context.LivingPlayers.Any(player => player.ResolveCurrentAreaId() == areaId);
    }

    private static PlayerData GetQueuedAmbushTarget(PlanningContext context)
    {
        return context.GetFirstQueuedAction(6)?.CommandInfo?.targetCharacterData as PlayerData;
    }

    private static CombatAreaId ResolveCommandTargetArea(CommandExecuteInfo command)
    {
        if (command == null)
        {
            return CombatAreaId.Unknown;
        }

        if (command.targetAreaId != CombatAreaId.Unknown)
        {
            return command.targetAreaId;
        }

        if (command.targetCharacterData != null)
        {
            return command.targetCharacterData.ResolveCurrentAreaId();
        }

        return AreaDefinition.GetAreaIdForLegacyCoord(command.targetCoord);
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

    private static CombatAreaId GetPreviousAreaInPath(CombatAreaId[] path, CombatAreaId targetAreaId)
    {
        if (path == null || path.Length == 0 || targetAreaId == CombatAreaId.Unknown)
        {
            return CombatAreaId.Unknown;
        }

        int index = Array.IndexOf(path, targetAreaId);
        if (index < 0)
        {
            return CombatAreaId.Unknown;
        }

        int previousIndex = index == 0 ? path.Length - 1 : index - 1;
        return path[previousIndex];
    }

    private static EnemyCommandData FindBestCommand(
        IEnumerable<EnemyCommandData> commands,
        Func<EnemyCommandData, bool> predicate)
    {
        return commands?
            .Where(command => command != null && predicate(command))
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

    private static PlayerData PickLowestHpPlayer(IEnumerable<PlayerData> players)
    {
        List<PlayerData> livingPlayers = players?.Where(IsAlive).ToList() ?? new List<PlayerData>();
        if (livingPlayers.Count == 0)
        {
            return null;
        }

        float lowestHp = livingPlayers.Min(player => player.hp);
        return PickRandom(livingPlayers.Where(player => Math.Abs(player.hp - lowestHp) < 0.001f).ToList());
    }

    private static EnemyData PickLowestHealthPercentEnemy(IEnumerable<EnemyData> enemies)
    {
        List<EnemyData> livingEnemies = enemies?.Where(IsAlive).ToList() ?? new List<EnemyData>();
        if (livingEnemies.Count == 0)
        {
            return null;
        }

        float lowestPercent = livingEnemies.Min(HealthPercent);
        return PickRandom(livingEnemies.Where(enemy => Math.Abs(HealthPercent(enemy) - lowestPercent) < 0.001f).ToList());
    }

    private static float HealthPercent(CharacterData characterData)
    {
        return characterData == null || characterData.maxHp <= 0
            ? 1
            : characterData.hp / characterData.maxHp;
    }

    private static T PickRandom<T>(IReadOnlyList<T> items)
    {
        return items == null || items.Count == 0
            ? default
            : items[Rng.Next(items.Count)];
    }

    private sealed class PlanningContext
    {
        public EnemyData Enemy { get; set; }
        public List<EnemyCommandData> Commands { get; set; } = new();
        public List<PlayerData> LivingPlayers { get; set; } = new();
        public List<EnemyData> LivingEnemies { get; set; } = new();
        public List<SlotProjection> Slots { get; } = new();
        public List<int> FreeSlots { get; } = new();
        public List<QueuedAction> QueuedActions { get; } = new();

        public bool HasQueuedCommand(int commandId)
        {
            return QueuedActions.Any(action => action.CommandId == commandId);
        }

        public int CountQueuedCommand(int commandId)
        {
            return QueuedActions.Count(action => action.CommandId == commandId);
        }

        public QueuedAction GetFirstQueuedAction(int commandId)
        {
            return QueuedActions
                .Where(action => action.CommandId == commandId)
                .OrderBy(action => action.SlotIndex)
                .FirstOrDefault();
        }
    }

    private sealed class SlotProjection
    {
        public int SlotIndex { get; set; }
        public CombatAreaId EnemyAreaId { get; set; } = CombatAreaId.Unknown;
        public List<PlayerData> SameAreaPlayers { get; set; } = new();
        public bool IsFree { get; set; }
        public bool HasSameAreaPlayer => SameAreaPlayers.Count > 0;
    }

    private sealed class QueuedAction
    {
        public int SlotIndex { get; set; }
        public CommandExecuteInfo CommandInfo { get; set; }
        public int CommandId => CommandInfo?.commandData?.commandId ?? -1;
    }

    private sealed class ReleaseCandidate
    {
        public EnemyCommandData Command { get; }
        public List<int> SlotIndexes { get; }
        private Func<int, EnemyActionPlan> BuildPlan { get; }

        public ReleaseCandidate(
            EnemyCommandData command,
            List<int> slotIndexes,
            Func<int, EnemyActionPlan> buildPlan)
        {
            Command = command;
            SlotIndexes = slotIndexes;
            BuildPlan = buildPlan;
        }

        public EnemyActionPlan BuildRandomPlan()
        {
            int slot = PickRandom(SlotIndexes);
            return slot < Timeline.MinSlotIndex
                ? null
                : BuildPlan(slot);
        }
    }
}

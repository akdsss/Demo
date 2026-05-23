using System.Collections.Generic;

public static class LegacyCommandAdapter
{
    public static PlannedAction ToPlannedAction(
        this CommandExecuteInfo commandExecuteInfo,
        int slotIndex,
        CombatFaction faction = CombatFaction.Unknown)
    {
        if (commandExecuteInfo == null || commandExecuteInfo.isDefault)
        {
            return PlannedAction.Empty(slotIndex, faction);
        }

        SkillDefinition skill = SkillDefinition.FromCommandData(commandExecuteInfo.commandData);
        CharacterState source = CharacterState.FromCharacterData(commandExecuteInfo.sourceCharacterData);
        CharacterState target = CharacterState.FromCharacterData(commandExecuteInfo.targetCharacterData);
        CombatFaction resolvedFaction = ResolveFaction(faction, source, commandExecuteInfo.commandData);

        PlannedAction action = new()
        {
            ActionId = BuildActionId(source, skill, slotIndex),
            IsDefault = false,
            SlotIndex = slotIndex,
            Faction = resolvedFaction,
            Source = source,
            Skill = skill,
            TargetCharacter = target.LegacyCharacterData == null ? null : target,
            LegacyCommandExecuteInfo = commandExecuteInfo
        };

        if (skill.TargetType == SkillTargetType.Area || skill.HasTag(SkillTag.Move))
        {
            CombatAreaId targetAreaId = commandExecuteInfo.targetAreaId;
            if (targetAreaId == CombatAreaId.Unknown && target.LegacyCharacterData != null)
            {
                targetAreaId = target.CurrentAreaId;
            }
            if (targetAreaId == CombatAreaId.Unknown)
            {
                targetAreaId = AreaDefinition.GetAreaIdForLegacyCoord(commandExecuteInfo.targetCoord);
            }
            action.TargetAreaId = targetAreaId;
            if (targetAreaId != CombatAreaId.Unknown)
            {
                action.TargetCoord = AreaDefinition.GetLegacyCoordForAreaId(targetAreaId);
                action.TargetArea = AreaDefinition.FromKnownArea(targetAreaId);
            }
        }
        else if (action.TargetCharacter != null)
        {
            action.TargetAreaId = action.TargetCharacter.CurrentAreaId;
            action.TargetArea = action.TargetCharacter.CurrentArea;
        }

        return action;
    }

    public static Timeline ToTimeline(
        this CharacterData characterData,
        CombatFaction faction = CombatFaction.Unknown,
        int slotCount = Timeline.MaxSlotIndex)
    {
        CombatFaction resolvedFaction = ResolveFaction(faction, CharacterState.FromCharacterData(characterData), null);
        Timeline timeline = new(resolvedFaction, slotCount);
        if (characterData?.commandQueue == null)
        {
            return timeline;
        }

        for (int i = 0; i < characterData.commandQueue.Count && i < timeline.Slots.Count; i++)
        {
            CommandExecuteInfo command = characterData.commandQueue[i];
            PlannedAction action = command.ToPlannedAction(i + 1, resolvedFaction);
            if (!action.IsDefault)
            {
                timeline.TryPlaceAction(action, out _);
            }
        }

        return timeline;
    }

    public static List<PlannedAction> ToPlannedActions(IEnumerable<CharacterData> characters)
    {
        List<PlannedAction> actions = new();
        if (characters == null)
        {
            return actions;
        }

        foreach (CharacterData character in characters)
        {
            if (character?.commandQueue == null)
            {
                continue;
            }

            CharacterState source = CharacterState.FromCharacterData(character);
            for (int i = 0; i < character.commandQueue.Count; i++)
            {
                PlannedAction action = character.commandQueue[i].ToPlannedAction(i + 1, source.Faction);
                if (!action.IsDefault)
                {
                    actions.Add(action);
                }
            }
        }

        return actions;
    }

    private static CombatFaction ResolveFaction(CombatFaction explicitFaction, CharacterState source, CommandData commandData)
    {
        if (explicitFaction != CombatFaction.Unknown)
        {
            return explicitFaction;
        }

        if (source != null && source.Faction != CombatFaction.Unknown)
        {
            return source.Faction;
        }

        return commandData switch
        {
            PlayerCommandData => CombatFaction.Player,
            EnemyCommandData => CombatFaction.Enemy,
            _ => CombatFaction.Unknown
        };
    }

    private static string BuildActionId(CharacterState source, SkillDefinition skill, int slotIndex)
    {
        string sourceId = source == null ? "unknown_source" : source.StableId;
        string skillId = skill == null ? "unknown_skill" : skill.Id;
        return $"{sourceId}_{skillId}_slot_{slotIndex}";
    }
}

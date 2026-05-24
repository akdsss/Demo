public class SkillValidator
{
    public SkillValidationResult Validate(SkillDefinition skill, ValidationContext context)
    {
        if (skill == null)
        {
            return SkillValidationResult.Fail(SkillFailReason.MissingSkill);
        }

        if (context == null || context.Source == null)
        {
            return SkillValidationResult.Fail(SkillFailReason.MissingSource, skill.FailTextKey);
        }

        CharacterState source = context.Source;
        if (source.IsDefeated)
        {
            return SkillValidationResult.Fail(SkillFailReason.SourceDefeated, skill.FailTextKey);
        }

        if (source.RemainingActions < context.RequiredActionCost)
        {
            return SkillValidationResult.Fail(SkillFailReason.ActionChanceInsufficient, skill.FailTextKey);
        }

        if (skill.MpCost > 0 && source.CurrentMp < skill.MpCost)
        {
            return SkillValidationResult.Fail(SkillFailReason.MpInsufficient, skill.FailTextKey);
        }

        if (source.HasBlockedTag(skill.Tags))
        {
            return SkillValidationResult.Fail(SkillFailReason.BlockedByStatus, skill.FailTextKey);
        }

        SkillValidationResult targetResult = ValidateTarget(skill, context);
        if (!targetResult.IsValid)
        {
            return targetResult;
        }

        SkillValidationResult slotResult = ValidateTimelineSlot(context);
        if (!slotResult.IsValid)
        {
            return slotResult;
        }

        return SkillValidationResult.Success();
    }

    private static SkillValidationResult ValidateTarget(SkillDefinition skill, ValidationContext context)
    {
        bool needsCharacterTarget =
            skill.TargetType == SkillTargetType.Ally ||
            skill.TargetType == SkillTargetType.Enemy ||
            skill.TargetType == SkillTargetType.Character;

        if (needsCharacterTarget && context.TargetCharacter == null)
        {
            return SkillValidationResult.Fail(SkillFailReason.MissingTarget, skill.FailTextKey);
        }

        if (context.TargetCharacter != null && context.TargetCharacter.IsDefeated)
        {
            return SkillValidationResult.Fail(SkillFailReason.TargetDefeated, skill.FailTextKey);
        }

        if (skill.HasTag(SkillTag.Melee) && context.TargetCharacter != null)
        {
            if (context.Source.CurrentAreaId != CombatAreaId.Unknown &&
                context.TargetCharacter.CurrentAreaId != CombatAreaId.Unknown &&
                context.Source.CurrentAreaId != context.TargetCharacter.CurrentAreaId)
            {
                return SkillValidationResult.Fail(SkillFailReason.MeleeTargetNotInSameArea, skill.FailTextKey);
            }
        }

        if (skill.HasTag(SkillTag.Move))
        {
            if (IsTargetSameAsSourceArea(context))
            {
                return SkillValidationResult.Fail(SkillFailReason.MoveTargetAlreadyCurrentArea, skill.FailTextKey);
            }

            if (context.RequiresDifferentTargetArea && context.TargetCharacter != null)
            {
                if (context.Source.CurrentAreaId != CombatAreaId.Unknown &&
                    context.TargetCharacter.CurrentAreaId != CombatAreaId.Unknown &&
                    context.Source.CurrentAreaId == context.TargetCharacter.CurrentAreaId)
                {
                    return SkillValidationResult.Fail(SkillFailReason.RushTargetMustBeInDifferentArea, skill.FailTextKey);
                }
            }
        }

        if (skill.RequiresDifferentTargetArea &&
            context.TargetCharacter != null &&
            context.TargetAreaId != CombatAreaId.Unknown &&
            context.TargetAreaId == context.TargetCharacter.CurrentAreaId)
        {
            return SkillValidationResult.Fail(SkillFailReason.RushTargetMustBeInDifferentArea, skill.FailTextKey);
        }

        if (skill.TargetType == SkillTargetType.Area &&
            context.TargetAreaId == CombatAreaId.Unknown &&
            context.TargetArea == null &&
            !context.TargetCoord.HasValue)
        {
            return SkillValidationResult.Fail(SkillFailReason.MissingTarget, skill.FailTextKey);
        }

        return SkillValidationResult.Success();
    }

    private static bool IsTargetSameAsSourceArea(ValidationContext context)
    {
        if (context.Source == null)
        {
            return false;
        }

        if (context.TargetAreaId != CombatAreaId.Unknown)
        {
            return context.TargetAreaId == context.Source.CurrentAreaId;
        }

        if (context.TargetCoord.HasValue &&
            AreaDefinition.GetAreaIdForLegacyCoord(context.TargetCoord.Value) == context.Source.CurrentAreaId)
        {
            return true;
        }

        if (context.TargetArea != null && context.Source.CurrentArea != null)
        {
            return context.TargetArea.IsSameArea(context.Source.CurrentArea);
        }

        return false;
    }

    private static SkillValidationResult ValidateTimelineSlot(ValidationContext context)
    {
        if (context.Timeline == null)
        {
            return SkillValidationResult.Success();
        }

        TimelineSlot slot = context.Timeline.GetSlot(context.SlotIndex);
        if (slot == null || !slot.CanPlaceAction)
        {
            return SkillValidationResult.Fail(SkillFailReason.TimelineSlotUnavailable);
        }

        return SkillValidationResult.Success();
    }
}

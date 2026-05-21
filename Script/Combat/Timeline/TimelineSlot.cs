public class TimelineSlot
{
    public int SlotIndex { get; set; }
    public CombatFaction Faction { get; set; } = CombatFaction.Unknown;
    public PlannedAction Action { get; private set; }
    public bool IsLocked { get; private set; }
    public bool IsRevealed { get; private set; } = true;

    public TimelineSlotState State
    {
        get
        {
            if (IsLocked)
            {
                return TimelineSlotState.Locked;
            }

            return HasAction ? TimelineSlotState.Occupied : TimelineSlotState.Empty;
        }
    }

    public bool HasAction
    {
        get { return Action != null && !Action.IsDefault; }
    }

    public bool CanPlaceAction
    {
        get { return !IsLocked && !HasAction; }
    }

    public TimelineSlot(int slotIndex, CombatFaction faction)
    {
        SlotIndex = slotIndex;
        Faction = faction;
        Action = PlannedAction.Empty(slotIndex, faction);
    }

    public bool TrySetAction(PlannedAction action)
    {
        if (!CanPlaceAction || action == null)
        {
            return false;
        }

        action.SlotIndex = SlotIndex;
        action.Faction = Faction;
        Action = action;
        return true;
    }

    public void Lock()
    {
        IsLocked = true;
    }

    public void Reveal()
    {
        IsRevealed = true;
        if (Action != null)
        {
            Action.IsRevealed = true;
        }
    }

    public void Hide()
    {
        IsRevealed = false;
        if (Action != null)
        {
            Action.IsRevealed = false;
        }
    }

    public void ClearIfUnlocked()
    {
        if (IsLocked)
        {
            return;
        }

        Action = PlannedAction.Empty(SlotIndex, Faction);
        IsRevealed = true;
    }
}

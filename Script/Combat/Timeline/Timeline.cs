using System.Collections.Generic;

public class Timeline
{
    public const int MinSlotIndex = 1;
    public const int MaxSlotIndex = 6;

    public CombatFaction Faction { get; }
    public List<TimelineSlot> Slots { get; } = new();

    public Timeline(CombatFaction faction = CombatFaction.Unknown, int slotCount = MaxSlotIndex)
    {
        Faction = faction;
        int normalizedSlotCount = slotCount <= 0 ? MaxSlotIndex : slotCount;
        for (int i = MinSlotIndex; i <= normalizedSlotCount; i++)
        {
            Slots.Add(new TimelineSlot(i, faction));
        }
    }

    public TimelineSlot GetSlot(int slotIndex)
    {
        if (slotIndex < MinSlotIndex || slotIndex > Slots.Count)
        {
            return null;
        }

        return Slots[slotIndex - 1];
    }

    public bool TryPlaceAction(PlannedAction action, out SkillFailReason failReason)
    {
        failReason = SkillFailReason.None;
        if (action == null)
        {
            failReason = SkillFailReason.MissingSkill;
            return false;
        }

        TimelineSlot slot = GetSlot(action.SlotIndex);
        if (slot == null || !slot.CanPlaceAction)
        {
            failReason = SkillFailReason.TimelineSlotUnavailable;
            return false;
        }

        bool placed = slot.TrySetAction(action);
        if (!placed)
        {
            failReason = SkillFailReason.TimelineSlotUnavailable;
        }

        return placed;
    }

    public void LockAll()
    {
        foreach (TimelineSlot slot in Slots)
        {
            slot.Lock();
        }
    }

    public void RevealAll()
    {
        foreach (TimelineSlot slot in Slots)
        {
            slot.Reveal();
        }
    }

    public void ClearUnlocked()
    {
        foreach (TimelineSlot slot in Slots)
        {
            slot.ClearIfUnlocked();
        }
    }
}

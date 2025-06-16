using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.Events.Vanilla.Meeting;
using MiraAPI.Modifiers;
using TownOfUs.Modifiers.Crewmate;

namespace TownOfUs.Events.Crewmate;

public static class ImitatorEvents
{
    [RegisterEvent]
    public static void MeetingHandler(StartMeetingEvent @event)
    {
        var imitators = ModifierUtils.GetActiveModifiers<ImitatorCacheModifier>();
        if (!imitators.Any()) return;
        foreach (var mod in imitators)
        {
            // This makes converted imitators not be imitators anymore
            if (mod.Player.Data.Role.GetType() != mod.OldRole.GetType()) mod.ModifierComponent?.RemoveModifier(mod);
        }
    }

    [RegisterEvent]
    public static void RoundStartEventHandler(RoundStartEvent @event)
    {
        if (@event.TriggeredByIntro) return;
        var imitators = ModifierUtils.GetActiveModifiers<ImitatorCacheModifier>();
        if (!imitators.Any()) return;
        foreach (var mod in imitators)
        {
            if (mod.Player.AmOwner) mod.UpdateRole();
        }
    }
}

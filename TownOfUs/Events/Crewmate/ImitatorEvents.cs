using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.Events.Vanilla.Meeting;
using MiraAPI.Modifiers;
using TownOfUs.Events.TouEvents;
using TownOfUs.Modifiers.Crewmate;
using TownOfUs.Utilities;

namespace TownOfUs.Events.Crewmate;

public static class ImitatorEvents
{
    [RegisterEvent]
    public static void MeetingHandler(StartMeetingEvent @event)
    {
        var imitators = ModifierUtils.GetActiveModifiers<ImitatorCacheModifier>().ToArray();

        if (imitators.Length == 0)
        {
            return;
        }

        // This makes converted imitators not be imitators anymore
        foreach (var mod in imitators.Where(x => !x.Player.IsCrewmate()))
        {
            mod.ModifierComponent?.RemoveModifier(mod);
        }
    }

    [RegisterEvent(1001)]
    public static void RoundStartEventHandler(RoundStartEvent @event)
    {
        if (@event.TriggeredByIntro)
        {
            return;
        }

        var imitators = ModifierUtils.GetActiveModifiers<ImitatorCacheModifier>();

        if (!imitators.Any())
        {
            return;
        }

        foreach (var mod in imitators)
        {
            if (mod.Player.AmOwner)
            {
                mod.UpdateRole();
            }
        }
    }

    [RegisterEvent]
    public static void ChangeRoleHandler(ChangeRoleEvent @event)
    {
        if (!PlayerControl.LocalPlayer)
        {
            return;
        }

        var player = @event.Player;

        if (player.HasModifier<ImitatorCacheModifier>() && !@event.NewRole.IsCrewmate())
        {
            player.RemoveModifier<ImitatorCacheModifier>();
        }
    }
}
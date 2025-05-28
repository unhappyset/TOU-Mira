using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Meeting;
using MiraAPI.Modifiers;
using TownOfUs.Modifiers.Crewmate;
using TownOfUs.Roles.Crewmate;

namespace TownOfUs.Events.Crewmate;

public static class ImitatorEvents
{
    [RegisterEvent]
    public static void MeetingHandler(StartMeetingEvent @event)
    {
        var imitators = ModifierUtils.GetActiveModifiers<ImitatorCacheModifier>();
        foreach (var mod in imitators)
        {
            mod.ModifierComponent?.RemoveModifier(mod);
        }
    }

    [RegisterEvent]
    public static void EjectionEventHandler(EjectionEvent @event)
    {
        if (PlayerControl.LocalPlayer?.Data?.Role is ImitatorRole imitatorRole)
        {
            imitatorRole.UpdateRole();
        }
    }
}

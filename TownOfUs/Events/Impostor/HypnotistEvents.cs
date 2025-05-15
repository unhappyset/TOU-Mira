using HarmonyLib;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Meeting;
using MiraAPI.Modifiers;
using MiraAPI.Roles;
using TownOfUs.Modifiers.Impostor;
using TownOfUs.Roles.Impostor;

namespace TownOfUs.Events.Impostor;

public static class HypnotistEvents
{
    [RegisterEvent]
    private static void StartMeetingEventHandler(StartMeetingEvent @event)
    {
        foreach (var mod in ModifierUtils.GetActiveModifiers<HypnotisedModifier>())
        {
            mod.UnHysteria();
        }
    }

    [RegisterEvent]
    public static void EjectionEventHandler(EjectionEvent @event)
    {
        foreach (var hypnotist in CustomRoleUtils.GetActiveRolesOfType<HypnotistRole>())
        {
            if (!hypnotist.HysteriaActive) continue;
            var mods = ModifierUtils.GetActiveModifiers<HypnotisedModifier>(x => x.Hypnotist == hypnotist.Player);
            mods.Do(x => x.Hysteria());
        }
    }
}

using HarmonyLib;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Meeting;
using MiraAPI.Modifiers;
using MiraAPI.Utilities;
using TownOfUs.Modifiers.Impostor;

namespace TownOfUs.Events.Impostor;

public static class BlackmailerEvents
{
    [RegisterEvent]
    public static void EjectionEvent(EjectionEvent @event)
    {
        var players = Helpers.GetAlivePlayers().Where(plr => plr.HasModifier<BlackmailedModifier>()).ToList();
        players.Do(x => x.RemoveModifier<BlackmailedModifier>());
    }
}

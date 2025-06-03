using HarmonyLib;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.Events.Vanilla.Meeting;
using MiraAPI.Modifiers;
using TownOfUs.Modifiers.Crewmate;
using TownOfUs.Modules;
using TownOfUs.Roles.Crewmate;
using TownOfUs.Utilities;

namespace TownOfUs.Events.Crewmate;

public static class JailorEvents
{
    [RegisterEvent]
    public static void EjectionEventHandler(EjectionEvent @event)
    {
        if (!AmongUsClient.Instance.AmHost) return;

        ModifierUtils.GetPlayersWithModifier<JailedModifier>().Do(x => x.RpcRemoveModifier<JailedModifier>());
    }

    [RegisterEvent]
    public static void AfterMurderEventHandler(AfterMurderEvent @event)
    {
        var source = @event.Source;
        var victim = @event.Target;

        if (victim.Data.Role is JailorRole && MeetingHud.Instance == null)
        {
            ModifierUtils.GetPlayersWithModifier<JailedModifier>().Do(x => x.RemoveModifier<JailedModifier>());
        }

        if (source.Data.Role is not JailorRole jailor) return;

        jailor.Executes--;

        var target = @event.Target;

        if (GameHistory.PlayerStats.TryGetValue(source.PlayerId, out var stats))
        {
            if (!target.IsCrewmate())
            {
                stats.CorrectKills += 1;
            }
            else if (source != target)
            {
                jailor.Executes = 0;
                stats.IncorrectKills += 1;
            }
        }
    }
}

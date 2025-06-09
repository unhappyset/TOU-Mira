using HarmonyLib;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.Events.Vanilla.Meeting;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using TownOfUs.Modifiers.Crewmate;
using TownOfUs.Modifiers.Game;
using TownOfUs.Modules;
using TownOfUs.Options.Roles.Crewmate;
using TownOfUs.Roles.Crewmate;
using TownOfUs.Utilities;

namespace TownOfUs.Events.Crewmate;

public static class JailorEvents
{
    [RegisterEvent]
    public static void EjectionEventHandler(EjectionEvent @event)
    {
        var sparedPlayers = ModifierUtils.GetPlayersWithModifier<JailSparedModifier>().ToList();
        sparedPlayers.Do(x => x.RemoveModifier<JailSparedModifier>());

        var players = ModifierUtils.GetPlayersWithModifier<JailedModifier>().ToList();
        if (!OptionGroupSingleton<JailorOptions>.Instance.JailInARow) players.Do(x => x.AddModifier<JailSparedModifier>(x.GetModifier<JailedModifier>()!.JailorId));
        players.Do(x => x.RemoveModifier<JailedModifier>());
    }

    [RegisterEvent]
    public static void AfterMurderEventHandler(AfterMurderEvent @event)
    {
        var source = @event.Source;
        var victim = @event.Target;

        if ((victim.Data.Role is JailorRole || victim.GetRoleWhenAlive() is JailorRole) && MeetingHud.Instance == null)
        {
            ModifierUtils.GetPlayersWithModifier<JailedModifier>().Do(x => x.RemoveModifier<JailedModifier>());
        }

        if (source.Data.Role is not JailorRole jailor) return;

        jailor.Executes--;

        var target = @event.Target;

        if (GameHistory.PlayerStats.TryGetValue(source.PlayerId, out var stats))
        {
            if (!target.IsCrewmate() || target.HasModifier<AllianceGameModifier>() || source.HasModifier<AllianceGameModifier>())
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

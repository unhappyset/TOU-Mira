using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.Modifiers;
using TownOfUs.Modifiers.Crewmate;
using TownOfUs.Modules;
using TownOfUs.Roles.Crewmate;
using TownOfUs.Utilities;

namespace TownOfUs.Events.Crewmate;

public static class DeputyEvents
{
    [RegisterEvent]
    public static void RoundStartHandler(RoundStartEvent @event)
    {
        if (PlayerControl.LocalPlayer.Data.Role is DeputyRole) DeputyRole.OnRoundStart();
    }
    [RegisterEvent]
    public static void AfterMurderEventHandler(AfterMurderEvent @event)
    {
        var source = @event.Source;
        var target = @event.Target;

        CheckForDeputyCamped(source, target);

        if (source.Data.Role is not DeputyRole) return;

        if (GameHistory.PlayerStats.TryGetValue(source.PlayerId, out var stats))
        {
            if (!target.IsCrewmate())
            {
                stats.CorrectKills += 1;
            }
            else if (source != target)
            {
                stats.IncorrectKills += 1;
            }
        }
    }

    private static void CheckForDeputyCamped(PlayerControl source, PlayerControl target)
    {
        if (!target.HasModifier<DeputyCampedModifier>()) return;

        var mod = target.GetModifier<DeputyCampedModifier>();

        if (mod == null) return;
        if (mod.Deputy.Data.Role is not DeputyRole deputy) return;

        deputy.Killer = source;
    }
}

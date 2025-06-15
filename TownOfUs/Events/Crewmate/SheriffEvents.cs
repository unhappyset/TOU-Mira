using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.GameOptions;
using MiraAPI.Hud;
using TownOfUs.Buttons.Crewmate;
using TownOfUs.Modules;
using TownOfUs.Options.Roles.Crewmate;
using TownOfUs.Roles;
using TownOfUs.Roles.Crewmate;
using TownOfUs.Utilities;

namespace TownOfUs.Events.Neutral;

public static class SheriffEvents
{
    [RegisterEvent]
    public static void RoundStartHandler(RoundStartEvent @event)
    {
        if (@event.TriggeredByIntro)
        {
            CustomButtonSingleton<SheriffShootButton>.Instance.FailedShot = false;
        }
        else if (PlayerControl.LocalPlayer.Data.Role is SheriffRole) SheriffRole.OnRoundStart();
    }
    [RegisterEvent]
    public static void AfterMurderEventHandler(AfterMurderEvent @event)
    {
        var source = @event.Source;

        if (source.Data.Role is not SheriffRole) return;

        var target = @event.Target;
        var options = OptionGroupSingleton<SheriffOptions>.Instance;

        if (GameHistory.PlayerStats.TryGetValue(source.PlayerId, out var stats))
        {
            if (target.IsImpostor() ||
                target.IsCrewmate() ||
                (target.Is(RoleAlignment.NeutralEvil) && options.ShootNeutralEvil) ||
                (target.Is(RoleAlignment.NeutralKilling) && options.ShootNeutralKiller))
            {
                stats.CorrectKills += 1;
            }
            else if (source == target)
            {
                stats.IncorrectKills += 1;
            }
        }
    }
}

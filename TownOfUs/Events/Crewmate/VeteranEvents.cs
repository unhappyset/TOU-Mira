using MiraAPI.Events;
using MiraAPI.Events.Mira;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Modifiers;
using MiraAPI.Networking;
using TownOfUs.Modifiers;
using TownOfUs.Modifiers.Crewmate;
using TownOfUs.Modifiers.Game;
using TownOfUs.Modules;
using TownOfUs.Options.Roles.Crewmate;
using TownOfUs.Roles.Crewmate;
using TownOfUs.Utilities;

namespace TownOfUs.Events.Crewmate;

public static class VeteranEvents
{
    [RegisterEvent]
    public static void MiraButtonClickEventHandler(MiraButtonClickEvent @event)
    {
        var button = @event.Button as CustomActionButton<PlayerControl>;
        var source = PlayerControl.LocalPlayer;
        var target = button?.Target;

        if (target == null || button == null || !button.CanClick()) return;

        CheckForVeteranAlert(@event, source, target);
    }

    [RegisterEvent]
    public static void BeforeMurderEventHandler(BeforeMurderEvent @event)
    {
        var source = @event.Source;
        var target = @event.Target;

        CheckForVeteranAlert(@event, source, target);
    }

    [RegisterEvent]
    public static void AfterMurderEventHandler(AfterMurderEvent @event)
    {
        var source = @event.Source;

        if (source.Data.Role is not VeteranRole) return;

        var target = @event.Target;

        if (GameHistory.PlayerStats.TryGetValue(source.PlayerId, out var stats))
        {
            if (!target.IsCrewmate() || target.HasModifier<AllianceGameModifier>())
            {
                stats.CorrectKills += 1;
            }
            else if (source != target)
            {
                stats.IncorrectKills += 1;
            }
        }
    }

    private static void CheckForVeteranAlert(MiraCancelableEvent miraEvent, PlayerControl source, PlayerControl target)
    {
        var preventAttack = source.HasModifier<IndirectAttackerModifier>();

        if (target.HasModifier<VeteranAlertModifier>() && source != target && (!preventAttack || 
            (preventAttack && source.IsImpostor() && !OptionGroupSingleton<VeteranOptions>.Instance.IndirectKills)))
        {
            if (!OptionGroupSingleton<VeteranOptions>.Instance.KilledOnAlert) miraEvent.Cancel();

            if (source.AmOwner && !preventAttack)
            {
                target.RpcCustomMurder(source);
            }
        }
    }
}

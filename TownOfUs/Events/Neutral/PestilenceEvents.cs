using MiraAPI.Events;
using MiraAPI.Events.Mira;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.Hud;
using MiraAPI.Networking;
using TownOfUs.Roles.Neutral;

namespace TownOfUs.Events.Neutral;

public static class PestilenceEvents
{
    [RegisterEvent(priority: -1)]
    public static void MiraButtonClickEventHandler(MiraButtonClickEvent @event)
    {
        var button = @event.Button as CustomActionButton<PlayerControl>;
        var source = PlayerControl.LocalPlayer;
        var target = button?.Target;

        if (target == null || button == null || !button.CanClick()) return;

        CheckForPest(@event, source, target);
    }

    [RegisterEvent(priority: -1)]
    public static void BeforeMurderEventHandler(BeforeMurderEvent @event)
    {
        var source = @event.Source;
        var target = @event.Target;

        CheckForPest(@event, source, target);
    }

    private static void CheckForPest(MiraCancelableEvent miraEvent, PlayerControl source, PlayerControl target)
    {
        if (target.Data.Role is PestilenceRole)
        {
            miraEvent.Cancel();

            if (source.AmOwner)
            {
                target.RpcCustomMurder(source);
            }
        }
    }
}

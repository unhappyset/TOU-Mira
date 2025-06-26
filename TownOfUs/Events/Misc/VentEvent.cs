using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Usables;
using MiraAPI.Utilities;

namespace TownOfUs.Events.Misc;

public static class VentCanUseEvent
{
    [RegisterEvent]

    public static void VentCanUseHandler(PlayerCanUseEvent @event)
    {
        if (GameOptionsManager.Instance.currentGameMode == AmongUs.GameOptions.GameModes.HideNSeek) return;
        if (!@event.IsVent) return;
        if (Helpers.GetAlivePlayers().Count > 2) return;

        var player = PlayerControl.LocalPlayer;
        if (player.inVent)
        {
            player.MyPhysics.RpcExitVent(Vent.currentVent.Id);
            player.MyPhysics.ExitAllVents();
        }

        @event.Cancel();
    }
}
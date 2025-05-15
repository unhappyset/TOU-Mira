using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.Events.Vanilla.Meeting;
using TownOfUs.Patches;
using TownOfUs.Roles;

namespace TownOfUs.Events.Misc;

public static class ZoomEvents
{

    [RegisterEvent]
    public static void RoundStartEventHandler(RoundStartEvent @event)
    {
        if (@event.TriggeredByIntro) return;
        if (PlayerControl.LocalPlayer.Data.IsDead && (PlayerControl.LocalPlayer.Data.Role is IGhostRole { Caught: true } || PlayerControl.LocalPlayer.Data.Role is not IGhostRole))
        {
            HudManagerPatches.ZoomButton.SetActive(true);
        }
    }

    [RegisterEvent]
    public static void ReportBodyEventHandler(ReportBodyEvent @event)
    {
        HudManagerPatches.ZoomButton.SetActive(false);
        if (HudManagerPatches.Zooming) HudManagerPatches.Zoom();
    }
}

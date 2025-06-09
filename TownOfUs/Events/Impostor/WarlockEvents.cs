using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.Hud;
using TownOfUs.Buttons.Impostor;
using TownOfUs.Roles.Impostor;
using TownOfUs.Utilities;

namespace TownOfUs.Events.Impostor;

public static class WarlockEvents
{
    [RegisterEvent]
    public static void AfterMurderEventHandler(AfterMurderEvent @event)
    {
        if (!@event.Source.AmOwner || !@event.Source.IsRole<WarlockRole>())
        {
            return;
        }

        var button = CustomButtonSingleton<WarlockKillButton>.Instance;
        if (button.BurstActive) ++button.Kills;
    }
}

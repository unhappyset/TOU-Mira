using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.GameOptions;
using TownOfUs.Options.Roles.Neutral;
using TownOfUs.Roles.Neutral;
using TownOfUs.Utilities;

namespace TownOfUs.Events.Neutral;

public static class DoomsayerEvents
{
    [RegisterEvent]
    public static void AfterMurderEventHandler(AfterMurderEvent @event)
    {
        if (@event.Source.Data.Role is DoomsayerRole && @event.Source.AmOwner)
        {
            DoomsayerRole.RpcDoomsayerWin(@event.Source);
        }
    }

    [RegisterEvent]
    public static void RoundStartEventHandler(RoundStartEvent @event)
    {
        if (@event.TriggeredByIntro) return;
        if (OptionGroupSingleton<DoomsayerOptions>.Instance.WinEndsGame) return;

        if (PlayerControl.LocalPlayer.Data.Role is DoomsayerRole doom && doom.AllGuessesCorrect)
        {
            PlayerControl.LocalPlayer.RpcPlayerExile();
        }
    }
}

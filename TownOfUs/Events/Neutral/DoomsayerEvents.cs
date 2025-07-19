using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.GameOptions;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using TownOfUs.Modifiers;
using TownOfUs.Options.Roles.Neutral;
using TownOfUs.Roles.Neutral;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfUs.Events.Neutral;

public static class DoomsayerEvents
{
    [RegisterEvent]
    public static void AfterMurderEventHandler(AfterMurderEvent @event)
    {
        if (@event.Source.Data.Role is DoomsayerRole doom && @event.Source.AmOwner &&
            (int)OptionGroupSingleton<DoomsayerOptions>.Instance.DoomsayerGuessesToWin == doom.NumberOfGuesses)
        {
            DoomsayerRole.RpcDoomsayerWin(@event.Source);
            DeathHandlerModifier.RpcUpdateDeathHandler(PlayerControl.LocalPlayer, "Victorious", DeathEventHandlers.CurrentRound, DeathHandlerOverride.SetFalse, lockInfo: DeathHandlerOverride.SetTrue);
        }
    }

    [RegisterEvent]
    public static void RoundStartEventHandler(RoundStartEvent @event)
    {
        if (@event.TriggeredByIntro)
        {
            return;
        }

        if (OptionGroupSingleton<DoomsayerOptions>.Instance.DoomWin is not DoomWinOptions.Leaves)
        {
            return;
        }

        var doom = CustomRoleUtils.GetActiveRolesOfType<DoomsayerRole>()
            .FirstOrDefault(x => x.AllGuessesCorrect && !x.Player.HasDied());
        if (doom != null)
        {
            if (doom.Player.AmOwner)
            {
                PlayerControl.LocalPlayer.RpcPlayerExile();
                var notif1 = Helpers.CreateAndShowNotification(
                    $"<b>You have successfully won as the {TownOfUsColors.Doomsayer.ToTextColor()}Doomsayer</color>, as you have guessed enough players successfully!</b>",
                    Color.white, spr: TouRoleIcons.Doomsayer.LoadAsset());

                notif1.Text.SetOutlineThickness(0.35f);
                notif1.transform.localPosition = new Vector3(0f, 1f, -20f);
            }
            else
            {
                var notif1 = Helpers.CreateAndShowNotification(
                    $"<b>The {TownOfUsColors.Doomsayer.ToTextColor()}Doomsayer</color>, {doom.Player.Data.PlayerName}, has successfully won, as they have guessed enough players!</b>",
                    Color.white, spr: TouRoleIcons.Doomsayer.LoadAsset());

                notif1.Text.SetOutlineThickness(0.35f);
                notif1.transform.localPosition = new Vector3(0f, 1f, -20f);
            }
        }
    }
}
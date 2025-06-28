using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Modifiers;
using MiraAPI.Networking;
using MiraAPI.Utilities;
using TownOfUs.Modifiers;
using TownOfUs.Modules;
using TownOfUs.Modules.Localization;
using TownOfUs.Options.Roles.Neutral;
using TownOfUs.Roles.Neutral;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfUs.Events.Neutral;

public static class JesterEvents
{
    [RegisterEvent]
    public static void RoundStartEventHandler(RoundStartEvent @event)
    {
        if (@event.TriggeredByIntro) return;
        if (OptionGroupSingleton<JesterOptions>.Instance.JestWin is JestWinOptions.EndsGame) return;
        var jest = PlayerControl.AllPlayerControls.ToArray()
            .FirstOrDefault(plr => plr.Data.IsDead && !plr.Data.Disconnected && plr.GetRoleWhenAlive() is JesterRole jestRole && jestRole.Voted && !jestRole.SentWinMsg);
        if (jest != null)
        {
            var jestRole = jest.GetRoleWhenAlive() as JesterRole;
            if (jestRole == null) return;
            jestRole.SentWinMsg = true;
            
            if (jest.AmOwner)
            {
                var notif1 = Helpers.CreateAndShowNotification(
                    $"<b>You have successfully won as the {TownOfUsColors.Jester.ToTextColor()}{TouLocale.Get(TouNames.Jester, "Jester")}</color>, by getting voted out!</b>", Color.white, spr: TouRoleIcons.Jester.LoadAsset());

                notif1.Text.SetOutlineThickness(0.35f);
                    notif1.transform.localPosition = new Vector3(0f, 1f, -20f);
            }
            else
            {
                var notif1 = Helpers.CreateAndShowNotification(
                    $"<b>The {TownOfUsColors.Jester.ToTextColor()}{TouLocale.Get(TouNames.Jester, "Jester")}</color>, {jest.Data.PlayerName}, has successfully won, as they were voted out!</b>", Color.white, spr: TouRoleIcons.Jester.LoadAsset());

                notif1.Text.SetOutlineThickness(0.35f);
                    notif1.transform.localPosition = new Vector3(0f, 1f, -20f);
            }

            if (OptionGroupSingleton<JesterOptions>.Instance.JestWin is not JestWinOptions.Haunts) return;
            if (!jest.AmOwner) return;

            var voters = jestRole.Voters.ToArray();
            Func<PlayerControl, bool> _playerMatch = plr => voters.Contains(plr.PlayerId) && !plr.HasDied() && !plr.HasModifier<InvulnerabilityModifier>() && plr != PlayerControl.LocalPlayer;

            var killMenu = CustomPlayerMenu.Create();
            killMenu.Begin(
                _playerMatch,
                plr =>
                {
                    killMenu.ForceClose();

                    if (plr != null)
                    {
                        PlayerControl.LocalPlayer.RpcCustomMurder(plr, teleportMurderer: false);
                    }
                });
            }
    }
}

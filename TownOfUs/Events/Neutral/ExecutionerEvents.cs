using HarmonyLib;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.Events.Vanilla.Player;
using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Modifiers;
using MiraAPI.Networking;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using TownOfUs.Modifiers;
using TownOfUs.Modules;
using TownOfUs.Options.Roles.Neutral;
using TownOfUs.Roles.Neutral;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfUs.Events.Neutral;

public static class ExecutionerEvents
{
    [RegisterEvent]
    public static void AfterMurderEventHandler(AfterMurderEvent @event)
    {
        CustomRoleUtils.GetActiveRolesOfType<ExecutionerRole>().Do(x => x.CheckTargetDeath(@event.Target));
        var role = @event.Source.Data.Role;
        if (@event.Source.HasDied()) role = @event.Source.GetRoleWhenAlive();
        if (role is ExecutionerRole exe && OptionGroupSingleton<ExecutionerOptions>.Instance.ExeWin is ExeWinOptions.Torments)
        {
            if (exe.Player.AmOwner)
            {
                PlayerControl.LocalPlayer.RpcPlayerExile();
                var notif1 = Helpers.CreateAndShowNotification(
                    $"<b>You have successfully won as the {TownOfUsColors.Executioner.ToTextColor()}Executioner</color>, as your target was exiled!</b>", Color.white, spr: TouRoleIcons.Executioner.LoadAsset());

                notif1.Text.SetOutlineThickness(0.35f);
                notif1.transform.localPosition = new Vector3(0f, 1f, -20f);
            }
            else
            {
                var notif1 = Helpers.CreateAndShowNotification(
                    $"<b>The {TownOfUsColors.Executioner.ToTextColor()}Executioner</color>, {exe.Player.Data.PlayerName}, has successfully won, as their target was exiled!</b>", Color.white, spr: TouRoleIcons.Executioner.LoadAsset());

                notif1.Text.SetOutlineThickness(0.35f);
                notif1.transform.localPosition = new Vector3(0f, 1f, -20f);
            }
        }
    }

    [RegisterEvent]
    public static void PlayerDeathEventHandler(PlayerDeathEvent @event)
    {
        if (@event.DeathReason != DeathReason.Exile) return;

        CustomRoleUtils.GetActiveRolesOfType<ExecutionerRole>().Do(x => x.CheckTargetEjection(@event.Player));
    }

    [RegisterEvent]
    public static void RoundStartEventHandler(RoundStartEvent @event)
    {
        if (@event.TriggeredByIntro) return;
        if (OptionGroupSingleton<ExecutionerOptions>.Instance.ExeWin is ExeWinOptions.EndsGame) return;

        var exe = CustomRoleUtils.GetActiveRolesOfType<ExecutionerRole>().FirstOrDefault(x => x.TargetVoted && !x.Player.HasDied());
        if (exe != null)
        {
            if (exe.Player.AmOwner)
            {
                if (OptionGroupSingleton<ExecutionerOptions>.Instance.ExeWin is ExeWinOptions.Torments)
                {
                    var voters = exe.Voters.ToArray();
                    Func<PlayerControl, bool> _playerMatch = plr => plr != exe.Target && plr != exe.Player && voters.Contains(plr.PlayerId) && !plr.HasDied() && !plr.HasModifier<InvulnerabilityModifier>();

                    var killMenu = CustomPlayerMenu.Create();
                    killMenu.transform.FindChild("PhoneUI").GetChild(0).GetComponent<SpriteRenderer>().material = PlayerControl.LocalPlayer.cosmetics.currentBodySprite.BodySprite.material;
                    killMenu.transform.FindChild("PhoneUI").GetChild(1).GetComponent<SpriteRenderer>().material = PlayerControl.LocalPlayer.cosmetics.currentBodySprite.BodySprite.material;
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
                else
                {
                    var notif1 = Helpers.CreateAndShowNotification(
                        $"<b>You have successfully won as the {TownOfUsColors.Executioner.ToTextColor()}Executioner</color>, as your target was exiled!</b>", Color.white, spr: TouRoleIcons.Executioner.LoadAsset());

                    notif1.Text.SetOutlineThickness(0.35f);
                    notif1.transform.localPosition = new Vector3(0f, 1f, -20f);
                }

                PlayerControl.LocalPlayer.RpcPlayerExile();
            }
            else if (OptionGroupSingleton<ExecutionerOptions>.Instance.ExeWin is ExeWinOptions.Nothing)
            {
                var notif1 = Helpers.CreateAndShowNotification(
                    $"<b>The {TownOfUsColors.Executioner.ToTextColor()}Executioner</color>, {exe.Player.Data.PlayerName}, has successfully won, as their target was exiled!</b>", Color.white, spr: TouRoleIcons.Executioner.LoadAsset());

                notif1.Text.SetOutlineThickness(0.35f);
                    notif1.transform.localPosition = new Vector3(0f, 1f, -20f);
            }
        }
    }
}

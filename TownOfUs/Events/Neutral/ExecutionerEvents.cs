using HarmonyLib;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.Events.Vanilla.Player;
using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Modifiers;
using MiraAPI.Networking;
using MiraAPI.Roles;
using TownOfUs.Modifiers;
using TownOfUs.Options.Roles.Neutral;
using TownOfUs.Roles.Neutral;
using TownOfUs.Utilities;

namespace TownOfUs.Events.Neutral;

public static class ExecutionerEvents
{
    [RegisterEvent]
    public static void AfterMurderEventHandler(AfterMurderEvent @event)
    {
        CustomRoleUtils.GetActiveRolesOfType<ExecutionerRole>().Do(x => x.CheckTargetDeath(@event.Target));
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
        if (OptionGroupSingleton<ExecutionerOptions>.Instance.ExeWin is ExeWinOptions.Nothing) return;
        if (PlayerControl.LocalPlayer.Data.Role is not ExecutionerRole exe) return;

        if (exe.TargetVoted && !PlayerControl.LocalPlayer.HasDied())
        {
            if (OptionGroupSingleton<ExecutionerOptions>.Instance.ExeWin is ExeWinOptions.Torments)
            {
                var voters = exe.Voters.ToArray();
                Func<PlayerControl, bool> _playerMatch = plr => plr != exe.Target && plr != exe.Player && voters.Contains(plr.PlayerId) && !plr.HasDied() && !plr.HasModifier<InvulnerabilityModifier>();

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

            PlayerControl.LocalPlayer.RpcPlayerExile();
        }
    }
}

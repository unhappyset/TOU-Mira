using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Modifiers;
using MiraAPI.Networking;
using TownOfUs.Modifiers;
using TownOfUs.Modules;
using TownOfUs.Options.Roles.Neutral;
using TownOfUs.Roles.Neutral;
using TownOfUs.Utilities;

namespace TownOfUs.Events.Neutral;

public static class JesterEvents
{
    [RegisterEvent]
    public static void RoundStartEventHandler(RoundStartEvent @event)
    {
        if (@event.TriggeredByIntro) return;
        if (OptionGroupSingleton<JesterOptions>.Instance.JestWin is not JestWinOptions.Haunts) return;
        var role = PlayerControl.LocalPlayer.GetRoleWhenAlive();
        if (PlayerControl.LocalPlayer.Data.Role is JesterRole) role = PlayerControl.LocalPlayer.Data.Role;
        if (role is not JesterRole jester) return;
        if (!jester.Voted) return;

        var voters = jester.Voters.ToArray();
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

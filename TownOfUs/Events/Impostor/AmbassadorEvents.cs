using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Roles;
using TownOfUs.Modifiers.Impostor;
using TownOfUs.Options.Roles.Impostor;
using TownOfUs.Roles.Impostor;
using TownOfUs.Utilities;

namespace TownOfUs.Events.Impostor;

public static class AmbassadorEvents
{
    [RegisterEvent]
    public static void RoundStartEventHandler(RoundStartEvent @event)
    {
        if (@event.TriggeredByIntro) return;
        
        var ambassador = CustomRoleUtils.GetActiveRolesOfType<AmbassadorRole>().FirstOrDefault();
        if (ambassador != null)
        {
            if (ambassador.RoundsCooldown > 0) --ambassador.RoundsCooldown;
            if (ambassador.SelectedPlr == null || ambassador.SelectedRole == null || ambassador.Player.Data.IsDead || ambassador.SelectedPlr.IsDead)
            {
                ambassador.Clear();
                return;
            }

            var player = ambassador.SelectedPlr._object;
            var role = ambassador.SelectedRole.Role;

            --ambassador.RetrainsAvailable;
            ambassador.RoundsCooldown = (int)OptionGroupSingleton<AmbassadorOptions>.Instance.RoundCooldown;
            ambassador.Clear();

            if (player.AmOwner)
            {
                var currenttime = player.killTimer;

                player.RpcAddModifier<AmbassadorRetrainedModifier>((ushort)player.Data.Role.Role);
                player.RpcChangeRole((ushort)role);

                player.SetKillTimer(currenttime);
            }
        }
    }
}
using MiraAPI.GameOptions;
using MiraAPI.Networking;
using TownOfUs.Utilities;
using MiraAPI.Utilities.Assets;
using Reactor.Utilities;
using TownOfUs.Options.Roles.Neutral;
using TownOfUs.Roles.Neutral;
using UnityEngine;

namespace TownOfUs.Buttons.Neutral;

public sealed class WerewolfKillButton : TownOfUsRoleButton<WerewolfRole, PlayerControl>
{
    public override string Name => "Kill";
    public override string Keybind => "ActionSecondary";
    public override Color TextOutlineColor => TownOfUsColors.Werewolf;
    public override float Cooldown => OptionGroupSingleton<WerewolfOptions>.Instance.RampageKillCooldown + MapCooldown;
    public override LoadableAsset<Sprite> Sprite => TouAssets.KillSprite;

    public override bool Enabled(RoleBehaviour? role) => role is WerewolfRole { Rampaging: true };

    protected override void OnClick()
    {
        if (Target == null)
        {
            Logger<TownOfUsPlugin>.Error("Werewolf Shoot: Target is null");
            return;
        }

        PlayerControl.LocalPlayer.RpcCustomMurder(Target);
    }

    public override PlayerControl? GetTarget()
    {
        return PlayerControl.LocalPlayer.GetClosestLivingPlayer(true, Distance);
    }
}

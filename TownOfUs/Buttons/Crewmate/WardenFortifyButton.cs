using TownOfUs.Utilities;
using MiraAPI.Utilities.Assets;
using Reactor.Utilities;
using TownOfUs.Roles.Crewmate;
using UnityEngine;

namespace TownOfUs.Buttons.Crewmate;

public sealed class WardenFortifyButton : TownOfUsRoleButton<WardenRole, PlayerControl>
{
    public override string Name => "Fortify";
    public override string Keybind => Keybinds.SecondaryAction;
    public override Color TextOutlineColor => TownOfUsColors.Warden;
    public override float Cooldown => 0.001f + MapCooldown;
    public override LoadableAsset<Sprite> Sprite => TouCrewAssets.FortifySprite;

    public override bool CanUse()
    {
        return base.CanUse() && Role is { Fortified: null };
    }

    public override PlayerControl? GetTarget() => PlayerControl.LocalPlayer.GetClosestLivingPlayer(true, Distance);

    protected override void OnClick()
    {
        if (Target == null)
        {
            Logger<TownOfUsPlugin>.Error("Warden Fortify: Target is null");
            return;
        }

        WardenRole.RpcWardenFortify(PlayerControl.LocalPlayer, Target);
    }
}

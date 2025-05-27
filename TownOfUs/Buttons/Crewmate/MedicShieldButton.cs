using TownOfUs.Utilities;
using MiraAPI.Utilities.Assets;
using Reactor.Utilities;
using TownOfUs.Roles.Crewmate;
using UnityEngine;
using MiraAPI.GameOptions;
using TownOfUs.Options.Roles.Crewmate;

namespace TownOfUs.Buttons.Crewmate;

public sealed class MedicShieldButton : TownOfUsRoleButton<MedicRole, PlayerControl>
{
    public override string Name => "Shield";
    public override string Keybind => "ActionQuaternary";
    public override Color TextOutlineColor => TownOfUsColors.Medic;
    public override float Cooldown => MapCooldown;
    public override LoadableAsset<Sprite> Sprite => TouCrewAssets.MedicSprite;
    public bool CanChangeTarget = OptionGroupSingleton<MedicOptions>.Instance.ChangeTarget;

    public override bool CanUse()
    {
        return base.CanUse() && (Role is { Shielded: null } || CanChangeTarget);
    }

    public override PlayerControl? GetTarget() => PlayerControl.LocalPlayer.GetClosestLivingPlayer(true, Distance);

    protected override void OnClick()
    {
        if (Target == null)
        {
            Logger<TownOfUsPlugin>.Error("Medic Shield: Target is null");
            return;
        }

        MedicRole.RpcMedicShield(PlayerControl.LocalPlayer, Target);
        CanChangeTarget = false;
    }
}

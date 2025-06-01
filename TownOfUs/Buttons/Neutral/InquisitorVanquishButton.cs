using MiraAPI.GameOptions;
using MiraAPI.Networking;
using TownOfUs.Utilities;
using MiraAPI.Utilities.Assets;
using Reactor.Utilities;
using TownOfUs.Options.Roles.Neutral;
using TownOfUs.Roles.Neutral;
using UnityEngine;

namespace TownOfUs.Buttons.Neutral;

public sealed class InquisitorVanquishButton : TownOfUsRoleButton<InquisitorRole, PlayerControl>, IDiseaseableButton
{
    public override string Name => "Vanquish";
    public override string Keybind => "ActionSecondary";
    public override Color TextOutlineColor => TownOfUsColors.Inquisitor;
    public override float Cooldown => OptionGroupSingleton<InquisitorOptions>.Instance.VanquishCooldown;
    public override LoadableAsset<Sprite> Sprite => TouAssets.KillSprite;
    public bool Usable { get; set; } = true;
    public void SetDiseasedTimer(float multiplier)
    {
        SetTimer(Cooldown * multiplier);
    }
    public override bool CanUse()
    {
        return base.CanUse() && Usable;
    }

    public override PlayerControl? GetTarget() => PlayerControl.LocalPlayer.GetClosestLivingPlayer(true, Distance);

    protected override void OnClick()
    {
        if (Target == null)
        {
            Logger<TownOfUsPlugin>.Error("Inquisitor Vanquish: Target is null");
            return;
        }

        PlayerControl.LocalPlayer.RpcCustomMurder(Target);
    }
}

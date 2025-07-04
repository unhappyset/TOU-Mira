using MiraAPI.Hud;
using MiraAPI.Utilities.Assets;
using TownOfUs.Roles.Impostor;
using UnityEngine;

namespace TownOfUs.Buttons.Impostor;

public sealed class EscapistMarkButton : TownOfUsRoleButton<EscapistRole>, IAftermathableButton
{
    public override string Name => "Mark Location";
    public override string Keybind => Keybinds.SecondaryAction;
    public override Color TextOutlineColor => TownOfUsColors.Impostor;
    public override float Cooldown => 0;
    public override LoadableAsset<Sprite> Sprite => TouImpAssets.MarkSprite;

    public override bool Enabled(RoleBehaviour? role)
    {
        return base.Enabled(role) && Role is { MarkedLocation: null };
    }

    public override bool CanUse()
    {
        return base.CanUse() && Role is { MarkedLocation: null };
    }

    protected override void OnClick()
    {
        EscapistRole.RpcMarkLocation(PlayerControl.LocalPlayer, PlayerControl.LocalPlayer.transform.position);

        // TouAudio.PlaySound(TouAudio.EscapistMarkSound);
        CustomButtonSingleton<EscapistRecallButton>.Instance.SetActive(true, Role);
        SetActive(false, Role);
    }
}
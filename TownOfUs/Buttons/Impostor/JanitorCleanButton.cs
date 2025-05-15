using MiraAPI.Utilities;
using MiraAPI.Utilities.Assets;
using TownOfUs.Roles.Impostor;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfUs.Buttons.Impostor;

public sealed class JanitorCleanButton : TownOfUsRoleButton<JanitorRole, DeadBody>, IAftermathableBodyButton
{
    public override string Name => "Clean";
    public override string Keybind => "ActionQuaternary";
    public override Color TextOutlineColor => TownOfUsColors.Impostor;
    public override float Cooldown => PlayerControl.LocalPlayer.GetKillCooldown() + MapCooldown;
    public override LoadableAsset<Sprite> Sprite => TouImpAssets.CleanButtonSprite;

    public override DeadBody? GetTarget() => PlayerControl.LocalPlayer.GetNearestDeadBody(Distance);

    protected override void OnClick()
    {
        if (Target == null)
        {
            return;
        }

        JanitorRole.RpcCleanBody(PlayerControl.LocalPlayer, Target.ParentId);
        TouAudio.PlaySound(TouAudio.JanitorCleanSound);

        PlayerControl.LocalPlayer.SetKillTimer(PlayerControl.LocalPlayer.GetKillCooldown());
    }

    public override void FixedUpdateHandler(PlayerControl playerControl)
    {
        Timer = PlayerControl.LocalPlayer.killTimer;

        if (CanUse())
        {
            Button?.SetEnabled();
        }
        else
        {
            Button?.SetDisabled();
        }

        Button?.SetCoolDown(Timer, Cooldown);
        FixedUpdate(playerControl);
    }
}

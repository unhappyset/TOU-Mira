using MiraAPI.GameOptions;
using MiraAPI.Utilities;
using MiraAPI.Utilities.Assets;
using TownOfUs.Options.Roles.Impostor;
using TownOfUs.Roles.Impostor;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfUs.Buttons.Impostor;

public sealed class JanitorCleanButton : TownOfUsRoleButton<JanitorRole, DeadBody>, IAftermathableBodyButton, IDiseaseableButton
{
    public override string Name => "Clean";
    public override string Keybind => "ActionQuaternary";
    public override Color TextOutlineColor => TownOfUsColors.Impostor;
    public override float Cooldown => OptionGroupSingleton<JanitorOptions>.Instance.CleanCooldown + MapCooldown;
    public override LoadableAsset<Sprite> Sprite => TouImpAssets.CleanButtonSprite;

    public override DeadBody? GetTarget() => PlayerControl.LocalPlayer.GetNearestDeadBody(Distance);
    public void SetDiseasedTimer(float multiplier)
    {
        SetTimer(Cooldown * multiplier);
    }

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

    /* public override void FixedUpdateHandler(PlayerControl playerControl)
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
    } */
}

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
    public override string Keybind => Keybinds.SecondaryAction;
    public override Color TextOutlineColor => TownOfUsColors.Impostor;
    public override float Cooldown => OptionGroupSingleton<JanitorOptions>.Instance.CleanCooldown + MapCooldown;
    public override float EffectDuration => OptionGroupSingleton<JanitorOptions>.Instance.CleanDelay + 0.001f;
    public override int MaxUses => (int)OptionGroupSingleton<JanitorOptions>.Instance.MaxClean;
    public override LoadableAsset<Sprite> Sprite => TouImpAssets.CleanButtonSprite;

    public override DeadBody? GetTarget() => PlayerControl.LocalPlayer.GetNearestDeadBody(Distance);
    public DeadBody? CleaningBody { get; set; }
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
        CleaningBody = Target;
    }
    public override void OnEffectEnd()
    {
        if (CleaningBody == Target && CleaningBody != null)
        {
            JanitorRole.RpcCleanBody(PlayerControl.LocalPlayer, CleaningBody.ParentId);
            TouAudio.PlaySound(TouAudio.JanitorCleanSound);

            PlayerControl.LocalPlayer.SetKillTimer(PlayerControl.LocalPlayer.GetKillCooldown());
        }
        CleaningBody = null;
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

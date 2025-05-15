using MiraAPI.GameOptions;
using MiraAPI.Modifiers.Types;
using MiraAPI.Utilities;
using TownOfUs.Options.Roles.Impostor;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfUs.Modifiers.Impostor;

public sealed class EclipsalBlindModifier : TimedModifier
{
    public override string ModifierName => "Blinded";
    public override bool HideOnUi => true;
    public override float Duration => OptionGroupSingleton<EclipsalOptions>.Instance.BlindDuration;
    public override bool AutoStart => true;

    public float VisionPerc { get; set; } = 1f;

    public override void OnActivate()
    {
        base.OnActivate();

        VisionPerc = 1f;

        if (PlayerControl.LocalPlayer.IsImpostor())
        {
            Player!.cosmetics.currentBodySprite.BodySprite.material.SetColor(ShaderID.VisorColor, Palette.VisorColor);
        }
        if (Player.AmOwner)
        {
            var notif1 = Helpers.CreateAndShowNotification(
                $"<b>{TownOfUsColors.ImpSoft.ToTextColor()} You were blinded by an Eclipsal!</color></b>", Color.white, spr: TouRoleIcons.Eclipsal.LoadAsset());

            notif1.Text.SetOutlineThickness(0.35f);
            notif1.transform.localPosition = new Vector3(0f, 1f, -20f);
        }
    }

    public override void OnDeath(DeathReason reason)
    {
        base.OnDeath(reason);

        ModifierComponent!.RemoveModifier(this);
    }

    public override void FixedUpdate()
    {
        base.FixedUpdate();

        var opts = OptionGroupSingleton<EclipsalOptions>.Instance;

        if (PlayerControl.LocalPlayer.IsImpostor())
        {
            Player!.cosmetics.currentBodySprite.BodySprite.material.SetColor(ShaderID.VisorColor, Color.black);
        }

        if (TimeRemaining > opts.BlindDuration - 1f)
        {
            VisionPerc = TimeRemaining - opts.BlindDuration + 1f;
        }
        else if (TimeRemaining < 1f)
        {
            VisionPerc = 1f - TimeRemaining;
        }
        else
        {
            VisionPerc = 0f;
        }
    }

    public override void OnDeactivate()
    {
        base.OnDeactivate();

        VisionPerc = 1f;

        if (PlayerControl.LocalPlayer.IsImpostor())
        {
            Player!.cosmetics.currentBodySprite.BodySprite.material.SetColor(ShaderID.VisorColor, Palette.VisorColor);
        }
    }
}

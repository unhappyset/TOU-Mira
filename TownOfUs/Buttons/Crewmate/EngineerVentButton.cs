using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Utilities;
using MiraAPI.Utilities.Assets;
using TownOfUs.Modifiers;
using TownOfUs.Modifiers.Neutral;
using TownOfUs.Options.Roles.Crewmate;
using TownOfUs.Roles.Crewmate;
using UnityEngine;

namespace TownOfUs.Buttons.Crewmate;

public sealed class EngineerVentButton : TownOfUsRoleButton<EngineerTouRole, Vent>
{
    public override string Name => "Vent";
    public override string Keybind => Keybinds.VentAction;
    public override Color TextOutlineColor => TownOfUsColors.Engineer;
    public override float Cooldown => OptionGroupSingleton<EngineerOptions>.Instance.VentCooldown + 0.001f + MapCooldown;
    public override float EffectDuration => OptionGroupSingleton<EngineerOptions>.Instance.VentDuration;
    public override int MaxUses => (int)OptionGroupSingleton<EngineerOptions>.Instance.MaxVents;
    public override LoadableAsset<Sprite> Sprite => TouCrewAssets.EngiVentSprite;

    private static readonly ContactFilter2D Filter = Helpers.CreateFilter(Constants.NotShipMask);

    public override Vent? GetTarget() => PlayerControl.LocalPlayer.GetNearestObjectOfType<Vent>(Distance, Filter);

    public override bool CanUse()
    {
        var newTarget = GetTarget();
        if (newTarget != Target)
        {
            SetOutline(false);
        }

        Target = IsTargetValid(newTarget) ? newTarget : null;
        SetOutline(true);

        return ((Timer <= 0 && !PlayerControl.LocalPlayer.inVent && Target != null) || PlayerControl.LocalPlayer.inVent)
            && !PlayerControl.LocalPlayer.HasModifier<GlitchHackedModifier>()
            && !PlayerControl.LocalPlayer.HasModifier<DisabledModifier>();
    }
    public override void ClickHandler()
    {
        if (!CanUse())
        {
            return;
        }

        OnClick();
        Button?.SetDisabled();
        if (EffectActive)
        {
            Timer = Cooldown;
            EffectActive = false;
        }
        else if (HasEffect)
        {
            EffectActive = true;
            Timer = EffectDuration;
        }
        else
        {
            Timer = Cooldown;
        }
    }
    protected override void OnClick()
    {
        if (!PlayerControl.LocalPlayer.inVent)
        {
            Target!.Use();
            if (MaxUses != 0) --UsesLeft;
        }
        else
        {
            OnEffectEnd();
            EffectActive = false;
            Timer = Cooldown;
        }
    }
    public override void OnEffectEnd()
    {
        if (PlayerControl.LocalPlayer.inVent)
        {
            PlayerControl.LocalPlayer.MyPhysics.RpcExitVent(Vent.currentVent.Id);
            PlayerControl.LocalPlayer.MyPhysics.ExitAllVents();
        }
    }
}

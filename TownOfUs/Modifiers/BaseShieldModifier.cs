using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Modifiers.Types;
using MiraAPI.PluginLoading;
using TownOfUs.Modifiers.Impostor;
using TownOfUs.Modifiers.Impostor.Venerer;
using TownOfUs.Modifiers.Neutral;
using TownOfUs.Options;

namespace TownOfUs.Modifiers;

[MiraIgnore]
public abstract class BaseShieldModifier : TimedModifier
{
    public override string ModifierName => "Shield Modifier";
    public virtual string ShieldDescription => "You are protected!";
    public override float Duration => 1f;
    public override bool AutoStart => false;
    public override bool HideOnUi => !TownOfUsPlugin.ShowShieldHud.Value;
    public override string GetDescription()
    {
        return !HideOnUi ? ShieldDescription : string.Empty;
    }
    public bool Visible = true;
    public virtual bool VisibleSymbol => false;
    public void SetVisible(bool visible)
    {
        Visible = visible;
    }

    public bool IsConcealed()
    {
        if (Player.HasModifier<MorphlingMorphModifier>() || Player.HasModifier<GlitchMimicModifier>() || Player.HasModifier<VenererCamouflageModifier>()
            || Player.HasModifier<SwoopModifier>() || !Player.Visible)
        {
            return true;
        }
        if (!Visible || Player.inVent)
        {
            return true;
        }

        var mushroom = UnityEngine.Object.FindObjectOfType<MushroomMixupSabotageSystem>();
        if (mushroom && mushroom.IsActive)
        {
            return true;
        }

        if (OptionGroupSingleton<GeneralOptions>.Instance.CamouflageComms)
        {
            if (!ShipStatus.Instance.Systems.TryGetValue(SystemTypes.Comms, out var commsSystem) || commsSystem == null)
            {
                return false;
            }

            var isActive = false;
            if (ShipStatus.Instance.Type == ShipStatus.MapType.Hq || ShipStatus.Instance.Type == ShipStatus.MapType.Fungle)
            {
                var hqSystem = commsSystem.Cast<HqHudSystemType>();
                if (hqSystem != null) isActive = hqSystem.IsActive;
            }
            else
            {
                var hudSystem = commsSystem.Cast<HudOverrideSystemType>();
                if (hudSystem != null) isActive = hudSystem.IsActive;
            }

            return isActive;
        }

        return false;
    }
}

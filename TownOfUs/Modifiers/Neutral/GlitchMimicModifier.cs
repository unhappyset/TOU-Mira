using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Modifiers.Types;
using TownOfUs.Buttons.Neutral;
using TownOfUs.Options.Roles.Neutral;
using TownOfUs.Utilities.Appearances;

namespace TownOfUs.Modifiers.Neutral;

public sealed class GlitchMimicModifier(PlayerControl target) : TimedModifier, IVisualAppearance
{
    public override float Duration => OptionGroupSingleton<GlitchOptions>.Instance.MimicDuration;
    public override string ModifierName => "Mimic";
    public override bool HideOnUi => true;
    public bool VisualPriority => true;

    public VisualAppearance? GetVisualAppearance()
    {
        return Player == null ? null : new VisualAppearance(target.GetDefaultModifiedAppearance(), TownOfUsAppearances.Mimic);
    }

    public override void OnActivate()
    {
        Player?.RawSetAppearance(this);
    }

    public override void OnDeactivate()
    {
        CustomButtonSingleton<GlitchMimicButton>.Instance.SetTimer(OptionGroupSingleton<GlitchOptions>.Instance.MimicCooldown);
        Player?.ResetAppearance();
    }
}

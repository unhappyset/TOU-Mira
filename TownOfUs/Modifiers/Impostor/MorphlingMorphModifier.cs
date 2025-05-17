using MiraAPI.GameOptions;
using MiraAPI.Modifiers.Types;
using TownOfUs.Options.Roles.Impostor;
using TownOfUs.Utilities.Appearances;

namespace TownOfUs.Modifiers.Impostor;

public sealed class MorphlingMorphModifier(PlayerControl target) : TimedModifier, IVisualAppearance
{
    public override float Duration => OptionGroupSingleton<MorphlingOptions>.Instance.MorphlingDuration;
    public override string ModifierName => "Morph";
    public override bool HideOnUi => true;
    public override bool AutoStart => true;
    public bool VisualPriority => true;

    public VisualAppearance GetVisualAppearance()
    {
        return new VisualAppearance(target.GetDefaultModifiedAppearance(), TownOfUsAppearances.Morph);
    }

    public override void OnActivate()
    {
        Player.RawSetAppearance(this);
    }

    public override void OnDeactivate()
    {
        Player.ResetAppearance();
    }
}

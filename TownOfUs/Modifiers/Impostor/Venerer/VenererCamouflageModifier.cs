using MiraAPI.GameOptions;
using MiraAPI.Modifiers.Types;
using TownOfUs.Options.Roles.Impostor;
using TownOfUs.Utilities.Appearances;

namespace TownOfUs.Modifiers.Impostor.Venerer;

public sealed class VenererCamouflageModifier : TimedModifier, IVenererModifier
{
    public override string ModifierName => "Camouflaged";
    public override float Duration => OptionGroupSingleton<VenererOptions>.Instance.AbilityDuration;
    public override bool AutoStart => true;

    public override void OnActivate()
    {
        Player.SetCamouflage();
    }

    public override void OnDeactivate()
    {
        Player.SetCamouflage(false);
    }
}

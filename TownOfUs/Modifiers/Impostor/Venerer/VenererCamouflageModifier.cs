using MiraAPI.GameOptions;
using TownOfUs.Options.Roles.Impostor;
using TownOfUs.Utilities.Appearances;

namespace TownOfUs.Modifiers.Impostor.Venerer;

public sealed class VenererCamouflageModifier : ConcealedModifier, IVenererModifier
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

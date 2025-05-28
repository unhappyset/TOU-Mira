using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using TownOfUs.Modifiers.Game.Universal;
using TownOfUs.Options.Modifiers.Universal;
using TownOfUs.Options.Roles.Impostor;
using TownOfUs.Utilities.Appearances;
using UnityEngine;

namespace TownOfUs.Modifiers.Impostor.Venerer;

public sealed class VenererCamouflageModifier : ConcealedModifier, IVenererModifier, IVisualAppearance
{
    public override string ModifierName => "Camouflaged";
    public override float Duration => OptionGroupSingleton<VenererOptions>.Instance.AbilityDuration;
    public override bool AutoStart => true;

    public VisualAppearance GetVisualAppearance()
    {
        var appearance = Player.GetDefaultAppearance();
        appearance.Speed = 1f;
        if (Player.HasModifier<MiniModifier>()) appearance.Speed = 1f / OptionGroupSingleton<MiniOptions>.Instance.MiniSpeed;
        else if (Player.HasModifier<GiantModifier>()) appearance.Speed = 1f / OptionGroupSingleton<GiantOptions>.Instance.GiantSpeed;
        else if (Player.HasModifier<FlashModifier>()) appearance.Speed = 1f / OptionGroupSingleton<FlashOptions>.Instance.FlashSpeed;
        appearance.Size = new Vector3(0.7f, 0.7f, 1f);
        appearance.ColorId = Player.Data.DefaultOutfit.ColorId;
        appearance.HatId = string.Empty;
        appearance.SkinId = string.Empty;
        appearance.VisorId = string.Empty;
        appearance.PlayerName = string.Empty;
        appearance.PetId = string.Empty;
        appearance.NameVisible = false;
        appearance.PlayerMaterialColor = Color.grey;
        return appearance;
    }
    
    public override void OnActivate()
    {
        Player.RawSetAppearance(this);
    }

    public override void OnDeactivate()
    {
        Player?.ResetAppearance();
    }
}

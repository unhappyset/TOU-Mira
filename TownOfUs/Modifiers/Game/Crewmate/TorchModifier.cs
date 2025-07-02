using MiraAPI.GameOptions;
using MiraAPI.Utilities.Assets;
using TownOfUs.Modules.Wiki;
using TownOfUs.Options.Modifiers;
using TownOfUs.Utilities;
using UnityEngine;
using static ShipStatus;

namespace TownOfUs.Modifiers.Game.Crewmate;

public sealed class TorchModifier : TouGameModifier, IWikiDiscoverable
{
    public override string ModifierName => "Torch";
    public override string IntroInfo => $"You can also see without lights on.";
    public override LoadableAsset<Sprite>? ModifierIcon => TouModifierIcons.Torch;
    public override string GetDescription() => "Your vision wont get reduced\nwhen the lights are sabotaged.";
    public override ModifierFaction FactionType => ModifierFaction.CrewmateVisibility;

    public override int GetAssignmentChance() => (int)OptionGroupSingleton<CrewmateModifierOptions>.Instance.TorchChance;
    public override int GetAmountPerGame() => (int)OptionGroupSingleton<CrewmateModifierOptions>.Instance.TorchAmount;

    public override bool IsModifierValidOn(RoleBehaviour role)
    {
        return base.IsModifierValidOn(role) && role.IsCrewmate() && Instance.Type != MapType.Fungle;
    }
    public string GetAdvancedDescription()
    {
        return
            "The lights being off do not affect your vision.";
    }

    public List<CustomButtonWikiDescription> Abilities { get; } = [];
}

using MiraAPI.GameOptions;
using MiraAPI.Utilities.Assets;
using TownOfUs.Modules.Wiki;
using TownOfUs.Options.Modifiers;
using UnityEngine;

namespace TownOfUs.Modifiers.Game.Universal;

public sealed class SixthSenseModifier : UniversalGameModifier, IWikiDiscoverable
{
    public override string ModifierName => "Sixth Sense";
    public override LoadableAsset<Sprite>? ModifierIcon => TouModifierIcons.SixthSense;
    public override string GetDescription() => "Know when someone interacts with you.";

    public override int GetAssignmentChance() => (int)OptionGroupSingleton<UniversalModifierOptions>.Instance.SixthSenseChance;
    public override int GetAmountPerGame() => (int)OptionGroupSingleton<UniversalModifierOptions>.Instance.SixthSenseAmount;
    public string GetAdvancedDescription()
    {
        return
            "You will know when someone uses their ability on you.";
    }

    public List<CustomButtonWikiDescription> Abilities { get; } = [];
}

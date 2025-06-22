using MiraAPI.GameOptions;
using MiraAPI.Utilities.Assets;
using TownOfUs.Modules.Wiki;
using TownOfUs.Options.Modifiers;
using UnityEngine;

namespace TownOfUs.Modifiers.Game.Universal;

public sealed class TiebreakerModifier : UniversalGameModifier, IWikiDiscoverable
{
    public override string ModifierName => "Tiebreaker";
    public override LoadableAsset<Sprite>? ModifierIcon => TouModifierIcons.Tiebreaker;
    public override string GetDescription() => "Your vote breaks ties";
    public override ModifierFaction FactionType => ModifierFaction.UniversalPassive;

    public override int GetAmountPerGame() => (int)OptionGroupSingleton<UniversalModifierOptions>.Instance.TiebreakerAmount != 0 ? 1 : 0;
    public override int GetAssignmentChance() => (int)OptionGroupSingleton<UniversalModifierOptions>.Instance.TiebreakerChance;
    public string GetAdvancedDescription()
    {
        return
            "Your vote allows you to break ties.";
    }

    public List<CustomButtonWikiDescription> Abilities { get; } = [];
}

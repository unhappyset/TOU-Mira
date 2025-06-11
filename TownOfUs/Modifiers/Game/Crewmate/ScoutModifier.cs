using MiraAPI.GameOptions;
using MiraAPI.Utilities.Assets;
using TownOfUs.Modules.Wiki;
using TownOfUs.Options.Modifiers;
using TownOfUs.Utilities;
using UnityEngine;
using static ShipStatus;

namespace TownOfUs.Modifiers.Game.Crewmate;

public sealed class ScoutModifier : TouGameModifier, IWikiDiscoverable
{
    public override string ModifierName => "Scout";
    public override LoadableAsset<Sprite>? ModifierIcon => TouModifierIcons.Scout;
    public override string GetDescription() => "Your vision is higher when lights are on, but very low when lights are off.";
    public override ModifierFaction FactionType => ModifierFaction.CrewmatePassive;

    public override int GetAssignmentChance() => (int)OptionGroupSingleton<CrewmateModifierOptions>.Instance.ScoutChance;
    public override int GetAmountPerGame() => (int)OptionGroupSingleton<CrewmateModifierOptions>.Instance.ScoutAmount;

    public override bool IsModifierValidOn(RoleBehaviour role)
    {
        return base.IsModifierValidOn(role) && role.IsCrewmate() && Instance.Type != MapType.Fungle;
    }
    public string GetAdvancedDescription()
    {
        return
            "While you can see twice as far as a regular crewmate, your vision falters when lights are off.";
    }

    public List<CustomButtonWikiDescription> Abilities { get; } = [];
}

using MiraAPI.GameOptions;
using MiraAPI.Utilities.Assets;
using TownOfUs.Modules.Wiki;
using TownOfUs.Options.Modifiers;
using TownOfUs.Roles.Crewmate;
using TownOfUs.Utilities;
using UnityEngine;
using static ShipStatus;

namespace TownOfUs.Modifiers.Game.Crewmate;

public sealed class SpyModifier : TouGameModifier, IWikiDiscoverable
{
    public override string ModifierName => "Spy";
    public override LoadableAsset<Sprite>? ModifierIcon => TouRoleIcons.Spy;
    public override string GetDescription() => "Gain extra information on the Admin Table.";
    public override ModifierFaction FactionType => ModifierFaction.Crewmate;

    public override int GetAssignmentChance() => (int)OptionGroupSingleton<CrewmateModifierOptions>.Instance.SpyChance;
    public override int GetAmountPerGame() => (int)OptionGroupSingleton<CrewmateModifierOptions>.Instance.SpyAmount;

    public override bool IsModifierValidOn(RoleBehaviour role)
    {
        return base.IsModifierValidOn(role) && role is not SpyRole && role.IsCrewmate() && Instance.Type != MapType.Fungle;
    }
    public string GetAdvancedDescription()
    {
        return
            "The Spy gains extra information on the admin table. They now not only see how many people are in a room, but will also see who is in every room.";
    }

    public List<CustomButtonWikiDescription> Abilities { get; } = [];
}

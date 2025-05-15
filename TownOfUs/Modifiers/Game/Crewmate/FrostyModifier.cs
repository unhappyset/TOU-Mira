using MiraAPI.GameOptions;
using MiraAPI.Utilities.Assets;
using TownOfUs.Modules.Wiki;
using TownOfUs.Options.Modifiers;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfUs.Modifiers.Game.Crewmate;

public sealed class FrostyModifier : TouGameModifier, IWikiDiscoverable
{
    public override string ModifierName => "Frosty";
    public override LoadableAsset<Sprite>? ModifierIcon => TouModifierIcons.Frosty;
    public override string GetDescription() => "Slow your killer for a short duration.";
    public override ModifierFaction FactionType => ModifierFaction.Crewmate;

    public override int GetAssignmentChance() => (int)OptionGroupSingleton<CrewmateModifierOptions>.Instance.FrostyChance;
    public override int GetAmountPerGame() => (int)OptionGroupSingleton<CrewmateModifierOptions>.Instance.FrostyAmount;

    public override bool IsModifierValidOn(RoleBehaviour role)
    {
        return base.IsModifierValidOn(role) && role.IsCrewmate();
    }
    public string GetAdvancedDescription()
    {
        return
            "After you die, your killer will be slowed down!"
               + MiscUtils.AppendOptionsText(GetType());
    }

    public List<CustomButtonWikiDescription> Abilities { get; } = [];
}

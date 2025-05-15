using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Utilities;
using MiraAPI.Utilities.Assets;
using TownOfUs.Modifiers.Crewmate;
using TownOfUs.Modules.Wiki;
using TownOfUs.Options.Modifiers;
using TownOfUs.Roles.Crewmate;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfUs.Modifiers.Game.Crewmate;

public sealed class InvestigatorModifier : TouGameModifier, IWikiDiscoverable
{
    public override string ModifierName => "Investigator";
    public override LoadableAsset<Sprite>? ModifierIcon => TouRoleIcons.Investigator;
    public override string GetDescription() => "You can see everyone's footprints.";
    public override ModifierFaction FactionType => ModifierFaction.Crewmate;

    public override int GetAssignmentChance() => (int)OptionGroupSingleton<CrewmateModifierOptions>.Instance.InvestigatorChance;
    public override int GetAmountPerGame() => (int)OptionGroupSingleton<CrewmateModifierOptions>.Instance.InvestigatorAmount;

    public override bool IsModifierValidOn(RoleBehaviour role)
    {
        return base.IsModifierValidOn(role) && role.IsCrewmate() && role is not InvestigatorRole;
    }

    public override void OnActivate()
    {
        if (!Player.AmOwner) return;

        Helpers.GetAlivePlayers().Where(plr => !plr.HasModifier<FootstepsModifier>())
            .ToList().ForEach(plr => plr.GetModifierComponent().AddModifier<FootstepsModifier>());
    }

    public override void OnDeactivate()
    {
        if (!Player.AmOwner) return;

        PlayerControl.AllPlayerControls.ToArray().Where(plr => plr.HasModifier<FootstepsModifier>())
            .ToList().ForEach(plr => plr.GetModifierComponent().RemoveModifier<FootstepsModifier>());
    }
    public string GetAdvancedDescription()
    {
        return
            "The Investigator can see player's footprints throughout the game. Swooped players' footprints will not be visible to the Investigator.";
    }

    public List<CustomButtonWikiDescription> Abilities { get; } = [];
}

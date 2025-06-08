using MiraAPI.GameOptions;
using MiraAPI.Utilities;
using MiraAPI.Utilities.Assets;
using TownOfUs.Modules.Wiki;
using TownOfUs.Options.Modifiers;
using TownOfUs.Roles;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfUs.Modifiers.Game.Alliance;

public sealed class EgotistModifier : AllianceGameModifier, IWikiDiscoverable
{
    public override string ModifierName => "Egotist";
    public override string IntroInfo => $"Your Ego is Thriving...";
    public override string Symbol => $"<size=100%>#</size>";
    public override float IntroSize => 4.4f;
    public override LoadableAsset<Sprite>? ModifierIcon => TouModifierIcons.Egotist;
    public override string GetDescription() => "Defy the crew,\nwin with killers.";
    public override int GetAssignmentChance() => (int)OptionGroupSingleton<AllianceModifierOptions>.Instance.EgotistChance;
    public override int GetAmountPerGame() => 1;
    public int Priority { get; set; } = 5;
    public List<CustomButtonWikiDescription> Abilities { get; } = [];

    public string GetAdvancedDescription()
    {
        return $"The Egotist is an Alliance modifier that only applies to crewmates (signified with <color=#669966> #</color>). As the Egotist, you can only win if Impostors or Neutral Killers win. It does not matter if you are alive or not, but if no crewmates remain after a meeting ends, you will leave in victory. You also do not get punished for harming the crewmates";
    }

    public override bool IsModifierValidOn(RoleBehaviour role)
    {
        return base.IsModifierValidOn(role) && role.IsCrewmate();
    }
    public override bool? DidWin(GameOverReason reason)
    {
        return Helpers.GetAlivePlayers().Any(x => (x.IsImpostor() || x.Is(RoleAlignment.NeutralKilling)) && x.Data.Role.DidWin(reason));
    }
}

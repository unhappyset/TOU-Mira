using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Utilities.Assets;
using TownOfUs.Modifiers.Game.Universal;
using TownOfUs.Modules.Wiki;
using TownOfUs.Options.Modifiers;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfUs.Modifiers.Game.Crewmate;

public sealed class OperativeModifier : TouGameModifier, IWikiDiscoverable
{
    public override string ModifierName => "Operative";
    public override LoadableAsset<Sprite>? ModifierIcon => TouModifierIcons.Operative;
    public override string GetDescription() => $"Utilize the Cameras from anywhere";
    public override ModifierFaction FactionType => ModifierFaction.Crewmate;

    public override int GetAssignmentChance() => (int)OptionGroupSingleton<CrewmateModifierOptions>.Instance.OperativeChance;
    public override int GetAmountPerGame() => (int)OptionGroupSingleton<CrewmateModifierOptions>.Instance.OperativeAmount;
	
    public override bool IsModifierValidOn(RoleBehaviour role)
	{
		return base.IsModifierValidOn(role) && role.IsCrewmate() && !role.Player.GetModifierComponent().HasModifier<SatelliteModifier>(true) && !role.Player.GetModifierComponent().HasModifier<ButtonBarryModifier>(true);
	}
    public string GetAdvancedDescription()
    {
        return
            $"Use cameras at anytime with a limited battery charge."
               + MiscUtils.AppendOptionsText(GetType());
    }

    public List<CustomButtonWikiDescription> Abilities { get; } = [];
}

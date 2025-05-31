using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Modifiers;
using MiraAPI.Utilities.Assets;
using TownOfUs.Buttons.Modifiers;
using TownOfUs.Modifiers.Game.Universal;
using TownOfUs.Modules.Wiki;
using TownOfUs.Options.Modifiers;
using TownOfUs.Options.Modifiers.Crewmate;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfUs.Modifiers.Game.Crewmate;

public sealed class ScientistModifier : TouGameModifier, IWikiDiscoverable
{
    public override string ModifierName => "Scientist";
    public override LoadableAsset<Sprite>? ModifierIcon => TouModifierIcons.Scientist;
    public override string GetDescription() => $"Access vitals anytime, anywhere, as long as you have charge";
    public override ModifierFaction FactionType => ModifierFaction.Crewmate;
    public override void OnActivate()
    {
        base.OnActivate();

        if (!Player.AmOwner) return;
        CustomButtonSingleton<ScientistButton>.Instance.AvailableCharge = OptionGroupSingleton<ScientistOptions>.Instance.StartingCharge;
    }
    public static void OnRoundStart(PlayerControl playerControl)
    {
        if (playerControl.HasModifier<ScientistModifier>()) CustomButtonSingleton<ScientistButton>.Instance.AvailableCharge += OptionGroupSingleton<ScientistOptions>.Instance.RoundCharge;
    }
    public static void OnTaskComplete(PlayerControl playerControl)
    {
        if (playerControl.HasModifier<ScientistModifier>()) CustomButtonSingleton<ScientistButton>.Instance.AvailableCharge += OptionGroupSingleton<ScientistOptions>.Instance.TaskCharge;
    }

    public override int GetAssignmentChance() => (int)OptionGroupSingleton<CrewmateModifierOptions>.Instance.ScientistChance;
    public override int GetAmountPerGame() => (int)OptionGroupSingleton<CrewmateModifierOptions>.Instance.ScientistAmount;

    public override bool IsModifierValidOn(RoleBehaviour role)
	{
		return base.IsModifierValidOn(role) && role.IsCrewmate() && role is not ScientistRole && !role.Player.GetModifierComponent().HasModifier<SatelliteModifier>(true) && !role.Player.GetModifierComponent().HasModifier<ButtonBarryModifier>(true);
	}
    public string GetAdvancedDescription()
    {
        return
            $"Access Vitals at anytime with a limited battery charge."
               + MiscUtils.AppendOptionsText(GetType());
    }

    public List<CustomButtonWikiDescription> Abilities { get; } = [];
}

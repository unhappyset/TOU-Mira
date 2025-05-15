using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Utilities.Assets;
using TownOfUs.Modules.Wiki;
using TownOfUs.Options.Modifiers;
using UnityEngine;

namespace TownOfUs.Modifiers.Game.Universal;

public sealed class SleuthModifier : UniversalGameModifier, IWikiDiscoverable
{
    public override string ModifierName => "Sleuth";
    public override LoadableAsset<Sprite>? ModifierIcon => TouModifierIcons.Sleuth;
    public override string GetDescription() => "Know the roles of bodies you report.";
    public List<byte> Reported { get; set; } = [];

    public override int GetAssignmentChance() => (int)OptionGroupSingleton<UniversalModifierOptions>.Instance.SleuthChance;
    public override int GetAmountPerGame() => (int)OptionGroupSingleton<UniversalModifierOptions>.Instance.SleuthAmount;

    public static bool SleuthVisibilityFlag(PlayerControl player)
    {
        if (PlayerControl.LocalPlayer.HasModifier<SleuthModifier>())
        {
            var mod = PlayerControl.LocalPlayer.GetModifier<SleuthModifier>()!;
            return mod.Reported.Contains(player.PlayerId);
        }

        return false;
    }
    public string GetAdvancedDescription()
    {
        return
            "You will see the roles of bodies you report.";
    }

    public List<CustomButtonWikiDescription> Abilities { get; } = [];
}

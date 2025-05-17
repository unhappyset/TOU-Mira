using MiraAPI.GameOptions;
using MiraAPI.Utilities.Assets;
using TownOfUs.Modules.Wiki;
using TownOfUs.Options.Modifiers;
using TownOfUs.Options.Modifiers.Universal;
using TownOfUs.Utilities.Appearances;
using UnityEngine;

namespace TownOfUs.Modifiers.Game.Universal;

public sealed class GiantModifier : UniversalGameModifier, IWikiDiscoverable, IVisualAppearance
{
    public override string ModifierName => "Giant";
    public override LoadableAsset<Sprite>? ModifierIcon => TouModifierIcons.Giant;
    public override string GetDescription() => "You are bigger than\nthe average player.";

    public override int GetAssignmentChance() =>
        (int)OptionGroupSingleton<UniversalModifierOptions>.Instance.GiantChance;
    public override int GetAmountPerGame() => (int)OptionGroupSingleton<UniversalModifierOptions>.Instance.GiantAmount;

    public VisualAppearance GetVisualAppearance()
    {
        var appearance = Player.GetDefaultAppearance();
        appearance.Speed = OptionGroupSingleton<GiantOptions>.Instance.GiantSpeed;
        appearance.Size = new Vector3(1f, 1f, 1f);
        return appearance;
    }

    public override void OnActivate()
    {
        Player.RawSetAppearance(this);
    }

    public override void OnDeactivate()
    {
        Player?.ResetAppearance(fullReset: true);
    }
    public string GetAdvancedDescription()
    {
        return
            $"You are bigger than regular players, and you also move {Math.Round(OptionGroupSingleton<GiantOptions>.Instance.GiantSpeed, 2)}x slower than regular players.";
    }

    public List<CustomButtonWikiDescription> Abilities { get; } = [];
}

using MiraAPI.GameOptions;
using MiraAPI.Utilities.Assets;
using TownOfUs.Modules.Wiki;
using TownOfUs.Options.Modifiers;
using TownOfUs.Utilities.Appearances;
using UnityEngine;

namespace TownOfUs.Modifiers.Game.Universal;

public sealed class MiniModifier : UniversalGameModifier, IWikiDiscoverable, IVisualAppearance
{
    public override string ModifierName => "Mini";
    public override LoadableAsset<Sprite>? ModifierIcon => TouModifierIcons.Mini;
    public override string GetDescription() => "You are smaller than\nthe average player.";

    public override int GetAssignmentChance() =>
        (int)OptionGroupSingleton<UniversalModifierOptions>.Instance.MiniChance;
    public override int GetAmountPerGame() => (int)OptionGroupSingleton<UniversalModifierOptions>.Instance.MiniAmount;

    public VisualAppearance GetVisualAppearance()
    {
        var appearance = Player.GetDefaultAppearance();
        appearance.Speed = 4f / 3f;
        appearance.Size = new Vector3(0.49f, 0.49f, 1f);
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
            $"You are smaller than regular players, and you also move faster than regular players.";
    }

    public List<CustomButtonWikiDescription> Abilities { get; } = [];
}

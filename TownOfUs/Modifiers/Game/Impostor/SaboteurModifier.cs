using MiraAPI.GameOptions;
using MiraAPI.Utilities.Assets;
using TownOfUs.Modules.Wiki;
using TownOfUs.Options.Modifiers;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfUs.Modifiers.Game.Impostor;

public sealed class SaboteurModifier : TouGameModifier, IWikiDiscoverable
{
    public override string ModifierName => "Saboteur";
    public override string GetDescription() => "You have reduced sabotage cooldowns";
    public override LoadableAsset<Sprite>? ModifierIcon => TouModifierIcons.Saboteur;
    public override ModifierFaction FactionType => ModifierFaction.Impostor;

    public float Timer { get; set; }

    public override int GetAssignmentChance() => (int)OptionGroupSingleton<ImpostorModifierOptions>.Instance.SaboteurChance;
    public override int GetAmountPerGame() => (int)OptionGroupSingleton<ImpostorModifierOptions>.Instance.SaboteurAmount;

    public override bool IsModifierValidOn(RoleBehaviour role)
    {
        return base.IsModifierValidOn(role) && role.IsImpostor();
    }
    public string GetAdvancedDescription()
    {
        return
            "You have a reduced cooldown when sabotaging."
               + MiscUtils.AppendOptionsText(GetType());
    }

    public List<CustomButtonWikiDescription> Abilities { get; } = [];
}

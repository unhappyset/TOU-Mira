using MiraAPI.PluginLoading;
using MiraAPI.Utilities.Assets;
using TownOfUs.Modules.Wiki;
using UnityEngine;

namespace TownOfUs.Modifiers.Game;

[MiraIgnore]
public abstract class DoubleShotModifier : TouGameModifier, IWikiDiscoverable
{
    public override string ModifierName => "Double Shot";
    public override string IntroInfo => "You also get a second chance when guessing.";

    public override LoadableAsset<Sprite>? ModifierIcon => TouModifierIcons.DoubleShot;
    public override ModifierFaction FactionType => ModifierFaction.AssailantUtility;

    public bool Used { get; set; }

    public string GetAdvancedDescription()
    {
        return
            "You get a second chance when you fail to shoot.";
    }

    public List<CustomButtonWikiDescription> Abilities { get; } = [];

    public override string GetDescription()
    {
        return "You have an extra chance when assassinating";
    }

    public override int GetAssignmentChance()
    {
        return 100;
    }

    public override int GetAmountPerGame()
    {
        return 0;
    }

    public override bool IsModifierValidOn(RoleBehaviour role)
    {
        return false;
    }
}
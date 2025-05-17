using MiraAPI.GameOptions;
using MiraAPI.Utilities.Assets;
using TownOfUs.Modules.Wiki;
using TownOfUs.Options.Modifiers;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfUs.Modifiers.Game.Crewmate;

public sealed class MultitaskerModifier : TouGameModifier, IWikiDiscoverable
{
    public override string ModifierName => "Multitasker";
    public override LoadableAsset<Sprite>? ModifierIcon => TouModifierIcons.Multitasker;
    public override string GetDescription() => "Your tasks are transparent.";
    public override ModifierFaction FactionType => ModifierFaction.Crewmate;

    public override int GetAssignmentChance() => (int)OptionGroupSingleton<CrewmateModifierOptions>.Instance.MultitaskerChance;
    public override int GetAmountPerGame() => (int)OptionGroupSingleton<CrewmateModifierOptions>.Instance.MultitaskerAmount;

    public override void Update()
    {
        if (!Player || Player.Data.IsDead || !Player.AmOwner)
        {
            return;
        }

        if (Minigame.Instance == null || IsExemptTask())
        {
            return;
        }

        SpriteRenderer[] rends = Minigame.Instance.GetComponentsInChildren<SpriteRenderer>();

        foreach (var t in rends)
        {
            var oldColor1 = t.color[0];
            var oldColor2 = t.color[1];
            var oldColor3 = t.color[2];
            t.color = new Color(oldColor1, oldColor2, oldColor3, 0.5f);
        }
    }

    public override bool IsModifierValidOn(RoleBehaviour role)
    {
        return base.IsModifierValidOn(role) && role.IsCrewmate();
    }

    public static bool IsExemptTask()
    {
        return Minigame.Instance.TryCast<VitalsMinigame>() ||
               Minigame.Instance.TryCast<CollectShellsMinigame>() ||
               Minigame.Instance.TryCast<MushroomDoorSabotageMinigame>();
    }
    public string GetAdvancedDescription()
    {
        return
            "All your menus are seethrough, allowing you to look behind menus!";
    }

    public List<CustomButtonWikiDescription> Abilities { get; } = [];
}

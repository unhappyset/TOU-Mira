﻿using Il2CppInterop.Runtime.Attributes;
using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Utilities.Assets;
using Reactor.Utilities.Extensions;
using TownOfUs.Buttons.Modifiers;
using TownOfUs.Modules.Anims;
using TownOfUs.Options.Modifiers;
using TownOfUs.Options.Modifiers.Universal;
using TownOfUs.Roles.Crewmate;
using TownOfUs.Utilities;
using UnityEngine;
using Object = UnityEngine.Object;

namespace TownOfUs.Modifiers.Game.Universal;

public sealed class SatelliteModifier : UniversalGameModifier, IWikiDiscoverable
{
    private readonly List<SpriteRenderer> CastedIcons = [];
    private readonly List<PlayerControl> CastedPlayers = [];
    public override string LocaleKey => "Satellite";
    public override string ModifierName => TouLocale.Get($"TouModifier{LocaleKey}");
    public override LoadableAsset<Sprite>? ModifierIcon => TouModifierIcons.Satellite;

    public override ModifierFaction FactionType => ModifierFaction.UniversalUtility;
    public override Color FreeplayFileColor => new Color32(180, 180, 180, 255);
    public int Priority { get; set; } = 5;

    public override string GetDescription()
    {
        return TouLocale.GetParsed($"TouModifier{LocaleKey}TabDescription");
    }

    public string GetAdvancedDescription()
    {
        return TouLocale.GetParsed($"TouModifier{LocaleKey}WikiDescription").Replace("<maxUses>",
                   $"{Math.Round(OptionGroupSingleton<SatelliteOptions>.Instance.MaxNumCast, 0)}") +
               MiscUtils.AppendOptionsText(GetType());
    }

    [HideFromIl2Cpp]
    public List<CustomButtonWikiDescription> Abilities
    {
        get
        {
            return new List<CustomButtonWikiDescription>
            {
                new(TouLocale.Get($"TouModifier{LocaleKey}Broadcast"),
                    TouLocale.GetParsed($"TouModifier{LocaleKey}BroadcastWikiDescription").Replace("<maxUses>",
                        $"{Math.Round(OptionGroupSingleton<SatelliteOptions>.Instance.MaxNumCast, 0)}"),
                    TouAssets.BroadcastSprite)
            };
        }
    }

    public override int GetAssignmentChance()
    {
        return (int)OptionGroupSingleton<UniversalModifierOptions>.Instance.SatelliteChance;
    }

    public override int GetAmountPerGame()
    {
        return (int)OptionGroupSingleton<UniversalModifierOptions>.Instance.SatelliteAmount;
    }

    public override bool IsModifierValidOn(RoleBehaviour role)
    {
        return base.IsModifierValidOn(role) && role is not MysticRole;
    }

    public void OnRoundStart()
    {
        CustomButtonSingleton<SatelliteButton>.Instance.Usable = true;
        ClearMapIcons();
    }

    public void NewMapIcon(PlayerControl player)
    {
        if (!CastedPlayers.Contains(player))
        {
            var newIcon = Object.Instantiate(MapBehaviour.Instance.TrackedHerePoint);
            newIcon.material = AnimStore.SetSpriteColourMatch(player, newIcon.material);

            var vector = player.transform.position;
            vector /= ShipStatus.Instance.MapScale;
            vector.x *= Mathf.Sign(ShipStatus.Instance.transform.localScale.x);
            vector.z = -1f;

            newIcon.transform.localPosition = vector;

            CastedPlayers.Add(player);
            CastedIcons.Add(newIcon);
        }
    }

    public void ClearMapIcons()
    {
        foreach (var gameObject in CastedIcons.Select(icon => icon.gameObject).Where(gameObject => gameObject != null))
        {
            gameObject.Destroy();
        }

        CastedIcons.Clear();
    }
}
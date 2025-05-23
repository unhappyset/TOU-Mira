using Il2CppInterop.Runtime.Attributes;
using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Modifiers;
using MiraAPI.Utilities.Assets;
using Reactor.Utilities.Extensions;
using TownOfUs.Buttons.Modifiers;
using TownOfUs.Modifiers.Game.Impostor;
using TownOfUs.Modules.Anims;
using TownOfUs.Modules.Wiki;
using TownOfUs.Options.Modifiers;
using TownOfUs.Options.Modifiers.Universal;
using TownOfUs.Roles.Crewmate;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfUs.Modifiers.Game.Universal;

public sealed class SatelliteModifier : UniversalGameModifier, IWikiDiscoverable
{
    public override string ModifierName => "Satellite";
    public override LoadableAsset<Sprite>? ModifierIcon => TouModifierIcons.Satellite;
    public override string GetDescription() => "You can broadcast a signal to detect all dead bodies on the map.";
    public override int GetAssignmentChance() => (int)OptionGroupSingleton<UniversalModifierOptions>.Instance.SatelliteChance;
    public override int GetAmountPerGame() => (int)OptionGroupSingleton<UniversalModifierOptions>.Instance.SatelliteAmount;
    private readonly List<PlayerControl> CastedPlayers = [];
    private readonly List<SpriteRenderer> CastedIcons = [];

    public override bool IsModifierValidOn(RoleBehaviour role)
    {
        return base.IsModifierValidOn(role) && !role.Player.GetModifierComponent().HasModifier<DisperserModifier>(true) && role is not MysticRole;
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
            var newIcon = UnityEngine.Object.Instantiate(MapBehaviour.Instance.TrackedHerePoint);
            newIcon.material = AnimStore.SetSpriteColourMatch(player, newIcon.material);

            Vector3 vector = player.transform.position;
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
    public string GetAdvancedDescription()
    {
        return "You can broadcast a signal to know where dead bodies are."
               + MiscUtils.AppendOptionsText(GetType());
    }

    [HideFromIl2Cpp]
    public List<CustomButtonWikiDescription> Abilities { get; } = [
        new("Broadcast",
            $"You can check for bodies on the map, which you can do {OptionGroupSingleton<SatelliteOptions>.Instance.MaxNumCast} time(s) per game.",
            TouAssets.BroadcastSprite),
    ];
}

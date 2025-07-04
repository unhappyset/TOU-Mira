using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Utilities;
using MiraAPI.Utilities.Assets;
using TownOfUs.Modifiers.Impostor;
using TownOfUs.Options.Roles.Impostor;
using TownOfUs.Roles.Impostor;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfUs.Buttons.Impostor;

public sealed class EclipsalBlindButton : TownOfUsRoleButton<EclipsalRole>, IAftermathableButton
{
    public override string Name => "Blind";
    public override string Keybind => Keybinds.SecondaryAction;
    public override Color TextOutlineColor => TownOfUsColors.Impostor;
    public override float Cooldown => OptionGroupSingleton<EclipsalOptions>.Instance.BlindCooldown + MapCooldown;
    public override float EffectDuration => OptionGroupSingleton<EclipsalOptions>.Instance.BlindDuration;
    public override LoadableAsset<Sprite> Sprite => TouImpAssets.BlindSprite;

    protected override void OnClick()
    {
        OverrideName("Unblinding");
        var blindRadius = OptionGroupSingleton<EclipsalOptions>.Instance.BlindRadius;
        var blindedPlayers =
            Helpers.GetClosestPlayers(PlayerControl.LocalPlayer, blindRadius * ShipStatus.Instance.MaxLightRadius);

        foreach (var player in blindedPlayers.Where(x => !x.HasDied() && !x.IsImpostor()))
        {
            player.RpcAddModifier<EclipsalBlindModifier>(PlayerControl.LocalPlayer);
        }
        // PlayerControl.LocalPlayer.RpcAddModifier<EclipsalBlindModifier>(PlayerControl.LocalPlayer);
    }

    public override void OnEffectEnd()
    {
        OverrideName("Blind");
    }
}
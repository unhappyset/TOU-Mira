using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Modifiers;
using MiraAPI.Utilities.Assets;
using Reactor.Utilities;
using TownOfUs.Modifiers.Neutral;
using TownOfUs.Options.Roles.Neutral;
using TownOfUs.Roles.Neutral;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfUs.Buttons.Neutral;

public sealed class ArsonistDouseButton : TownOfUsRoleButton<ArsonistRole, PlayerControl>
{
    public override string Name => "Douse";
    public override string Keybind => Keybinds.SecondaryAction;
    public override Color TextOutlineColor => TownOfUsColors.Arsonist;
    public override float Cooldown => OptionGroupSingleton<ArsonistOptions>.Instance.DouseCooldown + MapCooldown;
    public override LoadableAsset<Sprite> Sprite => TouNeutAssets.DouseButtonSprite;

    protected override void OnClick()
    {
        if (Target == null)
        {
            Logger<TownOfUsPlugin>.Error("Arsonist Attack: Target is null");
            return;
        }

        Target.RpcAddModifier<ArsonistDousedModifier>(PlayerControl.LocalPlayer.PlayerId);

        CustomButtonSingleton<ArsonistIgniteButton>.Instance.SetTimer(CustomButtonSingleton<ArsonistIgniteButton>
            .Instance.Cooldown);
    }

    public override bool IsTargetValid(PlayerControl? target)
    {
        return base.IsTargetValid(target) && !target!.HasModifier<ArsonistDousedModifier>();
    }

    public override PlayerControl? GetTarget()
    {
        return PlayerControl.LocalPlayer.GetClosestLivingPlayer(true, Distance,
            predicate: x => !x.HasModifier<ArsonistDousedModifier>());
    }
}
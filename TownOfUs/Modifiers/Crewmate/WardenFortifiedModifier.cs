using MiraAPI.Events;
using MiraAPI.GameOptions;
using MiraAPI.Utilities.Assets;
using TownOfUs.Events.TouEvents;
using TownOfUs.Options.Roles.Crewmate;
using UnityEngine;

namespace TownOfUs.Modifiers.Crewmate;

public sealed class WardenFortifiedModifier(PlayerControl warden) : BaseShieldModifier
{
    public override string ModifierName => "Fortified";
    public override LoadableAsset<Sprite>? ModifierIcon => TouRoleIcons.Warden;
    public override string ShieldDescription => "You are fortified by a Warden!\nNo one can interact with you.";

    public override bool HideOnUi
    {
        get
        {
            var show = OptionGroupSingleton<WardenOptions>.Instance.ShowFortified;
            var showShieldedEveryone = show == FortifyOptions.Everyone;
            var showShieldedSelf = PlayerControl.LocalPlayer.PlayerId == Player.PlayerId &&
                                   show is FortifyOptions.Self or FortifyOptions.SelfAndWarden;
            return !TownOfUsPlugin.ShowShieldHud.Value && (!showShieldedSelf || !showShieldedEveryone);
        }
    }

    public override bool VisibleSymbol
    {
        get
        {
            var show = OptionGroupSingleton<WardenOptions>.Instance.ShowFortified;
            var showShieldedEveryone = show == FortifyOptions.Everyone;
            var showShieldedSelf = PlayerControl.LocalPlayer.PlayerId == Player.PlayerId &&
                                   show is FortifyOptions.Self or FortifyOptions.SelfAndWarden;
            return showShieldedSelf || showShieldedEveryone;
        }
    }

    public PlayerControl Warden { get; } = warden;

    public override void OnActivate()
    {
        base.OnActivate();
        var touAbilityEvent = new TouAbilityEvent(AbilityType.WardenFortify, Warden, Player);
        MiraEventManager.InvokeEvent(touAbilityEvent);
    }

    public override void OnDeactivate()
    {
        Player.cosmetics.SetOutline(false, new Il2CppSystem.Nullable<Color>(TownOfUsColors.Warden));
    }

    public override void Update()
    {
        var show = OptionGroupSingleton<WardenOptions>.Instance.ShowFortified;

        var showShieldedEveryone = show == FortifyOptions.Everyone;
        var showShieldedSelf = PlayerControl.LocalPlayer.PlayerId == Player.PlayerId &&
                               show is FortifyOptions.Self or FortifyOptions.SelfAndWarden;
        var showShieldedWarden = PlayerControl.LocalPlayer.PlayerId == Warden.PlayerId &&
                                 show is FortifyOptions.Warden or FortifyOptions.SelfAndWarden;

        if (showShieldedEveryone || showShieldedSelf || showShieldedWarden)
            Player?.cosmetics.SetOutline(true, new Il2CppSystem.Nullable<Color>(TownOfUsColors.Warden));
    }

    public override void OnDeath(DeathReason reason)
    {
        Player.cosmetics.SetOutline(false, new Il2CppSystem.Nullable<Color>(TownOfUsColors.Warden));
    }
}
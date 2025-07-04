using MiraAPI.Events;
using MiraAPI.GameOptions;
using MiraAPI.Utilities.Assets;
using PowerTools;
using Reactor.Utilities.Extensions;
using TownOfUs.Events.TouEvents;
using TownOfUs.Modules.Anims;
using TownOfUs.Options;
using TownOfUs.Options.Roles.Crewmate;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfUs.Modifiers.Crewmate;

public sealed class ClericBarrierModifier(PlayerControl cleric) : BaseShieldModifier
{
    public override string ModifierName => "Barrier";
    public override LoadableAsset<Sprite>? ModifierIcon => TouRoleIcons.Cleric;
    public override string ShieldDescription => "You are shielded by a Cleric!\nNo one can interact with you.";
    public override float Duration => OptionGroupSingleton<ClericOptions>.Instance.BarrierCooldown;
    public override bool AutoStart => true;

    public override bool HideOnUi
    {
        get
        {
            var showBarrier = OptionGroupSingleton<ClericOptions>.Instance.ShowBarriered;
            var showBarrierSelf = PlayerControl.LocalPlayer.PlayerId == Player.PlayerId &&
                                  (showBarrier == BarrierOptions.Self || showBarrier == BarrierOptions.SelfAndCleric);
            return !TownOfUsPlugin.ShowShieldHud.Value && !showBarrierSelf;
        }
    }

    public override bool VisibleSymbol
    {
        get
        {
            var showBarrier = OptionGroupSingleton<ClericOptions>.Instance.ShowBarriered;
            var showBarrierSelf = PlayerControl.LocalPlayer.PlayerId == Player.PlayerId &&
                                  (showBarrier == BarrierOptions.Self || showBarrier == BarrierOptions.SelfAndCleric);
            return showBarrierSelf;
        }
    }

    public PlayerControl Cleric { get; } = cleric;
    public GameObject? ClericBarrier { get; set; }


    public override void OnActivate()
    {
        var touAbilityEvent = new TouAbilityEvent(AbilityType.ClericBarrier, Cleric, Player);
        MiraEventManager.InvokeEvent(touAbilityEvent);

        var showBarrier = OptionGroupSingleton<ClericOptions>.Instance.ShowBarriered;

        var showBarrierSelf = PlayerControl.LocalPlayer.PlayerId == Player.PlayerId &&
                              (showBarrier == BarrierOptions.Self || showBarrier == BarrierOptions.SelfAndCleric);
        var showBarrierCleric = PlayerControl.LocalPlayer.PlayerId == Cleric.PlayerId &&
                                (showBarrier == BarrierOptions.Cleric || showBarrier == BarrierOptions.SelfAndCleric);

        if (showBarrierSelf || showBarrierCleric || (PlayerControl.LocalPlayer.HasDied() &&
                                                     OptionGroupSingleton<GeneralOptions>.Instance.TheDeadKnow))
        {
            ClericBarrier = AnimStore.SpawnAnimBody(Player, TouAssets.ClericBarrier.LoadAsset(), false, -1.1f, -0.35f)!;
            ClericBarrier.GetComponent<SpriteAnim>().SetSpeed(2f);
        }
    }

    public override void OnDeactivate()
    {
        if (ClericBarrier?.gameObject != null) ClericBarrier.gameObject.Destroy();
    }
}
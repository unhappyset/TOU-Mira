using MiraAPI.GameOptions;
using MiraAPI.Utilities.Assets;
using TownOfUs.Modules.Anims;
using Reactor.Utilities.Extensions;
using TownOfUs.Options.Roles.Crewmate;
using UnityEngine;
using TownOfUs.Utilities;
using TownOfUs.Options;

namespace TownOfUs.Modifiers.Crewmate;

public sealed class MedicShieldModifier(PlayerControl medic) : BaseShieldModifier
{
    public override string ModifierName => "Medic Shield";
    public override LoadableAsset<Sprite>? ModifierIcon => TouRoleIcons.Medic;
    public override string ShieldDescription => "You are shielded by a Medic!\nYou may not die to other players";
    public PlayerControl Medic { get; } = medic;
    public GameObject? MedicShield { get; set; }
    public override bool HideOnUi
    {
        get
        {
            var showShielded = OptionGroupSingleton<MedicOptions>.Instance.ShowShielded;
            var showShieldedEveryone = showShielded == MedicOption.Everyone;
            var showShieldedSelf = PlayerControl.LocalPlayer.PlayerId == Player.PlayerId &&
                (showShielded is MedicOption.Shielded or MedicOption.ShieldedAndMedic);
            return !TownOfUsPlugin.ShowShieldHud.Value && (!showShieldedSelf || !showShieldedEveryone);
        }
    }
    public override bool VisibleSymbol
    {
        get
        {
            var showShielded = OptionGroupSingleton<MedicOptions>.Instance.ShowShielded;
            var showShieldedEveryone = showShielded == MedicOption.Everyone;
            var showShieldedSelf = PlayerControl.LocalPlayer.PlayerId == Player.PlayerId &&
                (showShielded is MedicOption.Shielded or MedicOption.ShieldedAndMedic);
            return showShieldedSelf || showShieldedEveryone;
        }
    }

    public override void OnActivate()
    {
        var showShielded = OptionGroupSingleton<MedicOptions>.Instance.ShowShielded;

        var showShieldedEveryone = showShielded == MedicOption.Everyone;
        var showShieldedSelf = PlayerControl.LocalPlayer.PlayerId == Player.PlayerId &&
            (showShielded is MedicOption.Shielded or MedicOption.ShieldedAndMedic);
        var showShieldedMedic = PlayerControl.LocalPlayer.PlayerId == Medic.PlayerId &&
                 (showShielded is MedicOption.Medic or MedicOption.ShieldedAndMedic);

        if (showShieldedEveryone || showShieldedSelf || showShieldedMedic || (PlayerControl.LocalPlayer.HasDied() && OptionGroupSingleton<GeneralOptions>.Instance.TheDeadKnow))
        {
            MedicShield = AnimStore.SpawnAnimBody(Player, TouAssets.MedicShield.LoadAsset(), false, -1.1f, -0.1f)!;
        }
    }
    public override void OnDeactivate()
    {
        if (MedicShield?.gameObject != null)
        {
            MedicShield.gameObject.Destroy();
        }
    }
    public override void Update()
    {
        if (!MeetingHud.Instance && MedicShield?.gameObject != null)
        {
            MedicShield?.SetActive(!IsConcealed());
        }
    }
}

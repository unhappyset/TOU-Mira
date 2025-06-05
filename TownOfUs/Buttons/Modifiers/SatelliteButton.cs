using HarmonyLib;
using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Modifiers;
using MiraAPI.Utilities.Assets;
using TownOfUs.Modifiers.Game.Universal;
using TownOfUs.Options.Modifiers.Universal;
using UnityEngine;
using Object = UnityEngine.Object;

namespace TownOfUs.Buttons.Modifiers;

public sealed class SatelliteButton : TownOfUsButton
{
    public override string Name => "Broadcast";
    public override string Keybind => "tou.ActionCustom2";
    public override Color TextOutlineColor => TownOfUsColors.Satellite;
    public override float Cooldown => OptionGroupSingleton<SatelliteOptions>.Instance.Cooldown + MapCooldown;
    public override int MaxUses => (int)OptionGroupSingleton<SatelliteOptions>.Instance.MaxNumCast;
    public override ButtonLocation Location => ButtonLocation.BottomLeft;
    public override LoadableAsset<Sprite> Sprite => TouAssets.BroadcastSprite;
    public bool Usable { get; set; } = OptionGroupSingleton<SatelliteOptions>.Instance.FirstRoundUse;

    public override bool Enabled(RoleBehaviour? role)
    {
        return PlayerControl.LocalPlayer != null &&
               PlayerControl.LocalPlayer.HasModifier<SatelliteModifier>() &&
               !PlayerControl.LocalPlayer.Data.IsDead;
    }

    public override bool CanUse()
    {
        return base.CanUse() && Usable;
    }
    public override void CreateButton(Transform parent)
    {
        base.CreateButton(parent);

        Button!.usesRemainingSprite.sprite = TouAssets.AbilityCounterBodySprite.LoadAsset();
    }
    protected override void OnClick()
    {
        var deadBodies = Object.FindObjectsOfType<DeadBody>().ToList();

        deadBodies.Do(x => PlayerControl.LocalPlayer.AddModifier<SatelliteArrowModifier>(x, Color.white));
        if (OptionGroupSingleton<SatelliteOptions>.Instance.OneUsePerRound) Usable = false;
        // will return to this once i get more freetime
        //deadBodies.Do(x => PlayerControl.LocalPlayer.GetModifier<SatelliteModifier>().NewMapIcon(MiscUtils.PlayerById(x.ParentId)));
    }
}

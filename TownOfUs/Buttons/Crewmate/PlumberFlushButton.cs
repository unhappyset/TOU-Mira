using MiraAPI.GameOptions;
using MiraAPI.Utilities.Assets;
using TownOfUs.Options.Roles.Crewmate;
using TownOfUs.Roles.Crewmate;
using UnityEngine;

namespace TownOfUs.Buttons.Crewmate;

public sealed class PlumberFlushButton : TownOfUsRoleButton<PlumberRole>
{
    public override string Name => "Flush";
    public override string Keybind => "ActionQuaternary";
    public override Color TextOutlineColor => TownOfUsColors.Plumber;
    public override float Cooldown => OptionGroupSingleton<PlumberOptions>.Instance.FlushCooldown + MapCooldown;
    public override LoadableAsset<Sprite> Sprite => TouCrewAssets.FlushSprite;

    protected override void OnClick()
    {
        PlumberRole.RpcPlumberFlush(PlayerControl.LocalPlayer);
    }
}

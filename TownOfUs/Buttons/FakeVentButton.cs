using MiraAPI.Roles;
using MiraAPI.Utilities.Assets;
using UnityEngine;

namespace TownOfUs.Buttons;

public sealed class FakeVentButton : TownOfUsButton
{
    public override string Name => " ";
    public override Color TextOutlineColor => Color.clear;
    public override float Cooldown => 0.001f;
    public override LoadableAsset<Sprite> Sprite => TouAssets.BlankSprite;

    public bool Show { get; set; } = true;

    public override bool Enabled(RoleBehaviour? role)
    {
        return TownOfUsPlugin.OffsetButtons.Value && Show && HudManager.InstanceExists && !MeetingHud.Instance && role != null && !role.IsImpostor
             && (!role.CanVent || (role is ICustomRole customRole && !customRole.Configuration.CanUseVent));
    }
    protected override void OnClick()
    {
    }
}

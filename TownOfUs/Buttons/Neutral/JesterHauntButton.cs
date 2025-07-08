using MiraAPI.Hud;
using MiraAPI.Modifiers;
using MiraAPI.Networking;
using MiraAPI.Utilities.Assets;
using TownOfUs.Modifiers;
using TownOfUs.Modifiers.Neutral;
using TownOfUs.Modules;
using TownOfUs.Roles.Neutral;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfUs.Buttons.Neutral;

public sealed class JesterHauntButton : TownOfUsRoleButton<GlitchRole>, IAftermathableButton
{
    public override string Name => "Haunt";
    public override string Keybind => Keybinds.SecondaryAction;
    public override Color TextOutlineColor => TownOfUsColors.Jester;
    public override float Cooldown => 0.01f;
    public override LoadableAsset<Sprite> Sprite => TouNeutAssets.JesterHauntSprite;
    public override ButtonLocation Location => ButtonLocation.BottomRight;
    public override bool ShouldPauseInVent => false;

    public override bool Enabled(RoleBehaviour? role)
    {
        return role is NeutralGhostRole && ModifierUtils.GetActiveModifiers<MisfortuneTargetModifier>().Any() && role.Player.GetRoleWhenAlive() is JesterRole;
    }

    protected override void OnClick()
    {
        var playerMenu = CustomPlayerMenu.Create();
        playerMenu.transform.FindChild("PhoneUI").GetChild(0).GetComponent<SpriteRenderer>().material =
            PlayerControl.LocalPlayer.cosmetics.currentBodySprite.BodySprite.material;
        playerMenu.transform.FindChild("PhoneUI").GetChild(1).GetComponent<SpriteRenderer>().material =
            PlayerControl.LocalPlayer.cosmetics.currentBodySprite.BodySprite.material;
        playerMenu.Begin(
            plr => !plr.HasDied() && plr.HasModifier<MisfortuneTargetModifier>() && !plr.HasModifier<InvulnerabilityModifier>() && plr != PlayerControl.LocalPlayer,
            plr =>
            {
                playerMenu.ForceClose();

                if (plr != null)
                {
                    PlayerControl.LocalPlayer.RpcCustomMurder(plr, teleportMurderer: false);

                    foreach (var mod in ModifierUtils.GetActiveModifiers<MisfortuneTargetModifier>())
                    {
                        mod.ModifierComponent?.RemoveModifier(mod);
                    }
                }
            });
    }

    public override bool CanUse()
    {
        return ModifierUtils.GetActiveModifiers<MisfortuneTargetModifier>().Any();
    }
}
﻿using MiraAPI.Hud;
using MiraAPI.Modifiers;
using MiraAPI.Utilities.Assets;
using TownOfUs.Modifiers;
using TownOfUs.Modifiers.Neutral;
using TownOfUs.Networking;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfUs.Buttons.Neutral;

public sealed class ExeTormentButton : TownOfUsButton
{
    public override string Name => TouLocale.Get("TouRoleExecutionerTorment", "Torment");
    public override BaseKeybind Keybind => Keybinds.PrimaryAction;
    public override Color TextOutlineColor => TownOfUsColors.Executioner;
    public override float Cooldown => 0.01f;
    public override LoadableAsset<Sprite> Sprite => TouNeutAssets.ExeTormentSprite;
    public override ButtonLocation Location => ButtonLocation.BottomRight;
    public override bool ShouldPauseInVent => false;
    public override bool UsableInDeath => true;
    public bool Show { get; set; }

    public override bool Enabled(RoleBehaviour? role)
    {
        return Show && ModifierUtils.GetActiveModifiers<MisfortuneTargetModifier>().Any();
    }

    protected override void OnClick()
    {
        if (Minigame.Instance != null)
        {
            return;
        }

        var playerMenu = CustomPlayerMenu.Create();
        playerMenu.transform.FindChild("PhoneUI").GetChild(0).GetComponent<SpriteRenderer>().material =
            PlayerControl.LocalPlayer.cosmetics.currentBodySprite.BodySprite.material;
        playerMenu.transform.FindChild("PhoneUI").GetChild(1).GetComponent<SpriteRenderer>().material =
            PlayerControl.LocalPlayer.cosmetics.currentBodySprite.BodySprite.material;
        playerMenu.Begin(
            plr => !plr.HasDied() && plr.HasModifier<MisfortuneTargetModifier>() &&
                   !plr.HasModifier<InvulnerabilityModifier>() && plr != PlayerControl.LocalPlayer,
            plr =>
            {
                playerMenu.ForceClose();

                if (plr != null && ModifierUtils.GetActiveModifiers<MisfortuneTargetModifier>().Any())
                {
                    PlayerControl.LocalPlayer.RpcGhostRoleMurder(plr);
                    foreach (var mod in ModifierUtils.GetActiveModifiers<MisfortuneTargetModifier>())
                    {
                        mod.ModifierComponent?.RemoveModifier(mod);
                    }

                    Show = false;
                }
            });
    }

    public override bool CanUse()
    {
        if (HudManager.Instance.Chat.IsOpenOrOpening || MeetingHud.Instance)
        {
            return false;
        }

        return ModifierUtils.GetActiveModifiers<MisfortuneTargetModifier>().Any();
    }
}
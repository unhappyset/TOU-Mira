﻿using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Modifiers;
using MiraAPI.Utilities;
using MiraAPI.Utilities.Assets;
using Reactor.Utilities;
using TownOfUs.Modifiers;
using TownOfUs.Modifiers.Neutral;
using TownOfUs.Modules;
using TownOfUs.Options.Roles.Crewmate;
using TownOfUs.Roles.Crewmate;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfUs.Buttons.Crewmate;

public sealed class PlumberBlockButton : TownOfUsRoleButton<PlumberRole, Vent>
{
    private static readonly ContactFilter2D Filter = Helpers.CreateFilter(Constants.Usables);
    public override string Name => TouLocale.Get("TouRolePlumberBlock", "Block");
    public override BaseKeybind Keybind => Keybinds.PrimaryAction;
    public override Color TextOutlineColor => TownOfUsColors.Plumber;
    public override float Cooldown => OptionGroupSingleton<PlumberOptions>.Instance.BlockCooldown + MapCooldown;
    public override int MaxUses => (int)OptionGroupSingleton<PlumberOptions>.Instance.MaxBarricades;
    public override LoadableAsset<Sprite> Sprite => TouCrewAssets.BlockSprite;
    public int ExtraUses { get; set; }

    public override bool IsTargetValid(Vent? target)
    {
        return base.IsTargetValid(target) && !PlumberRole.VentsBlocked.Select(x => x.Key).Contains(target!.Id) &&
               !Role.FutureBlocks.Contains(target.Id);
    }

    public override Vent? GetTarget()
    {
        var vent = PlayerControl.LocalPlayer.GetNearestObjectOfType<Vent>(Distance / 4, Filter);
        if (vent == null)
        {
            vent = PlayerControl.LocalPlayer.GetNearestObjectOfType<Vent>(Distance / 3, Filter);
        }

        if (vent == null)
        {
            vent = PlayerControl.LocalPlayer.GetNearestObjectOfType<Vent>(Distance / 2, Filter);
        }

        if (vent == null)
        {
            vent = PlayerControl.LocalPlayer.GetNearestObjectOfType<Vent>(Distance, Filter);
        }

        if (ModCompatibility.IsSubmerged() && vent != null && (vent.Id == 0 || vent.Id == 14))
        {
            vent = null;
        }

        if (vent != null && PlayerControl.LocalPlayer.CanUseVent(vent))
        {
            return vent;
        }

        return null;
    }

    public override bool CanUse()
    {
        var newTarget = GetTarget();
        if (newTarget != Target)
        {
            Target?.SetOutline(false, false);
        }

        Target = IsTargetValid(newTarget) ? newTarget : null;
        SetOutline(true);

        if (HudManager.Instance.Chat.IsOpenOrOpening || MeetingHud.Instance)
        {
            return false;
        }

        if (PlayerControl.LocalPlayer.HasModifier<GlitchHackedModifier>() || PlayerControl.LocalPlayer
                .GetModifiers<DisabledModifier>().Any(x => !x.CanUseAbilities))
        {
            return false;
        }

        return Timer <= 0 && Target != null && UsesLeft > 0;
    }

    protected override void OnClick()
    {
        if (Target == null)
        {
            Logger<TownOfUsPlugin>.Error($"{Name}: Target is null");
            return;
        }

        var notif1 = Helpers.CreateAndShowNotification(
            TouLocale.Get("TouRolePlumberBlockNotif"),
            Color.white, new Vector3(0f, 1f, -20f), spr: TouRoleIcons.Plumber.LoadAsset());
        notif1.AdjustNotification();

        PlumberRole.RpcPlumberBlockVent(PlayerControl.LocalPlayer, Target.Id);

        var flush = CustomButtonSingleton<PlumberFlushButton>.Instance;

        flush?.SetTimer(flush.Cooldown);
    }
}
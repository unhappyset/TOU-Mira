﻿using System.Text;
using AmongUs.GameOptions;
using Il2CppInterop.Runtime.Attributes;
using MiraAPI.Events;
using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Modifiers;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using Reactor.Networking.Attributes;
using Reactor.Networking.Rpc;
using TownOfUs.Buttons.Impostor;
using TownOfUs.Events.TouEvents;
using TownOfUs.Modifiers.Impostor;
using TownOfUs.Options.Roles.Impostor;
using TownOfUs.Roles.Crewmate;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfUs.Roles.Impostor;

public sealed class UndertakerRole(IntPtr cppPtr)
    : ImpostorRole(cppPtr), ITownOfUsRole, IWikiDiscoverable, IDoomable, ICrewVariant
{
    public void FixedUpdate()
    {
        if (Player == null || Player.Data.Role is not UndertakerRole || Player.HasDied() || !Player.AmOwner ||
            MeetingHud.Instance || (!HudManager.Instance.UseButton.isActiveAndEnabled &&
                                    !HudManager.Instance.PetButton.isActiveAndEnabled))
        {
            return;
        }

        HudManager.Instance.KillButton.ToggleVisible(OptionGroupSingleton<UndertakerOptions>.Instance.UndertakerKill ||
                                                     (Player != null && Player.GetModifiers<BaseModifier>()
                                                         .Any(x => x is ICachedRole)) ||
                                                     (Player != null && MiscUtils.ImpAliveCount == 1));
    }

    public RoleBehaviour CrewVariant => RoleManager.Instance.GetRole((RoleTypes)RoleId.Get<AltruistRole>());
    public DoomableType DoomHintType => DoomableType.Death;
    public string LocaleKey => "Undertaker";
    public string RoleName => TouLocale.Get($"TouRole{LocaleKey}");
    public string RoleDescription => TouLocale.GetParsed($"TouRole{LocaleKey}IntroBlurb");
    public string RoleLongDescription => TouLocale.GetParsed($"TouRole{LocaleKey}TabDescription");

    public string GetAdvancedDescription()
    {
        return
            TouLocale.GetParsed($"TouRole{LocaleKey}WikiDescription") +
            MiscUtils.AppendOptionsText(GetType());
    }

    public Color RoleColor => TownOfUsColors.Impostor;
    public ModdedRoleTeams Team => ModdedRoleTeams.Impostor;
    public RoleAlignment RoleAlignment => RoleAlignment.ImpostorSupport;

    public CustomRoleConfiguration Configuration => new(this)
    {
        UseVanillaKillButton = true,
        CanUseVent = OptionGroupSingleton<UndertakerOptions>.Instance.CanVent,
        Icon = TouRoleIcons.Undertaker
    };

    [HideFromIl2Cpp]
    public StringBuilder SetTabText()
    {
        return ITownOfUsRole.SetNewTabText(this);
    }

    [HideFromIl2Cpp]
    public List<CustomButtonWikiDescription> Abilities
    {
        get
        {
            return new List<CustomButtonWikiDescription>
            {
                new(TouLocale.GetParsed($"TouRole{LocaleKey}Drag", "Drag"),
                    TouLocale.GetParsed($"TouRole{LocaleKey}DragWikiDescription"),
                    TouImpAssets.DragSprite),
                new(TouLocale.GetParsed($"TouRole{LocaleKey}Drop", "Drop"),
                    TouLocale.GetParsed($"TouRole{LocaleKey}DropWikiDescription"),
                    TouImpAssets.DropSprite)
            };
        }
    }

    [MethodRpc((uint)TownOfUsRpc.DragBody, LocalHandling = RpcLocalHandling.Before)]
    public static void RpcStartDragging(PlayerControl playerControl, byte bodyId)
    {
        playerControl.GetModifierComponent()?.AddModifier(new DragModifier(bodyId));

        var touAbilityEvent =
            new TouAbilityEvent(AbilityType.UndertakerDrag, playerControl, Helpers.GetBodyById(bodyId));
        MiraEventManager.InvokeEvent(touAbilityEvent);

        if (playerControl.AmOwner)
        {
            CustomButtonSingleton<UndertakerDragDropButton>.Instance.SetDrop();
        }
    }

    [MethodRpc((uint)TownOfUsRpc.DropBody, LocalHandling = RpcLocalHandling.Before)]
    public static void RpcStopDragging(PlayerControl playerControl, Vector2 dropLocation)
    {
        var dragMod = playerControl.GetModifier<DragModifier>()!;
        var dropPos = (Vector3)dropLocation;
        dropPos.z = dropPos.y / 1000f;
        dragMod.DeadBody!.transform.position = dropPos;

        var touAbilityEvent = new TouAbilityEvent(AbilityType.UndertakerDrop, playerControl, dragMod.DeadBody);
        MiraEventManager.InvokeEvent(touAbilityEvent);

        playerControl.GetModifierComponent()?.RemoveModifier(dragMod);

        if (playerControl.AmOwner)
        {
            CustomButtonSingleton<UndertakerDragDropButton>.Instance.SetDrag();
        }
    }
}
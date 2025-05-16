using System.Globalization;
using System.Text;
using AmongUs.GameOptions;
using Il2CppInterop.Runtime.Attributes;
using MiraAPI.Modifiers;
using MiraAPI.Roles;
using Reactor.Networking.Attributes;
using Reactor.Utilities;
using TownOfUs.Modifiers.Crewmate;
using TownOfUs.Modules.Wiki;
using TownOfUs.Patches.Stubs;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfUs.Roles.Crewmate;

public sealed class WardenRole(IntPtr cppPtr) : CrewmateRole(cppPtr), ITownOfUsRole, IWikiDiscoverable, IDoomable
{
    public string RoleName => "Warden";
    public string RoleDescription => "Fortify Crewmates";
    public string RoleLongDescription => "Fortify crewmates to prevent interactions with them";
    public Color RoleColor => TownOfUsColors.Warden;
    public ModdedRoleTeams Team => ModdedRoleTeams.Crewmate;
    public RoleAlignment RoleAlignment => RoleAlignment.CrewmateProtective;
    public DoomableType DoomHintType => DoomableType.Protective;
    public override bool IsAffectedByComms => false;
    public CustomRoleConfiguration Configuration => new(this)
    {
        IntroSound = CustomRoleUtils.GetIntroSound(RoleTypes.Scientist),
        Icon = TouRoleIcons.Warden,
    };

    public PlayerControl? Fortified { get; set; }

    [HideFromIl2Cpp]
    public StringBuilder SetTabText()
    {
        var stringB = ITownOfUsRole.SetNewTabText(this);

        if (Fortified != null)
        {
            stringB.Append(CultureInfo.InvariantCulture, $"\n<b>Fortified: </b>{Color.white.ToTextColor()}{Fortified.Data.PlayerName}</color>");
        }

        return stringB;
    }

    public void Clear()
    {
        SetFortifiedPlayer(null);
    }

    public override void OnDeath(DeathReason reason)
    {
        RoleStubs.RoleBehaviourOnDeath(this, reason);

        Clear();
    }

    public override void Deinitialize(PlayerControl targetPlayer)
    {
        RoleStubs.RoleBehaviourDeinitialize(this, targetPlayer);

        Clear();
    }

    public void PlayerControlFixedUpdate(PlayerControl playerControl)
    {
        if (Fortified != null && Fortified.HasDied())
            Clear();
    }

    public void SetFortifiedPlayer(PlayerControl? player)
    {
        Fortified?.RemoveModifier<WardenFortifiedModifier>();

        Fortified = player;

        Fortified?.AddModifier<WardenFortifiedModifier>(Player);
    }

    [MethodRpc((uint)TownOfUsRpc.WardenFortify, SendImmediately = true)]
    public static void RpcWardenFortify(PlayerControl player, PlayerControl target)
    {
        if (player.Data.Role is not WardenRole)
        {
            Logger<TownOfUsPlugin>.Error("RpcWardenFortify - Invalid warden");
            return;
        }

        var warden = player.GetRole<WardenRole>();
        warden?.SetFortifiedPlayer(target);
    }

    [MethodRpc((uint)TownOfUsRpc.ClearWardenFortify, SendImmediately = true)]
    public static void RpcClearWardenFortify(PlayerControl player)
    {
        if (player.Data.Role is not WardenRole)
        {
            Logger<TownOfUsPlugin>.Error("RpcClearWardenFortify - Invalid warden");
            return;
        }

        var warden = player.GetRole<WardenRole>();
        warden?.SetFortifiedPlayer(null);
    }

    [MethodRpc((uint)TownOfUsRpc.WardenNotify, SendImmediately = true)]
    public static void RpcWardenNotify(PlayerControl player, PlayerControl source, PlayerControl target)
    {
        if (player.Data.Role is not WardenRole)
        {
            Logger<TownOfUsPlugin>.Error("RpcWardenNotify - Invalid warden");
            return;
        }

        // Logger<TownOfUsPlugin>.Error("RpcWardenNotify");
        if (player.AmOwner)
            Coroutines.Start(MiscUtils.CoFlash(TownOfUsColors.Warden));

        if (source.AmOwner)
            Coroutines.Start(MiscUtils.CoFlash(TownOfUsColors.Warden));
    }
    
    public string GetAdvancedDescription()
    {
        return
            "The Warden is a Crewmate Protective role that can fortify players to prevent them from being interacted with. "
            + MiscUtils.AppendOptionsText(GetType());
    }
    
    [HideFromIl2Cpp]
    public List<CustomButtonWikiDescription> Abilities { get; } = [
        new("Fortify",
            $"Fortify a player to prevent them from being interacted with. If anyone tries to interact with a fortified player, the ability will not work and both the Warden and fortified player will be alerted with a purple flash.",
            TouCrewAssets.FortifySprite),
    ];
}

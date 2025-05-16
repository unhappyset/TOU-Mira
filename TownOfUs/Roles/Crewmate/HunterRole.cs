using System.Text;
using AmongUs.GameOptions;
using Il2CppInterop.Runtime.Attributes;
using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Networking;
using MiraAPI.Roles;
using Reactor.Networking.Attributes;
using Reactor.Utilities;
using TownOfUs.Buttons.Crewmate;
using TownOfUs.Modules.Wiki;
using TownOfUs.Options.Roles.Crewmate;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfUs.Roles.Crewmate;

public sealed class HunterRole(IntPtr cppPtr) : CrewmateRole(cppPtr), ITouCrewRole, IWikiDiscoverable, IDoomable
{
    public string RoleName => "Hunter";
    public string RoleDescription => "Stalk The <color=#FF0000FF>Impostor</color>";
    public string RoleLongDescription => "Stalk player interactions and kill impostors, but not Crewmates";
    public Color RoleColor => TownOfUsColors.Hunter;
    public ModdedRoleTeams Team => ModdedRoleTeams.Crewmate;
    public RoleAlignment RoleAlignment => RoleAlignment.CrewmateKilling;
    public DoomableType DoomHintType => DoomableType.Hunter;
    public bool IsPowerCrew => CaughtPlayers.Any(x => !x.HasDied()); // Disable end game checks if a Hunter has alive targets
    public override bool IsAffectedByComms => false;
    public CustomRoleConfiguration Configuration => new(this)
    {
        Icon = TouRoleIcons.Hunter,
        IntroSound = CustomRoleUtils.GetIntroSound(RoleTypes.Impostor),
    };

    public PlayerControl? LastVoted { get; set; }

    [HideFromIl2Cpp]
    public List<PlayerControl> CaughtPlayers { get; } = [];

    [MethodRpc((uint)TownOfUsRpc.CatchPlayer, SendImmediately = true)]
    public static void RpcCatchPlayer(PlayerControl hunter, PlayerControl source)
    {
        if (hunter.Data.Role is not HunterRole role)
        {
            Logger<TownOfUsPlugin>.Error("RpcCatchPlayer - Invalid hunter");
            return;
        }

        if (!role.CaughtPlayers.Contains(source))
        {
            role.CaughtPlayers.Add(source);

            if (hunter.AmOwner)
            {
                Coroutines.Start(MiscUtils.CoFlash(TownOfUsColors.Hunter));

                CustomButtonSingleton<HunterStalkButton>.Instance.ResetCooldownAndOrEffect();
            }
        }
    }

    public static void Retribution(PlayerControl hunter, PlayerControl target)
    {
        if (hunter.Data.Role is not HunterRole)
        {
            Logger<TownOfUsPlugin>.Error("RpcCatchPlayer - Invalid hunter");
            return;
        }

        PlayerControl.LocalPlayer.RpcCustomMurder(target, resetKillTimer: false, createDeadBody: false, teleportMurderer: false, showKillAnim: false, playKillSound: false);
    }

    [HideFromIl2Cpp]
    public StringBuilder SetTabText()
    {
        return ITownOfUsRole.SetNewTabText(this);
    }
    
    public string GetAdvancedDescription()
    {
        return
            "The Hunter is a Crewmate Killing role that can stalk players during the round. If a stalked player uses any ability while being stalked, they're added to their hitlist and can be killed. " +
            "The Hunter won't know what ability was used. The hitlist is permanent and doesn’t reset. Killing Crewmates has no penalty. " +
            (OptionGroupSingleton<HunterOptions>.Instance.RetributionOnVote
                ? "If ejected, they kill the last player who voted them unless they're invincible. "
                : string.Empty)
               + MiscUtils.AppendOptionsText(GetType());
    }

    [HideFromIl2Cpp]
    public List<CustomButtonWikiDescription> Abilities { get; } = [
        new("Stalk",
            $"Choose a target to stalk. You can stalk {OptionGroupSingleton<HunterOptions>.Instance.StalkUses} players. " +
            $"If they use any ability while stalked, they’re added to your hitlist and can be killed.",
            TouCrewAssets.StalkButtonSprite),
    ];

}

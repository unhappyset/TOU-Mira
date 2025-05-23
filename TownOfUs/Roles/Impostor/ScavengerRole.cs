using Il2CppInterop.Runtime.Attributes;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Roles;
using System.Globalization;
using System.Text;
using TownOfUs.Modifiers;
using TownOfUs.Modifiers.Impostor;
using TownOfUs.Modules.Wiki;
using TownOfUs.Options.Roles.Impostor;
using TownOfUs.Patches.Stubs;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfUs.Roles.Impostor;

public sealed class ScavengerRole(IntPtr cppPtr) : ImpostorRole(cppPtr), ITownOfUsRole, IWikiDiscoverable, IDoomable
{
    public string RoleName => "Scavenger";
    public string RoleDescription => "Hunt Down Your Prey";
    public string RoleLongDescription => "Kill your given targets for a reduced kill cooldown";
    public Color RoleColor => TownOfUsColors.Impostor;
    public ModdedRoleTeams Team => ModdedRoleTeams.Impostor;
    public RoleAlignment RoleAlignment => RoleAlignment.ImpostorKilling;
    public DoomableType DoomHintType => DoomableType.Hunter;
    public CustomRoleConfiguration Configuration => new(this)
    {
        Icon = TouRoleIcons.Scavenger,
        IntroSound = TouAudio.WarlockIntroSound,
    };

    public bool GameStarted { get; set; }
    public float TimeRemaining { get; set; }
    public PlayerControl? Target { get; set; }
    public bool Scavenging { get; set; }

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

    public void FixedUpdate()
    {
        if (Player == null || Player.Data.Role is not ScavengerRole) return;
        if (AmongUsClient.Instance.GameState != InnerNet.InnerNetClient.GameStates.Started) return;
        if (!Player.AmOwner) return;

        if (!GameStarted && Player.killTimer > 0f) GameStarted = true;

        // scavenge mode starts once kill timer reaches 0
        if (Player.killTimer <= 0f && !Scavenging && GameStarted && !Player.HasDied())
        {
            // Logger<TownOfUsPlugin>.Message($"Scavenge Begin");
            Scavenging = true;
            TimeRemaining = OptionGroupSingleton<ScavengerOptions>.Instance.ScavengeDuration;

            Target = Player.GetClosestLivingPlayer(false, float.MaxValue, predicate: x => !x.HasModifier<FirstDeadShield>())!;

            Target.AddModifier<ScavengerArrowModifier>(Player, TownOfUsColors.Impostor);
        }

        if (TimeRemaining > 0)
            TimeRemaining -= Time.deltaTime;

        if ((TimeRemaining <= 0 || MeetingHud.Instance || Player.HasDied()) && Scavenging)
        {
            Clear();

            // Logger<TownOfUsPlugin>.Message($"Scavenge End");
            Player.SetKillTimer(PlayerControl.LocalPlayer.GetKillCooldown());
        }
    }

    [HideFromIl2Cpp]
    public StringBuilder SetTabText()
    {
        var stringB = ITownOfUsRole.SetNewTabText(this);

        if (Target != null && Scavenging)
        {
            stringB.Append("\n<b>Scavenge Time:</b> ");
            stringB.Append(CultureInfo.InvariantCulture, $"{Color.white.ToTextColor()}{TimeRemaining.ToString("0", CultureInfo.InvariantCulture)}</color>");
            stringB.Append("\n\n<b>Current Target:</b> ");
            stringB.Append(CultureInfo.InvariantCulture, $"{Color.white.ToTextColor()}{Target.Data.PlayerName}</color>");
        }

        return stringB;
    }

    public void Clear()
    {
        var players = ModifierUtils.GetPlayersWithModifier<ScavengerArrowModifier>();

        foreach (var player in players)
        {
            player.RemoveModifier<ScavengerArrowModifier>();
        }

        Scavenging = false;
        TimeRemaining = 0;
        Target = null;
    }

    public void OnPlayerKilled(PlayerControl victim)
    {
        if (!Player.AmOwner) return;

        if (victim == Target)
        {
            // extend scavenge duration
            TimeRemaining += OptionGroupSingleton<ScavengerOptions>.Instance.ScavengeIncreaseDuration;

            // set kill timer
            Player.SetKillTimer(OptionGroupSingleton<ScavengerOptions>.Instance.ScavengeCorrectKillCooldown);

            // get new target
            Target = Player.GetClosestLivingPlayer(false, float.MaxValue)!;

            // update arrow to point to new target
            Target.AddModifier<ScavengerArrowModifier>(Player, TownOfUsColors.Impostor);
        }
        else
        {
            // set kill timer
            Player.SetKillTimer(PlayerControl.LocalPlayer.GetKillCooldown() * OptionGroupSingleton<ScavengerOptions>.Instance.ScavengeIncorrectKillCooldown);

            // clear arrows
            Clear();
        }
    }
    public string GetAdvancedDescription()
    {
        return $"The Scavenger is an Impostor Killing role that gets new targets after every kill and when the round starts. "
            + "If they kill their target, they get a reduced kill cooldown, but if they don't, their cooldown is increased significantly. "
            + MiscUtils.AppendOptionsText(GetType());
    }

    [HideFromIl2Cpp]
    public List<CustomButtonWikiDescription> Abilities { get; } = [];
}

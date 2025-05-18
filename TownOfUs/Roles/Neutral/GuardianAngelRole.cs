using System.Text;
using AmongUs.GameOptions;
using Il2CppInterop.Runtime.Attributes;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Roles;
using Reactor.Networking.Attributes;
using Reactor.Utilities;
using TownOfUs.Modifiers;
using TownOfUs.Modifiers.Game.Alliance;
using TownOfUs.Modifiers.Neutral;
using TownOfUs.Modules.Wiki;
using TownOfUs.Options.Roles.Neutral;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfUs.Roles.Neutral;

public sealed class GuardianAngelTouRole(IntPtr cppPtr) : NeutralRole(cppPtr), ITownOfUsRole, IWikiDiscoverable, IDoomable, IAssignableTargets
{
    public string RoleName => "Guardian Angel";
    public string RoleDescription => TargetString();
    public string RoleLongDescription => TargetString();
    public Color RoleColor => TownOfUsColors.GuardianAngel;
    public ModdedRoleTeams Team => ModdedRoleTeams.Custom;
    public RoleAlignment RoleAlignment => RoleAlignment.NeutralBenign;
    public DoomableType DoomHintType => DoomableType.Protective;
    public CustomRoleConfiguration Configuration => new(this)
    {
        Icon = TouRoleIcons.GuardianAngel,
        IntroSound = TouAudio.GuardianAngelSound,
        GhostRole = (RoleTypes)RoleId.Get<NeutralGhostRole>(),
    };

    public PlayerControl? Target { get; set; }

    public bool SetupIntroTeam(IntroCutscene instance, ref Il2CppSystem.Collections.Generic.List<PlayerControl> yourTeam)
    {
        if (Player != PlayerControl.LocalPlayer)
            return true;

        var gaTeam = new Il2CppSystem.Collections.Generic.List<PlayerControl>();

        gaTeam.Add(PlayerControl.LocalPlayer);
        gaTeam.Add(Target!);

        yourTeam = gaTeam;

        return true;
    }

    public override bool DidWin(GameOverReason gameOverReason)
    {
        var target = ModifierUtils.GetPlayersWithModifier<GuardianAngelTargetModifier>().FirstOrDefault();
        return target?.Data.Role.DidWin(gameOverReason) == true;
    }

    public static bool GASeesRoleVisibilityFlag(PlayerControl player)
    {
        var gaKnowsTargetRole = OptionGroupSingleton<GuardianAngelOptions>.Instance.GAKnowsTargetRole &&
            PlayerControl.LocalPlayer.IsRole<GuardianAngelTouRole>() &&
            PlayerControl.LocalPlayer.GetRole<GuardianAngelTouRole>()!.Target == player;

        return gaKnowsTargetRole;
    }
    public static bool GATargetSeesVisibilityFlag(PlayerControl player)
    {
        var gaTargetKnows = OptionGroupSingleton<GuardianAngelOptions>.Instance.GATargetKnows &&
            player.HasModifier<GuardianAngelTargetModifier>();

        var gaKnowsTargetRole = PlayerControl.LocalPlayer.IsRole<GuardianAngelTouRole>() &&
            PlayerControl.LocalPlayer.GetRole<GuardianAngelTouRole>()!.Target == player;

        return gaTargetKnows || gaKnowsTargetRole;
    }

    public void CheckTargetDeath(PlayerControl source, PlayerControl victim)
    {
        if (Player.HasDied()) return;

        // Logger<TownOfUsPlugin>.Error($"OnPlayerDeath '{victim.Data.PlayerName}'");
        if (Target == null || victim == Target)
        {
            var roleType = OptionGroupSingleton<GuardianAngelOptions>.Instance.OnTargetDeath switch
            {
                BecomeOptions.Crew => (ushort)RoleTypes.Crewmate,
                BecomeOptions.Jester => RoleId.Get<JesterRole>(),
                BecomeOptions.Survivor => RoleId.Get<SurvivorRole>(),
                BecomeOptions.Amnesiac => RoleId.Get<AmnesiacRole>(),
                BecomeOptions.Mercenary => RoleId.Get<MercenaryRole>(),
                _ => (ushort)RoleTypes.Crewmate,
            };

            // Logger<TownOfUsPlugin>.Error($"OnPlayerDeath - ChangeRole: '{roleType}'");
            Player.ChangeRole(roleType);

            if ((roleType == RoleId.Get<JesterRole>() && OptionGroupSingleton<JesterOptions>.Instance.ScatterOn) ||
                (roleType == RoleId.Get<SurvivorRole>() && OptionGroupSingleton<SurvivorOptions>.Instance.ScatterOn))
            {
                Player.GetModifier<ScatterModifier>()?.OnGameStart();
            }
        }
    }

    private string TargetString()
    {
        if (!Target)
        {
            return "Protect Your Target With Your Life!";
        }

        return $"Protect {Target?.Data.PlayerName} With Your Life!";
    }

    public void AssignTargets()
    {
        // Logger<TownOfUsPlugin>.Error($"SelectGATargets");
        var evilTargetPercent = (int)OptionGroupSingleton<GuardianAngelOptions>.Instance.EvilTargetPercent;

        var gas = PlayerControl.AllPlayerControls.ToArray()
            .Where(x => x.IsRole<GuardianAngelTouRole>() && !x.HasDied());

        foreach (var ga in gas)
        {
            var filtered = PlayerControl.AllPlayerControls.ToArray()
                .Where(x => !x.IsRole<GuardianAngelTouRole>() && !x.HasDied() &&
                    !x.HasModifier<PlayerTargetModifier>() && !x.HasModifier<LoverModifier>()).ToList();

            if (evilTargetPercent > 0f)
            {
                System.Random rnd = new();
                var chance = rnd.Next(0, 100);

                if (chance < evilTargetPercent)
                {
                    filtered = [.. filtered.Where(x => x.IsImpostor() || x.Is(RoleAlignment.NeutralKilling) || (x.Is(RoleAlignment.NeutralEvil) && OptionGroupSingleton<GuardianAngelOptions>.Instance.TargetNeutEvils))];
                }
            }
            else
            {
                filtered = [.. filtered.Where(x => x.Is(ModdedRoleTeams.Crewmate))];
            }  
            if (!OptionGroupSingleton<GuardianAngelOptions>.Instance.TargetNeutEvils) filtered = [.. filtered.Where(x => !x.Is(RoleAlignment.NeutralEvil))];

            System.Random rndIndex = new();
            var randomTarget = filtered[rndIndex.Next(0, filtered.Count)];

            // Logger<TownOfUsPlugin>.Info($"Setting GA Target: {randomTarget.Data.PlayerName}");
            RpcSetGATarget(ga, randomTarget);
        }
    }

    [MethodRpc((uint)TownOfUsRpc.SetGATarget, SendImmediately = true)]
    public static void RpcSetGATarget(PlayerControl player, PlayerControl target)
    {
        if (player.Data.Role is not GuardianAngelTouRole)
        {
            Logger<TownOfUsPlugin>.Error("RpcSetGATarget - Invalid guardian angel");
            return;
        }

        if (target == null) return;

        var role = player.GetRole<GuardianAngelTouRole>();

        if (role == null) return;

        // Logger<TownOfUsPlugin>.Message($"RpcSetGATarget - Target: '{target.Data.PlayerName}'");
        role.Target = target;

        target.AddModifier<GuardianAngelTargetModifier>(player.PlayerId);
    }
    [HideFromIl2Cpp]
    public StringBuilder SetTabText()
    {
        return ITownOfUsRole.SetNewTabText(this);
    }

    public string GetAdvancedDescription()
    {
        return $"The Guardian Angel is a Neutral Benign that needs to protect their target (signified by <color=#B3FFFFFF>★</color>) from getting killed/ejected." + MiscUtils.AppendOptionsText(GetType());
    }

    [HideFromIl2Cpp]
    public List<CustomButtonWikiDescription> Abilities { get; } = [
        new("Protect",
            "Protect your target from getting killed.",
            TouNeutAssets.ProtectSprite)    
    ];
}

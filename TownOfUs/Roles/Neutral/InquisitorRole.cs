using System.Globalization;
using System.Text;
using AmongUs.GameOptions;
using Il2CppInterop.Runtime.Attributes;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using Reactor.Networking.Attributes;
using Reactor.Utilities;
using Reactor.Utilities.Extensions;
using TownOfUs.Modifiers.Neutral;
using TownOfUs.Modules.Wiki;
using TownOfUs.Options.Roles.Neutral;
using TownOfUs.Patches.Stubs;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfUs.Roles.Neutral;

public sealed class InquisitorRole(IntPtr cppPtr) : NeutralRole(cppPtr), ITownOfUsRole, IWikiDiscoverable, IDoomable, IAssignableTargets
{
    public string RoleName => "Inquisitor";
    public string RoleDescription => "Vanquish The Heretics!";
    public string RoleLongDescription => "Vanquish your Heretics or get them killed.\nYou will win after every heretic dies.\nIf they're all dead after a meeting ends, you'll leave & win.";
    public Color RoleColor => TownOfUsColors.Inquisitor;
    public bool CanVanquish { get; set; } = true;
    public ModdedRoleTeams Team => ModdedRoleTeams.Custom;
    public RoleAlignment RoleAlignment => RoleAlignment.NeutralEvil;
    public DoomableType DoomHintType => DoomableType.Hunter;
    public CustomRoleConfiguration Configuration => new(this)
    {
        IntroSound = CustomRoleUtils.GetIntroSound(RoleTypes.Shapeshifter),
        Icon = TouRoleIcons.Inquisitor,
        MaxRoleCount = 1,
        GhostRole = (RoleTypes)RoleId.Get<NeutralGhostRole>(),
    };
    public int Priority { get; set; } = 5;

    public List<PlayerControl> Targets { get; set; } = [];
    public List<RoleBehaviour> TargetRoles { get; set; } = [];
    public bool TargetsDead { get; set; }

    [HideFromIl2Cpp]
    public List<byte> Voters { get; set; } = [];

    public override void Initialize(PlayerControl player)
    {
        RoleStubs.RoleBehaviourInitialize(this, player);
        CanVanquish = true;

        // if Inuquisitor was revived
        if (Targets.Count == 0)
        {
            Targets = ModifierUtils.GetPlayersWithModifier<InquisitorHereticModifier>([HideFromIl2Cpp] (x) => x.OwnerId == Player.PlayerId).ToList();
            TargetRoles = ModifierUtils.GetActiveModifiers<InquisitorHereticModifier>().Select(x => x.TargetRole).OrderBy(x => x.NiceName).ToList();
        }
    }
    public override void OnMeetingStart()
    {
        RoleStubs.RoleBehaviourOnMeetingStart(this);

        if (Player.AmOwner)
        {
            GenerateReport();
        }
    }
    private void GenerateReport()
    {
        Logger<TownOfUsPlugin>.Info($"Generating Inquisitor report");

        var reportBuilder = new StringBuilder();

        if (Player == null) return;

        foreach (var player in GameData.Instance.AllPlayers.ToArray().Where(x => !x.Object.HasDied() && x.Object.HasModifier<InquisitorInquiredModifier>()))
        {
            if (player.Object.HasModifier<InquisitorHereticModifier>())
            {
                reportBuilder.AppendLine(TownOfUsPlugin.Culture, $"Your inquiry reveals that {player.PlayerName} is a heretic!\n");
                var roles = TargetRoles;
                var lastRole = roles[roles.Count - 1];
                roles.Remove(roles[roles.Count - 1]);

                if (roles.Count != 0)
                {
                    reportBuilder.Append(TownOfUsPlugin.Culture, $"(");
                    foreach (var role2 in roles)
                    {
                        reportBuilder.Append(TownOfUsPlugin.Culture, $"{role2.NiceName}, ");
                    }
                    reportBuilder = reportBuilder.Remove(reportBuilder.Length - 2, 2);
                    reportBuilder.Append(TownOfUsPlugin.Culture, $" or {lastRole.NiceName})");
                }
            }
            else
                reportBuilder.AppendLine(TownOfUsPlugin.Culture, $"Your inquiry reveals that {player.PlayerName} is not a heretic!");

            player.Object.RemoveModifier<InquisitorInquiredModifier>();
        }

        var report = reportBuilder.ToString();

        if (HudManager.Instance && report.Length > 0)
        {
            var title = $"<color=#{TownOfUsColors.Inquisitor.ToHtmlStringRGBA()}>Inquisitor Report</color>";
            MiscUtils.AddFakeChat(Player.Data, title, report, false, true);
        }
    }

    public override bool CanUse(IUsable usable)
    {
        if (!GameManager.Instance.LogicUsables.CanUse(usable, Player))
        {
            return false;
        }
        Console console = usable.TryCast<Console>()!;
        return (console == null) || console.AllowImpostor;
    }

    public override bool DidWin(GameOverReason gameOverReason)
    {
        return TargetsDead || WinConditionMet();
    }
    public bool WinConditionMet()
    {
        if (Player.HasDied()) return false;
        if (!CanVanquish) return false;
        if (!TargetsDead) return false;

        var result = Helpers.GetAlivePlayers().Count <= 2 && MiscUtils.KillersAliveCount == 1;
        return result;
    }

    public void CheckTargetDeath(PlayerControl exiled)
    {
        if (Player.HasDied()) return;
        if (Targets.Count == 0) return;

        if (Targets.All(x => x.HasDied()))
        {
            // Logger<TownOfUsPlugin>.Error($"CheckTargetEjection - exiled: {exiled.Data.PlayerName}");
            RpcInquisitorWin(Player);
        }
    }

    public void AssignTargets()
    {
        var inquis = PlayerControl.AllPlayerControls.ToArray()
            .Where(x => x.IsRole<InquisitorRole>() && !x.HasDied());

        foreach (var exe in inquis)
        {
            var required = (int)OptionGroupSingleton<InquisitorOptions>.Instance.AmountOfHeretics;
            var players = PlayerControl.AllPlayerControls.ToArray().ToList();
            players.Shuffle();
            players.Shuffle();
            players.RemoveAll(x => x.Data.Role is InquisitorRole);
            players.Shuffle();

            var neut = players.Any(x => x.IsNeutral()) ? players.FirstOrDefault(x => x.IsNeutral()) : players.Random();
            players.Remove(neut);
            players.Shuffle();

            var imp = players.Any(x => x.IsImpostor()) ? players.FirstOrDefault(x => x.IsImpostor()) : players.Random();
            players.Remove(imp);
            players.Shuffle();

            var crew = players.Any(x => x.IsCrewmate()) ? players.FirstOrDefault(x => x.IsCrewmate()) : players.Random();
            players.Remove(crew);
            players.Shuffle();

            List<PlayerControl> filtered = [neut, imp, crew];
            var other = players.Random();
            if (required is 4 or 5 && players.Count >= 1 && other != null)
            {
                filtered.Add(other);
                players.Remove(other);
            }
            players.Shuffle();
            other = players.Random();
            if (required is 5 && players.Count >= 1 && other != null) filtered.Add(other);

            if (filtered.Count > 0)
            {
                filtered = filtered.OrderBy(x => x.Data.Role.NiceName).ToList();
                foreach (var player in filtered)
                {
                    players.Remove(player);
                    RpcAddInquisTarget(exe, player);
                }
            }
        }
    }

    [MethodRpc((uint)TownOfUsRpc.AddInquisTarget, SendImmediately = true)]
    public static void RpcAddInquisTarget(PlayerControl player, PlayerControl target)
    {
        if (player.Data.Role is not InquisitorRole)
        {
            Logger<TownOfUsPlugin>.Error("RpcAddInquisTarget - Invalid Inquisitor");
            return;
        }

        var role = player.GetRole<InquisitorRole>();

        if (role == null) return;
        if (target == null) return;

        role.Targets.Add(target);
        role.TargetRoles.Add(target.Data.Role);
        target.AddModifier<InquisitorHereticModifier>(player.PlayerId);
    }

    [MethodRpc((uint)TownOfUsRpc.InquisitorWin, SendImmediately = true)]
    public static void RpcInquisitorWin(PlayerControl player)
    {
        if (player.Data.Role is not InquisitorRole)
        {
            Logger<TownOfUsPlugin>.Error("RpcInquisitorWin - Invalid Inquisitor");
            return;
        }

        var exe = player.GetRole<InquisitorRole>();
        exe!.TargetsDead = true;
    }
    [HideFromIl2Cpp]
    public StringBuilder SetTabText()
    {
        var stringB = ITownOfUsRole.SetNewTabText(this);
        stringB.AppendLine(CultureInfo.InvariantCulture, $"<b>The roles of your Heretics:</b>");
        foreach (var role in TargetRoles)
        {
            var newText = $"{role.TeamColor.ToTextColor()}{role.NiceName}";
            stringB.AppendLine(CultureInfo.InvariantCulture, $"{newText}");
        }

        return stringB;
    }

    public string GetAdvancedDescription()
    {
        return $"The Inquisitor is a Neutral Evil role that wins by either vanquishing or passively letting their targets (Heretics) die. The only information provided is their roles, and it's up to the Inquisitor to identify those players and get them killed by any means neccesary." + MiscUtils.AppendOptionsText(GetType());
    }
}

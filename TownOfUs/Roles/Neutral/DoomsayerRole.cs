using System.Text;
using AmongUs.GameOptions;
using Il2CppInterop.Runtime.Attributes;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Networking;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using Reactor.Networking.Attributes;
using Reactor.Utilities;
using Reactor.Utilities.Extensions;
using TownOfUs.Modifiers.Crewmate;
using TownOfUs.Modifiers.Neutral;
using TownOfUs.Modules;
using TownOfUs.Modules.Components;
using TownOfUs.Modules.Wiki;
using TownOfUs.Options.Roles.Neutral;
using TownOfUs.Patches.Stubs;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfUs.Roles.Neutral;

public sealed class DoomsayerRole(IntPtr cppPtr) : NeutralRole(cppPtr), ITownOfUsRole, IWikiDiscoverable, IDoomable
{
    public string RoleName => "Doomsayer";
    public string RoleDescription => "Guess People's Roles To Win!";
    public string RoleLongDescription => "Win by guessing player's roles";
    public Color RoleColor => TownOfUsColors.Doomsayer;
    public ModdedRoleTeams Team => ModdedRoleTeams.Custom;
    public RoleAlignment RoleAlignment => RoleAlignment.NeutralEvil;
    public DoomableType DoomHintType => DoomableType.Insight;
    public CustomRoleConfiguration Configuration => new(this)
    {
        IntroSound = CustomRoleUtils.GetIntroSound(RoleTypes.Shapeshifter),
        Icon = TouRoleIcons.Doomsayer,
        GhostRole = (RoleTypes)RoleId.Get<NeutralGhostRole>(),
    };

    public int NumberOfGuesses { get; set; }
    public int IncorrectGuesses { get; set; }
    public bool AllGuessesCorrect { get; set; }

    private MeetingMenu meetingMenu;

    [HideFromIl2Cpp]
    public StringBuilder SetTabText()
    {
        return ITownOfUsRole.SetNewTabText(this);
    }

    public override void Initialize(PlayerControl player)
    {
        RoleStubs.RoleBehaviourInitialize(this, player);

        if (Player.AmOwner)
        {
            meetingMenu = new MeetingMenu(
                this,
                ClickGuess,
                MeetingAbilityType.Click,
                TouAssets.Guess,
                null!,
                IsExempt);
        }
    }

    public override void OnMeetingStart()
    {
        RoleStubs.RoleBehaviourOnMeetingStart(this);

        if (Player.AmOwner)
        {
            meetingMenu.GenButtons(MeetingHud.Instance, Player.AmOwner && !Player.HasDied() && !Player.HasModifier<JailedModifier>());

            NumberOfGuesses = 0;
            IncorrectGuesses = 0;
            AllGuessesCorrect = false;
        }

        GenerateReport();
    }

    public override void OnVotingComplete()
    {
        RoleStubs.RoleBehaviourOnVotingComplete(this);

        if (Player.AmOwner)
        {
            meetingMenu.HideButtons();
        }
    }

    public override void Deinitialize(PlayerControl targetPlayer)
    {
        RoleStubs.RoleBehaviourDeinitialize(this, targetPlayer);

        if (Player.AmOwner)
        {
            meetingMenu?.Dispose();
            meetingMenu = null!;
        }
    }

    private void GenerateReport()
    {
        Logger<TownOfUsPlugin>.Info($"Generating Doomsayer report");

        var reportBuilder = new StringBuilder();

        if (Player == null) return;

        foreach (var player in GameData.Instance.AllPlayers.ToArray().Where(x => !x.Object.HasDied() && x.Object.HasModifier<DoomsayerObservedModifier>()))
        {
            var role = player.Object.Data.Role;
            var doomableRole = role as IDoomable;
            var hintType = DoomableType.Default;
            var cachedMod = player.Object.GetModifiers<BaseModifier>().FirstOrDefault(x => x is ICachedRole) as ICachedRole;
            if (cachedMod != null)
            {
                role = cachedMod.CachedRole;
                doomableRole = role as IDoomable;
            }
            var unguessableMod = player.Object.GetModifiers<BaseModifier>().FirstOrDefault(x => x is IUnguessable) as IUnguessable;
            if (unguessableMod != null)
            {
                role = unguessableMod.AppearAs;
                doomableRole = role as IDoomable;
            }

            if (doomableRole != null)
            {
                hintType = doomableRole.DoomHintType;
            }

            if (hintType == DoomableType.Default) continue;

            switch (hintType)
            {
                case DoomableType.Perception:
                    reportBuilder.AppendLine(TownOfUsPlugin.Culture, $"You observe that {player.PlayerName} has an altered perception of reality\n");
                    break;
                case DoomableType.Insight:
                    reportBuilder.AppendLine(TownOfUsPlugin.Culture, $"You observe that {player.PlayerName} has an insight for private information\n");
                    break;
                case DoomableType.Death:
                    reportBuilder.AppendLine(TownOfUsPlugin.Culture, $"You observe that {player.PlayerName} has an unusual obsession with dead bodies\n");
                    break;
                case DoomableType.Hunter:
                    reportBuilder.AppendLine(TownOfUsPlugin.Culture, $"You observe that {player.PlayerName} is well trained in hunting down prey\n");
                    break;
                case DoomableType.Fearmonger:
                    reportBuilder.AppendLine(TownOfUsPlugin.Culture, $"You observe that {player.PlayerName} spreads fear amonst the group\n");
                    break;
                case DoomableType.Protective:
                    reportBuilder.AppendLine(TownOfUsPlugin.Culture, $"You observe that {player.PlayerName} hides to guard themself or others\n");
                    break;
                case DoomableType.Trickster:
                    reportBuilder.AppendLine(TownOfUsPlugin.Culture, $"You observe that {player.PlayerName} has a trick up their sleeve\n");
                    break;
                case DoomableType.Relentless:
                    reportBuilder.AppendLine(TownOfUsPlugin.Culture, $"You observe that {player.PlayerName} is capable of performing relentless attacks\n");
                    break;
            }
            var roles = MiscUtils.AllRoles.Where(x => x is IDoomable doomRole && doomRole.DoomHintType == hintType && x is not IUnguessable).OrderBy(x => x.NiceName).ToList();
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

            player.Object.RemoveModifier<DoomsayerObservedModifier>();
        }

        var report = reportBuilder.ToString();

        if (HudManager.Instance && report.Length > 0)
        {
            var title = $"<color=#{TownOfUsColors.Doomsayer.ToHtmlStringRGBA()}>Doomsayer Report</color>";
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
        return AllGuessesCorrect;
    }

    public bool WinConditionMet()
    {
        if (Player.HasDied()) return false;
        if (!OptionGroupSingleton<DoomsayerOptions>.Instance.WinEndsGame) return false;

        return AllGuessesCorrect;
    }

    public void ClickGuess(PlayerVoteArea voteArea, MeetingHud meetingHud)
    {
        if (meetingHud.state == MeetingHud.VoteStates.Discussion)
        {
            return;
        }

        var player = GameData.Instance.GetPlayerById(voteArea.TargetPlayerId).Object;

        var shapeMenu = GuesserMenu.Create();
        shapeMenu.Begin(IsRoleValid, ClickRoleHandle);

        void ClickRoleHandle(RoleBehaviour role)
        {
            var realRole = player.Data.Role;

            var cachedMod = player.GetModifiers<BaseModifier>().FirstOrDefault(x => x is ICachedRole) as ICachedRole;
            if (cachedMod != null)
            {
                realRole = cachedMod.CachedRole;
            }

            var pickVictim = role.Role == realRole.Role;
            var victim = pickVictim ? player : Player;

            ClickHandler(victim, voteArea.TargetPlayerId);
        }

        void ClickHandler(PlayerControl victim, byte targetId)
        {
            var opts = OptionGroupSingleton<DoomsayerOptions>.Instance;

            NumberOfGuesses++;
            meetingMenu?.HideSingle(targetId);

            var playersAlive = PlayerControl.AllPlayerControls.ToArray().Count(x => !x.HasDied() && !x.IsJailed() && x != Player);

            if (victim == Player)
            {
                IncorrectGuesses++;
                if (!opts.DoomsayerGuessAllAtOnce)
                {
                    Coroutines.Start(MiscUtils.CoFlash(Color.red));
                    meetingMenu?.HideButtons();
                    shapeMenu.Close();
                    return;
                }
            }
            else if (!opts.DoomsayerGuessAllAtOnce)
            {
                Coroutines.Start(MiscUtils.CoFlash(Color.green));
            }

            if ((NumberOfGuesses < 2 && playersAlive < 3) || (NumberOfGuesses < 3 && playersAlive > 2))
            {
                shapeMenu.Close();
                return;
            }

            if (IncorrectGuesses > 0 && opts.DoomsayerGuessAllAtOnce)
            {
                Coroutines.Start(MiscUtils.CoFlash(Color.red));
            }
            else
            {
                // no incorrect guesses so this should be the target not the Doomsayer
                Player.RpcCustomMurder(victim, createDeadBody: false, teleportMurderer: false);
            }

            meetingMenu?.HideButtons();

            shapeMenu.Close();
        }
    }

    public bool IsExempt(PlayerVoteArea voteArea)
    {
        return voteArea.TargetPlayerId == Player.PlayerId || Player.Data.IsDead || voteArea.AmDead || voteArea.GetPlayer()?.HasModifier<JailedModifier>() == true;
    }

    private static bool IsRoleValid(RoleBehaviour role)
    {
        var unguessableRole = role as IUnguessable;
        if (role.IsDead || role is IGhostRole || (unguessableRole != null && !unguessableRole.IsGuessable))
        {
            return false;
        }

        return true;
    }

    [MethodRpc((uint)TownOfUsRpc.DoomsayerWin, SendImmediately = true)]
    public static void RpcDoomsayerWin(PlayerControl player)
    {
        if (player.Data.Role is not DoomsayerRole)
        {
            Logger<TownOfUsPlugin>.Error("RpcDoomsayerWin - Invalid Doomsayer");
            return;
        }

        var doom = player.GetRole<DoomsayerRole>();
        doom!.AllGuessesCorrect = true;

        if (GameHistory.PlayerStats.TryGetValue(player.PlayerId, out var stats))
        {
            stats.CorrectAssassinKills++;
        }
    }

    public string GetAdvancedDescription()
    {
        return "The Doomsayer is a Neutral Evil role that wins by guessing 3 players' roles in the meeting. They can also observe players to get a hint of what their roles are the following meeting." + MiscUtils.AppendOptionsText(GetType());
    }

    [HideFromIl2Cpp]
    public List<CustomButtonWikiDescription> Abilities { get; } = [
        new("Observe",
            "Observe a player, gaining a hint in the next meeting what their role could be.",
            TouNeutAssets.Observe)
    ];
}

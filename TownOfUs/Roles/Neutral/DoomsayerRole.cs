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
using TownOfUs.Modifiers.Impostor;
using TownOfUs.Modifiers.Neutral;
using TownOfUs.Modules;
using TownOfUs.Modules.Components;
using TownOfUs.Modules.Wiki;
using TownOfUs.Options.Roles.Neutral;
using TownOfUs.Patches.Stubs;
using TownOfUs.Roles.Crewmate;
using TownOfUs.Roles.Impostor;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfUs.Roles.Neutral;

public sealed class DoomsayerRole(IntPtr cppPtr) : NeutralRole(cppPtr), ITownOfUsRole, IWikiDiscoverable
{
    public string RoleName => "Doomsayer";
    public string RoleDescription => "Guess People's Roles To Win!";
    public string RoleLongDescription => "Win by guessing player's roles";
    public Color RoleColor => TownOfUsColors.Doomsayer;
    public ModdedRoleTeams Team => ModdedRoleTeams.Custom;
    public RoleAlignment RoleAlignment => RoleAlignment.NeutralEvil;
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
            meetingMenu!.GenButtons(MeetingHud.Instance, Player.AmOwner && !Player.HasDied() && !Player.HasModifier<JailedModifier>());

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
            meetingMenu!.HideButtons();
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
            if (player.Role is not ICustomRole customRole) continue;

            if (player.Object.HasModifier<TraitorCacheModifier>())
            {
                reportBuilder.AppendLine(TownOfUsPlugin.Culture, $"You observe that {player.PlayerName} has a trick up their sleeve");
                reportBuilder.AppendLine("(Executioner, Jester, Mayor, Politician, Swapper, Traitor, Venerer, Veteran or Plumber)");
            }
            else
            {
                switch (customRole)
                {
                    case AurialRole or ImitatorRole or MysticRole or MorphlingRole or SpyRole or GlitchRole:
                        reportBuilder.AppendLine(TownOfUsPlugin.Culture, $"You observe that {player.PlayerName} has an altered perception of reality");
                        reportBuilder.AppendLine("(Aurial, Imitator, Morphling, Mystic, Spy or Glitch)");
                        break;
                    case BlackmailerRole or DetectiveRole or OracleRole or SnitchRole or DoomsayerRole or TrapperRole or MercenaryRole:
                        reportBuilder.AppendLine(TownOfUsPlugin.Culture, $"You observe that {player.PlayerName} has an insight for private information");
                        reportBuilder.AppendLine("(Blackmailer, Detective, Doomsayer, Oracle, Snitch, Trapper or Mercenary)");
                        break;
                    case AltruistRole or AmnesiacRole or JanitorRole or MediumRole or SoulCollectorRole or UndertakerRole or VampireRole:
                        reportBuilder.AppendLine(TownOfUsPlugin.Culture, $"You observe that {player.PlayerName} has an unusual obsession with dead bodies");
                        reportBuilder.AppendLine("(Altruist, Amnesiac, Janitor, Medium, Soul Collector, Undertaker or Vampire)");
                        break;
                    case HunterRole or InvestigatorRole or LookoutRole or ScavengerRole or TrackerTouRole or WerewolfRole:
                        reportBuilder.AppendLine(TownOfUsPlugin.Culture, $"You observe that {player.PlayerName} is well trained in hunting down prey");
                        reportBuilder.AppendLine("(Hunter, Investigator, Lookout, Scavenger, Swooper, Tracker or Werewolf)");
                        break;
                    case ArsonistRole or HypnotistRole or MinerRole or PlaguebearerRole or PestilenceRole or ProsecutorRole or SeerRole or TransporterRole:
                        reportBuilder.AppendLine(TownOfUsPlugin.Culture, $"You observe that {player.PlayerName} spreads fear amonst the group");
                        reportBuilder.AppendLine("(Arsonist, Hypnotist, Miner, Plaguebearer, Prosecutor, Seer or Transporter)");
                        break;
                    case EngineerTouRole or EscapistRole or GrenadierRole or GuardianAngelTouRole or MedicRole or SurvivorRole or WardenRole or ClericRole:
                        reportBuilder.AppendLine(TownOfUsPlugin.Culture, $"You observe that {player.PlayerName} hides to guard themself or others");
                        reportBuilder.AppendLine("(Engineer, Escapist, Grenadier, Guardian Angel, Medic, Survivor, Warden or Cleric)");
                        break;
                    case ExecutionerRole or JesterRole or MayorRole or PoliticianRole or SwapperRole or TraitorRole or VenererRole or VeteranRole or PlumberRole:
                        reportBuilder.AppendLine(TownOfUsPlugin.Culture, $"You observe that {player.PlayerName} has a trick up their sleeve");
                        reportBuilder.AppendLine("(Executioner, Jester, Mayor, Politician, Swapper, Traitor, Venerer, Veteran or Plumber)");
                        break;
                    case BomberRole or DeputyRole or JuggernautRole or JailorRole or SheriffRole or VigilanteRole or WarlockRole:
                        reportBuilder.AppendLine(TownOfUsPlugin.Culture, $"You observe that {player.PlayerName} is capable of performing relentless attacks");
                        reportBuilder.AppendLine("(Bomber, Deputy, Jailor, Juggernaut, Sheriff, Vigilante, or Warlock)");
                        break;
                }
            }

            player.Object.RemoveModifier<DoomsayerObservedModifier>();
        }

        var report = reportBuilder.ToString();

        if (HudManager.Instance && report.Length > 0)
        {
            var title = $"<color=#{TownOfUsColors.Doomsayer.ToHtmlStringRGBA()}>Doomsayer Report</color>";
            MiscUtils.AddFakeChat(Player.Data, title, report, true, true);
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
            var pickVictim = role.Role == player.Data.Role.Role;

            if (player.IsImpostor())
            {
                if (role.Role == player.Data.Role.Role && !player.HasModifier<TraitorCacheModifier>())
                    pickVictim = true;
                else if (role is TraitorRole && player.HasModifier<TraitorCacheModifier>())
                    pickVictim = true;
                else
                    pickVictim = false;
            }

            var victim = pickVictim ? player : Player!;

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
        return voteArea.TargetPlayerId == Player!.PlayerId || Player.Data.IsDead || voteArea.AmDead || voteArea.GetPlayer()?.HasModifier<JailedModifier>() == true;
    }

    private static bool IsRoleValid(RoleBehaviour role)
    {
        if (role.IsDead || role is IGhostRole || role is PestilenceRole || role is MayorRole)
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

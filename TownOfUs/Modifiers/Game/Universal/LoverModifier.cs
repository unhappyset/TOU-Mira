using System.Collections;
using HarmonyLib;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.GameEnd;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Networking;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using MiraAPI.Utilities.Assets;
using Reactor.Networking.Attributes;
using Reactor.Utilities;
using TownOfUs.GameOver;
using TownOfUs.Modifiers.Neutral;
using TownOfUs.Modules.Wiki;
using TownOfUs.Options.Modifiers;
using TownOfUs.Options.Modifiers.Alliance;
using TownOfUs.Roles;
using TownOfUs.Roles.Neutral;
using TownOfUs.Utilities;

namespace TownOfUs.Modifiers.Game.Alliance;

public sealed class LoverModifier : AllianceGameModifier, IWikiDiscoverable, IAssignableTargets
{
    public override string ModifierName => "Lover";
    public override string Symbol => "♥";
    public override string IntroInfo => LoverString();
    public override float IntroSize => 3f;
    public override bool HideOnUi => false;
    public override LoadableAsset<UnityEngine.Sprite>? ModifierIcon => TouModifierIcons.Lover;
    public override string GetDescription() => LoverString();
    public PlayerControl? OtherLover { get; set; }
    public override int GetAmountPerGame() => 0;
    public override int GetAssignmentChance() => 0;
    public int Priority { get; set; } = 4;
    public List<CustomButtonWikiDescription> Abilities { get; } = [];

    public string GetAdvancedDescription()
    {
        return
            $"As a lover, you can chat with your other lover (signified with <color=#FF66CCFF>♥</color>) during the round, and you can win with your lover if you are both a part of the final 3 players."
               + MiscUtils.AppendOptionsText(GetType());
    }

    public override void OnActivate()
    {
        if (!Player.AmOwner) return;
        HudManager.Instance.Chat.gameObject.SetActive(true);
        if (TutorialManager.InstanceExists)
        {
            Coroutines.Start(SetTutorialTarget(this, Player));
        }
    }
    private static IEnumerator SetTutorialTarget(LoverModifier loverMod, PlayerControl localPlr)
    {
        yield return new UnityEngine.WaitForSeconds(0.01f);
        var impTargetPercent = (int)OptionGroupSingleton<LoversOptions>.Instance.LovingImpPercent;

        var players = PlayerControl.AllPlayerControls.ToArray()
            .Where(x => !x.HasDied() && !x.HasModifier<ExecutionerTargetModifier>() && x.Data.Role is not InquisitorRole).ToList();
        players.Shuffle();

        players.Remove(localPlr);

        var crewmates = new List<PlayerControl>();
        var impostors = new List<PlayerControl>();

        foreach (var player in players.SelectMany(_ => players))
        {
            if (player.IsImpostor() || (player.Is(RoleAlignment.NeutralKilling) && OptionGroupSingleton<LoversOptions>.Instance.NeutralLovers))
                impostors.Add(player);
            else if (player.Is(ModdedRoleTeams.Crewmate) || ((player.Is(RoleAlignment.NeutralBenign) || player.Is(RoleAlignment.NeutralEvil)) && OptionGroupSingleton<LoversOptions>.Instance.NeutralLovers))
                crewmates.Add(player);
        }

        if (localPlr.IsImpostor() && !OptionGroupSingleton<LoversOptions>.Instance.ImpLovers)
        {
            impostors = impostors.Where(player => !player.IsImpostor()).ToList();
            players = players.Where(player => !player.IsImpostor()).ToList();
        }

        if (impTargetPercent > 0f && impostors.Count != 0)
        {
            Random rnd = new();
            var chance2 = rnd.Next(0, 100);

            if (chance2 < impTargetPercent)
            {
                players = impostors;
            }
        }
        else
        {
            players = crewmates;
        }

        Random rndIndex = new();
        var randomTarget = players[rndIndex.Next(0, players.Count)];
        
        var sourceModifier = randomTarget.AddModifier<LoverModifier>();
        yield return new UnityEngine.WaitForSeconds(0.01f);
        sourceModifier!.OtherLover = localPlr;
        loverMod!.OtherLover = randomTarget;
    }
    public override void OnDeactivate()
    {
        if (!Player.AmOwner) return;
        HudManager.Instance.Chat.gameObject.SetActive(false);
        if (TutorialManager.InstanceExists)
        {
            var players = ModifierUtils.GetPlayersWithModifier<LoverModifier>().ToList();
            players.Do(x => x.RpcRemoveModifier<LoverModifier>());
        }
    }

    public override bool? DidWin(GameOverReason reason)
    {
        return reason == CustomGameOver.GameOverReason<LoverGameOver>() ? true : null;
    }

    public static bool WinConditionMet(LoverModifier[] lovers)
    {
        var bothLoversAlive = Helpers.GetAlivePlayers().Count(x => x.HasModifier<LoverModifier>()) >= 2;

        return Helpers.GetAlivePlayers().Count <= 3 && lovers.Length == 2 && bothLoversAlive;
    }

    public void OnRoundStart()
    {
        if (!Player.AmOwner) return;
        HudManager.Instance.Chat.SetVisible(true);
    }

    public string LoverString()
    {
        return !OtherLover ? "You are in love with nobody" : $"You are in love with {OtherLover!.Data.PlayerName}";
    }

    public void KillOther(bool isHidden = false)
    {
        if (!Player || !OtherLover || OtherLover!.Data.IsDead)
        {
            Logger<TownOfUsPlugin>.Error("Invalid Lover");
            return;
        }
        if (MeetingHud.Instance != null) isHidden = true;

        if (!OtherLover.HasModifier<InvulnerabilityModifier>())
        {
            var murderResultFlags = MurderResultFlags.Succeeded;

            var beforeMurderEvent = new BeforeMurderEvent(OtherLover!, OtherLover!);
            MiraEventManager.InvokeEvent(beforeMurderEvent);

            if (beforeMurderEvent.IsCancelled)
            {
                murderResultFlags = MurderResultFlags.FailedError;
            }

            var murderResultFlags2 = MurderResultFlags.DecisionByHost | murderResultFlags;

            OtherLover!.CustomMurder(OtherLover!, murderResultFlags2, true, !isHidden, true, !isHidden, true);
        }
    }

    public PlayerControl? GetOtherLover()
    {
        return OtherLover;
    }

    public void AssignTargets()
    {
        foreach (var lover in PlayerControl.AllPlayerControls.ToArray().Where(x => x.HasModifier<LoverModifier>()).ToList())
        {
            lover.RemoveModifier<LoverModifier>();
        }

        Random rnd = new();
        var chance = rnd.Next(0, 100);

        if (chance <= (int)OptionGroupSingleton<AllianceModifierOptions>.Instance.LoversChance)
        {
            var impTargetPercent = (int)OptionGroupSingleton<LoversOptions>.Instance.LovingImpPercent;

            var players = PlayerControl.AllPlayerControls.ToArray()
                .Where(x => !x.HasDied() && !x.HasModifier<ExecutionerTargetModifier>() && x.Data.Role is not InquisitorRole).ToList();
            players.Shuffle();

            Random rndIndex1 = new();
            var randomLover = players[rndIndex1.Next(0, players.Count)];
            players.Remove(randomLover);

            var crewmates = new List<PlayerControl>();
            var impostors = new List<PlayerControl>();

            foreach (var player in players.SelectMany(_ => players))
            {
                if (player.IsImpostor() || (player.Is(RoleAlignment.NeutralKilling) && OptionGroupSingleton<LoversOptions>.Instance.NeutralLovers))
                    impostors.Add(player);
                else if (player.Is(ModdedRoleTeams.Crewmate) || ((player.Is(RoleAlignment.NeutralBenign) || player.Is(RoleAlignment.NeutralEvil)) && OptionGroupSingleton<LoversOptions>.Instance.NeutralLovers))
                    crewmates.Add(player);
            }

            if (crewmates.Count < 2 || impostors.Count < 1)
            {
                Logger<TownOfUsPlugin>.Error("Not enough players to select lovers");
                return;
            }

            if (randomLover.IsImpostor() && !OptionGroupSingleton<LoversOptions>.Instance.ImpLovers)
            {
                impostors = impostors.Where(player => !player.IsImpostor()).ToList();
                players = players.Where(player => !player.IsImpostor()).ToList();
            }

            if (impTargetPercent > 0f)
            {
                Random rnd2 = new();
                var chance2 = rnd2.Next(0, 100);

                if (chance2 < impTargetPercent)
                {
                    players = impostors;
                }
            }
            else
            {
                players = crewmates;
            }

            Random rndIndex = new();
            var randomTarget = players[rndIndex.Next(0, players.Count)];
            RpcSetOtherLover(randomLover, randomTarget);
        }
    }

    [MethodRpc((uint)TownOfUsRpc.SetOtherLover, SendImmediately = true)]
    private static void RpcSetOtherLover(PlayerControl player, PlayerControl target)
    {
        if (PlayerControl.AllPlayerControls.ToArray().Where(x => x.HasModifier<LoverModifier>()).ToList().Count > 0)
        {
            Logger<TownOfUsPlugin>.Error("RpcSetOtherLover - Lovers Already Spawned!");
            return;
        }

        var targetModifier = target.AddModifier<LoverModifier>();
        var sourceModifier = player.AddModifier<LoverModifier>();
        targetModifier!.OtherLover = player;
        sourceModifier!.OtherLover = target;
    }
}

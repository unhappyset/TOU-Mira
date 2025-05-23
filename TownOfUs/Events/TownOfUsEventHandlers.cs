using System.Collections;
using HarmonyLib;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.Events.Vanilla.Player;
using MiraAPI.Events.Vanilla.Usables;
using MiraAPI.Utilities;
using TownOfUs.Modules;
using TownOfUs.Patches;
using TownOfUs.Roles;
using TownOfUs.Roles.Crewmate;
using TownOfUs.Utilities;
using UnityEngine;
using Reactor.Utilities.Extensions;
using PowerTools;
using Reactor.Utilities;
using Object = UnityEngine.Object;
using MiraAPI.Modifiers;
using TownOfUs.Modifiers.Game.Universal;
using TownOfUs.Modules.Anims;
using MiraAPI.Hud;
using MiraAPI.Modifiers.Types;

namespace TownOfUs.Events;

public static class TownOfUsEventHandlers
{
    [RegisterEvent]
    public static void ClearBodiesAndResetPlayersEventHandler(RoundStartEvent @event)
    {
        GameObject.FindObjectsOfType<DeadBody>().ToList().ForEach(x => x.gameObject.Destroy());

        foreach (var player in PlayerControl.AllPlayerControls)
        {
            player.MyPhysics.ResetAnimState();
            player.MyPhysics.ResetMoveState();
        }
    }

    [RegisterEvent]
    public static void AfterMurderEventHandler(AfterMurderEvent murderEvent)
    {
        var source = murderEvent.Source;
        var target = murderEvent.Target;

        GameHistory.AddMurder(source, target);
        
        if (target.AmOwner)
        {
            HudManager.Instance.SetHudActive(target, target.Data.Role, false);
            HudManager.Instance.SetHudActive(target, target.Data.Role, true);
        }

        if (target.Data.Role is IAnimated animated)
            {
                animated.IsVisible = false;
                animated.SetVisible();
            }
        foreach (var button in CustomButtonManager.Buttons.Where(x => x.Enabled(target.Data.Role)).OfType<IAnimated>())
        {
            button.IsVisible = false;
            button.SetVisible();
        }
        foreach (var modifier in target.GetModifiers<GameModifier>().Where(x => x is IAnimated))
        {
            var animatedMod = modifier as IAnimated;
            if (animatedMod != null)
            {
                animatedMod.IsVisible = false;
                animatedMod.SetVisible();
            }
        }

        // here we're adding support for kills during a meeting
        if (MeetingHud.Instance)
        {
            HandleMeetingMurder(MeetingHud.Instance, source, target);
        }
        else
        {
            var body = GameObject.FindObjectsOfType<DeadBody>().FirstOrDefault(x => x.ParentId == target.PlayerId);

            if (target.HasModifier<MiniModifier>() && body != null)
            {
                body.transform.localScale *= 0.7f;
            }

            if (target.HasModifier<GiantModifier>() && body != null)
            {
                body.transform.localScale /= 0.7f;
            }

            if (target.AmOwner)
            {
                if (Minigame.Instance != null)
                {
                    Minigame.Instance.Close();
                    Minigame.Instance.Close();
                }

                if (MapBehaviour.Instance != null)
                {
                    MapBehaviour.Instance.Close();
                    MapBehaviour.Instance.Close();
                }
            }
        }
    }

    [RegisterEvent]
    public static void PlayerCanUseEventHandler(PlayerCanUseEvent @event)
    {
        if (!PlayerControl.LocalPlayer || !PlayerControl.LocalPlayer.Data || !PlayerControl.LocalPlayer.Data.Role)
        {
            return;
        }

        // Prevent last 2 players from venting
        if (@event.IsVent)
        {
            var aliveCount = PlayerControl.AllPlayerControls.ToArray().Count(x => !x.HasDied());

            if (PlayerControl.LocalPlayer.inVent && aliveCount <= 2 && PlayerControl.LocalPlayer.Data.Role is not IGhostRole)
            {
                PlayerControl.LocalPlayer.MyPhysics.RpcExitVent(Vent.currentVent.Id);
                PlayerControl.LocalPlayer.MyPhysics.ExitAllVents();
            }

            if (aliveCount <= 2)
                @event.Cancel();
        }
    }

    [RegisterEvent]
    public static void GameEndEventHandler(GameEndEvent @event)
    {
        EndGamePatches.BuildEndGameSummary(@event.EndGameManager);
    }
    
    [RegisterEvent]
    public static void PlayerLeaveEventHandler(PlayerLeaveEvent @event)
    {
        if (MeetingHud.Instance == null) return;

        var player = @event.ClientData.Character;

        if (!player) return;

        var pva = MeetingHud.Instance.playerStates.First(x => x.TargetPlayerId == player.PlayerId);
        
        if (!pva) return;

        pva.AmDead = true;
        pva.Overlay.gameObject.SetActive(true);
        pva.Overlay.color = Color.white;
        pva.XMark.gameObject.SetActive(false);
        pva.XMark.transform.localScale = Vector3.one;

        MeetingMenu.Instances.Do(x => x.HideSingle(player.PlayerId));
    }

    private static IEnumerator CoAnimateDeath(PlayerVoteArea voteArea)
    {
        var animDic = new Dictionary<AnimationClip, AnimationClip>
        {
            {TouAssets.MeetingDeathBloodAnim1.LoadAsset(), TouAssets.MeetingDeathAnim1.LoadAsset()},
            {TouAssets.MeetingDeathBloodAnim2.LoadAsset(), TouAssets.MeetingDeathAnim2.LoadAsset()},
            {TouAssets.MeetingDeathBloodAnim3.LoadAsset(), TouAssets.MeetingDeathAnim3.LoadAsset()}
        };
        var trueAnim = animDic.Random();
        var animation = Object.Instantiate(TouAssets.MeetingDeathPrefab.LoadAsset(), voteArea.transform);
        animation.transform.localPosition = new Vector3(-0.8f, 0, 0);
        animation.transform.localScale = new Vector3(0.375f, 0.375f, 1f);
        animation.gameObject.layer = animation.transform.GetChild(0).gameObject.layer = voteArea.gameObject.layer;

        var animationRend = animation.GetComponent<SpriteRenderer>();
        animationRend.material = voteArea.PlayerIcon.cosmetics.currentBodySprite.BodySprite.material;

        voteArea.Overlay.gameObject.SetActive(false);
        animation.gameObject.SetActive(false);

        Coroutines.Start(MiscUtils.CoFlash(Palette.ImpostorRed, 0.5f, 0.15f));
        yield return new WaitForSeconds(UnityEngine.Random.RandomRange(0.4f, 1.1f));

        voteArea.PlayerIcon.gameObject.SetActive(false);
        animation.gameObject.SetActive(true);
        var bodysAnim = animation.GetComponent<SpriteAnim>();
        
        var bloodAnim = animation.transform.GetChild(0).GetComponent<SpriteAnim>();
        
            bloodAnim.Play(trueAnim.Key);
            bodysAnim.Play(trueAnim.Value);

            bodysAnim.SetSpeed(1.05f);
            bloodAnim.SetSpeed(1.05f);
            var bodyAnimLength = bodysAnim.m_currAnim.length;

        yield return new WaitForSeconds(0.1f);
        SoundManager.Instance.PlaySound(voteArea.GetPlayer()!.KillSfx, false);

        yield return new WaitForSeconds(bodyAnimLength - 0.25f);
        voteArea.Overlay.gameObject.SetActive(true);
        animation.gameObject.Destroy();
        voteArea.XMark.gameObject.SetActive(true);
        SoundManager.Instance.PlaySound(MeetingHud.Instance.MeetingIntro.PlayerDeadSound, false);
        Coroutines.Start(MiscUtils.BetterBloop(voteArea.XMark.transform));
        voteArea.Overlay.gameObject.SetActive(true);
    }

    private static void HandleMeetingMurder(MeetingHud instance, PlayerControl source, PlayerControl target)
    {
        // To handle murders during a meeting
        var targetVoteArea = instance.playerStates.First(x => x.TargetPlayerId == target.PlayerId);

        if (!targetVoteArea)
        {
            return;
        }

        if (targetVoteArea.DidVote)
        {
            targetVoteArea.UnsetVote();
        }

        targetVoteArea.AmDead = true;
        targetVoteArea.Overlay.gameObject.SetActive(true);
        targetVoteArea.Overlay.color = Color.white;
        targetVoteArea.XMark.gameObject.SetActive(false);
        targetVoteArea.XMark.transform.localScale = Vector3.one;

        if (instance.state != MeetingHud.VoteStates.Animating && instance.state != MeetingHud.VoteStates.Discussion)
        {
            targetVoteArea.Overlay.gameObject.SetActive(false);
            if (target.Data.Role is MayorRole) MayorRole.DestroyReveal(targetVoteArea);
            Coroutines.Start(CoAnimateDeath(targetVoteArea));
        }

        // hide meeting menu button for victim
        if (!source.AmOwner && !target.AmOwner)
        {
            MeetingMenu.Instances.Do(x => x.HideSingle(target.PlayerId));
        }
        // hide meeting menu buttons on the victim's screen
        else if (target.AmOwner)
        {
            MeetingMenu.Instances.Do(x => x.HideButtons());
        }

        foreach (var pva in instance.playerStates)
        {
            if (pva.VotedFor != target.PlayerId || pva.AmDead) continue;

            pva.UnsetVote();

            var voteAreaPlayer = MiscUtils.PlayerById(pva.TargetPlayerId);

            if (voteAreaPlayer == null) continue;

            var voteData = voteAreaPlayer.GetVoteData();
            var votes = voteData.Votes.RemoveAll(x => x.Suspect == target.PlayerId);
            voteData.VotesRemaining += votes;

            if (!voteAreaPlayer.AmOwner) continue;

            instance.ClearVote();
        }

        instance.SetDirtyBit(1U);

        if (AmongUsClient.Instance.AmHost)
        { 
            instance.CheckForEndVoting();
        }
    }
}

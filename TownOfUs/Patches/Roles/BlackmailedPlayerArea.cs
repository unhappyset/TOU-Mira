using HarmonyLib;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Utilities;
using TownOfUs.Modifiers.Impostor;
using TownOfUs.Options.Roles.Impostor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace TownOfUs.Patches.Roles;

[HarmonyPatch(typeof(PlayerVoteArea))]
public static class BlackmailedPlayerArea
{
    public static bool shookAlready = true;
    public static SpriteRenderer BmOverlay;

    [HarmonyPostfix]
    [HarmonyPatch(nameof(PlayerVoteArea.SetCosmetics))]
    public static void SetCosmeticsPostfix(PlayerVoteArea __instance, NetworkedPlayerInfo playerInfo)
    {
        var bmMod = playerInfo?.Object?.GetModifier<BlackmailedModifier>();

        if (bmMod == null)
        {
            return;
        }

        var amOwner = playerInfo?.Object?.AmOwner;
        var bmOwns = bmMod.BlackMailerId == PlayerControl.LocalPlayer.PlayerId;
        var targetSeeOnly = OptionGroupSingleton<BlackmailerOptions>.Instance.OnlyTargetSeesBlackmail;

        if (amOwner == true || bmOwns || !targetSeeOnly)
        {
            shookAlready = false;
            var bmIcon = Object.Instantiate(__instance.XMark, __instance.XMark.transform.parent);
            bmIcon.transform.localPosition = new Vector3(-0.804f, -0.212f, -2);
            bmIcon.transform.localScale = new Vector3(0.75f, 0.75f, 0.75f);
            bmIcon.sprite = TouAssets.BlackmailLetterSprite.LoadAsset();
            bmIcon.gameObject.SetActive(true);

            BmOverlay = Object.Instantiate(__instance.XMark, __instance.XMark.transform.parent);
            BmOverlay.transform.localPosition = new Vector3(0, 0, -2);
            BmOverlay.transform.localScale = new Vector3(0.769f, 1, 1);
            BmOverlay.sprite = TouAssets.BlackmailOverlaySprite.LoadAsset();
            BmOverlay.gameObject.SetActive(true);
            __instance.ColorBlindName.gameObject.SetActive(false);
        }
    }

    [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Update))]
    public static class BmHudUpdate
    {
        public static void Postfix(MeetingHud __instance)
        {
            if (__instance.state != MeetingHud.VoteStates.Animating && !shookAlready)
            {
                shookAlready = true;
                __instance.StartCoroutine(Effects.SwayX(BmOverlay.transform));
            }
            var bmOpt = OptionGroupSingleton<BlackmailerOptions>.Instance;
            var maxAliveNeeded = (int)bmOpt.MaxAliveForVoting;
            var targetSeeOnly = bmOpt.OnlyTargetSeesBlackmail;

            if (__instance.state == MeetingHud.VoteStates.NotVoted && (Helpers.GetAlivePlayers().Count > maxAliveNeeded))
            {
                foreach (var player in ModifierUtils.GetPlayersWithModifier<BlackmailedModifier>())
                {
                    var playerState = __instance.playerStates.FirstOrDefault(x => x.TargetPlayerId == player.PlayerId);
                    if (playerState != null && !playerState.DidVote)
                    {
                        playerState.SetVote(252);
                        if (targetSeeOnly)
                        {
                            playerState.Flag.enabled = false;
                        }

                        if (player.AmOwner)
                        {
                            MeetingHud.Instance.Confirm(252);
                        }
                    }
                }
            }
        }
    }
}
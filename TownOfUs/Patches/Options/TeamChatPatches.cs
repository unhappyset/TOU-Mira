using HarmonyLib;
using MiraAPI.GameOptions;
using Reactor.Networking.Attributes;
using Reactor.Utilities.Extensions;
using TMPro;
using TownOfUs.Options;
using TownOfUs.Roles.Neutral;
using TownOfUs.Utilities;
using UnityEngine;
using Object = UnityEngine.Object;

namespace TownOfUs.Patches.Options;

public static class TeamChatPatches
{
    private static TextMeshPro? _teamText;
    public static bool TeamChatActive;
    public static void ToggleTeamChat()
    {
        // WIP
        TeamChatActive = !TeamChatActive;
        if (!TeamChatActive)
        {
            HudManagerPatches.TeamChatButton.transform.Find("Inactive").gameObject.SetActive(true);
        }
        else
        {
            HudManager.Instance.Chat.Toggle();
        }
    }
    [MethodRpc((uint)TownOfUsRpc.SendVampTeamChat, SendImmediately = true)]
    public static void RpcSendVampTeamChat(PlayerControl player, string text)
    {
        if ((PlayerControl.LocalPlayer.Data.Role is VampireRole && player != PlayerControl.LocalPlayer) || (PlayerControl.LocalPlayer.HasDied() && OptionGroupSingleton<GeneralOptions>.Instance.TheDeadKnow))
        {
            MiscUtils.AddTeamChat(player.Data, $"<color=#{TownOfUsColors.Vampire.ToHtmlStringRGBA()}>{player.Data.PlayerName} (Vampire Chat)</color>", text);
        }
    }
    [MethodRpc((uint)TownOfUsRpc.SendImpTeamChat, SendImmediately = true)]
    public static void RpcSendImpTeamChat(PlayerControl player, string text)
    {
        if ((PlayerControl.LocalPlayer.IsImpostor() && player != PlayerControl.LocalPlayer) || (PlayerControl.LocalPlayer.HasDied() && OptionGroupSingleton<GeneralOptions>.Instance.TheDeadKnow))
        {
            MiscUtils.AddTeamChat(player.Data, $"<color=#{TownOfUsColors.ImpSoft.ToHtmlStringRGBA()}>{player.Data.PlayerName} (Impostor Chat)</color>", text);
        }
    }
    /* [HarmonyPatch(typeof(ChatController), nameof(ChatController.LateUpdate))]
    public static class LateUpdatePatch
    {
        public static void Postfix(ChatController __instance)
        {
            if (TeamChatActive)
            {
                var FreeChat = GameObject.Find("FreeChatInputField");
                var typeBg = FreeChat.transform.FindChild("Background");
                var typeText = FreeChat.transform.FindChild("Text");

                typeBg.GetComponent<SpriteRenderer>().color = new Color(0.2f, 0.1f, 0.1f, 0.6f);
                typeBg.GetComponent<ButtonRolloverHandler>().ChangeOutColor(new Color(0.2f, 0.1f, 0.1f, 0.6f));
                typeBg.GetComponent<ButtonRolloverHandler>().OverColor = new Color(0.6f, 0.1f, 0.1f, 1f);
                if (typeText.TryGetComponent<TextMeshPro>(out var txt))
                {
                    txt.color = Color.white;
                    txt.SetFaceColor(Color.white);
                }
            }
        }
    } */
    [HarmonyPatch(typeof(ChatController), nameof(ChatController.Toggle))]
    public static class TogglePatch
    {
        public static void Postfix(ChatController __instance)
        {
            if (__instance.IsOpenOrOpening)
            {
                if (_teamText == null)
                {
                    _teamText = Object.Instantiate(__instance.sendRateMessageText, __instance.sendRateMessageText.transform.parent);
                    _teamText.text = string.Empty;
                    _teamText.color = TownOfUsColors.ImpSoft;
                }
                _teamText.text = string.Empty;
                var ChatScreenContainer = GameObject.Find("ChatScreenContainer");
                // var FreeChat = GameObject.Find("FreeChatInputField");
                var Background = ChatScreenContainer.transform.FindChild("Background");
                // var typeBg = FreeChat.transform.FindChild("Background");
                // var typeText = FreeChat.transform.FindChild("Text");

                if (TeamChatActive)
                {
                    var ogChat = HudManager.Instance.Chat.chatButton;
                    ogChat.transform.Find("Inactive").gameObject.SetActive(true);
                    ogChat.transform.Find("Active").gameObject.SetActive(false);
                    ogChat.transform.Find("Selected").gameObject.SetActive(false);

                    var genOpt = OptionGroupSingleton<GeneralOptions>.Instance;
                    Background.GetComponent<SpriteRenderer>().color = new Color(0.2f, 0.1f, 0.1f, 0.8f);
                    //typeBg.GetComponent<SpriteRenderer>().color = new Color(0.2f, 0.1f, 0.1f, 0.6f);
                    //typeText.GetComponent<TextMeshPro>().color = Color.white;
                    if (MeetingHud.Instance)
                    {
                        ChatScreenContainer.transform.localPosition = new Vector3(-4.33f, -2.236f, 0);
                    }
                    else
                    {
                        ChatScreenContainer.transform.localPosition = new Vector3(-3.49f, -2.236f, 0);
                    }

                    if (PlayerControl.LocalPlayer.IsImpostor() && genOpt is { FFAImpostorMode: false, ImpostorChat.Value: true } && !PlayerControl.LocalPlayer.Data.IsDead && _teamText != null)
                    {
                        _teamText.text = "Impostor Chat is Open. Only Impostors can see this.";
                        _teamText.color = TownOfUsColors.ImpSoft;
                    }
                    else if (PlayerControl.LocalPlayer.Data.Role is VampireRole && genOpt.VampireChat && !PlayerControl.LocalPlayer.Data.IsDead && _teamText != null)
                    {
                        _teamText.text = "Vampire Chat is Open. Only Vampires can see this.";
                        _teamText.color = TownOfUsColors.Vampire;
                    }
                }
                else
                {
                    Background.GetComponent<SpriteRenderer>().color = Color.white;
                    /* typeBg.GetComponent<SpriteRenderer>().color = Color.white;
                    typeBg.GetComponent<ButtonRolloverHandler>().ChangeOutColor(Color.white);
                    typeBg.GetComponent<ButtonRolloverHandler>().OverColor = new Color(0f, 1f, 0f, 1f);
                    if (typeText.TryGetComponent<TextMeshPro>(out var txt))
                    {
                        txt.color = new Color(0.6706f, 0.8902f, 0.8667f, 1f);
                        txt.SetFaceColor(new Color(0.6706f, 0.8902f, 0.8667f, 1f));
                    }
                    typeText.GetComponent<TextMeshPro>().color = new Color(0.6706f, 0.8902f, 0.8667f, 1f); */
                    ChatScreenContainer.transform.localPosition = new Vector3(-3.49f, -2.236f, 0);
                }
            }
            else if (__instance.IsClosedOrClosing && TeamChatActive)
            {
                ToggleTeamChat();
            }
        }
    }
    [HarmonyPatch(typeof(ChatController), nameof(ChatController.ForceClosed))]
    public static class ForceClosePatch
    {
        public static void Postfix(ChatController __instance)
        {
            if (TeamChatActive)
            {
                ToggleTeamChat();
            }
        }
    }
}

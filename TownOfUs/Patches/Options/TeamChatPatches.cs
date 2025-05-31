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
    private static TextMeshPro? _noticeText;
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

    [HarmonyPostfix]
    [HarmonyPatch(typeof(ChatController), nameof(ChatController.Awake))]
    public static void AwakePostfix(ChatController __instance)
    {
        _noticeText = Object.Instantiate(__instance.sendRateMessageText, __instance.sendRateMessageText.transform.parent);
        _noticeText.text = string.Empty;
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(ChatController), nameof(ChatController.Update))]
    public static void UpdatePostfix(ChatController __instance)
    {
        var ChatScreenContainer = GameObject.Find("ChatScreenContainer");
        var Background = ChatScreenContainer.transform.FindChild("Background");

        if (TeamChatActive)
        {
            Background.GetComponent<SpriteRenderer>().color = new Color(0.2f, 0.2f, 0.27f, 0.6f);
            if (MeetingHud.Instance)
            {
                ChatScreenContainer.transform.localPosition = new Vector3(-4.19f, -2.236f, 0);
            }
            else
            {
                ChatScreenContainer.transform.localPosition = new Vector3(-3.49f, -2.236f, 0);
            }
        }
        else
        {
            Background.GetComponent<SpriteRenderer>().color = Color.white;
            ChatScreenContainer.transform.localPosition = new Vector3(-3.49f, -2.236f, 0);
        }

        var genOpt = OptionGroupSingleton<GeneralOptions>.Instance;
        if (_noticeText == null)
        {
            return;
        }

        if (PlayerControl.LocalPlayer.IsImpostor() && genOpt is { FFAImpostorMode: false, ImpostorChat.Value: true } && !PlayerControl.LocalPlayer.Data.IsDead)
        {
            _noticeText.text = "Impostor Chat is Open. Only Impostors can see this.";
        }
        else if (PlayerControl.LocalPlayer.Data.Role is VampireRole && genOpt.VampireChat && !PlayerControl.LocalPlayer.Data.IsDead)
        {
            _noticeText.text = "Vampire Chat is Open. Only Vampires can see this.";
        }
        else
        {
            _noticeText.text = string.Empty;
        }
    }
}

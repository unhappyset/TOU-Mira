using HarmonyLib;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using TMPro;
using TownOfUs.Modifiers.Crewmate;
using TownOfUs.Modifiers.Impostor;
using TownOfUs.Options.Roles.Crewmate;
using TownOfUs.Patches.Options;
using Object = UnityEngine.Object;

namespace TownOfUs.Patches.Roles;

[HarmonyPatch(typeof(ChatController))]
public static class ChatControllerPatches
{
    private static TextMeshPro? _noticeText;

    [HarmonyPostfix]
    [HarmonyPatch(nameof(ChatController.Awake))]
    public static void AwakePostfix(ChatController __instance)
    {
        _noticeText = Object.Instantiate(__instance.sendRateMessageText, __instance.sendRateMessageText.transform.parent);
        _noticeText.text = string.Empty;
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(ChatController.UpdateChatMode))]
    public static void UpdatePostfix(ChatController __instance)
    {
        if (_noticeText == null || !MeetingHud.Instance)
        {
            return;
        }

        if (PlayerControl.LocalPlayer != null && PlayerControl.LocalPlayer.HasModifier<BlackmailedModifier>() && !PlayerControl.LocalPlayer.Data.IsDead)
        {
            _noticeText.text = "You have been blackmailed.";
            __instance.freeChatField.SetVisible(false);
            __instance.quickChatField.SetVisible(false);
        }
        if (PlayerControl.LocalPlayer != null && PlayerControl.LocalPlayer.HasModifier<JailedModifier>() && !TeamChatPatches.TeamChatActive && !PlayerControl.LocalPlayer.Data.IsDead)
        {
            if (OptionGroupSingleton<JailorOptions>.Instance.JaileePublicChat)
            {
                _noticeText.text = "You are jailed. You can use public chat.";
            }
            else
            {
                _noticeText.text = "You are jailed. You cannot use public chat.";
                __instance.freeChatField.SetVisible(false);
                __instance.quickChatField.SetVisible(false);
            }
        }
        else if (TeamChatPatches.TeamChatActive)
        {
            __instance.quickChatField.SetVisible(false);
            _noticeText.text = string.Empty;
        }
        else
        {
            _noticeText.text = string.Empty;
        }
    }
}

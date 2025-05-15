using HarmonyLib;
using MiraAPI.Modifiers;
using TMPro;
using TownOfUs.Modifiers.Impostor;
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
        if (_noticeText == null)
        {
            return;
        }

        if (PlayerControl.LocalPlayer != null && PlayerControl.LocalPlayer.HasModifier<BlackmailedModifier>() && !PlayerControl.LocalPlayer.Data.IsDead)
        {
            _noticeText.text = "You have been blackmailed.";
            __instance.freeChatField.SetVisible(false);
            __instance.quickChatField.SetVisible(false);
        }
        else
        {
            _noticeText.text = string.Empty;
        }
    }
}

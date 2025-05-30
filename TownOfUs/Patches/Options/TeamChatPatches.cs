using HarmonyLib;
using MiraAPI.GameOptions;
using TMPro;
using TownOfUs.Options;
using TownOfUs.Roles.Neutral;
using TownOfUs.Utilities;
using Object = UnityEngine.Object;

namespace TownOfUs.Patches.Roles;

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
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(ChatController), nameof(ChatController.Awake))]
    public static void AwakePostfix(ChatController __instance)
    {
        _noticeText = Object.Instantiate(__instance.sendRateMessageText, __instance.sendRateMessageText.transform.parent);
        _noticeText.text = string.Empty;
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(ChatController), nameof(ChatController.UpdateChatMode))]
    public static void UpdatePostfix(ChatController __instance)
    {
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

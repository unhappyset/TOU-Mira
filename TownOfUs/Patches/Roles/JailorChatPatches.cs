using System.Globalization;
using HarmonyLib;
using MiraAPI.Modifiers;
using TownOfUs.Modifiers.Crewmate;
using TownOfUs.Roles.Crewmate;
using TownOfUs.Utilities;

namespace TownOfUs.Patches.Roles;

[HarmonyPatch]
public static class JailorChatPatches
{
    private static bool jailorMessage;

    [HarmonyPatch(typeof(ChatController), nameof(ChatController.AddChat))]
    [HarmonyPrefix]
    public static bool PrivateJaileeChatPatch(ChatController __instance, [HarmonyArgument(0)] ref PlayerControl sourcePlayer, ref string chatText)
    {
        var chatTextLower = chatText.ToLower(CultureInfo.InvariantCulture);

        if (chatTextLower.Replace(" ", string.Empty).StartsWith("/jail", StringComparison.InvariantCulture) && (sourcePlayer.IsRole<JailorRole>() || sourcePlayer.Data.Role is ImitatorRole imi && imi.OldRole is JailorRole) && MeetingHud.Instance)
        {
            if (PlayerControl.LocalPlayer.IsRole<JailorRole>() || PlayerControl.LocalPlayer.IsJailed() || PlayerControl.LocalPlayer.Data.Role is ImitatorRole imi2 && imi2.OldRole is JailorRole)
            {
                if (chatTextLower.StartsWith("/jail ", StringComparison.InvariantCulture))
                    chatText = chatText[6..];
                else if (chatTextLower.StartsWith("/jail", StringComparison.InvariantCulture))
                    chatText = chatText[5..];
                else if (chatTextLower.StartsWith("/ jail ", StringComparison.InvariantCulture))
                    chatText = chatText[7..];
                else if (chatTextLower.StartsWith("/ jail", StringComparison.InvariantCulture))
                    chatText = chatText[6..];

                jailorMessage = true;

                /*if (sourcePlayer != PlayerControl.LocalPlayer && PlayerControl.LocalPlayer.IsJailed() && !sourcePlayer.Data.IsDead)
                    sourcePlayer = PlayerControl.LocalPlayer;*/

                return true;
            }
            else
            {
                return false;
            }
        }

        if (chatTextLower.StartsWith('/'))
        {
            if (sourcePlayer == PlayerControl.LocalPlayer)
            {
                var title = "<color=#8BFDFD>System</color>";
                var msg = "Invalid Command. Use the wiki by clicking on the globe icon if you need help regarding your role or modifier(s).";
                MiscUtils.AddFakeChat(PlayerControl.LocalPlayer.Data, title, msg);
            }
            return false;
        }

        if (sourcePlayer.IsJailed() && MeetingHud.Instance)
        {
            if (sourcePlayer.AmOwner || PlayerControl.LocalPlayer.GetRole<JailorRole>() || PlayerControl.LocalPlayer.Data.Role is ImitatorRole imi2 && imi2.OldRole is JailorRole)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        if (PlayerControl.LocalPlayer.IsJailed() && MeetingHud.Instance)
        {
            return false;
        }

        return true;
    }

    [HarmonyPatch(typeof(ChatBubble), nameof(ChatBubble.SetName))]
    [HarmonyPostfix]
    public static void SetNamePatch(ChatBubble __instance, [HarmonyArgument(0)] string playerName)
    {
        if ((PlayerControl.LocalPlayer.IsRole<JailorRole>() || PlayerControl.LocalPlayer.Data.Role is ImitatorRole imi && imi.OldRole is JailorRole) && MeetingHud.Instance)
        {
            var jailor = PlayerControl.LocalPlayer.GetRole<JailorRole>()!;
            var jailed = jailor.Jailed;
            if (PlayerControl.LocalPlayer.Data.Role is ImitatorRole imi2 && imi2.OldRole is JailorRole) jailed = PlayerControl.AllPlayerControls.ToArray().FirstOrDefault(x => x.GetModifier<JailedModifier>()?.JailorId == PlayerControl.LocalPlayer.PlayerId)!;

            if (jailed != null && jailed.Data.PlayerName == playerName)
            {
                __instance.NameText.color = jailor.RoleColor;
                __instance.NameText.text = playerName + " (Jailed)";
            }
            else if (jailorMessage)
            {
                __instance.NameText.color = jailor.RoleColor;
                __instance.NameText.text = "Jailor";
                __instance.SetCosmetics(PlayerControl.LocalPlayer.Data);
                jailorMessage = false;
            }
        }

        if (PlayerControl.LocalPlayer.IsJailed() && MeetingHud.Instance && jailorMessage)
        {
            __instance.NameText.color = TownOfUsColors.Jailor;
            __instance.NameText.text = "Jailor";
            __instance.SetCosmetics(PlayerControl.LocalPlayer.Data);
            jailorMessage = false;
        }
    }
}

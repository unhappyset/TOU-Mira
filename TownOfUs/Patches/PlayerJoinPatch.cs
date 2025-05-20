
using System.Collections;
using HarmonyLib;
using Reactor.Utilities;
using TownOfUs.Modules;
using TownOfUs.Patches.Misc;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfUs.Patches;

    [HarmonyPatch(typeof(PlayerPhysics), nameof(PlayerPhysics.CoSpawnPlayer))]
    public static class PlayerJoinPatch
    {
        public static bool SentOnce { get; private set; }
        public static HudManager HUD => HudManager.Instance;
        public static ChatController Chat => HUD.Chat;
        public static void Postfix(PlayerPhysics __instance)
        {
            if (AmongUsClient.Instance && !TutorialManager.InstanceExists) ChatPatches.DoHostSetup();
            if (!AmongUsClient.Instance || !PlayerControl.LocalPlayer || !__instance.myPlayer || TutorialManager.InstanceExists)
                return;
                
            TouRoleManagerPatches.ReplaceRoleManager = false;
            if (__instance.myPlayer == PlayerControl.LocalPlayer && GameHistory.EndGameSummary != string.Empty && TownOfUsPlugin.ShowSummaryMessage.Value)
            {
                var factionText = string.Empty;
                var msg = string.Empty;
                if (GameHistory.WinningFaction != string.Empty) factionText = $"<size=80%>Winning Team: {GameHistory.WinningFaction}</size>\n";
                var title = $"<color=#8BFDFD>System (Toggleable In Options)</color>\n<size=62%>{factionText}{GameHistory.EndGameSummary}</size>";
                MiscUtils.AddFakeChat(PlayerControl.LocalPlayer.Data, title, msg);
            }
            if (__instance.myPlayer == PlayerControl.LocalPlayer && !SentOnce && TownOfUsPlugin.ShowWelcomeMessage.Value)
            {
                var name = $"<color=#8BFDFD>System</color>";
                var msg = $"Welcome to Town of Us Mira v{TownOfUsPlugin.Version}!\nUse the wiki (the globe icon) to get more info on roles or modifiers, where you can use the searchbar. Otherwise use /help in the chat to get a list of commands.\nYou can also disable this message through your options menu.";
                MiscUtils.AddFakeChat(PlayerControl.LocalPlayer.Data, name, msg, true);
                Coroutines.Start(CoSetSentOnce(5f));
            }
            else if (__instance.myPlayer == PlayerControl.LocalPlayer && !TownOfUsPlugin.ShowWelcomeMessage.Value)
            {
                Coroutines.Start(CoSetSentOnce(2.48f));
            }
        }
        private static IEnumerator CoSetSentOnce(float time)
        {
            yield return new WaitForSeconds(time);
            SentOnce = true;
        }
    }
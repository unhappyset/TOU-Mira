using System.Collections;
using HarmonyLib;
using InnerNet;
using Reactor.Networking.Attributes;
using Reactor.Utilities;
using TownOfUs.Modules;
using TownOfUs.Patches.Misc;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfUs.Patches;


[HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.CreatePlayer))]
public static class PlayerJoinPatch
{
    public static bool SentOnce { get; private set; }
    public static HudManager HUD => HudManager.Instance;
    public static ChatController Chat => HUD.Chat;
    public static void Postfix(ClientData clientData)
    {
        if (AmongUsClient.Instance.AmHost)
        {
            Coroutines.Start(CoSendJoinMsg(clientData));
        }
        else
        {
            Coroutines.Start(DelaySend(clientData));
        }
    }
    private static IEnumerator DelaySend(ClientData clientData)
    {
        yield return new WaitForSeconds(0.5f);
        while (!AmongUsClient.Instance) yield return null;
        Logger<TownOfUsPlugin>.Message("Client Initialized?");
        RpcSendMsg(clientData);
    }
    [MethodRpc((uint)TownOfUsRpc.SendJoinMsg, SendImmediately = false)]
    public static void RpcSendMsg(ClientData clientData)
    {
        Coroutines.Start(CoSendJoinMsg(clientData));
    }
    private static IEnumerator CoSendJoinMsg(ClientData clientData)
    {
        while (!PlayerControl.LocalPlayer) yield return null;
        var player = clientData.Character;

        while (!player) yield return null;
        Logger<TownOfUsPlugin>.Message("New Player Joined, Resetting Host");
        if (AmongUsClient.Instance && !TutorialManager.InstanceExists) ChatPatches.DoHostSetup();

        if (!player.AmOwner) yield break;
        Logger<TownOfUsPlugin>.Message("Sending Message to Local Player...");
        TouRoleManagerPatches.ReplaceRoleManager = false;

        var time = 0f;
        if (GameHistory.EndGameSummary != string.Empty && TownOfUsPlugin.ShowSummaryMessage.Value)
        {
            var factionText = string.Empty;
            var msg = string.Empty;
            if (GameHistory.WinningFaction != string.Empty) factionText = $"<size=80%>Winning Team: {GameHistory.WinningFaction}</size>\n";
            var title = $"<color=#8BFDFD>System (Toggleable In Options)</color>\n<size=62%>{factionText}{GameHistory.EndGameSummary}</size>";
            MiscUtils.AddFakeChat(PlayerControl.LocalPlayer.Data, title, msg);
        }
        if (!SentOnce && TownOfUsPlugin.ShowWelcomeMessage.Value)
        {
            var name = $"<color=#8BFDFD>System</color>";
            var msg = $"Welcome to Town of Us Mira v{TownOfUsPlugin.Version}!\nUse the wiki (the globe icon) to get more info on roles or modifiers, where you can use the searchbar. Otherwise use /help in the chat to get a list of commands.\nYou can also disable this message through your options menu.";
            MiscUtils.AddFakeChat(PlayerControl.LocalPlayer.Data, name, msg, true);
            time = 5f;
        }
        else if (!TownOfUsPlugin.ShowWelcomeMessage.Value)
        {
            time = 2.48f;
        }
        if (time == 0) yield break;
        yield return new WaitForSeconds(time);
        Logger<TownOfUsPlugin>.Message("Offset Wiki Button (if needed)");
        SentOnce = true;
    }
}
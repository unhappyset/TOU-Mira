using System.Globalization;
using HarmonyLib;
using MiraAPI.GameOptions;
using MiraAPI.Utilities;
using Reactor.Networking.Attributes;
using Reactor.Utilities;
using Reactor.Utilities.Extensions;
using TownOfUs.Modules;
using TownOfUs.Options;
using TownOfUs.Patches.Options;
using TownOfUs.Roles.Crewmate;
using TownOfUs.Roles.Neutral;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfUs.Patches.Misc;

[HarmonyPatch(typeof(ChatController), nameof(ChatController.SendChat))]
public static class ChatPatches
{
    // ReSharper disable once InconsistentNaming
    public static bool Prefix(ChatController __instance)
    {
        var text = __instance.freeChatField.Text.ToLower(CultureInfo.InvariantCulture);
        var textRegular = __instance.freeChatField.Text;

        if (text.Replace(" ", string.Empty).StartsWith("/", StringComparison.OrdinalIgnoreCase)
            && text.Replace(" ", string.Empty).Contains("summary", StringComparison.OrdinalIgnoreCase))
        {
            var title = $"<color=#8BFDFD>System</color>";
            var msg = "No game summary to show!";
            if (GameHistory.EndGameSummary != string.Empty)
            {
                var factionText = string.Empty;
                if (GameHistory.WinningFaction != string.Empty) factionText = $"<size=80%>Winning Team: {GameHistory.WinningFaction}</size>\n";
                title = $"<color=#8BFDFD>System</color>\n<size=62%>{factionText}{GameHistory.EndGameSummary}</size>";
                msg = string.Empty;
            }
            MiscUtils.AddFakeChat(PlayerControl.LocalPlayer.Data, title, msg);

            __instance.freeChatField.Clear();
            __instance.quickChatMenu.Clear();
            __instance.quickChatField.Clear();
            __instance.UpdateChatMode();
            return false;
        }
        else if (text.Replace(" ", string.Empty).StartsWith("/nerfme", StringComparison.OrdinalIgnoreCase))
        {

            var title = $"<color=#8BFDFD>System</color>";
            var msg = "You cannot Nerf yourself outside of the lobby!";
            if (LobbyBehaviour.Instance)
            {
                VisionPatch.NerfMe = !VisionPatch.NerfMe;
                msg = $"Toggled Nerf Status To {VisionPatch.NerfMe}!";
            }
            MiscUtils.AddFakeChat(PlayerControl.LocalPlayer.Data, title, msg);
            
            __instance.freeChatField.Clear();
            __instance.quickChatMenu.Clear();
            __instance.quickChatField.Clear();
            __instance.UpdateChatMode();
            return false;
        }
        else if (text.Replace(" ", string.Empty).StartsWith("/setname", StringComparison.OrdinalIgnoreCase))
        {
            var title = $"<color=#8BFDFD>System</color>";
                if (text.StartsWith("/setname ", StringComparison.OrdinalIgnoreCase))
                    textRegular = textRegular[9..];
                else if (text.StartsWith("/setname", StringComparison.OrdinalIgnoreCase))
                    textRegular = textRegular[8..];
                else if (text.StartsWith("/ setname ", StringComparison.OrdinalIgnoreCase))
                    textRegular = textRegular[10..];
                else if (text.StartsWith("/ setname", StringComparison.OrdinalIgnoreCase))
                    textRegular = textRegular[9..];
            var msg = "You cannot change your name outside of the lobby!";
            if (LobbyBehaviour.Instance)
            {
                if (textRegular.Length < 2)
                {
                    msg = $"The player name must be at least 2 characters long!";
                }
                else
                {
                    // This is done to prevent the player from being kicked for changing their name as they're not the host
                    PlayerControl.LocalPlayer.CmdCheckName(textRegular);
                    msg = $"Changed player name for the next match to: {textRegular}";
                }
            }
            MiscUtils.AddFakeChat(PlayerControl.LocalPlayer.Data, title, msg);
            
            __instance.freeChatField.Clear();
            __instance.quickChatMenu.Clear();
            __instance.quickChatField.Clear();
            __instance.UpdateChatMode();
            return false;
        }
        // if this could be added it would be pretty useful - Atony
        else if (text.Replace(" ", string.Empty).StartsWith("/sethost", StringComparison.OrdinalIgnoreCase))
        {
            var title = $"<color=#8BFDFD>System</color>";
                if (text.StartsWith("/sethost ", StringComparison.OrdinalIgnoreCase))
                    textRegular = textRegular[9..];
                else if (text.StartsWith("/sethost", StringComparison.OrdinalIgnoreCase))
                    textRegular = textRegular[8..];
                else if (text.StartsWith("/ sethost ", StringComparison.OrdinalIgnoreCase))
                    textRegular = textRegular[10..];
                else if (text.StartsWith("/ sethost", StringComparison.OrdinalIgnoreCase))
                    textRegular = textRegular[9..];
            var msg = "You are not the current host!";
            if (PlayerControl.LocalPlayer.IsHost())
            {
                var playerCon = PlayerControl.AllPlayerControls.ToArray().FirstOrDefault(x => string.Equals(x.Data.PlayerName, textRegular, StringComparison.OrdinalIgnoreCase));
                var player = AmongUsClient.Instance.allClients.ToArray().FirstOrDefault(x => string.Equals(x.PlayerName, textRegular, StringComparison.OrdinalIgnoreCase));
                if (LobbyBehaviour.Instance && player != null && playerCon != null)
                {
                    msg = $"{textRegular} is now the host!\n" +
                    $"<size=75%>This command is <b>experimental</b>. If a player joins after this point, the command must be run again from the original host to restore permissions. The new host may also NOT change server visibility.</size>";
                    RpcChangeHost(PlayerControl.LocalPlayer, player.Id, playerCon);
                }
                else if (LobbyBehaviour.Instance)
                {
                    msg = $"Could not find the specified player! ({textRegular})";
                }
                else
                {
                    msg = "You cannot change who the host is outside of the lobby!";
                }
            }
            MiscUtils.AddFakeChat(PlayerControl.LocalPlayer.Data, title, msg);
            
            __instance.freeChatField.Clear();
            __instance.quickChatMenu.Clear();
            __instance.quickChatField.Clear();
            __instance.UpdateChatMode();
            return false;
        }
        else if (text.Replace(" ", string.Empty).StartsWith("/help", StringComparison.OrdinalIgnoreCase))
        {
            var title = $"<color=#8BFDFD>System</color>";
            List<string> randomNames = ["Atony", "Alchlc", "angxlwtf", "Digi", "donners", "K3ndo", "MyDragonBreath", "Pietro", "twix", "xerm", "XtraCube", "Zeo", "Slushie"];
            var msg = "<size=75%>Chat Commands:\n" +
                "/help - Shows this message\n" +
                $"/jail - If you are the <b><color=#{Color.gray.ToHtmlStringRGBA()}>Jailor</color></b>, you can send a message to your Jail target by typing something like <b>/jail Hello!</b>\n" +
                "/nerfme - Cuts your vision in half\n" +
                $"/sethost - Changes the host to be another player, will reset and break if someone connects afterwards. Run the command again to fix it.\n" +
                $"/setname - Change your name to whatever text follows the command (like /setname {randomNames.Random()}) for the next match.\n" +
                "/summary - Shows the previous end game summary\n</size>";
            
            MiscUtils.AddFakeChat(PlayerControl.LocalPlayer.Data, title, msg);

            __instance.freeChatField.Clear();
            __instance.quickChatMenu.Clear();
            __instance.quickChatField.Clear();
            __instance.UpdateChatMode();
            return false;
        }
        else if (TeamChatPatches.TeamChatActive && !PlayerControl.LocalPlayer.HasDied() && (PlayerControl.LocalPlayer.Data.Role is JailorRole || PlayerControl.LocalPlayer.IsJailed() ||PlayerControl.LocalPlayer.Data.Role is VampireRole || PlayerControl.LocalPlayer.IsImpostor()))
        {
            var genOpt = OptionGroupSingleton<GeneralOptions>.Instance;

            if (PlayerControl.LocalPlayer.Data.Role is JailorRole)
            {
                TeamChatPatches.RpcSendJailorChat(PlayerControl.LocalPlayer, textRegular);
                MiscUtils.AddTeamChat(PlayerControl.LocalPlayer.Data, $"<color=#{TownOfUsColors.Jailor.ToHtmlStringRGBA()}>{PlayerControl.LocalPlayer.Data.PlayerName} (Jailor)</color>", textRegular, onLeft: false);

                __instance.freeChatField.Clear();
                __instance.quickChatMenu.Clear();
                __instance.quickChatField.Clear();
                __instance.UpdateChatMode();

                return false;
            }
            else if (PlayerControl.LocalPlayer.IsJailed())
            {
                TeamChatPatches.RpcSendJaileeChat(PlayerControl.LocalPlayer, textRegular);
                MiscUtils.AddTeamChat(PlayerControl.LocalPlayer.Data, $"<color=#{TownOfUsColors.Jailor.ToHtmlStringRGBA()}>{PlayerControl.LocalPlayer.Data.PlayerName} (Jailed)</color>", textRegular, onLeft: false);

                __instance.freeChatField.Clear();
                __instance.quickChatMenu.Clear();
                __instance.quickChatField.Clear();
                __instance.UpdateChatMode();

                return false;
            }
            else if (PlayerControl.LocalPlayer.Data.Role is VampireRole && genOpt.VampireChat)
            {
                TeamChatPatches.RpcSendVampTeamChat(PlayerControl.LocalPlayer, textRegular);
                MiscUtils.AddTeamChat(PlayerControl.LocalPlayer.Data, $"<color=#{TownOfUsColors.Vampire.ToHtmlStringRGBA()}>{PlayerControl.LocalPlayer.Data.PlayerName} (Vampire Chat)</color>", textRegular, onLeft: false);

                __instance.freeChatField.Clear();
                __instance.quickChatMenu.Clear();
                __instance.quickChatField.Clear();
                __instance.UpdateChatMode();

                return false;
            }
            else if (PlayerControl.LocalPlayer.IsImpostor() && genOpt is { FFAImpostorMode: false, ImpostorChat.Value: true })
            {
                TeamChatPatches.RpcSendImpTeamChat(PlayerControl.LocalPlayer, textRegular);
                MiscUtils.AddTeamChat(PlayerControl.LocalPlayer.Data, $"<color=#{TownOfUsColors.ImpSoft.ToHtmlStringRGBA()}>{PlayerControl.LocalPlayer.Data.PlayerName} (Impostor Chat)</color>", textRegular, onLeft: false);

                __instance.freeChatField.Clear();
                __instance.quickChatMenu.Clear();
                __instance.quickChatField.Clear();
                __instance.UpdateChatMode();

                return false;
            }
            return true;
        }
        return true;
    }

    [MethodRpc((uint)TownOfUsRpc.ChangeHost, SendImmediately = true)]
    private static void RpcChangeHost(PlayerControl host, int id, PlayerControl playerCon)
    {
        if (!host.IsHost())
        {
            Logger<TownOfUsPlugin>.Error($"{host.Data.PlayerName} is not the host!");
            return;
        }
        else if (id == -1)
        {
            Logger<TownOfUsPlugin>.Error($"Invalid client id: {id}");
            return;
        }
        AmongUsClient.Instance.HostId = id;
        DoHostSetup();
		GameStartManager.Instance.StartCoroutine(GameStartManager.Instance.HostInfoPanel.SetCosmetics(playerCon.Data));
    }
	internal static void DoHostSetup()
	{
        var manager = GameStartManager.Instance;
		string text = InnerNet.GameCode.IntToGameName(AmongUsClient.Instance.GameId);
		if (!AmongUsClient.Instance.AmHost)
		{
			manager.HostPrivacyButtons.gameObject.SetActive(false);
			manager.ClientPrivacyValue.gameObject.SetActive(true);
			manager.StartButton.gameObject.SetActive(false);
			manager.StartButtonClient.gameObject.SetActive(true);
			manager.GameStartTextParent.SetActive(false);
			manager.HostInfoPanelButtons.gameObject.SetActive(false);
			manager.ClientInfoPanelButtons.gameObject.SetActive(true);
			return;
		}
		if (text != null)
		{
			manager.HostPrivacyButtons.gameObject.SetActive(true);
			manager.ClientPrivacyValue.gameObject.SetActive(false);
		}
        AmongUsClient.Instance.OnBecomeHost();
		manager.HostInfoPanelButtons.gameObject.SetActive(true);
		manager.ClientInfoPanelButtons.gameObject.SetActive(false);
		manager.StartButton.gameObject.SetActive(true);
		manager.StartButtonClient.gameObject.SetActive(false);
	}
}

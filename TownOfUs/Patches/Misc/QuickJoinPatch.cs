using HarmonyLib;
using Il2CppSystem;
using InnerNet;
using Reactor.Utilities.Extensions;
using TMPro;
using UnityEngine;

namespace TownOfUs.Patches.Misc;

[HarmonyPatch]

public static class LobbyJoin
{
    static int GameId;

    static GameObject LobbyText;

    static TextMeshPro Text;

    [HarmonyPatch(typeof(InnerNetClient), nameof(InnerNetClient.JoinGame))]
    [HarmonyPostfix]

    public static void Postfix(InnerNetClient __instance)
    {
        GameId = __instance.GameId;
    }

    [HarmonyPatch(typeof(EnterCodeManager), nameof(EnterCodeManager.OnEnable))]
    [HarmonyPostfix]

    public static void OnEnable(EnterCodeManager __instance)
    {
        if (LobbyText)
        {
            LobbyText.SetActive(GameId != 0);
            return;
        }

        LobbyText = GameObject.Instantiate(__instance.transform.FindChild("Header").gameObject, __instance.transform);
        LobbyText.name = "LobbyText";
        Text = LobbyText.transform.GetChild(1).GetComponent<TextMeshPro>();
        Text.fontSizeMin = 2.55f;
        Text.fontSizeMax = 2.55f;
        Text.fontSize = 2.55f;
        Text.text = string.Empty;
        Text.alignment = TextAlignmentOptions.Center;
        LobbyText.transform.localPosition = new(0.9f, 0f, 0f);
        LobbyText.transform.GetChild(0).gameObject.Destroy();
        LobbyText.SetActive(GameId != 0);
    }

    [HarmonyPatch(typeof(EnterCodeManager), nameof(EnterCodeManager.OnDisable))]
    [HarmonyPostfix]

    public static void OnDisable()
    {
        if (LobbyText)
        {
            LobbyText.SetActive(false);
        }
    }

    [HarmonyPatch(typeof(MainMenuManager), nameof(MainMenuManager.LateUpdate))]
    [HarmonyPostfix]

    public static void Update()
    {
        if (GameId == 0 || !LobbyText || !LobbyText.active) return;

        if (Input.GetKeyDown(KeyCode.Tab))
        {
            AmongUsClient.Instance.StartCoroutine(AmongUsClient.Instance.CoJoinOnlineGameFromCode(GameId, true));
        }
        if (LobbyText && Text)
        {
            Text.text = $"Prev Lobby: {GameCode.IntToGameName(GameId)}\nClick Tab key to \nattempt joining";
        }
    }
}

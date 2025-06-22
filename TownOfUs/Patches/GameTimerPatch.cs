using HarmonyLib;
using MiraAPI.GameOptions;
using TownOfUs.Options;
using UnityEngine;
using TMPro;

namespace TownOfUs.Patches;

[HarmonyPatch]
public static class GameTimerPatch
{
    public static bool Enabled { get; set; }
    public static bool TriggerEndGame { get; set; }
    public static float GameTimer { get; set; }

    public static GameObject GameTimerObj;

    private static void CreateGameTimer(HudManager instance)
    {
        var pingTracker = UnityEngine.Object.FindObjectOfType<PingTracker>(true);
        GameTimerObj = UnityEngine.Object.Instantiate(pingTracker.gameObject, instance.transform);
        GameTimerObj.name = "GameTimerText";
        GameTimerObj.GetComponent<AspectPosition>().DistanceFromEdge = new Vector3(-0.5f, 5.6f);

        TimeSpan ts = TimeSpan.FromSeconds(GameTimer);

        var timerText = GameTimerObj.GetComponent<TextMeshPro>();
        timerText.text = $"<size=200%>Time:{ts.ToString(format: @"mm\:ss", TownOfUsPlugin.Culture)}</size>";
        timerText.alignment = TextAlignmentOptions.TopLeft;
        timerText.verticalAlignment = VerticalAlignmentOptions.Top;
    }

    private static void EndGame()
    {
        Enabled = false;
        TriggerEndGame = true;
        GameManager.Instance.ShouldCheckForGameEnd = true;
    }

    public static void BeginTimer()
    {
        Enabled = true;
        GameTimer = OptionGroupSingleton<WinOptions>.Instance.GameTimeLimit.GetFloatData();
    }

    public static void UpdateGameTimer(HudManager instance)
    {
        if (GameTimerObj != null)
        {
            GameTimerObj.SetActive(false);
        }

        if (!OptionGroupSingleton<WinOptions>.Instance.GameTimerEnabled)
        {
            return;
        }

        if (GameTimerObj == null)
        {
            CreateGameTimer(instance);
        }

        if (GameTimerObj != null)
        {
            if (Enabled && GameTimer > 0 && (!MeetingHud.Instance || !ExileController.Instance || GameTimer > 240f))
            {
                GameTimer -= Time.deltaTime;
                GameTimer = Math.Max(GameTimer, 0);

                if (AmongUsClient.Instance.AmHost && GameTimer <= 0)
                {
                    EndGame();
                }
            }

            TimeSpan ts = TimeSpan.FromSeconds(GameTimer);

            var timerText = GameTimerObj.GetComponent<TextMeshPro>();
            timerText.text = $"<size=200%>Time:{ts.ToString(format: @"mm\:ss", TownOfUsPlugin.Culture)}</size>";

            GameTimerObj.SetActive(true);
        }
    }

    [HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
    [HarmonyPostfix]
    public static void HudManagerUpdatePatch(HudManager __instance)
    {
        UpdateGameTimer(__instance);
    }
}

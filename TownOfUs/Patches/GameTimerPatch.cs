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
        GameTimerObj.GetComponent<AspectPosition>().DistanceFromEdge = new Vector3(-0.6f, 5.5f);

        TimeSpan ts = TimeSpan.FromSeconds(GameTimer);

        var timerText = GameTimerObj.GetComponent<TextMeshPro>();
        timerText.text = $"<size=200%>Time:{ts.ToString(format: @"mm\:ss", TownOfUsPlugin.Culture)}</size>";
        timerText.alignment = TextAlignmentOptions.TopLeft;
        timerText.verticalAlignment = VerticalAlignmentOptions.Top;

        GameTimerObj.SetActive(false);
    }

    public static void UpdateGameTimer(HudManager instance)
    {
        if (GameTimerObj != null)
        {
            GameTimerObj.SetActive(false);
        }

        if (!OptionGroupSingleton<GameTimerOptions>.Instance.GameTimerEnabled)
        {
            return;
        }

        if (GameTimerObj == null)
        {
            CreateGameTimer(instance);
        }

        if (GameTimerObj == null)
        {
            return;
        }

        var inMeeting = MeetingHud.Instance || ExileController.Instance;
        if (Enabled && GameTimer > 0 && (!inMeeting || GameTimer > 240f))
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

        var colour = GameTimer switch
        {
            < 30f => Color.red,
            < 60f => Color.yellow,
            _ => Color.green
        };

        timerText.text = $"<size=200%>Time:{colour.ToTextColor()}{ts.ToString(format: @"mm\:ss", TownOfUsPlugin.Culture)}</color></size>";

        GameTimerObj.SetActive(true);
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
        TriggerEndGame = false;
        GameTimer = OptionGroupSingleton<GameTimerOptions>.Instance.GameTimeLimit.GetFloatData() * 60f;
    }

    [HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
    [HarmonyPostfix]
    public static void HudManagerUpdatePatch(HudManager __instance)
    {
        if (PlayerControl.LocalPlayer == null ||
            PlayerControl.LocalPlayer.Data == null ||
            PlayerControl.LocalPlayer.Data.Role == null ||
            !ShipStatus.Instance ||
            TutorialManager.InstanceExists ||
            AmongUsClient.Instance.GameState != InnerNet.InnerNetClient.GameStates.Started)
        {
            return;
        }

        UpdateGameTimer(__instance);
    }
}

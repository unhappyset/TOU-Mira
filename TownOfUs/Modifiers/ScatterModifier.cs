using MiraAPI.Modifiers.Types;
using MiraAPI.Networking;
using TMPro;
using TownOfUs.Utilities;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace TownOfUs.Modifiers;

public class ScatterModifier(float time) : TimedModifier
{
    public override string ModifierName => "Scatter";
    public override float Duration => time;
    public override bool AutoStart => false;
    public override bool HideOnUi => true;
    public override bool RemoveOnComplete => true;

    private readonly List<Vector3> _locations = [];
    private float soundTimer = 1f;
    private GameObject? scatterUI;
    private TextMeshProUGUI? scatterText;
    private Image? scatterBar;

    public override string GetDescription()
    {
        var roundedTime = (int)Math.Round(Math.Max(TimeRemaining, 0), 0);

        var textColor = roundedTime switch
        {
            > 10 => Color.green,
            > 5 => Color.yellow,
            _ => Color.red,
        };

        return $"{textColor.ToTextColor()}<size=80%>{roundedTime}s</size></color>";
    }

    public override void OnActivate()
    {
        base.OnActivate();

        if (!Player.AmOwner) return;

        scatterUI = Object.Instantiate(TouAssets.ScatterUI.LoadAsset(), HudManager.Instance.transform);
        scatterUI.transform.localPosition = new Vector3(-3.22f, 2.26f, -10f);

        scatterText = scatterUI.transform.FindChild("ScatterCanvas").FindChild("ScatterText").gameObject.GetComponent<TextMeshProUGUI>();
        scatterText.text = $"Scatter: {Duration}s";

        scatterBar = scatterUI.transform.FindChild("ScatterCanvas").FindChild("ScatterBar").gameObject.GetComponent<Image>();
        scatterBar.fillAmount = 1f;

        var scatterIcon = scatterUI.transform.FindChild("ScatterCanvas").FindChild("ScatterIcon").gameObject.GetComponent<Image>();
        scatterIcon.sprite = Player.Data.Role.RoleIconSolid;
    }

    public override void OnMeetingStart()
    {
        ResetTimer();
    }

    public override void FixedUpdate()
    {
        base.FixedUpdate();

        var roundedTime = (int)Math.Round(Math.Max(TimeRemaining, 0f), 0f);

        var textColor = roundedTime switch
        {
            > 10 => Color.green,
            > 5 => Color.yellow,
            _ => Color.red,
        };

        if (Player.AmOwner && scatterText != null)
            scatterText.text = $"Scatter: {textColor.ToTextColor()}{roundedTime}s</color>";

        if (Player.AmOwner && scatterBar != null)
        {
            scatterBar.fillAmount = Math.Clamp(TimeRemaining / Duration, 0f, 1f);
            scatterBar.color = textColor;
        }

        if (Player.AmOwner && roundedTime <= 11f)
        {
            soundTimer -= Time.fixedDeltaTime;

            if (soundTimer <= 0f)
            {
                var num = roundedTime / 10f;
                var pitch = 1.5f - num / 2f;
                SoundManager.Instance.PlaySoundImmediate(GameManagerCreator.Instance.HideAndSeekManagerPrefab.FinalHideCountdownSFX, false, 1f, pitch, SoundManager.Instance.SfxChannel);
                soundTimer = 1f;
            }
        }

        if (Player.AmOwner) 
            scatterUI!.SetActive(true);

        if (!TimerActive || Player.HasDied() || MeetingHud.Instance != null)
        {
            soundTimer = 1f;
            TimeRemaining = Duration;

            if (Player.AmOwner) 
                scatterUI!.SetActive(false);

            return;
        }

        foreach (var location in _locations)
        {
            var magnitude = (location - Player.transform.localPosition).magnitude;
            if (magnitude < 5f) return;
        }

        TimeRemaining = Duration;

        _locations.Insert(0, Player.transform.localPosition);
        if (_locations.Count > 3)
        {
            _locations.RemoveAt(3);
        }
    }

    public override void OnTimerComplete()
    {
        if (Player.AmOwner && !Player.HasDied())
        {
            Player.RpcCustomMurder(Player);
        }
    }

    public void OnGameStart()
    {
        StartTimer();
    }

    public void OnRoundStart()
    {
        ResetTimer();
        ResumeTimer();
    }
}

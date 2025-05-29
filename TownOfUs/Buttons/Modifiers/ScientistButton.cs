using AmongUs.GameOptions;
using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Modifiers;
using MiraAPI.Utilities.Assets;
using TownOfUs.Modifiers;
using TownOfUs.Modifiers.Game.Crewmate;
using TownOfUs.Modifiers.Neutral;
using TownOfUs.Options.Modifiers.Crewmate;
using UnityEngine;

namespace TownOfUs.Buttons.Modifiers;

public sealed class ScientistButton : TownOfUsButton
{
    public override string Name => "Vitals";
    public override string Keybind => "tou.ActionCustom2";
    public override Color TextOutlineColor => TownOfUsColors.Scientist;
    public override float Cooldown => OptionGroupSingleton<ScientistOptions>.Instance.DisplayCooldown + MapCooldown;
    public override float EffectDuration => AvailableCharge < OptionGroupSingleton<ScientistOptions>.Instance.DisplayDuration ? AvailableCharge : OptionGroupSingleton<ScientistOptions>.Instance.DisplayDuration;
    public override ButtonLocation Location => ButtonLocation.BottomLeft;
    public override LoadableAsset<Sprite> Sprite => TouAssets.VitalsSprite;
    public float AvailableCharge { get; set; } = OptionGroupSingleton<ScientistOptions>.Instance.StartingCharge;
    public VitalsMinigame? vitals;

    public override bool Enabled(RoleBehaviour? role)
    {
        return PlayerControl.LocalPlayer != null &&
               PlayerControl.LocalPlayer.HasModifier<ScientistModifier>() &&
               !PlayerControl.LocalPlayer.Data.IsDead;
    }

	private void RefreshAbilityButton()
	{
		if (AvailableCharge > 0f && !PlayerControl.LocalPlayer.AreCommsAffected())
		{
			DestroyableSingleton<HudManager>.Instance.AbilityButton.SetEnabled();
			return;
		}
		DestroyableSingleton<HudManager>.Instance.AbilityButton.SetDisabled();
	}
    
    protected override void FixedUpdate(PlayerControl playerControl)
    {
        if (!playerControl.AmOwner || MeetingHud.Instance)
        {
            return;
        }
        if (vitals != null)
        {
            AvailableCharge -= Time.deltaTime;
            vitals.BatteryText.text = $"{(int)AvailableCharge}";
            if (AvailableCharge <= 0f)
            {
                vitals.Close();
                RefreshAbilityButton();
                return;
            }
        }
        else
        {
            RefreshAbilityButton();
        }

        Button?.usesRemainingText.gameObject.SetActive(true);
        Button?.usesRemainingSprite.gameObject.SetActive(true);
        Button!.usesRemainingText.text = (int)AvailableCharge + "%";
    }

    public override bool CanUse()
    {
        return Timer <= 0 && !EffectActive && AvailableCharge > 0f &&
            !PlayerControl.LocalPlayer.HasModifier<DisabledModifier>() && 
            !PlayerControl.LocalPlayer.HasModifier<GlitchHackedModifier>();
    }
    public override void ClickHandler()
    {
        if (!CanUse() || Minigame.Instance != null)
        {
            return;
        }

        OnClick();
        Button?.SetDisabled();
        if (EffectActive)
        {
            Timer = Cooldown;
            EffectActive = false;
        }
        else if (HasEffect)
        {
            EffectActive = true;
            Timer = EffectDuration;
        }
        else
        {
            Timer = Cooldown;
        }
    }

    protected override void OnClick()
    {
		if (!OptionGroupSingleton<ScientistOptions>.Instance.MoveWithMenu) PlayerControl.LocalPlayer.NetTransform.Halt();

        var science = RoleManager.Instance.GetRole(RoleTypes.Scientist).Cast<ScientistRole>();
		vitals = UnityEngine.Object.Instantiate<VitalsMinigame>(science.VitalsPrefab);
		vitals.transform.SetParent(Camera.main.transform, false);
		vitals.transform.localPosition = new Vector3(0f, 0f, -50f);
		vitals.BatteryText.gameObject.SetActive(true);
		vitals.Begin(null);
    }
    public override void OnEffectEnd()
    {
        base.OnEffectEnd();

        if (vitals != null)
        {
            vitals.Close();
            RefreshAbilityButton();
            vitals = null;
        }
    }
}

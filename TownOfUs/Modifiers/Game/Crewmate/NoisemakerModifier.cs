﻿using AmongUs.GameOptions;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Utilities.Assets;
using TownOfUs.Options.Modifiers;
using TownOfUs.Options.Modifiers.Crewmate;
using TownOfUs.Roles;
using TownOfUs.Utilities;
using UnityEngine;
using Object = UnityEngine.Object;

namespace TownOfUs.Modifiers.Game.Crewmate;

public sealed class NoisemakerModifier : TouGameModifier, IWikiDiscoverable
{
    public override string LocaleKey => "Noisemaker";
    public override string ModifierName => TouLocale.Get($"TouModifier{LocaleKey}");
    public override string IntroInfo => TouLocale.GetParsed($"TouModifier{LocaleKey}IntroBlurb");

    public override string GetDescription()
    {
        return TouLocale.GetParsed($"TouModifier{LocaleKey}TabDescription").Replace("<noiseTime>",
            $"{OptionGroupSingleton<NoisemakerOptions>.Instance.AlertDuration}");
    }

    public string GetAdvancedDescription()
    {
        return TouLocale.GetParsed($"TouModifier{LocaleKey}WikiDescription")
               + MiscUtils.AppendOptionsText(GetType());
    }

    public override LoadableAsset<Sprite>? ModifierIcon => TouModifierIcons.Noisemaker;
    public override Color FreeplayFileColor => new Color32(140, 255, 255, 255);

    public override ModifierFaction FactionType => ModifierFaction.CrewmatePostmortem;

    public List<CustomButtonWikiDescription> Abilities { get; } = [];

    public override int GetAssignmentChance()
    {
        return (int)OptionGroupSingleton<CrewmateModifierOptions>.Instance.NoisemakerChance;
    }

    public override int GetAmountPerGame()
    {
        return (int)OptionGroupSingleton<CrewmateModifierOptions>.Instance.NoisemakerAmount;
    }

    private void SoundDynamics(AudioSource source)
    {
        if (!PlayerControl.LocalPlayer)
        {
            source.volume = 0f;
            return;
        }

        source.volume = 1f;
        var truePosition = PlayerControl.LocalPlayer.GetTruePosition();
        source.volume = SoundManager.GetSoundVolume(Player.GetTruePosition(), truePosition, 7f, 50f, 0.5f);
    }

    public void NotifyOfDeath(PlayerControl player)
    {
        if (!player.HasModifier<NoisemakerModifier>())
        {
            return;
        }

        if (PlayerControl.LocalPlayer.IsImpostor() &&
            !OptionGroupSingleton<NoisemakerOptions>.Instance.ImpostorsAlerted)
        {
            return;
        }

        if (PlayerControl.LocalPlayer.Is(RoleAlignment.NeutralKilling) &&
            !OptionGroupSingleton<NoisemakerOptions>.Instance.NeutsAlerted)
        {
            return;
        }

        if (PlayerControl.LocalPlayer.AreCommsAffected() &&
            OptionGroupSingleton<NoisemakerOptions>.Instance.CommsAffected)
        {
            return;
        }

        if (Object.FindObjectsOfType<DeadBody>().FirstOrDefault(x => x.ParentId == player.PlayerId) == null &&
            OptionGroupSingleton<NoisemakerOptions>.Instance.BodyCheck)
        {
            return;
        }

        if (Constants.ShouldPlaySfx())
        {
            var audio = SoundManager.Instance.PlaySound(TouAudio.NoisemakerDeathSound.LoadAsset(), false, 1,
                SoundManager.Instance.SfxChannel);
            SoundDynamics(audio);
            VibrationManager.Vibrate(1f, PlayerControl.LocalPlayer.GetTruePosition(), 7f, 1.2f);
        }

        var noise = RoleManager.Instance.GetRole(RoleTypes.Noisemaker).Cast<NoisemakerRole>();
        var deathArrowPrefab =
            Object.Instantiate(noise.deathArrowPrefab, Player.transform.position, Quaternion.identity);

        var deathArrow = deathArrowPrefab.GetComponent<NoisemakerArrow>();
        deathArrow.SetDuration(OptionGroupSingleton<NoisemakerOptions>.Instance.AlertDuration);
        if (Player.AmOwner)
        {
            deathArrow.alwaysMaxSize = true;
        }

        deathArrow.gameObject.SetActive(true);
        deathArrow.target = Player.GetTruePosition();
    }

    public override bool IsModifierValidOn(RoleBehaviour role)
    {
        return base.IsModifierValidOn(role) && role.IsCrewmate() && role is not NoisemakerRole;
    }
}
using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Modifiers;
using MiraAPI.Utilities.Assets;
using TownOfUs.Modifiers.Neutral;
using TownOfUs.Options.Roles.Neutral;
using TownOfUs.Roles.Neutral;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfUs.Buttons.Neutral;

public sealed class GlitchMimicButton : TownOfUsRoleButton<GlitchRole>, IAftermathableButton
{
    public override string Name => "Mimic";
    public override string Keybind => "ActionQuaternary";
    public override Color TextOutlineColor => TownOfUsColors.Glitch;
    public override float Cooldown => OptionGroupSingleton<GlitchOptions>.Instance.MimicCooldown + MapCooldown;
    public override float EffectDuration => OptionGroupSingleton<GlitchOptions>.Instance.MimicDuration;
    public override LoadableAsset<Sprite> Sprite => TouNeutAssets.MimicSprite;
    public override ButtonLocation Location => ButtonLocation.BottomRight;
    public override bool Enabled(RoleBehaviour? role) => role is GlitchRole;

    protected override void OnClick()
    {
        if (!EffectActive)
        {
            var playerMenu = CustomPlayerMenu.Create();
            playerMenu.Begin(
                plr => !plr.HasDied() && plr != PlayerControl.LocalPlayer,
                plr =>
                {
                    playerMenu.ForceClose();

                    if (plr != null)
                    {
                        TouAudio.PlaySound(TouAudio.MimicSound);
                        // THE ANIMATION NEEDS TO BE DONE IN SUCH A WAY THAT THE PLAYER DOESN'T MIMIC UNTIL THE ANIMATION ENDS
                        // GOStore.SpawnGOATPlayer(PlayerControl.LocalPlayer, GOStore.GlitchMimic).AddComponent<UE_DeleteAfter>().endTime = 3;
                        PlayerControl.LocalPlayer.RpcAddModifier<GlitchMimicModifier>(plr);

                        EffectActive = true;
                        Timer = EffectDuration;
                        OverrideName("Unmimic");
                    }
                    else
                    {
                        EffectActive = false;
                        Timer = 1f;
                    }
                });
        }
        else
        {
            PlayerControl.LocalPlayer.RpcRemoveModifier<GlitchMimicModifier>();
            OverrideName("Mimic");
            TouAudio.PlaySound(TouAudio.UnmimicSound);
        }
    }

    public override void OnEffectEnd()
    {
        TouAudio.PlaySound(TouAudio.UnmimicSound);
        OverrideName("Mimic");
    }

    public override bool CanUse()
    {
        return ((Timer <= 0 && !EffectActive) || (EffectActive && Timer <= (EffectDuration - 2f))) && !PlayerControl.LocalPlayer.HasModifier<GlitchHackedModifier>();
    }

    public override void ClickHandler()
    {
        if (!CanUse())
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
}

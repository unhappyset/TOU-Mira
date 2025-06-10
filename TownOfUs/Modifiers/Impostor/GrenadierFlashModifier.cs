using MiraAPI.GameOptions;
using MiraAPI.Utilities;
using TownOfUs.Modules;
using TownOfUs.Options.Roles.Impostor;
using TownOfUs.Utilities;
using UnityEngine;
using Color = UnityEngine.Color;

namespace TownOfUs.Modifiers.Impostor;

public sealed class GrenadierFlashModifier : DisabledModifier, IDisposable
{
    public override string ModifierName => "Flashed";
    public override bool HideOnUi => true;
    public override float Duration => OptionGroupSingleton<GrenadierOptions>.Instance.GrenadeDuration + 0.5f;
    public override bool AutoStart => true;

    private readonly Color normalVision = new Color(0.83f, 0.83f, 0.83f, 0f);
    private readonly Color dimVision = new Color(0.83f, 0.83f, 0.83f, 0.2f);
    private readonly Color blindVision = new Color(0.83f, 0.83f, 0.83f, 1f);

    private ScreenFlash? flash;

    public override void OnActivate()
    {
        base.OnActivate();

        flash = new();

        if (Player.AmOwner)
        {
            var notif1 = Helpers.CreateAndShowNotification(
                $"<b>{TownOfUsColors.ImpSoft.ToTextColor()}You were flashed by a Grenadier!</color></b>", Color.white, spr: TouRoleIcons.Grenadier.LoadAsset());

            notif1.Text.SetOutlineThickness(0.35f);
            notif1.transform.localPosition = new Vector3(0f, 1f, -20f);
        }
    }

    public override void FixedUpdate()
    {
        base.FixedUpdate();

        if (!Player.IsImpostor() && PlayerControl.LocalPlayer.IsImpostor())
            {
                if (TimeRemaining <= Duration - 0.5f && TimeRemaining >= 0.5f)
                    Player.cosmetics.currentBodySprite.BodySprite.material.SetColor(ShaderID.VisorColor, Color.black);
                else
                    Player.cosmetics.currentBodySprite.BodySprite.material.SetColor(ShaderID.VisorColor, Palette.VisorColor);
            }

        if (PlayerControl.LocalPlayer.PlayerId == Player.PlayerId)
        {
            if (TimeRemaining > Duration - 0.5f)
            {
                float fade = (TimeRemaining - Duration) * -2.0f;

                if (ShouldPlayerBeBlinded(Player))
                {
                    SetFlash(Color.Lerp(normalVision, blindVision, fade));
                }
                else if (ShouldPlayerBeDimmed(Player))
                {
                    SetFlash(Color.Lerp(normalVision, dimVision, fade));
                }
                else
                {
                    SetFlash(normalVision);
                }
            }
            else if (TimeRemaining <= Duration - 0.5f && TimeRemaining >= 0.5f)
            {
                if (ShouldPlayerBeBlinded(Player))
                {
                    SetFlash(blindVision);
                }
                else if (ShouldPlayerBeDimmed(Player))
                {
                    SetFlash(dimVision);
                }
                else
                {
                    SetFlash(normalVision);
                }
            }
            else if (TimeRemaining < 0.5f)
            {
                float fade2 = TimeRemaining * -2.0f + 1.0f;

                if (ShouldPlayerBeBlinded(Player))
                {
                    SetFlash(Color.Lerp(blindVision, normalVision, fade2));
                }
                else if (ShouldPlayerBeDimmed(Player))
                {
                    SetFlash(Color.Lerp(dimVision, normalVision, fade2));
                }
                else
                {
                    SetFlash(normalVision);
                }
            }
            else
            {
                SetFlash(normalVision);

                TimeRemaining = 0.0f;
            }

            if (MeetingHud.Instance)
            {
                SetFlash(normalVision);

                TimeRemaining = 0.0f;
            }
        }
    }

    public override void OnDeactivate()
    {
        if (Player.AmOwner)
        {
            SetFlash(normalVision);

            flash?.Destroy();
        }

        if (!Player.IsImpostor() && PlayerControl.LocalPlayer.IsImpostor())
        {
            Player.cosmetics.currentBodySprite.BodySprite.material.SetColor(ShaderID.VisorColor, Palette.VisorColor);
        }
    }

    private void SetFlash(Color color)
    {
        if (flash != null)
        {
            flash.SetColour(color);
            flash.SetActive(true);

            if (color == normalVision)
            {
                flash.SetActive(false);
            }
        }
    }

    private static bool ShouldPlayerBeDimmed(PlayerControl player)
    {
        return (player.IsImpostor() || player.HasDied()) && !MeetingHud.Instance;
    }

    private static bool ShouldPlayerBeBlinded(PlayerControl player)
    {
        return !player.IsImpostor() && !player.HasDied() && !MeetingHud.Instance;
    }

    public void Dispose() => flash?.Dispose();
}

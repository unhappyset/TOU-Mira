using MiraAPI.GameOptions;
using MiraAPI.Networking;
using MiraAPI.Utilities.Assets;
using TownOfUs.Options;
using TownOfUs.Options.Modifiers.Alliance;
using TownOfUs.Options.Roles.Impostor;
using TownOfUs.Roles.Impostor;
using TownOfUs.Utilities;
using TownOfUs.Utilities.Appearances;
using UnityEngine;

namespace TownOfUs.Buttons.Impostor;

public sealed class WarlockKillButton : TownOfUsRoleButton<WarlockRole, PlayerControl>, IDiseaseableButton
{
    public override string Name => "Kill";
    public override string Keybind => Keybinds.PrimaryAction;
    public override Color TextOutlineColor => TownOfUsColors.Impostor;
    public override float Cooldown => PlayerControl.LocalPlayer.GetKillCooldown() + MapCooldown;
    public override LoadableAsset<Sprite> Sprite => TouAssets.KillSprite;
    public void SetDiseasedTimer(float multiplier)
    {
        SetTimer(Cooldown * multiplier);
    }

    public float Charge { get; set; }
    public bool BurstActive { get; set; }
    public int Kills { get; set; }

    protected override void FixedUpdate(PlayerControl playerControl)
    {
        if (BurstActive)
        {
            Charge -= 100 * Time.fixedDeltaTime / OptionGroupSingleton<WarlockOptions>.Instance.DischargeTimeDuration;

            if (Charge <= 0)
            {
                Charge = 0;
                BurstActive = false;
                SetTimer(Cooldown);
            }
        }
        else
        {
            if (Timer <= 0 && Charge < 100)
            {
                var duration = OptionGroupSingleton<WarlockOptions>.Instance.ChargeTimeDuration * (1f + Kills * OptionGroupSingleton<WarlockOptions>.Instance.AddedTimeDuration);
                var delta = Time.fixedDeltaTime;
                Charge += 100 * delta / duration;

                var num = Mathf.Clamp((100 - Charge) / 100, 0f, 1f);
                Button?.SetCooldownFill(num);
            }
        }

        Button?.usesRemainingText.gameObject.SetActive(Timer <= 0);
        Button?.usesRemainingSprite.gameObject.SetActive(Timer <= 0);
        Button!.usesRemainingText.text = (int)Charge + "%";

        base.FixedUpdate(playerControl);
    }

    public override bool CanUse()
    {
        return base.CanUse() && Charge > 0;
    }

    public override bool CanClick()
    {
        return base.CanClick() && (BurstActive || Charge > 0);
    }

    protected override void OnClick()
    {
        if (Target == null)
        {
            return;
        }

        if (!Target.Data.IsDead) PlayerControl.LocalPlayer.RpcCustomMurder(Target);

        if (Target.Data.IsDead && Charge >= 100 && !BurstActive)
        {
            BurstActive = true;
            Kills = 0;
        }
        else if (Target.Data.IsDead && Charge <= 100 && !BurstActive)
        {
            Charge = 0;
        }
    }

    public override void ClickHandler()
    {
        if (!CanClick())
        {
            return;
        }

        OnClick();
        Button?.SetDisabled();
        if (!BurstActive)
        {
            Timer = Cooldown;
        }
    }

    public override PlayerControl? GetTarget()
    {
        var genOpt = OptionGroupSingleton<GeneralOptions>.Instance;
        var closePlayer = PlayerControl.LocalPlayer.GetClosestLivingPlayer(true, Distance);

        var includePostors = genOpt.FFAImpostorMode ||
            (PlayerControl.LocalPlayer.IsLover() && OptionGroupSingleton<LoversOptions>.Instance.LoverKillTeammates) ||
            (genOpt.KillDuringCamoComms && closePlayer?.GetAppearanceType() == TownOfUsAppearances.Camouflage);
        return PlayerControl.LocalPlayer.GetClosestLivingPlayer(includePostors, Distance);
    }
}

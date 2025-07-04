using MiraAPI.GameOptions;
using MiraAPI.Networking;
using MiraAPI.Utilities;
using MiraAPI.Utilities.Assets;
using Reactor.Utilities;
using TownOfUs.Options.Roles.Neutral;
using TownOfUs.Roles.Neutral;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfUs.Buttons.Neutral;

public sealed class SoulCollectorReapButton : TownOfUsRoleButton<SoulCollectorRole, PlayerControl>, IDiseaseableButton,
    IKillButton
{
    public override string Name => "Reap";
    public override string Keybind => Keybinds.PrimaryAction;
    public override Color TextOutlineColor => TownOfUsColors.SoulCollector;
    public override float Cooldown => OptionGroupSingleton<SoulCollectorOptions>.Instance.KillCooldown + MapCooldown;
    public override LoadableAsset<Sprite> Sprite => TouNeutAssets.ReapSprite;

    public void SetDiseasedTimer(float multiplier)
    {
        SetTimer(Cooldown * multiplier);
    }

    protected override void OnClick()
    {
        if (Target == null)
        {
            Logger<TownOfUsPlugin>.Error("Soul Collector Reap: Target is null");
            return;
        }

        PlayerControl.LocalPlayer.RpcCustomMurder(Target, createDeadBody: false);

        var notif1 = Helpers.CreateAndShowNotification(
            $"<b>{TownOfUsColors.SoulCollector.ToTextColor()}You have taken {Target.Data.PlayerName}'s soul from their body, leaving a soulless player behind.</color></b>",
            Color.white, spr: TouRoleIcons.SoulCollector.LoadAsset());

        notif1.Text.SetOutlineThickness(0.35f);
        notif1.transform.localPosition = new Vector3(0f, 1f, -20f);
    }

    public override PlayerControl? GetTarget()
    {
        return PlayerControl.LocalPlayer.GetClosestLivingPlayer(true, Distance);
    }
}
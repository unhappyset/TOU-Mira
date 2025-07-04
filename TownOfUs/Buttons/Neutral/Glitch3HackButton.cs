using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Modifiers;
using TownOfUs.Utilities;
using MiraAPI.Utilities.Assets;
using Reactor.Utilities;
using TownOfUs.Modifiers.Neutral;
using TownOfUs.Options.Roles.Neutral;
using TownOfUs.Roles.Neutral;
using UnityEngine;
using MiraAPI.Utilities;

namespace TownOfUs.Buttons.Neutral;

public sealed class GlitchHackButton : TownOfUsRoleButton<GlitchRole, PlayerControl>, IAftermathablePlayerButton
{
    public override string Name => "Hack";
    public override string Keybind => "tou.ActionCustom";
    public override Color TextOutlineColor => TownOfUsColors.Glitch;
    public override float Cooldown => OptionGroupSingleton<GlitchOptions>.Instance.HackCooldown + MapCooldown;
    public override LoadableAsset<Sprite> Sprite => TouNeutAssets.HackSprite;
    public override ButtonLocation Location => ButtonLocation.BottomRight;
    public override bool ShouldPauseInVent => false;

    public override PlayerControl? GetTarget() => PlayerControl.LocalPlayer.GetClosestLivingPlayer(true, Distance);

    protected override void OnClick()
    {
        if (Target == null)
        {
            Logger<TownOfUsPlugin>.Error("Glitch Hack: Target is null");
            return;
        }
        var notif1 = Helpers.CreateAndShowNotification($"<b>Once {Target.Data.PlayerName} attempts to use an ability, all their abilities will get disabled.</b>", Color.white, new Vector3(0f, 1f, -20f), spr: TouRoleIcons.Glitch.LoadAsset());
        notif1.Text.SetOutlineThickness(0.35f);

        TouAudio.PlaySound(TouAudio.HackedSound);
        Target.RpcAddModifier<GlitchHackedModifier>(PlayerControl.LocalPlayer.PlayerId);
    }
}

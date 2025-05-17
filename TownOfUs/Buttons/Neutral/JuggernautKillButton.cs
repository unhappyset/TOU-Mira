using MiraAPI.GameOptions;
using MiraAPI.Networking;
using TownOfUs.Utilities;
using MiraAPI.Utilities.Assets;
using Reactor.Utilities;
using TownOfUs.Options.Roles.Neutral;
using TownOfUs.Roles.Neutral;
using UnityEngine;

namespace TownOfUs.Buttons.Neutral;

public sealed class JuggernautKillButton : TownOfUsRoleButton<JuggernautRole, PlayerControl>
{
    public override string Name => "Kill";
    public override string Keybind => "ActionSecondary";
    public override Color TextOutlineColor => TownOfUsColors.Juggernaut;
    public override LoadableAsset<Sprite> Sprite => TouAssets.KillSprite;
    public override float Cooldown => GetCooldown();

    public static float BaseCooldown => OptionGroupSingleton<JuggernautOptions>.Instance.KillCooldown + MapCooldown;

    protected override void OnClick()
    {
        if (Target == null)
        {
            Logger<TownOfUsPlugin>.Error("Juggernaut Shoot: Target is null");
            return;
        }

        PlayerControl.LocalPlayer.RpcCustomMurder(Target);
    }

    public override PlayerControl? GetTarget() => PlayerControl.LocalPlayer.GetClosestLivingPlayer(true, Distance);

    public static float GetCooldown()
    {
        var juggernaut = PlayerControl.LocalPlayer.Data.Role as JuggernautRole;

        if (juggernaut == null) return BaseCooldown;

        var options = OptionGroupSingleton<JuggernautOptions>.Instance;

        return Math.Max(BaseCooldown - (options.KillCooldownReduction * juggernaut.KillCount), 0);
    }
}

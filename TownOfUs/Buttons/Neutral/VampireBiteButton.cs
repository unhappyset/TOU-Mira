using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Networking;
using MiraAPI.Roles;
using MiraAPI.Utilities.Assets;
using Reactor.Utilities;
using TownOfUs.Modifiers.Game.Alliance;
using TownOfUs.Modifiers.Neutral;
using TownOfUs.Modules;
using TownOfUs.Options.Modifiers.Alliance;
using TownOfUs.Options.Roles.Neutral;
using TownOfUs.Roles;
using TownOfUs.Roles.Neutral;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfUs.Buttons.Neutral;

public sealed class VampireBiteButton : TownOfUsRoleButton<VampireRole, PlayerControl>, IDiseaseableButton
{
    public override string Name => "Bite";
    public override string Keybind => "ActionSecondary";
    public override Color TextOutlineColor => TownOfUsColors.Vampire;
    public override float Cooldown => OptionGroupSingleton<VampireOptions>.Instance.BiteCooldown + MapCooldown;
    public override LoadableAsset<Sprite> Sprite => TouNeutAssets.BiteSprite;
    public void SetDiseasedTimer(float multiplier)
    {
        SetTimer(Cooldown * multiplier);
    }

    protected override void FixedUpdate(PlayerControl playerControl)
    {
        var numOfVampiresAlive = CustomRoleUtils.GetActiveRolesOfType<VampireRole>().Count(plr => !plr.Player.HasDied());
        Button?.OverrideText(numOfVampiresAlive < 2 ? "Bite" : "Kill");
    }

    public override bool IsTargetValid(PlayerControl? target)
    {
        return base.IsTargetValid(target) && target != null && (!target.IsRole<VampireRole>() || (PlayerControl.LocalPlayer.IsLover() && OptionGroupSingleton<LoversOptions>.Instance.LoverKillTeammates));
    }

    public override PlayerControl? GetTarget() => PlayerControl.LocalPlayer.GetClosestLivingPlayer(true, Distance, predicate: plr => !plr.IsRole<VampireRole>() || (PlayerControl.LocalPlayer.IsLover() && OptionGroupSingleton<LoversOptions>.Instance.LoverKillTeammates));

    protected override void OnClick()
    {
        if (Target == null)
        {
            Logger<TownOfUsPlugin>.Error("Bite: Target is null");
            return;
        }

        if (ConvertCheck(Target))
            VampireRole.RpcVampireBite(PlayerControl.LocalPlayer, Target);
        else
            PlayerControl.LocalPlayer.RpcCustomMurder(Target);
    }

    private static bool ConvertCheck(PlayerControl target)
    {
        if (target == null) return false;
        if (target.Data.Role is VampireRole) return false;
        if (target.IsImpostor()) return false;
        if (target.Is(RoleAlignment.NeutralKilling)) return false;

        var options = OptionGroupSingleton<VampireOptions>.Instance;
        //var vampireCount = CustomRoleUtils.GetActiveRolesOfType<VampireRole>().Count(x => !x.Player.HasDied());
        var vampireCount = CustomRoleUtils.GetActiveRolesOfType<VampireRole>().Count(); 
        var totalVamps = GameHistory.AllRoles.Count(x => x is VampireRole);

        var canConvertRole = true;

        if (target.HasModifier<LoverModifier>())
        {
            canConvertRole = options.CanConvertLovers;
        }
        else if (target.Is(RoleAlignment.NeutralBenign))
        {
            canConvertRole = options.CanConvertNeutralBenign;
        }
        else if (target.Is(RoleAlignment.NeutralEvil))
        {
            canConvertRole = options.CanConvertNeutralEvil;
        }

        return canConvertRole && vampireCount < 2 && totalVamps < options.MaxVampires && (!PlayerControl.LocalPlayer.HasModifier<VampireBittenModifier>() || options.CanConvertAsNewVamp);
    }
}

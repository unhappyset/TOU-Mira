using System.Collections;
using Il2CppInterop.Runtime;
using MiraAPI.GameOptions;
using MiraAPI.Networking;
using MiraAPI.Utilities;
using MiraAPI.Utilities.Assets;
using Reactor.Utilities;
using TownOfUs.Options.Roles.Crewmate;
using TownOfUs.Roles.Crewmate;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfUs.Buttons.Crewmate;

public sealed class HunterKillButton : TownOfUsRoleButton<HunterRole, PlayerControl>, IDiseaseableButton
{
    public override string Name => "Kill";
    public override string Keybind => "ActionSecondary";
    public override Color TextOutlineColor => TownOfUsColors.Hunter;
    public override float Cooldown => OptionGroupSingleton<HunterOptions>.Instance.HunterKillCooldown + MapCooldown;
    public override LoadableAsset<Sprite> Sprite => TouAssets.KillSprite;
    public void SetDiseasedTimer(float multiplier)
    {
        SetTimer(Cooldown * multiplier);
    }

    private static IEnumerator CoSetBodyReportable(byte bodyId)
    {
        var waitDelegate = DelegateSupport.ConvertDelegate<Il2CppSystem.Func<bool>>(() => Helpers.GetBodyById(bodyId) != null);
        yield return new WaitUntil(waitDelegate);
        var body = Helpers.GetBodyById(bodyId);

        if (body != null)
        {
            body.gameObject.layer = LayerMask.NameToLayer("Ship");
            body.Reported = true;
        }
    }

    protected override void OnClick()
    {
        if (Target == null)
        {
            Logger<TownOfUsPlugin>.Error("Hunter HunterKill: Target is null");
            return;
        }

        PlayerControl.LocalPlayer.RpcCustomMurder(Target);

        if (!OptionGroupSingleton<HunterOptions>.Instance.HunterBodyReport)
        {
            Coroutines.Start(CoSetBodyReportable(Target.PlayerId));
        }
    }

    public override PlayerControl? GetTarget()
    {
        return PlayerControl.LocalPlayer.GetClosestLivingPlayer(true, Distance);
    }

    public override bool IsTargetValid(PlayerControl? target)
    {
        if (!Role.CaughtPlayers.Contains(target!)) return false;

        return base.IsTargetValid(target);
    }
}

using System.Collections;
using Il2CppInterop.Runtime;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Networking;
using MiraAPI.Utilities;
using MiraAPI.Utilities.Assets;
using Reactor.Utilities;
using TownOfUs.Modifiers;
using TownOfUs.Options.Roles.Crewmate;
using TownOfUs.Roles;
using TownOfUs.Roles.Crewmate;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfUs.Buttons.Crewmate;

public sealed class SheriffShootButton : TownOfUsRoleButton<SheriffRole, PlayerControl>
{
    public override string Name => "Shoot";
    public override string Keybind => "ActionSecondary";
    public override Color TextOutlineColor => TownOfUsColors.Sheriff;
    public override float Cooldown => OptionGroupSingleton<SheriffOptions>.Instance.KillCooldown + MapCooldown;
    public override LoadableAsset<Sprite> Sprite => TouCrewAssets.SheriffShootSprite;
    public bool Usable { get; set; } = OptionGroupSingleton<SheriffOptions>.Instance.FirstRoundUse;

    public override bool CanUse()
    {
        return base.CanUse() && Usable;
    }

    private void Misfire()
    {
        if (Target == null)
        {
            Logger<TownOfUsPlugin>.Error("Misfire: Target is null");
            return;
        }

        if (OptionGroupSingleton<SheriffOptions>.Instance.MisfireKillsBoth)
        {
            PlayerControl.LocalPlayer.RpcCustomMurder(Target);
        }

        PlayerControl.LocalPlayer.RpcCustomMurder(PlayerControl.LocalPlayer);
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
            Logger<TownOfUsPlugin>.Error("Sheriff Shoot: Target is null");
            return;
        }

        if (Target.HasModifier<FirstDeadShield>())
        {
            return;
        }

        var alignment = RoleAlignment.CrewmateSupport;
        var options = OptionGroupSingleton<SheriffOptions>.Instance;

        if (Target.Data.Role is ITownOfUsRole touRole)
            alignment = touRole.RoleAlignment;
        else if (Target.IsImpostor())
            alignment = RoleAlignment.ImpostorSupport;

        switch (alignment)
        {
            case RoleAlignment.ImpostorConcealing:
            case RoleAlignment.ImpostorKilling:
            case RoleAlignment.ImpostorSupport:
                PlayerControl.LocalPlayer.RpcCustomMurder(Target);
                break;

            case RoleAlignment.NeutralBenign:
            case RoleAlignment.CrewmateInvestigative:
            case RoleAlignment.CrewmateKilling:
            case RoleAlignment.CrewmateProtective:
            case RoleAlignment.CrewmatePower:
            case RoleAlignment.CrewmateSupport:
                Misfire();
                break;

            case RoleAlignment.NeutralKilling:
                if (!options.ShootNeutralKiller)
                {
                    Misfire();
                }
                else
                {
                    PlayerControl.LocalPlayer.RpcCustomMurder(Target);
                }
                break;

            case RoleAlignment.NeutralEvil:
                if (!options.ShootNeutralEvil)
                {
                    Misfire();
                }
                else
                {
                    PlayerControl.LocalPlayer.RpcCustomMurder(Target);
                }
                break;
            default:
                Misfire();
                break;
        }

        if (!OptionGroupSingleton<SheriffOptions>.Instance.SheriffBodyReport)
        {
            Coroutines.Start(CoSetBodyReportable(Target.PlayerId));
        }
    }

    public override PlayerControl? GetTarget()
    {
        return PlayerControl.LocalPlayer.GetClosestLivingPlayer(true, Distance);
    }
}

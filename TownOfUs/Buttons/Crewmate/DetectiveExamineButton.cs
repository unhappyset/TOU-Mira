using MiraAPI.GameOptions;
using TownOfUs.Utilities;
using MiraAPI.Utilities.Assets;
using TownOfUs.Options.Roles.Crewmate;
using TownOfUs.Roles.Crewmate;
using UnityEngine;

namespace TownOfUs.Buttons.Crewmate;

public sealed class DetectiveExamineButton : TownOfUsRoleButton<DetectiveRole, PlayerControl>
{
    public override string Name => "Examine";
    public override string Keybind => "ActionSecondary";
    public override Color TextOutlineColor => TownOfUsColors.Detective;
    public override float Cooldown => OptionGroupSingleton<DetectiveOptions>.Instance.ExamineCooldown + MapCooldown;
    public override LoadableAsset<Sprite> Sprite => TouCrewAssets.ExamineSprite;

    public override bool CanUse()
    {
        return base.CanUse() && Role is { InvestigatedPlayers.Count: > 0 };
    }

    public override PlayerControl? GetTarget() => PlayerControl.LocalPlayer.GetClosestLivingPlayer(true, Distance);

    protected override void OnClick()
    {
        if (Target == null)
        {
            return;
        }

        Role.ExaminePlayer(Target);
    }
}

using MiraAPI.Utilities;
using MiraAPI.Utilities.Assets;
using TownOfUs.Modules.Components;
using TownOfUs.Roles.Crewmate;
using UnityEngine;

namespace TownOfUs.Buttons.Crewmate;

public sealed class DetectiveInspectButton : TownOfUsRoleButton<DetectiveRole, CrimeSceneComponent>
{
    public override string Name => "Inspect";
    public override string Keybind => Keybinds.SecondaryAction;
    public override Color TextOutlineColor => TownOfUsColors.Detective;
    public override float Cooldown => 1f + MapCooldown;
    public override LoadableAsset<Sprite> Sprite => TouCrewAssets.InspectSprite;

    public override CrimeSceneComponent? GetTarget() => PlayerControl.LocalPlayer.GetNearestObjectOfType<CrimeSceneComponent>(Distance, Helpers.CreateFilter(Constants.NotShipMask));

    public override void SetOutline(bool active)
    {
        // placeholder
    }

    protected override void OnClick()
    {
        if (Target == null)
        {
            return;
        }

        Role.InvestigatingScene = Target;
        Role.InvestigatedPlayers.AddRange(Target.GetScenePlayers());
        var notif1 = Helpers.CreateAndShowNotification($"<b>{TownOfUsColors.Detective.ToTextColor()}You have inspected the crime scene of {Target?.DeadPlayer!.Data.PlayerName} Anyone who gets near the scene or was the killer will flash red when examined.</b></color>", Color.white, new Vector3(0f, 1f, -20f), spr: TouRoleIcons.Detective.LoadAsset());
        notif1.Text.SetOutlineThickness(0.35f);
        TouAudio.PlaySound(TouAudio.QuestionSound);
    }
}

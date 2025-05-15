using MiraAPI.GameOptions;
using MiraAPI.Utilities;
using MiraAPI.Utilities.Assets;
using Reactor.Utilities;
using TownOfUs.Options.Roles.Crewmate;
using TownOfUs.Roles.Crewmate;
using UnityEngine;

namespace TownOfUs.Buttons.Crewmate;

public sealed class PlumberBlockButton : TownOfUsRoleButton<PlumberRole, Vent>
{
    public override string Name => "Barricade";
    public override string Keybind => "ActionSecondary";
    public override Color TextOutlineColor => TownOfUsColors.Plumber;
    public override float Cooldown => OptionGroupSingleton<PlumberOptions>.Instance.FlushCooldown + MapCooldown;
    public override int MaxUses => (int)OptionGroupSingleton<PlumberOptions>.Instance.MaxBarricades;
    public override LoadableAsset<Sprite> Sprite => TouCrewAssets.BarricadeSprite;

    public override bool IsTargetValid(Vent? target)
    {
        return base.IsTargetValid(target) && !Role.VentsBlocked.Contains(target!.Id) && !Role.FutureBlocks.Contains(target.Id);
    }

    private static readonly ContactFilter2D Filter = Helpers.CreateFilter(Constants.NotShipMask);

    public override Vent? GetTarget() => PlayerControl.LocalPlayer.GetNearestObjectOfType<Vent>(Distance, Filter);

    protected override void OnClick()
    {
        if (Target == null)
        {
            Logger<TownOfUsPlugin>.Error($"{Name}: Target is null");
            return;
        }

        var notif1 = Helpers.CreateAndShowNotification($"<b>{TownOfUsColors.Plumber.ToTextColor()}This vent will be blocked at the beginning of the next round.</b></color>", Color.white, new Vector3(0f, 1f, -20f), spr: TouRoleIcons.Plumber.LoadAsset());
        notif1.Text.SetOutlineThickness(0.35f);

        PlumberRole.RpcPlumberBlockVent(PlayerControl.LocalPlayer, Target.Id);
    }
}

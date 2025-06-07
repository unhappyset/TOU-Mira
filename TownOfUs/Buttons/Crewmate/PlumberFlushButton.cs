using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Utilities;
using MiraAPI.Utilities.Assets;
using Reactor.Utilities;
using TownOfUs.Modules;
using TownOfUs.Options.Roles.Crewmate;
using TownOfUs.Roles.Crewmate;
using UnityEngine;

namespace TownOfUs.Buttons.Crewmate;

public sealed class PlumberFlushButton : TownOfUsRoleButton<PlumberRole, Vent>
{
    public override string Name => "Flush";
    public override string Keybind => Keybinds.SecondaryAction;
    public override Color TextOutlineColor => TownOfUsColors.Plumber;
    public override float Cooldown => OptionGroupSingleton<PlumberOptions>.Instance.FlushCooldown + MapCooldown;
    public override LoadableAsset<Sprite> Sprite => TouCrewAssets.FlushSprite;
    private static readonly ContactFilter2D Filter = Helpers.CreateFilter(Constants.NotShipMask);

    protected override void OnClick()
    {
        if (Target == null)
        {
            Logger<TownOfUsPlugin>.Error($"{Name}: Target is null");
            return;
        }

        PlumberRole.RpcPlumberFlush(PlayerControl.LocalPlayer);

        var block = CustomButtonSingleton<PlumberBlockButton>.Instance;

        block?.SetTimer(block.Cooldown);
    }

    public override Vent? GetTarget()
    {
        var vent = PlayerControl.LocalPlayer.GetNearestObjectOfType<Vent>(Distance, Filter);

        if (ModCompatibility.IsSubmerged() && vent != null && (vent.Id == 0 || vent.Id == 14))
        {
            vent = null;
        }

        return vent;
    }
}

using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Utilities;
using MiraAPI.Utilities.Assets;
using TownOfUs.Options.Roles.Crewmate;
using TownOfUs.Roles.Crewmate;
using UnityEngine;

namespace TownOfUs.Buttons.Crewmate;

public sealed class TransporterTransportButton : TownOfUsRoleButton<TransporterRole>
{
    public override string Name => "Transport";
    public override string Keybind => "ActionQuaternary";
    public override Color TextOutlineColor => TownOfUsColors.Transporter;
    public override float Cooldown => OptionGroupSingleton<TransporterOptions>.Instance.TransporterCooldown + MapCooldown;
    public override int MaxUses => (int)OptionGroupSingleton<TransporterOptions>.Instance.MaxNumTransports;
    public override LoadableAsset<Sprite> Sprite => TouCrewAssets.Transport;

    public override void ClickHandler()
    {
        if (!CanClick())
        {
            return;
        }
        OnClick();
    }

    protected override void OnClick()
    {
        var player1Menu = CustomPlayerMenu.Create();

        player1Menu.Begin(
            plr => ((!plr.Data.Disconnected && !plr.Data.IsDead) || Helpers.GetBodyById(plr.PlayerId)) && (plr.moveable || plr.inVent),
            plr =>
            {
                player1Menu.ForceClose();

                if (plr == null) return;

                var player2Menu = CustomPlayerMenu.Create();

                player2Menu.Begin(
                    plr2 => plr2.PlayerId != plr.PlayerId && ((!plr2.Data.Disconnected && !plr2.Data.IsDead) || Helpers.GetBodyById(plr2.PlayerId) || Utilities.MiscUtils.GetFakePlayer(plr2)?.body) && (plr2.moveable || plr2.inVent),
                    plr2 =>
                    {
                        TransporterRole.RpcTransport(PlayerControl.LocalPlayer, plr.PlayerId, plr2!.PlayerId);
                        player2Menu.Close();
                    }
                );
                foreach (var panel in player2Menu.potentialVictims)
                {
                    panel.PlayerIcon.cosmetics.SetPhantomRoleAlpha(1f);
                }
            }
        );
        foreach (var panel in player1Menu.potentialVictims)
        {
            panel.PlayerIcon.cosmetics.SetPhantomRoleAlpha(1f);
        }
    }
}

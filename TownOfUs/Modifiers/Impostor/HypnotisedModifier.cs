using MiraAPI.Modifiers;
using MiraAPI.Utilities;
using TownOfUs.Utilities;
using TownOfUs.Utilities.Appearances;
using UnityEngine;
using Random = UnityEngine.Random;

namespace TownOfUs.Modifiers.Impostor;

public sealed class HypnotisedModifier(PlayerControl hypnotist) : BaseModifier
{
    public override string ModifierName => "Hypnotised";
    public override bool HideOnUi => true;
    public PlayerControl Hypnotist { get; } = hypnotist;

    public bool HysteriaActive { get; set; }
    private List<PlayerControl> players = [];

    public override void OnDeath(DeathReason reason)
    {
        ModifierComponent!.RemoveModifier(this);
    }

    public override void OnDeactivate()
    {
        UnHysteria();

        players.Clear();
    }

    public void Hysteria()
    {
        if (Player.HasDied()) return;
        if (!Player.AmOwner) return;
        if (HysteriaActive) return;

        // Logger<TownOfUsPlugin>.Message($"HypnotisedModifier.Hysteria - {Player.Data.PlayerName}");
        players = PlayerControl.AllPlayerControls.ToArray().Where(x => !x.HasDied() && x != Player).ToList();

        foreach (var player in players)
        {
            int hidden = Random.RandomRangeInt(0, 3);
            if (hidden == 0)
            {
                var morph = new VisualAppearance(Player.GetDefaultModifiedAppearance(), TownOfUsAppearances.Morph);

                player?.RawSetAppearance(morph);
            }
            else if (hidden == 1)
            {
                player.SetCamouflage(true);
            }
            else
            {
                var swoop = new VisualAppearance(player.GetDefaultModifiedAppearance(), TownOfUsAppearances.Swooper)
                {
                    HatId = string.Empty,
                    SkinId = string.Empty,
                    VisorId = string.Empty,
                    PlayerName = string.Empty,
                    PetId = string.Empty,
                    RendererColor = Color.clear,
                    NameColor = Color.clear,
                    ColorBlindTextColor = Color.clear,
                };

                player.RawSetAppearance(swoop);
            }
        }

        if (Player.AmOwner)
        {
            var notif1 = Helpers.CreateAndShowNotification(
                $"<b>{TownOfUsColors.ImpSoft.ToTextColor()}You are under a Mass Hysteria!</color></b>", Color.white, spr: TouRoleIcons.Hypnotist.LoadAsset());

            notif1.Text.SetOutlineThickness(0.35f);
            notif1.transform.localPosition = new Vector3(0f, 1f, -20f);
        }

        HysteriaActive = true;
    }

    public void UnHysteria()
    {
        if (!Player.AmOwner) return;
        if (!HysteriaActive) return;

        // Logger<TownOfUsPlugin>.Message($"HypnotisedModifier.UnHysteria - {Player.Data.PlayerName}");
        foreach (var player in players)
        {
            Player.ResetAppearance();
            player?.cosmetics.ToggleNameVisible(true);
        }

        HysteriaActive = false;
    }
}

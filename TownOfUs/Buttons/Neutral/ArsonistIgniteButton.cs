using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Modifiers;
using MiraAPI.Networking;
using MiraAPI.Utilities;
using MiraAPI.Utilities.Assets;
using Reactor.Networking.Attributes;
using TownOfUs.Modifiers;
using TownOfUs.Modifiers.Neutral;
using TownOfUs.Modules;
using TownOfUs.Options.Roles.Neutral;
using TownOfUs.Roles.Neutral;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfUs.Buttons.Neutral;

public sealed class ArsonistIgniteButton : TownOfUsRoleButton<ArsonistRole>
{
    public override string Name => "Ignite";
    public override string Keybind => "ActionSecondary";
    public override Color TextOutlineColor => TownOfUsColors.Arsonist;
    public override float Cooldown => OptionGroupSingleton<ArsonistOptions>.Instance.DouseCooldown + MapCooldown;
    public override LoadableAsset<Sprite> Sprite => TouNeutAssets.IgniteButtonSprite;

    private static List<PlayerControl> PlayersInRange => Helpers.GetClosestPlayers(PlayerControl.LocalPlayer, OptionGroupSingleton<ArsonistOptions>.Instance.IgniteRadius * ShipStatus.Instance.MaxLightRadius);

    public Ignite? Ignite { get; set; }

    public override bool CanUse()
    {
        var count = PlayersInRange.Count(x => x.HasModifier<ArsonistDousedModifier>());

        if (count > 0 && !PlayerControl.LocalPlayer.HasDied() && Timer <= 0)
        {
            var pos = PlayerControl.LocalPlayer.transform.position;
            pos.z += 0.001f;

            if (Ignite == null)
            {
                Ignite = Ignite.CreateIgnite(pos);
            }
            else
            {
                Ignite.Transform.localPosition = pos;
            }
        }
        else
        {
            if (Ignite != null)
            {
                Ignite.Clear();
                Ignite = null;
            }
        }

        return base.CanUse() && count > 0;
    }

    protected override void OnClick()
    {
        var dousedPlayers = PlayersInRange.Where(x => x.HasModifier<ArsonistDousedModifier>()).ToList();

        PlayerControl.LocalPlayer.RpcAddModifier<IndirectAttackerModifier>(false);
        foreach (var doused in dousedPlayers)
        {
            if (doused.HasModifier<FirstDeadShield>()) continue;

            PlayerControl.LocalPlayer.RpcCustomMurder(doused, resetKillTimer: false, teleportMurderer: false);
            RpcIgniteSound(doused);
        }
        PlayerControl.LocalPlayer.RpcRemoveModifier<IndirectAttackerModifier>();

        TouAudio.PlaySound(TouAudio.ArsoIgniteSound);

        CustomButtonSingleton<ArsonistDouseButton>.Instance.ResetCooldownAndOrEffect();
    }

    [MethodRpc((uint)TownOfUsRpc.IgniteSound, SendImmediately = true)]
    public static void RpcIgniteSound(PlayerControl player)
    {
        if (player.AmOwner) TouAudio.PlaySound(TouAudio.ArsoIgniteSound);
    }
}

using HarmonyLib;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Utilities;
using TownOfUs.Modifiers.Crewmate;
using TownOfUs.Modifiers.Game.Impostor;
using TownOfUs.Options;
using TownOfUs.Options.Roles.Impostor;
using TownOfUs.Roles;
using TownOfUs.Roles.Impostor;
using TownOfUs.Utilities;

namespace TownOfUs.Events.Impostor;

public static class TraitorEvents
{
    [RegisterEvent]
    public static void RoundStartEventHandler(RoundStartEvent @event)
    {
        if (@event.TriggeredByIntro) return;
        var traitor = ModifierUtils.GetActiveModifiers<ToBecomeTraitorModifier>().FirstOrDefault();
        if (traitor != null && traitor.Player.AmOwner)
        {
            var alives = Helpers.GetAlivePlayers().ToList();
            foreach (var player in alives)
            {
                if (player.IsImpostor() || (player.Is(RoleAlignment.NeutralKilling) && OptionGroupSingleton<TraitorOptions>.Instance.NeutralKillingStopsTraitor))
                {
                    return;
                }
            }
            if (PlayerControl.LocalPlayer.Data.IsDead) return;
            if (alives.Count < OptionGroupSingleton<TraitorOptions>.Instance.LatestSpawn) return;
            ToBecomeTraitorModifier.RpcSetTraitor(PlayerControl.LocalPlayer);
            if (OptionGroupSingleton<AssassinOptions>.Instance.TraitorCanAssassin)
            {
                PlayerControl.LocalPlayer.RpcAddModifier<AssassinModifier>();
            }
        }

        if (PlayerControl.LocalPlayer?.Data?.Role is not TraitorRole traitorRole) return;
        traitorRole.UpdateRole();
    }
}

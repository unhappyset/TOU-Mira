using HarmonyLib;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.Events.Vanilla.Player;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Networking;
using MiraAPI.Utilities;
using TownOfUs.Modifiers;
using TownOfUs.Modifiers.Game.Alliance;
using TownOfUs.Options.Modifiers.Alliance;
using TownOfUs.Utilities;

namespace TownOfUs.Events.Modifiers;

public static class LoverEvents
{
    [RegisterEvent(400)]
    public static void PlayerDeathEventHandler(PlayerDeathEvent @event)
    {
        if (!PlayerControl.LocalPlayer.IsHost())
        {
            return;
        }
        if (@event.Player == null || !@event.Player.TryGetModifier<LoverModifier>(out var loveMod)
            || !OptionGroupSingleton<LoversOptions>.Instance.BothLoversDie || loveMod.OtherLover == null
            || loveMod.OtherLover.HasDied() || loveMod.OtherLover.HasModifier<InvulnerabilityModifier>())
        {
            return;
        }
        switch (@event.DeathReason)
        {
            case DeathReason.Exile:
                loveMod.OtherLover.RpcPlayerExile();
                if (loveMod.OtherLover.TryGetModifier<DeathHandlerModifier>(out var deathHandler))
                {
                    deathHandler.CauseOfDeath = "Heartbreak";
                    deathHandler.DiedThisRound = false;
                    deathHandler.RoundOfDeath = DeathEventHandlers.CurrentRound;
                    deathHandler.LockInfo = true;
                }
                break;
            case DeathReason.Kill:
                loveMod.OtherLover.RpcCustomMurder(loveMod.OtherLover);
                if (loveMod.OtherLover.TryGetModifier<DeathHandlerModifier>(out var deathHandler2))
                {
                    deathHandler2.CauseOfDeath = "Heartbreak";
                    deathHandler2.DiedThisRound = !MeetingHud.Instance;
                    deathHandler2.RoundOfDeath = DeathEventHandlers.CurrentRound;
                    deathHandler2.LockInfo = true;
                }
                break;
        }
    }

    [RegisterEvent]
    public static void RoundStartHandler(RoundStartEvent @event)
    {
        ModifierUtils.GetActiveModifiers<LoverModifier>().Do(x => x.OnRoundStart());
    }
}
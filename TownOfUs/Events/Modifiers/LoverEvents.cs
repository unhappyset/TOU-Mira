using HarmonyLib;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.Events.Vanilla.Meeting;
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
    [RegisterEvent]
    public static void AfterMurderEventHandler(AfterMurderEvent @event)
    {
        if (!@event.Target.TryGetModifier<LoverModifier>(out var loveMod) || !PlayerControl.LocalPlayer.IsHost() ||
            loveMod.OtherLover == null || loveMod.OtherLover.HasDied() ||
            loveMod.OtherLover.HasModifier<InvulnerabilityModifier>())
        {
            return;
        }

        if (OptionGroupSingleton<LoversOptions>.Instance.BothLoversDie)
        {
            loveMod.OtherLover.RpcCustomMurder(loveMod.OtherLover);
        }
    }

    [RegisterEvent]
    public static void EjectionEventHandler(EjectionEvent @event)
    {
        var exiled = @event.ExileController?.initData?.networkedPlayer?.Object;
        if (exiled == null || !exiled.TryGetModifier<LoverModifier>(out var loveMod))
        {
            return;
        }

        if (OptionGroupSingleton<LoversOptions>.Instance.BothLoversDie && loveMod.OtherLover != null &&
            !loveMod.OtherLover.HasDied() && !loveMod.OtherLover.HasModifier<InvulnerabilityModifier>())
        {
            loveMod.OtherLover.Exiled();
        }
    }

    [RegisterEvent]
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
                break;
            case DeathReason.Kill:
                loveMod.OtherLover.RpcCustomMurder(loveMod.OtherLover);
                break;
        }
    }

    [RegisterEvent]
    public static void RoundStartHandler(RoundStartEvent @event)
    {
        ModifierUtils.GetActiveModifiers<LoverModifier>().Do(x => x.OnRoundStart());
    }
}
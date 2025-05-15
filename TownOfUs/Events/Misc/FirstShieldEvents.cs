using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.Modifiers;
using TownOfUs.Modifiers;

namespace TownOfUs.Events.Misc;

public static class FirstShieldEvents
{
    [RegisterEvent]
    public static void BeforeMurderEventHandler(BeforeMurderEvent @event)
    {
        CheckForFirstDeathShield(@event, @event.Target, @event.Source);
    }

    [RegisterEvent]
    public static void RemoveShieldEventHandler(RoundStartEvent @event)
    {
        if (!@event.TriggeredByIntro && PlayerControl.LocalPlayer.HasModifier<FirstDeadShield>())
            PlayerControl.LocalPlayer.RpcRemoveModifier<FirstDeadShield>();
    }

    private static void CheckForFirstDeathShield(MiraCancelableEvent @event, PlayerControl target, PlayerControl source)
    {
        if (!target.HasModifier<FirstDeadShield>() || source == target || (source.TryGetModifier<IndirectAttackerModifier>(out var indirect) && indirect.IgnoreShield)) return;

        @event.Cancel();
    }
}

using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using TownOfUs.Modifiers.Game.Universal;

namespace TownOfUs.Events.Modifiers;

public static class ButtonBarryEvents
{
    [RegisterEvent]
    public static void RoundStartHandler(RoundStartEvent @event)
    {
        if (@event.TriggeredByIntro) return; // Never run when round starts.
        ButtonBarryModifier.OnRoundStart();
    }
}
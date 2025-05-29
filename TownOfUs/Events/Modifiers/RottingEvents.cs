using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.Modifiers;
using Reactor.Utilities;
using TownOfUs.Modifiers.Game.Crewmate;

namespace TownOfUs.Events.Modifiers;

public static class RottingEvents
{
    [RegisterEvent]
    public static void AfterMurderEventHandler(AfterMurderEvent @event)
    {
        if (@event.Target.HasModifier<RottingModifier>() && MeetingHud.Instance == null)
        {
            Coroutines.Start(RottingModifier.StartRotting(@event.Target));
        }
    }
}

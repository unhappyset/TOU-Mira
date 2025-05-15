using HarmonyLib;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.Modifiers;
using TownOfUs.Modifiers;

namespace TownOfUs.Events.Misc;

public static class ScatterEvents
{
    [RegisterEvent]
    public static void GameStartHandler(RoundStartEvent @event)
    {
        if (!@event.TriggeredByIntro) return;

        ModifierUtils.GetActiveModifiers<ScatterModifier>().Do(x => x.OnGameStart());
    }

    [RegisterEvent]
    public static void RoundStartHandler(RoundStartEvent @event)
    {
        if (@event.TriggeredByIntro) return;

        ModifierUtils.GetActiveModifiers<ScatterModifier>().Do(x => x.OnRoundStart());
    }
}

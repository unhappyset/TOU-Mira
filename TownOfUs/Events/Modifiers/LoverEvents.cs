using HarmonyLib;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.Events.Vanilla.Meeting;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
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
        if (!@event.Target.TryGetModifier<LoverModifier>(out var loveMod)) return;

        if (OptionGroupSingleton<LoversOptions>.Instance.BothLoversDie) loveMod.KillOther();
    }
    [RegisterEvent]
    public static void EjectionEventHandler(EjectionEvent @event)
    {
        var exiled = @event.ExileController?.initData?.networkedPlayer?.Object;
        if (exiled == null || !exiled.TryGetModifier<LoverModifier>(out var loveMod)) return;

        if (OptionGroupSingleton<LoversOptions>.Instance.BothLoversDie && loveMod.OtherLover != null &&
            !loveMod.OtherLover.HasDied() && !loveMod.OtherLover.HasModifier<InvulnerabilityModifier>())
        {
            loveMod.OtherLover.Exiled();
        }
    }

    [RegisterEvent]
    public static void RoundStartHandler(RoundStartEvent @event)
    {
        ModifierUtils.GetActiveModifiers<LoverModifier>().Do(x => x.OnRoundStart());
    }
}

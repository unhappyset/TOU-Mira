using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.Hud;
using TownOfUs.Buttons.Neutral;
using TownOfUs.Roles.Neutral;

namespace TownOfUs.Events.Neutral;

public static class JuggernautEvents
{
    [RegisterEvent]
    public static void AfterMurderEventHandler(AfterMurderEvent @event)
    {
        var source = @event.Source;
        if (!source.AmOwner || source.Data.Role is not JuggernautRole juggernaut || MeetingHud.Instance != null) return;
        juggernaut.KillCount++;
        CustomButtonSingleton<JuggernautKillButton>.Instance.ResetCooldownAndOrEffect();
    }
}

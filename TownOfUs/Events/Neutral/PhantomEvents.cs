using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Player;
using TownOfUs.Roles.Neutral;

namespace TownOfUs.Events.Neutral;

public static class PhantomEvents
{
    [RegisterEvent]
    public static void CompleteTaskEventHandler(CompleteTaskEvent @event)
    {
        if (@event.Player.Data.Role is not PhantomTouRole phantom) return;

        phantom.CheckTaskRequirements();
    }
}

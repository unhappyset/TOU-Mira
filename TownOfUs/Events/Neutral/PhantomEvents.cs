using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Player;
using MiraAPI.GameOptions;
using MiraAPI.Utilities;
using TownOfUs.Options.Roles.Neutral;
using TownOfUs.Roles.Neutral;
using UnityEngine;

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

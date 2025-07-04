using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Player;
using TownOfUs.Roles.Crewmate;

namespace TownOfUs.Events.Crewmate;

public static class HaunterEvents
{
    [RegisterEvent]
    public static void CompleteTaskEventHandler(CompleteTaskEvent @event)
    {
        if (@event.Player.Data.Role is not HaunterRole haunter)
        {
            return;
        }

        haunter.CheckTaskRequirements();
    }
    // Ideally, haunter shouldn't stay around and should keep the impostor revealed. It also shouldn't show the arrow at all
    /* public static void OnRoundStart(RoundStartEvent @event)
    {
        var haunterList = CustomRoleUtils.GetActiveRolesOfType<HaunterRole>();
        if (!haunterList.Any()) return;

        foreach (var haunter2 in haunterList)
        {
            if (!haunter2.CompletedAllTasks || haunter2.Caught)
            {
                continue;
            }
            var player = haunter2.Player;
            if (player.AmOwner) Patches.HudManagerPatches.ZoomButton.SetActive(true);
            haunter2.Caught = true;
            player.Exiled();

            if (player.AmOwner)
            {
                HudManager.Instance.AbilityButton.SetEnabled();
            }

            if (player.HasModifier<HaunterArrowModifier>()) player.RemoveModifier<HaunterArrowModifier>();
        }
    } */
}
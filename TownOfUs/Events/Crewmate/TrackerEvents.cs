using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Meeting;
using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Roles;
using TownOfUs.Buttons.Crewmate;
using TownOfUs.Options.Roles.Crewmate;
using TownOfUs.Roles.Crewmate;

namespace TownOfUs.Events.Crewmate;

public static class TrackerEvents
{
    [RegisterEvent]
    public static void EjectionEventEventHandler(EjectionEvent @event)
    {
        if (!OptionGroupSingleton<TrackerOptions>.Instance.ResetOnNewRound) return;

        foreach (var tracker in CustomRoleUtils.GetActiveRolesOfType<TrackerTouRole>())
        {
            tracker.Clear();
        }

        var button = CustomButtonSingleton<TrackerTrackButton>.Instance;
        button.SetUses((int)OptionGroupSingleton<TrackerOptions>.Instance.MaxTracks);
        TownOfUsColors.UseBasic = false;
        button.SetTextOutline(button.TextOutlineColor);
        if (button.Button != null) button.Button.usesRemainingSprite.color = button.TextOutlineColor;
        TownOfUsColors.UseBasic = TownOfUsPlugin.UseCrewmateTeamColor.Value;
    }
}

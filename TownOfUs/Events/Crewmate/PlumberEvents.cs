using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Meeting;
using MiraAPI.Events.Vanilla.Player;
using MiraAPI.Events.Vanilla.Usables;
using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Roles;
using TownOfUs.Buttons.Crewmate;
using TownOfUs.Options.Roles.Crewmate;
using TownOfUs.Roles.Crewmate;

namespace TownOfUs.Events.Crewmate;

public static class PlumberEvents
{
    [RegisterEvent]
    public static void CompleteTaskEvent(CompleteTaskEvent @event)
    {
        if (@event.Player.AmOwner && @event.Player.Data.Role is PlumberRole &&
            OptionGroupSingleton<PlumberOptions>.Instance.TaskUses)
        {
            var button = CustomButtonSingleton<PlumberBlockButton>.Instance;
            ++button.UsesLeft;
            ++button.ExtraUses;
            button.SetUses(button.UsesLeft);
        }
    }

    [RegisterEvent]
    public static void PlayerCanUseEventHandler(PlayerCanUseEvent @event)
    {
        if (!@event.IsVent) return;
        var vent = @event.Usable.TryCast<Vent>();

        if (vent == null) return;
        if (CustomRoleUtils.GetActiveRolesOfType<PlumberRole>()
            .Any(plumber => plumber.VentsBlocked.Contains(vent.Id))) @event.Cancel();
    }

    [RegisterEvent]
    public static void EjectionEventHandler(EjectionEvent @event)
    {
        foreach (var plumber in CustomRoleUtils.GetActiveRolesOfType<PlumberRole>()) plumber.SetupBarricades();
    }
}
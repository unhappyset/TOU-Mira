using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.Events.Vanilla.Meeting;
using MiraAPI.Roles;
using TownOfUs.Modules.Components;
using TownOfUs.Roles.Crewmate;
using TownOfUs.Roles.Neutral;
using TownOfUs.Utilities;

namespace TownOfUs.Events.Crewmate;

public static class DetectiveEvents
{
    [RegisterEvent]
    public static void ReportBodyEventHandler(ReportBodyEvent @event)
    {
        if (@event.Target == null) return;

        if (@event.Reporter.Data.Role is DetectiveRole detective)
        {
            detective?.Report(@event.Target.PlayerId);
        }
    }

    [RegisterEvent]
    public static void EjectionEventEventHandler(EjectionEvent @event)
    {
        CrimeSceneComponent.Clear();
    }

    [RegisterEvent]
    public static void AfterMurderEventHandler(AfterMurderEvent @event)
    {
        if (@event.Source.IsRole<SoulCollectorRole>()) return;

        var detectives = CustomRoleUtils.GetActiveRolesOfType<DetectiveRole>();
        if (!detectives.Any()) return;

        var victim = @event.Target;
        var bodyPos = victim.transform.position;
        bodyPos.y -= 0.3f;
        bodyPos.x -= 0.11f;

        CrimeSceneComponent.CreateCrimeScene(victim, bodyPos);
    }
}

using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.Events.Vanilla.Meeting;
using MiraAPI.Hud;
using MiraAPI.Modifiers;
using MiraAPI.Roles;
using TownOfUs.Modifiers;
using TownOfUs.Modifiers.Crewmate;
using TownOfUs.Roles.Crewmate;
using TownOfUs.Utilities;

namespace TownOfUs.Events.Crewmate;

public static class MedicEvents
{
    [RegisterEvent]
    public static void RoundStartHandler(RoundStartEvent @event)
    {
        if (PlayerControl.LocalPlayer.Data.Role is MedicRole) MedicRole.OnRoundStart();
    }
    [RegisterEvent]
    public static void BeforeMurderEventHandler(BeforeMurderEvent @event)
    {
        var source = @event.Source;
        var target = @event.Target;

        if (CheckForMedicShield(@event, source, target))
        {
            ResetButtonTimer(source);
        }
    }

    [RegisterEvent]
    public static void AfterMurderEventHandler(AfterMurderEvent @event)
    {
        var victim = @event.Target;

        foreach (var medic in CustomRoleUtils.GetActiveRolesOfType<MedicRole>())
        {
            if (victim == medic.Shielded)
                medic.Clear();
        }
    }

    [RegisterEvent]
    public static void ReportBodyEventHandler(ReportBodyEvent @event)
    {
        if (@event.Target == null) return;

        if (@event.Reporter.Data.Role is MedicRole medic)
        {
            medic?.Report(@event.Target.PlayerId);
        }
    }

    private static bool CheckForMedicShield(MiraCancelableEvent @event, PlayerControl source, PlayerControl target)
    {
        if (!target.HasModifier<MedicShieldModifier>() || 
            MeetingHud.Instance ||
            source == null ||
            target.PlayerId == source.PlayerId || 
            (source.TryGetModifier<IndirectAttackerModifier>(out var indirect) && indirect.IgnoreShield))
        {
            return false;
        }

        @event.Cancel();

        var medic = target.GetModifier<MedicShieldModifier>()?.Medic.GetRole<MedicRole>();

        if (medic != null && source.AmOwner)
        {
            MedicRole.RpcMedicShieldAttacked(medic.Player, source, target);
        }

        return true;
    }

    private static void ResetButtonTimer(PlayerControl source, CustomActionButton<PlayerControl>? button = null)
    {
        button?.SetTimer(button.Cooldown);

        // Reset impostor kill cooldown if they attack a shielded player
        if (!source.AmOwner || !source.IsImpostor()) return;

        var killCooldown = source.GetKillCooldown(); // GameOptionsManager.Instance.currentNormalGameOptions.KillCooldown;
        source.SetKillTimer(killCooldown);
    }
}

using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Meeting;
using MiraAPI.Events.Vanilla.Meeting.Voting;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using TownOfUs.Modifiers.Game;
using TownOfUs.Options.Roles.Crewmate;
using TownOfUs.Roles.Crewmate;
using TownOfUs.Utilities;

namespace TownOfUs.Events.Crewmate;

public static class ProsecutorEvents
{
    [RegisterEvent]
    public static void VoteEvent(CheckForEndVotingEvent @event)
    {
        if (!@event.IsVotingComplete)
        {
            return;
        }

        var prosecutor = CustomRoleUtils.GetActiveRolesOfType<ProsecutorRole>()
                            .FirstOrDefault(x => !x.Player.HasDied() && x.HasProsecuted && x.ProsecuteVictim != byte.MaxValue);

        if (prosecutor == null)
        {
            return;
        }

        if (prosecutor.ProsecutionsCompleted >=
            OptionGroupSingleton<ProsecutorOptions>.Instance.MaxProsecutions)
        {
            return;
        }

        foreach (var plr in PlayerControl.AllPlayerControls.ToArray())
        {
            plr.GetVoteData().Votes.Clear();
            plr.GetVoteData().VotesRemaining = 0;
        }

        var prosdata = prosecutor.Player.GetVoteData();

        for (var i = 0; i < 5; i++)
        {
            prosdata.VoteForPlayer(prosecutor.ProsecuteVictim);
        }
    }

    [RegisterEvent]
    public static void WrapUpEvent(EjectionEvent @event)
    {
        var player = @event.ExileController.initData.networkedPlayer?.Object;
        if (player == null)
        {
            return;
        }

        foreach (var pros in CustomRoleUtils.GetActiveRolesOfType<ProsecutorRole>())
        {
            pros.Cleanup();
            if (pros.Player.TryGetModifier<AllianceGameModifier>(out var allyMod) && !allyMod.GetsPunished)
            {
                return;
            }

            if (player.TryGetModifier<AllianceGameModifier>(out var allyMod2) && !allyMod2.GetsPunished)
            {
                return;
            }

            if (pros.HasProsecuted && player.IsCrewmate())
            {
                if (OptionGroupSingleton<ProsecutorOptions>.Instance.ExileOnCrewmate)
                {
                    pros.Player.Exiled();
                }
                else
                {
                    pros.ProsecutionsCompleted = (int)OptionGroupSingleton<ProsecutorOptions>.Instance.MaxProsecutions;
                }
            }
        }
    }
}
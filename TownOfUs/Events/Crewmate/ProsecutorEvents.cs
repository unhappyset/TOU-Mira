using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Meeting;
using MiraAPI.Events.Vanilla.Meeting.Voting;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using TownOfUs.Modifiers;
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

    [RegisterEvent(400)]
    public static void WrapUpEvent(EjectionEvent @event)
    {
        var player = @event.ExileController.initData.networkedPlayer?.Object;
        if (player == null)
        {
            return;
        }

        foreach (var pros in CustomRoleUtils.GetActiveRolesOfType<ProsecutorRole>())
        {
            if (PlayerControl.LocalPlayer.IsHost() && pros.HasProsecuted && player.TryGetModifier<DeathHandlerModifier>(out var deathHandler) && !deathHandler.LockInfo)
            {
                DeathHandlerModifier.RpcUpdateDeathHandler(player, "Prosecuted", DeathEventHandlers.CurrentRound, DeathHandlerOverride.SetFalse, $"By {pros.Player.Data.PlayerName}", lockInfo: DeathHandlerOverride.SetTrue);
            }

            if (pros.Player.TryGetModifier<AllianceGameModifier>(out var allyMod) && !allyMod.GetsPunished)
            {
                pros.Cleanup();
                return;
            }

            if (player.TryGetModifier<AllianceGameModifier>(out var allyMod2) && !allyMod2.GetsPunished)
            {
                pros.Cleanup();
                return;
            }

            if (pros.HasProsecuted && player.IsCrewmate())
            {
                pros.Cleanup();
                if (OptionGroupSingleton<ProsecutorOptions>.Instance.ExileOnCrewmate)
                {
                    pros.Player.Exiled();
                    if (PlayerControl.LocalPlayer.IsHost()) DeathHandlerModifier.RpcUpdateDeathHandler(pros.Player, "Punished", DeathEventHandlers.CurrentRound, DeathHandlerOverride.SetFalse, lockInfo: DeathHandlerOverride.SetTrue);
                }
                else
                {
                    pros.ProsecutionsCompleted = (int)OptionGroupSingleton<ProsecutorOptions>.Instance.MaxProsecutions;
                }
            }
        }
    }
}
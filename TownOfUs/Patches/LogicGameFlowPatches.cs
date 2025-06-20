using HarmonyLib;
using MiraAPI.GameEnd;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using Reactor.Utilities;
using TownOfUs.GameOver;
using TownOfUs.Modifiers.Crewmate;
using TownOfUs.Modifiers.Game;
using TownOfUs.Modifiers.Game.Alliance;
using TownOfUs.Options.Roles.Impostor;
using TownOfUs.Roles;
using TownOfUs.Utilities;

namespace TownOfUs.Patches;

[HarmonyPatch]
public static class LogicGameFlowPatches
{
    [HarmonyPatch(typeof(GameData), nameof(GameData.RecomputeTaskCounts))]
    [HarmonyPrefix]
    private static bool RecomputeTasksPatch(GameData __instance)
    {
        if (__instance == null) return false;
        __instance.TotalTasks = 0;
        __instance.CompletedTasks = 0;
        for (var i = 0; i < __instance.AllPlayers.Count; i++)
        {
            var playerInfo = __instance.AllPlayers.ToArray()[i];
            if (!playerInfo.Disconnected && playerInfo.Tasks != null && playerInfo.Object &&
                (GameOptionsManager.Instance.currentNormalGameOptions.GhostsDoTasks || !playerInfo.IsDead) && !playerInfo._object.IsImpostor() &&
                !(
                    (playerInfo._object.TryGetModifier<AllianceGameModifier>(out var allyMod) && !allyMod.DoesTasks)
                    || !playerInfo._object.Data.Role.TasksCountTowardProgress
                ))
            {
                for (var j = 0; j < playerInfo.Tasks.Count; j++)
                {
                    __instance.TotalTasks++;
                    if (playerInfo.Tasks.ToArray()[j].Complete) __instance.CompletedTasks++;
                }
            }
        }

        if (__instance.TotalTasks == 0) __instance.TotalTasks = 1; // This results in avoiding unfair task wins by essentially defaulting to 0/1 which can never lead to a win

        return false;
    }
    public static bool CheckEndGameViaTasks(LogicGameFlowNormal instance)
    {
        GameData.Instance.RecomputeTaskCounts();

        if (GameData.Instance.TotalTasks > 0 && GameData.Instance.TotalTasks <= GameData.Instance.CompletedTasks)
        {
            instance.Manager.RpcEndGame(GameOverReason.CrewmatesByTask, false);

            return true;
        }

        return false;
    }

    [HarmonyPatch(typeof(LogicGameFlowNormal), nameof(LogicGameFlowNormal.CheckEndCriteria))]
    [HarmonyPrefix]
    public static bool CheckEndCriteriaPatch(LogicGameFlowNormal __instance)
    {
        if (TutorialManager.InstanceExists)
        {
            return true;
        }

        if (!AmongUsClient.Instance.AmHost)
        {
            return false;
        }

        if (!GameData.Instance)
        {
            return false;
        }

        if (ShipStatus.Instance.Systems.ContainsKey(SystemTypes.LifeSupp))
        {
            var lifeSuppSystemType = ShipStatus.Instance.Systems[SystemTypes.LifeSupp].Cast<LifeSuppSystemType>();
            if (lifeSuppSystemType is { Countdown: < 0f })
            {
                __instance.EndGameForSabotage();
                lifeSuppSystemType.Countdown = 10000f;

                return false;
            }
        }

        foreach (ISystemType systemType2 in ShipStatus.Instance.Systems.Values)
        {
            var sabo = systemType2.TryCast<ICriticalSabotage>();
            if (sabo == null) continue;
            ICriticalSabotage criticalSabotage = sabo;
            if (criticalSabotage != null && criticalSabotage.Countdown < 0f)
            {
                __instance.EndGameForSabotage();
                criticalSabotage.ClearSabotage();
            }
        }

        if (CheckEndGameViaTasks(__instance))
        {
            return false;
        }

        // End game if there are 3 players alive and 2 are lovers.
        var activeLovers = ModifierUtils.GetActiveModifiers<LoverModifier>().ToArray();
        if (!ExileController.Instance && LoverModifier.WinConditionMet(activeLovers))
        {
            CustomGameOver.Trigger<LoverGameOver>(activeLovers.Select(x => x.Player.Data).ToArray());
            return false;
        }

        // If any neutral win condition is met -> game over
        // Using RoleAlignment as a quick and basic way to prioritise NeutralEvil wins over NeutralKiller wins
        if (CustomRoleUtils.GetActiveRolesOfTeam(ModdedRoleTeams.Custom)
                .OrderBy(x => (x as ITownOfUsRole)!.RoleAlignment)
                .FirstOrDefault(x => x is ITownOfUsRole role && role.WinConditionMet()) is { } winner)
        {
            Logger<TownOfUsPlugin>.Message($"Game Over");
            CustomGameOver.Trigger<NeutralGameOver>([winner.Player.Data]);

            return false;
        }

        // Prevents game end when all impostors are dead but there are neutral killers left alive
        if (MiscUtils.NKillersAliveCount > 0 || (MiscUtils.ImpAliveCount > 0 && MiscUtils.CrewKillersAliveCount > 0))
        {
            return false;
        }
        // Prevents game end when all impostors are dead but there is a possibility for a traitor to spawn given the conditions
        var possibleTraitor = ModifierUtils.GetActiveModifiers<ToBecomeTraitorModifier>().FirstOrDefault();
        if (Helpers.GetAlivePlayers().Count > (int)OptionGroupSingleton<TraitorOptions>.Instance.LatestSpawn - 1 && possibleTraitor != null)
        {
            return false;
        }

        return true;
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(LogicGameFlowNormal), nameof(LogicGameFlowNormal.IsGameOverDueToDeath))]
    public static void Postfix(LogicGameFlowNormal __instance, ref bool __result)
    {
        __result = false;
    }
}

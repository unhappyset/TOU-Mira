using HarmonyLib;
using MiraAPI.GameOptions;
using TownOfUs.Options;
using TaskLength = NormalPlayerTask.TaskLength;

namespace TownOfUs.Patches.Options;

[HarmonyPatch]
public static class TaskAssignmentPatch
{
    [HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.AddTasksFromList))]
    [HarmonyPrefix]
    public static void Prefix(ref int count, ref Il2CppSystem.Collections.Generic.List<NormalPlayerTask> unusedTasks)
    {
        var type = unusedTasks[0].Length;

        if (type is TaskLength.Short)
        {
            count += OptionGroupSingleton<TownOfUsMapOptions>.Instance.GetMapBasedShortTasks();
            count = Math.Clamp(count, 0, 8);
        }
        else if (type is TaskLength.Long)
        {
            count += OptionGroupSingleton<TownOfUsMapOptions>.Instance.GetMapBasedLongTasks();
            count = Math.Clamp(count, 0, 4);
        }
    }

    [HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.Begin))]
    [HarmonyPrefix]
    public static void Prefix()
    {
        var options = GameOptionsManager.Instance.currentNormalGameOptions;
        options.NumCommonTasks = Math.Clamp(options.NumCommonTasks, 0, 4);
        options.NumShortTasks = Math.Clamp(options.NumShortTasks, 0, 8);
        options.NumLongTasks = Math.Clamp(options.NumLongTasks, 0, 4);
    }
}
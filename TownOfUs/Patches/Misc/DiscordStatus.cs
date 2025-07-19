using Discord;
using HarmonyLib;

namespace TownOfUs.Patches.Misc;

[HarmonyPatch]
public static class DiscordStatus
{
    [HarmonyPatch(typeof(ActivityManager), nameof(ActivityManager.UpdateActivity))]
    [HarmonyPrefix]
    public static void Prefix([HarmonyArgument(0)] Activity activity)
    {
        activity.Details += $" - Town of Us Mira v{TownOfUsPlugin.Version}" + (TownOfUsPlugin.IsDevBuild ? " (DEV)" : string.Empty);
    }
}
using HarmonyLib;
using TownOfUs.Modules;

namespace TownOfUs.Patches;

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.Die))]
public static class FirstDeadPatch
{
    public static string? PlayerName { get; set; }

    public static void Postfix(PlayerControl __instance, DeathReason reason)
    {
        PlayerName ??= __instance.name;
        GameHistory.DeathHistory.Add((__instance.PlayerId, reason));
    }
}

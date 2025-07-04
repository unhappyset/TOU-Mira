using HarmonyLib;
using TownOfUs.Roles;
using UnityEngine;
using Object = UnityEngine.Object;

namespace TownOfUs.Patches.Roles;

[HarmonyPatch]
public static class GhostRoleUsePatches
{
    [HarmonyPrefix]
    [HarmonyPatch(typeof(DeconControl), nameof(DeconControl.Use))]
    public static bool DeconControlUsePatch(DeconControl __instance)
    {
        // DeconControl.CanUse was inlined, so we "uninline" it here
        __instance.CanUse(PlayerControl.LocalPlayer.Data, out var canUse, out _);
        if (!canUse)
        {
            return false;
        }

        __instance.cooldown = 6f;
        if (Constants.ShouldPlaySfx())
        {
            SoundManager.Instance.PlaySound(__instance.UseSound, false);
        }

        __instance.OnUse.Invoke();

        return false;
    }

    [HarmonyPatch(typeof(MovingPlatformBehaviour))]
    [HarmonyPatch(nameof(MovingPlatformBehaviour.Use), typeof(PlayerControl))]
    [HarmonyPrefix]
    public static bool MovingPlatformBehaviourUsePrefixPatch(Il2CppSystem.Object __instance,
        [HarmonyArgument(0)] PlayerControl player, ref bool __state)
    {
        __state = false;

        if (player.Data.Role is IGhostRole { GhostActive: true } && player.Data.IsDead)
        {
            // Logger<TownOfUsPlugin>.Message($"CanUsePrefixPatch IsDead");
            player.Data.IsDead = false;
            __state = true;
        }

        return true;
    }

    [HarmonyPatch(typeof(MovingPlatformBehaviour))]
    [HarmonyPatch(nameof(MovingPlatformBehaviour.Use), typeof(PlayerControl))]
    [HarmonyPostfix]
    public static void MovingPlatformBehaviourUsePostfixPatch(Il2CppSystem.Object __instance,
        [HarmonyArgument(0)] PlayerControl player, ref bool __state)
    {
        if (__state)
            // Logger<TownOfUsPlugin>.Message($"CanUsePostfixPatch IsDead");
        {
            player.Data.IsDead = true;
        }
    }

    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.CheckUseZipline))]
    [HarmonyPrefix]
    public static void CheckUseZiplinePrefixPatch(PlayerControl target, ref bool __state)
    {
        __state = false;
        var targetData = target.CachedPlayerData;

        if (target.Data.Role is IGhostRole { GhostActive: true } && target.Data.IsDead)
        {
            targetData.IsDead = false;
            __state = true;
        }
    }

    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.CheckUseZipline))]
    [HarmonyPostfix]
    public static void CheckUseZiplinePostfixPatch(PlayerControl target, ref bool __state)
    {
        var targetData = target.CachedPlayerData;

        if (__state)
        {
            targetData.IsDead = true;
        }
    }

    [HarmonyPatch(typeof(OpenDoorConsole), nameof(OpenDoorConsole.Use))]
    [HarmonyPrefix]
    public static bool OpenDoorConsoleUsePrefixPatch(OpenDoorConsole __instance)
    {
        __instance.CanUse(PlayerControl.LocalPlayer.Data, out var canUse, out _);

        if (!canUse)
        {
            return false;
        }

        __instance.myDoor.SetDoorway(true);

        return false;
    }

    [HarmonyPatch(typeof(DoorConsole), nameof(DoorConsole.Use))]
    [HarmonyPrefix]
    public static bool DoorConsoleUsePrefixPatch(DoorConsole __instance)
    {
        __instance.CanUse(PlayerControl.LocalPlayer.Data, out var canUse, out _);

        if (!canUse)
        {
            return false;
        }

        PlayerControl.LocalPlayer.NetTransform.Halt();
        var minigame = Object.Instantiate(__instance.MinigamePrefab, Camera.main.transform);
        minigame.transform.localPosition = new Vector3(0f, 0f, -50f);

        try
        {
            minigame.Cast<IDoorMinigame>().SetDoor(__instance.MyDoor);
        }
        catch (InvalidCastException)
        {
            /* ignored */
        }

        minigame.Begin(null);

        return false;
    }
}
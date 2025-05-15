using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using HarmonyLib;

namespace TownOfUs.Patches.Stubs;

[HarmonyPatch]
[SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "Stub methods.")]
public static class RoleStubs
{
    [HarmonyReversePatch]
    [HarmonyPatch(typeof(RoleBehaviour), nameof(RoleBehaviour.Initialize))]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void RoleBehaviourInitialize(RoleBehaviour __instance, PlayerControl player)
    {
        // nothing needed
    }

    [HarmonyReversePatch]
    [HarmonyPatch(typeof(RoleBehaviour), nameof(RoleBehaviour.Deinitialize))]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void RoleBehaviourDeinitialize(RoleBehaviour instance, PlayerControl player)
    {
        // nothing needed
    }

    [HarmonyReversePatch]
    [HarmonyPatch(typeof(RoleBehaviour), nameof(RoleBehaviour.OnDeath))]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void RoleBehaviourOnDeath(RoleBehaviour instance, DeathReason reason)
    {
        // nothing needed
    }

    [HarmonyReversePatch]
    [HarmonyPatch(typeof(RoleBehaviour), nameof(RoleBehaviour.OnMeetingStart))]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void RoleBehaviourOnMeetingStart(RoleBehaviour instance)
    {
        // nothing needed
    }

    [HarmonyReversePatch]
    [HarmonyPatch(typeof(RoleBehaviour), nameof(RoleBehaviour.OnVotingComplete))]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void RoleBehaviourOnVotingComplete(RoleBehaviour instance)
    {
        // nothing needed
    }
}

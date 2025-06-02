using HarmonyLib;
using MiraAPI.Roles;
using Reactor.Utilities.Extensions;
using TownOfUs.Roles.Impostor;
using TownOfUs.Roles.Neutral;
using TownOfUs.Utilities;

namespace TownOfUs.Patches;

[HarmonyPatch(typeof(DummyBehaviour))]
public static class DummyBehaviourPatches
{
    [HarmonyPostfix]
    [HarmonyPatch(nameof(DummyBehaviour.Start))]
    public static void DummyStartPatch(DummyBehaviour __instance)
    {
        var roleList = MiscUtils.AllRoles
            .Where(role => role is ICustomRole)
            .Where(role => role is not TraitorRole)
            .Where(role => role is not NeutralGhostRole)
            .Where(role => !role.TryCast<CrewmateGhostRole>())
            .Where(role => !role.TryCast<ImpostorGhostRole>())
            .ToList();

        var roleType = RoleId.Get(roleList.Random()!.GetType());
        __instance.myPlayer.RpcChangeRole(roleType, false);

        __instance.myPlayer.RpcSetName(AccountManager.Instance.GetRandomName());
    }
}
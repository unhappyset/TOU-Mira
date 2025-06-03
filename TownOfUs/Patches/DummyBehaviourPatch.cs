using HarmonyLib;
using MiraAPI.Roles;
using Reactor.Utilities.Extensions;
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
            .Where(role => !role.IsImpostor())
            .Where(role => role is not NeutralGhostRole)
            .Where(role => !role.TryCast<CrewmateGhostRole>())
            .Where(role => !role.TryCast<ImpostorGhostRole>())
            .ToList();

        var roleType = RoleId.Get(roleList.Random()!.GetType());
        __instance.myPlayer.RpcChangeRole(roleType, false);

        __instance.myPlayer.RpcSetName(AccountManager.Instance.GetRandomName());

        __instance.myPlayer.SetSkin(HatManager.Instance.allSkins[UnityEngine.Random.Range(0, HatManager.Instance.allSkins.Count)].ProdId, 0);
        __instance.myPlayer.SetNamePlate(HatManager.Instance.allNamePlates[UnityEngine.Random.RandomRangeInt(0, HatManager.Instance.allNamePlates.Count)].ProdId);
        __instance.myPlayer.SetPet(HatManager.Instance.allPets[UnityEngine.Random.RandomRangeInt(0, HatManager.Instance.allPets.Count)].ProdId);
        var colorId = UnityEngine.Random.Range(0, Palette.PlayerColors.Length);
        __instance.myPlayer.SetColor(colorId);
        __instance.myPlayer.SetHat(HatManager.Instance.allHats[UnityEngine.Random.RandomRangeInt(0, HatManager.Instance.allHats.Count)].ProdId, colorId);
        __instance.myPlayer.SetVisor(HatManager.Instance.allVisors[UnityEngine.Random.RandomRangeInt(0, HatManager.Instance.allVisors.Count)].ProdId, colorId);
    }
}
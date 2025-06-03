using HarmonyLib;
using MiraAPI.Modifiers;
using MiraAPI.Roles;
using Reactor.Utilities.Extensions;
using TownOfUs.Modifiers.Game;
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
        var dum = __instance.myPlayer;
        dum.RpcChangeRole(roleType, false);

        dum.RpcSetName(AccountManager.Instance.GetRandomName());

        dum.SetSkin(HatManager.Instance.allSkins[UnityEngine.Random.Range(0, HatManager.Instance.allSkins.Count)].ProdId, 0);
        dum.SetNamePlate(HatManager.Instance.allNamePlates[UnityEngine.Random.RandomRangeInt(0, HatManager.Instance.allNamePlates.Count)].ProdId);
        dum.SetPet(HatManager.Instance.allPets[UnityEngine.Random.RandomRangeInt(0, HatManager.Instance.allPets.Count)].ProdId);
        var colorId = UnityEngine.Random.Range(0, Palette.PlayerColors.Length);
        dum.SetColor(colorId);
        dum.SetHat(HatManager.Instance.allHats[UnityEngine.Random.RandomRangeInt(0, HatManager.Instance.allHats.Count)].ProdId, colorId);
        dum.SetVisor(HatManager.Instance.allVisors[UnityEngine.Random.RandomRangeInt(0, HatManager.Instance.allVisors.Count)].ProdId, colorId);

        var randomUniMod = MiscUtils.AllModifiers.Where(x => x is UniversalGameModifier touGameMod && touGameMod.IsModifierValidOn(dum.Data.Role)).Random();
        if (randomUniMod != null) dum.RpcAddModifier(randomUniMod.GetType());
        
        var randomTeamMod = MiscUtils.AllModifiers.Where(x => x is TouGameModifier touGameMod && touGameMod.IsModifierValidOn(dum.Data.Role)).Random();
        if (randomTeamMod != null) dum.RpcAddModifier(randomTeamMod.GetType());
    }
}
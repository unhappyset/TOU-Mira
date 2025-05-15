using HarmonyLib;
using MiraAPI.GameOptions;
using TownOfUs.Options;

namespace TownOfUs
{
    [HarmonyPatch]

    public static class AirshipDoors
    {
        [HarmonyPatch(typeof(AirshipStatus), nameof(AirshipStatus.OnEnable))]
        [HarmonyPostfix]

        public static void Postfix(AirshipStatus __instance)
        {
            if (!OptionGroupSingleton<BetterMapOptions>.Instance.AirshipPolusDoors) return;

            var polusdoor = PrefabLoader.Polus.GetComponentInChildren<DoorConsole>().MinigamePrefab;
            foreach (var door in __instance.GetComponentsInChildren<DoorConsole>())
            {
                door.MinigamePrefab = polusdoor;
            }
        }
    }
}
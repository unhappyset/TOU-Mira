using HarmonyLib;
using TownOfUs.Modules.RainbowMod;
using UnityEngine;

namespace TownOfUs.RainbowMod;

[HarmonyPatch(typeof(PlayerMaterial), nameof(PlayerMaterial.SetColors), typeof(int), typeof(Renderer))]
public static class SetPlayerMaterialPatch
{
    public static bool Prefix([HarmonyArgument(0)] int colorId, [HarmonyArgument(1)] Renderer rend)
    {
        var r = rend.gameObject.GetComponent<RainbowBehaviour>();
        if (r == null)
        {
            r = rend.gameObject.AddComponent<RainbowBehaviour>();
        }

        r.AddRend(rend, colorId);
        return !RainbowUtils.IsRainbow(colorId);
    }
}

[HarmonyPatch(typeof(PlayerMaterial), nameof(PlayerMaterial.SetColors), typeof(Color), typeof(Renderer))]
public static class SetPlayerMaterialPatch2
{
    public static bool Prefix([HarmonyArgument(1)] Renderer rend)
    {
        var r = rend.gameObject.GetComponent<RainbowBehaviour>();
        if (r == null)
        {
            r = rend.gameObject.AddComponent<RainbowBehaviour>();
        }

        r.AddRend(rend, 0);
        return true;
    }
}

[HarmonyPatch(typeof(PlayerTab))]
public static class PlayerTabPatch
{
    [HarmonyPatch(nameof(PlayerTab.Update))]
    [HarmonyPostfix]
    public static void UpdatePostfix(PlayerTab __instance)
    {
        for (var i = 0; i < __instance.ColorChips.Count; i++)
        {
            if (RainbowUtils.IsRainbow(i))
            {
                __instance.ColorChips[i].Inner.SpriteColor = RainbowUtils.Rainbow;
                break;
            }
        }
    }
}
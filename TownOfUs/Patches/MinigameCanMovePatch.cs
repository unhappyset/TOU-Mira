using HarmonyLib;
using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Modifiers;
using TownOfUs.Buttons.Modifiers;
using TownOfUs.Modifiers.Game.Crewmate;
using TownOfUs.Options.Modifiers.Crewmate;

namespace TownOfUs.Patches;

[HarmonyPatch]
public static class MinigameCanMovePatch
{
    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.CanMove), MethodType.Getter)]
    [HarmonyPrefix]
    public static bool PlayerControlCanMovePatch(PlayerControl __instance, ref bool __result)
    {
        // Only allows Scientist Vitals to allow you to move, not just vitals on the map
        if (PlayerControl.LocalPlayer.HasModifier<ScientistModifier>() && Minigame.Instance is VitalsMinigame && CustomButtonSingleton<ScientistButton>.Instance.EffectActive && OptionGroupSingleton<ScientistOptions>.Instance.MoveWithMenu)
        {
            __result = __instance.moveable;
            return false;
        }

        return true;
    }
}
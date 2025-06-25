using HarmonyLib;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using TownOfUs.Modifiers;
using TownOfUs.Options;
using TownOfUs.Options.Modifiers.Alliance;
using TownOfUs.Utilities;
using TownOfUs.Utilities.Appearances;

namespace TownOfUs.Patches.Options;

// Is there a better way I can do this??
[HarmonyPatch(typeof(ImpostorRole), nameof(ImpostorRole.IsValidTarget))]
public static class ImpostorTargeting
{
    public static void Postfix(ImpostorRole __instance, NetworkedPlayerInfo target, ref bool __result)
    {
        var genOpt = OptionGroupSingleton<GeneralOptions>.Instance;

        __result &= !(target?.Object?.TryGetModifier<DisabledModifier>(out var mod) == true && !mod.CanBeInteractedWith) &&
            (target?.Object?.IsImpostor() == false ||
            genOpt.FFAImpostorMode ||
            (PlayerControl.LocalPlayer.IsLover() && OptionGroupSingleton<LoversOptions>.Instance.LoverKillTeammates) ||
            (genOpt.KillDuringCamoComms && target?.Object?.GetAppearanceType() == TownOfUsAppearances.Camouflage));
    }
}

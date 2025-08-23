
using HarmonyLib;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using Reactor.Utilities.Extensions;
using TownOfUs.Interfaces;
using TownOfUs.Modifiers.Game;
using TownOfUs.Options;
using TownOfUs.Roles;
using TownOfUs.Utilities;

namespace TownOfUs.Patches;

[HarmonyPatch]
public static class IntroScenePatches
{
    [HarmonyPatch(typeof(IntroCutscene._ShowRole_d__41), nameof(IntroCutscene._ShowRole_d__41.MoveNext))]
    [HarmonyPriority(Priority.Last)]
    public static class ShowModifierPatch_Role
    {
        public static void Postfix(IntroCutscene._ShowRole_d__41 __instance)
        {
            if (PlayerControl.LocalPlayer.Data.Role is ITownOfUsRole custom)
            {
                __instance.__4__this.RoleText.text = custom.RoleName;
                __instance.__4__this.YouAreText.text = custom.YouAreText;
                __instance.__4__this.RoleBlurbText.text = custom.RoleDescription;
            }

            var teamModifier = PlayerControl.LocalPlayer.GetModifiers<TouGameModifier>().FirstOrDefault();
            if (teamModifier != null && OptionGroupSingleton<GeneralOptions>.Instance.TeamModifierReveal)
            {
                var color = MiscUtils.GetRoleColour(teamModifier.ModifierName.Replace(" ", string.Empty));
                if (teamModifier is IColoredModifier colorMod)
                {
                    color = colorMod.ModifierColor;
                }

                __instance.__4__this.RoleBlurbText.text =
                    $"<size={teamModifier.IntroSize}>\n</size>{__instance.__4__this.RoleBlurbText.text}\n<size={teamModifier.IntroSize}><color=#{color.ToHtmlStringRGBA()}>{teamModifier.IntroInfo}</color></size>";
            }
        }
    }
}


using System.Globalization;
using System.Text;
using AmongUs.GameOptions;
using HarmonyLib;
using MiraAPI.Modifiers;
using MiraAPI.Modifiers.Types;
using MiraAPI.Roles;
using TownOfUs.Modules;
using TownOfUs.Roles.Crewmate;
using TownOfUs.Roles.Neutral;
using TownOfUs.Utilities;

namespace TownOfUs.Patches
{
    [HarmonyPatch(typeof(HauntMenuMinigame), nameof(HauntMenuMinigame.SetHauntTarget))]
    public static class HauntMenuMinigamePatch
    {
        public static void Postfix(HauntMenuMinigame __instance)
        {
            if (GameOptionsManager.Instance.CurrentGameOptions.GameMode == GameModes.HideNSeek) return;
            var target = __instance.HauntTarget;
            var role = target.Data.Role;
            if (target.Data.IsDead && !TutorialManager.InstanceExists && role is not PhantomTouRole or GuardianAngelRole or HaunterRole)
            {
                role = target.GetRoleWhenAlive();
            }

            var rColor = role is ICustomRole custom ? custom.RoleColor : role.TeamColor;

            __instance.NameText.text = $"<size=90%>{__instance.NameText.text} - {rColor.ToTextColor()}{role.NiceName}</color></size>";
            __instance.FilterText.text = string.Empty;

            var modifiers = target.GetModifiers<GameModifier>().OrderBy(x => x.ModifierName).ToList();
            if (modifiers.Count != 0)
            {
                var modifierTextBuilder = new StringBuilder($"<color=#FFFFFF><size=100%>(");
                bool first = true;
                foreach (var modifier in modifiers)
                {
                    var color = MiscUtils.GetRoleColour(modifier.ModifierName.Replace(" ", string.Empty));
                    if (modifier is IColoredModifier colorMod) color = colorMod.ModifierColor;
                    if (!first) modifierTextBuilder.Append(", ");
                    modifierTextBuilder.Append(CultureInfo.InvariantCulture, $"{color.ToTextColor()}{modifier.ModifierName}</color>");
                    first = false;
                }
                modifierTextBuilder.Append($")</size></color>");
                __instance.FilterText.text = modifierTextBuilder.ToString();
            }
        }
    }
}
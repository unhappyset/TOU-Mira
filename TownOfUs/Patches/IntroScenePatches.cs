using HarmonyLib;
using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Modifiers;
using MiraAPI.Roles;
using TMPro;
using TownOfUs.Modifiers.Game;
using TownOfUs.Options;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfUs.Patches;

[HarmonyPatch]
public static class IntroScenePatches
{
    [HarmonyPatch(typeof(IntroCutscene), nameof(IntroCutscene.BeginImpostor))]
    [HarmonyPrefix]
    public static bool ImpostorBeginPatch(IntroCutscene __instance)
    {
        if (OptionGroupSingleton<GeneralOptions>.Instance.ImpsKnowRoles && !OptionGroupSingleton<GeneralOptions>.Instance.FFAImpostorMode) return true;
        __instance.TeamTitle.text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.Impostor, Array.Empty<Il2CppSystem.Object>());
        __instance.TeamTitle.color = Palette.ImpostorRed;

        var player = __instance.CreatePlayer(0, 1, PlayerControl.LocalPlayer.Data, true);
        __instance.ourCrewmate = player;
        return false;
    }

    [HarmonyPatch(typeof(IntroCutscene), nameof(IntroCutscene.OnDestroy))]
    [HarmonyPrefix]
    public static void IntroCutsceneOnDestroyPatch()
    {
        HudManager.Instance.SetHudActive(PlayerControl.LocalPlayer, PlayerControl.LocalPlayer.Data.Role, false);
        HudManager.Instance.SetHudActive(PlayerControl.LocalPlayer, PlayerControl.LocalPlayer.Data.Role, true);

        foreach (var button in CustomButtonManager.Buttons.Where(x => x.Enabled(PlayerControl.LocalPlayer.Data.Role)))
        {
            button.SetTimer(OptionGroupSingleton<GeneralOptions>.Instance.GameStartCd);
        }

        if (PlayerControl.LocalPlayer.IsImpostor())
        {
            PlayerControl.LocalPlayer.SetKillTimer(OptionGroupSingleton<GeneralOptions>.Instance.GameStartCd);
        }

        var modsTab = MiraAPI.Modifiers.ModifierDisplay.ModifierDisplayComponent.Instance;
        if (modsTab != null && !modsTab.IsOpen)
        {
            modsTab.ToggleTab();
        }
        var panelThing = HudManager.Instance.TaskStuff.transform.FindChild("RolePanel");
        if (panelThing != null)
        {
            var panel = panelThing.gameObject.GetComponent<TaskPanelBehaviour>();
            var role = PlayerControl.LocalPlayer.Data.Role as ICustomRole;
            if (role == null) return;
            
            panel.open = true;

            var tabText = panel.tab.gameObject.GetComponentInChildren<TextMeshPro>();
            var ogPanel = HudManager.Instance.TaskStuff.transform.FindChild("TaskPanel").gameObject.GetComponent<TaskPanelBehaviour>();
            if (tabText.text != role.RoleName)
            {
                tabText.text = role.RoleName;
            }

            var y = ogPanel.taskText.textBounds.size.y + 1;
            panel.closedPosition = new Vector3(ogPanel.closedPosition.x, ogPanel.open ? y + 0.2f : 2f, ogPanel.closedPosition.z);
            panel.openPosition = new Vector3(ogPanel.openPosition.x, ogPanel.open ? y : 2f, ogPanel.openPosition.z);

            panel.SetTaskText(role.SetTabText().ToString());
        }
    }
    [HarmonyPatch(typeof(SpawnInMinigame), nameof(SpawnInMinigame.Close))]
    [HarmonyPrefix]
    public static void SpawnInMinigameClosePatch() => IntroCutsceneOnDestroyPatch();
}

public static class ModifierIntroPatch
{
    private static TextMeshPro ModifierText;
    [HarmonyPatch(typeof(IntroCutscene), nameof(IntroCutscene.BeginCrewmate))]
    public static class IntroCutscene_BeginCrewmate
    {
        public static void Postfix(IntroCutscene __instance)
        {
            var modifier = PlayerControl.LocalPlayer.GetModifiers<AllianceGameModifier>().FirstOrDefault();
            if (modifier != null)
                ModifierText = UnityEngine.Object.Instantiate(__instance.RoleText, __instance.RoleText.transform.parent, false);
        }
    }

    [HarmonyPatch(typeof(IntroCutscene), nameof(IntroCutscene.BeginImpostor))]
    public static class IntroCutscene_BeginImpostor
    {
        public static void Postfix(IntroCutscene __instance)
        {
            var modifier = PlayerControl.LocalPlayer.GetModifiers<AllianceGameModifier>().FirstOrDefault();
            if (modifier != null)
                ModifierText = UnityEngine.Object.Instantiate(__instance.RoleText, __instance.RoleText.transform.parent, false);
        }
    }
    [HarmonyPatch(typeof(IntroCutscene._CoBegin_d__35), nameof(IntroCutscene._CoBegin_d__35.MoveNext))]
    public static class ShowModifierPatch_CoBegin
    {
        public static void Postfix(IntroCutscene._ShowRole_d__41 __instance)
        {
            var modifier = PlayerControl.LocalPlayer.GetModifiers<AllianceGameModifier>().FirstOrDefault();

            if (ModifierText != null && modifier != null)
            {
                ModifierText.text = $"<size={modifier.IntroSize}>{modifier.IntroInfo}</size>";

                ModifierText.color = MiscUtils.GetRoleColour(modifier.ModifierName.Replace(" ", string.Empty));
                if (modifier is IColoredModifier colorMod) ModifierText.color = colorMod.ModifierColor;
                ModifierText.transform.position =
                __instance.__4__this.transform.position - new Vector3(0f, 1.6f, 0f);
                ModifierText.gameObject.SetActive(true);
            }
        }
    }
    [HarmonyPatch(typeof(IntroCutscene._ShowTeam_d__38), nameof(IntroCutscene._ShowTeam_d__38.MoveNext))]
    public static class ShowModifierPatch_MoveNext
    {
        public static void Postfix(IntroCutscene._ShowRole_d__41 __instance)
        {
            var modifier = PlayerControl.LocalPlayer.GetModifiers<AllianceGameModifier>().FirstOrDefault();
            
            if (ModifierText != null && modifier != null)
            {
                ModifierText.text = $"<size={modifier.IntroSize}>{modifier.IntroInfo}</size>";

                ModifierText.color = MiscUtils.GetRoleColour(modifier.ModifierName.Replace(" ", string.Empty));
                if (modifier is IColoredModifier colorMod) ModifierText.color = colorMod.ModifierColor;
                ModifierText.transform.position =
                __instance.__4__this.transform.position - new Vector3(0f, 1.6f, 0f);
                ModifierText.gameObject.SetActive(true);
            }
        }
    }
    [HarmonyPatch(typeof(IntroCutscene._ShowRole_d__41), nameof(IntroCutscene._ShowRole_d__41.MoveNext))]
    public static class ShowModifierPatch_Role
    {
        public static void Postfix(IntroCutscene._ShowRole_d__41 __instance)
        {
            var modifier = PlayerControl.LocalPlayer.GetModifiers<AllianceGameModifier>().FirstOrDefault();
            
            if (ModifierText != null && modifier != null)
            {
                ModifierText.text = $"<size={modifier.IntroSize}>{modifier.IntroInfo}</size>";

                ModifierText.color = MiscUtils.GetRoleColour(modifier.ModifierName.Replace(" ", string.Empty));
                if (modifier is IColoredModifier colorMod) ModifierText.color = colorMod.ModifierColor;
                ModifierText.transform.position =
                __instance.__4__this.transform.position - new Vector3(0f, 1.6f, 0f);
                ModifierText.gameObject.SetActive(true);
            }
        }
    }
}

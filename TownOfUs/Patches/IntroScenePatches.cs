
using HarmonyLib;
using MiraAPI.GameOptions;
using MiraAPI.Utilities;
using TownOfUs.Options;
using TownOfUs.Roles;
using TownOfUs.Roles.Other;
using TownOfUs.Utilities;
using Object = Il2CppSystem.Object;

namespace TownOfUs.Patches;

[HarmonyPatch]
public static class IntroScenePatches
{
    [HarmonyPatch(typeof(IntroCutscene), nameof(IntroCutscene.BeginImpostor))]
    [HarmonyPatch(typeof(IntroCutscene), nameof(IntroCutscene.BeginCrewmate))]
    [HarmonyPriority(Priority.Last)]
    [HarmonyPostfix]
    public static void ShowTeamPatchPostfix(IntroCutscene __instance)
    {
        if (PlayerControl.LocalPlayer.IsImpostor())
        {
            if (OptionGroupSingleton<GeneralOptions>.Instance.FFAImpostorMode)
            {
                __instance.TeamTitle.text =
                    DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.Impostor,
                        Array.Empty<Object>());
                __instance.TeamTitle.color = Palette.ImpostorRed;
            }

            panel.open = true;

            var tabText = panel.tab.gameObject.GetComponentInChildren<TextMeshPro>();
            var ogPanel = HudManager.Instance.TaskStuff.transform.FindChild("TaskPanel").gameObject
                .GetComponent<TaskPanelBehaviour>();
            if (tabText.text != role.RoleName)
            {
                tabText.text = role.RoleName;
            }

            var y = ogPanel.taskText.textBounds.size.y + 1;
            panel.closedPosition = new Vector3(ogPanel.closedPosition.x, ogPanel.open ? y + 0.2f : 2f,
                ogPanel.closedPosition.z);
            panel.openPosition = new Vector3(ogPanel.openPosition.x, ogPanel.open ? y : 2f, ogPanel.openPosition.z);

            panel.SetTaskText(role.SetTabText().ToString());
        }

        foreach (var id in SpectatorRole.TrackedSpectators)
        {
            var spec = MiscUtils.PlayerById(id);

            if (!spec)
                continue;

            spec!.Visible = false;
            spec.Die(DeathReason.Exile, false);

            if (spec.AmOwner)
            {
                HudManager.Instance.SetHudActive(false);
                HudManager.Instance.ShadowQuad.gameObject.SetActive(false);
            }
        }

        SpectatorRole.InitList();
    }

    [HarmonyPatch(typeof(SpawnInMinigame), nameof(SpawnInMinigame.Close))]
    [HarmonyPrefix]
    public static void SpawnInMinigameClosePatch()
    {
        IntroCutsceneOnDestroyPatch();
    }
}

public static class ModifierIntroPatch
{
    private static TextMeshPro ModifierText;

    public static void RunModChecks()
    {
        var option = OptionGroupSingleton<GeneralOptions>.Instance.ModifierReveal;
        var modifier = PlayerControl.LocalPlayer.GetModifiers<AllianceGameModifier>().FirstOrDefault();
        var uniModifier = PlayerControl.LocalPlayer.GetModifiers<UniversalGameModifier>().FirstOrDefault();

        if (modifier != null && option is ModReveal.Alliance)
        {
            ModifierText.text = $"<size={modifier.IntroSize}>{modifier.IntroInfo}</size>";

            ModifierText.color = MiscUtils.GetRoleColour(modifier.ModifierName.Replace(" ", string.Empty));
            if (modifier is IColoredModifier colorMod)
            {
                ModifierText.color = colorMod.ModifierColor;
            }
        }
        else if (uniModifier != null && option is ModReveal.Universal)
        {
            ModifierText.text = $"<size=4><color=#FFFFFF>Modifier: </color>{uniModifier.ModifierName}</size>";

            ModifierText.color = MiscUtils.GetRoleColour(uniModifier.ModifierName.Replace(" ", string.Empty));
            if (uniModifier is IColoredModifier colorMod)
            {
                ModifierText.color = colorMod.ModifierColor;
                var player = __instance.CreatePlayer(0, 1, PlayerControl.LocalPlayer.Data, true);
                __instance.ourCrewmate = player;
            }
        }
        else
        {
            ModifierText.text = string.Empty;
        }
    }

    [HarmonyPatch(typeof(IntroCutscene), nameof(IntroCutscene.BeginCrewmate))]
    public static class IntroCutscene_BeginCrewmate
    {
        public static void Prefix(ref Il2CppSystem.Collections.Generic.List<PlayerControl> teamToDisplay)
        {
            foreach (var player in teamToDisplay.ToArray())
            {
                if (SpectatorRole.TrackedSpectators.Contains(player.PlayerId))
                    teamToDisplay.Remove(player);
            }
        }

        public static void Postfix(IntroCutscene __instance)
        {
            ModifierText =
                UnityEngine.Object.Instantiate(__instance.RoleText, __instance.RoleText.transform.parent, false);
            SetHiddenImpostors(__instance);
        }
    }

    public static void SetHiddenImpostors(IntroCutscene __instance)
    {
        var amount = Helpers.GetAlivePlayers().Count(x => x.IsImpostor());
        __instance.ImpostorText.text =
            DestroyableSingleton<TranslationController>.Instance.GetString(
                amount == 1 ? StringNames.NumImpostorsS : StringNames.NumImpostorsP, amount);
        __instance.ImpostorText.text = __instance.ImpostorText.text.Replace("[FF1919FF]", "<color=#FF1919FF>");
        __instance.ImpostorText.text = __instance.ImpostorText.text.Replace("[]", "</color>");

        if (!OptionGroupSingleton<RoleOptions>.Instance.RoleListEnabled) return;

        var players = GameData.Instance.PlayerCount;

        if (players < 7)
        {
            return;
        }

        var list = OptionGroupSingleton<RoleOptions>.Instance;

        int maxSlots = players < 15 ? players : 15;

        List<RoleListOption> buckets = [];
        if (list.RoleListEnabled)
        {
            for (int i = 0; i < maxSlots; i++)
            {
                int slotValue = i switch
                {
                    0 => list.Slot1,
                    1 => list.Slot2,
                    2 => list.Slot3,
                    3 => list.Slot4,
                    4 => list.Slot5,
                    5 => list.Slot6,
                    6 => list.Slot7,
                    7 => list.Slot8,
                    8 => list.Slot9,
                    9 => list.Slot10,
                    10 => list.Slot11,
                    11 => list.Slot12,
                    12 => list.Slot13,
                    13 => list.Slot14,
                    14 => list.Slot15,
                    _ => -1
                };

                buckets.Add((RoleListOption)slotValue);
            }
        }

        if (!buckets.Any(x => x is RoleListOption.Any)) return;


        __instance.ImpostorText.text =
            DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.NumImpostorsP, 256);
        __instance.ImpostorText.text = __instance.ImpostorText.text.Replace("[FF1919FF]", "<color=#FF1919FF>");
        __instance.ImpostorText.text = __instance.ImpostorText.text.Replace("[]", "</color>");
        __instance.ImpostorText.text = __instance.ImpostorText.text.Replace("256", "???");
    }
}

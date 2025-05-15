using AmongUs.GameOptions;
using HarmonyLib;
using Reactor.Localization.Utilities;
using UnityEngine;

namespace TownOfUs.Patches.Misc;

[HarmonyPatch(typeof(MainMenuManager), nameof(MainMenuManager.Start))]
public static class LogoPatch
{
    public static void Postfix()
    {
        RoleManager.Instance.GetRole(RoleTypes.CrewmateGhost).StringName =
            CustomStringName.CreateAndRegister("Crewmate Ghost");
        RoleManager.Instance.GetRole(RoleTypes.ImpostorGhost).StringName =
            CustomStringName.CreateAndRegister("Impostor Ghost");

        var touLogo = new GameObject("bannerLogo_TownOfUs")
        {
            transform =
            {
                localScale = new Vector3(0.8f, 0.8f, 1f),
            },
        };

        var renderer = touLogo.AddComponent<SpriteRenderer>();
        renderer.sprite = TouAssets.Banner.LoadAsset();

        var position = touLogo.AddComponent<AspectPosition>();
        position.DistanceFromEdge = new Vector3(-0.2f, 2.3f, 8f);
        position.Alignment = AspectPosition.EdgeAlignments.Top;

        position.StartCoroutine(Effects.Lerp(0.1f, new System.Action<float>((p) => { position.AdjustPosition(); })));

        var scaler = touLogo.AddComponent<AspectScaledAsset>();
        var renderers = new Il2CppSystem.Collections.Generic.List<SpriteRenderer>();
        renderers.Add(renderer);

        scaler.spritesToScale = renderers;
        scaler.aspectPosition = position;

        touLogo.transform.SetParent(GameObject.Find("RightPanel").transform);
    }
}

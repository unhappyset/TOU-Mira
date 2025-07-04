using System.Collections;
using HarmonyLib;
using Reactor.Utilities;
using Reactor.Utilities.Extensions;
using UnityEngine;

namespace TownOfUs.Patches;

public static class KillOverlayPatch
{
    [HarmonyPatch(typeof(OverlayKillAnimation), nameof(OverlayKillAnimation.CoShow), typeof(KillOverlay))]
    [HarmonyPrefix]
    public static void SetKillAnimationMaskInteraction(OverlayKillAnimation __instance)
    {
        var flame = GameObject.Find("KillOverlay").transform.FindChild("QuadParent");
        if (flame != null)
        {
            flame.transform.localPosition = new Vector3(0f, 0f);
            if (flame.transform.FindChild("BackgroundFlame").TryGetComponent<SpriteRenderer>(out var flameSprite))
            {
                flameSprite.sprite = TouAssets.KillBG.LoadAsset();
            }
            //flame.transform.localRotation = Quaternion.Euler(new Vector3(0, 0, 0));
        }

        if (ExileController.Instance)
        {
            //__instance.flameParent.transform.localPosition -= new Vector3(1.5f, 1.5f);
            if (flame != null)
            {
                flame.transform.localPosition = new Vector3(0, -1.5f);
                //flame.transform.localRotation = Quaternion.Euler(new Vector3(0, 0, -45));
                if (flame.transform.FindChild("BackgroundFlame").TryGetComponent<SpriteRenderer>(out var flameSprite))
                {
                    flameSprite.sprite = TouAssets.RetributionBG.LoadAsset();
                }
            }

            __instance.GetComponentsInChildren<SpriteRenderer>(true).ToList()
                .ForEach(x => x.maskInteraction = SpriteMaskInteraction.None);
            __instance.transform.localPosition -= new Vector3(2.4f, 1.5f);
        }

        Coroutines.Start(CoKillDestroy());
    }

    private static IEnumerator CoKillDestroy()
    {
        var obj = GameObject.Find("KillOverlay").transform.GetChild(2).gameObject;
        yield return new WaitForSeconds(5f);

        obj?.Destroy();
    }
}
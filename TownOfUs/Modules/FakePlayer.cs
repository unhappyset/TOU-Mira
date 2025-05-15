using System.Globalization;
using AmongUs.Data;
using HarmonyLib;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using PowerTools;
using TMPro;
using TownOfUs.Modifiers.Game.Universal;
using TownOfUs.Options.Modifiers.Universal;
using UnityEngine;
using Object = UnityEngine.Object;

namespace TownOfUs.Modules;

// Code Review: Should be using a MonoBehaviour
public sealed class FakePlayer : IDisposable
{
    private static readonly List<FakePlayer> FakePlayers = [];

    private const string DefaultPetName = "EmptyPet(Clone)";
    private const string NameTextObjName = "NameText_TMP";
    private const string ColorBindTextName = "ColorblindName_TMP";

    private readonly GameObject? body;
    private readonly SpriteRenderer? rend;
    private GameObject? colorBindTextObj;

    private static Vector3 Scale => new(0.35f, 0.35f, 0.35f);
    private static Vector2 PosOffset => new(-0.045f, 0.575f);

    private struct PlayerCosmicInfo
    {
        public CosmeticsLayer Cosmetics;

        public NetworkedPlayerInfo.PlayerOutfit OutfitInfo;

        public bool FlipX;

        public int ColorInfo;
    }

    public FakePlayer(PlayerControl player)
    {
        var playerOutfit = player.Data.DefaultOutfit;

        var cosmicInfo = new PlayerCosmicInfo
        {
            Cosmetics = player.cosmetics,
            FlipX = player.cosmetics.currentBodySprite.BodySprite.flipX,
            OutfitInfo = playerOutfit,
            ColorInfo = playerOutfit.ColorId,
        };

        cosmicInfo.Cosmetics.Visible = true;

        body = new GameObject("FakePlayer");
        body.layer = LayerMask.NameToLayer("Players");

        CreateNameTextParentObj(player, body, cosmicInfo);

        rend = CreateBodyImage(cosmicInfo);
        var cosmetics = CreateCosmetics(rend, cosmicInfo);

        DataManager.Settings.Accessibility.OnChangedEvent += new Action(SwitchColorName);

        DecorateDummy(cosmetics, cosmicInfo, player);

        SpriteAnimNodeSync[] syncs = body.GetComponentsInChildren<SpriteAnimNodeSync>(true);
        for (int i = 0; i < syncs.Length; ++i)
        {
            SpriteAnimNodeSync sync = syncs[i];
            if (sync != null)
            {
                Object.Destroy(sync);
            }
        }

        Vector3 vector = player.transform.position;
        vector.z = vector.y / 1000f;

        body.transform.position = vector;

        if (player.HasModifier<GiantModifier>()) body.transform.localScale /= 0.7f;
        else if (player.HasModifier<MiniModifier>()) body.transform.localScale *= 0.7f;
        if (player.HasModifier<ShyModifier>())
        {
            var colour = player.cosmetics.currentBodySprite.BodySprite.color;
            var transparency = OptionGroupSingleton<ShyOptions>.Instance.FinalTransparency / 100;

            colour.a = transparency;
            cosmetics.currentBodySprite.BodySprite.color = colour;

            cosmetics.nameText.color = cosmetics.nameText.color.SetAlpha(transparency);

            if (DataManager.Settings.Accessibility.ColorBlindMode)
                cosmetics.colorBlindText.color = cosmetics.colorBlindText.color.SetAlpha(transparency);

            //player.SetHatAndVisorAlpha(transparency);
            cosmetics.skin.layer.color = cosmetics.skin.layer.color.SetAlpha(transparency);

            foreach (var animation in cosmetics.GetComponentsInParent<SpriteRenderer>())
            {
                animation.color = animation.color.SetAlpha(transparency);
            }
            foreach (var animation in cosmetics.GetComponentsInChildren<SpriteRenderer>())
            {
                animation.color = animation.color.SetAlpha(transparency);
            }

            foreach (var animation in body.GetComponentsInParent<SpriteRenderer>())
            {
                animation.color = animation.color.SetAlpha(transparency);
            }
            foreach (var animation in body.GetComponentsInChildren<SpriteRenderer>())
            {
                animation.color = animation.color.SetAlpha(transparency);
            }
        }

        FakePlayers.Add(this);
    }

    public void SwitchColorName()
    {
        colorBindTextObj?.SetActive(DataManager.Settings.Accessibility.ColorBlindMode);
    }

    /* public void Camo()
    {
        cosmetics!.SetHat(string.Empty, cosmicInfo.ColorInfo);
        cosmetics.SetVisor(string.Empty, cosmicInfo.ColorInfo);
        cosmetics.SetSkin(string.Empty, cosmicInfo.ColorInfo);

        PlayerMaterial.SetColors(Color.grey, cosmetics.currentBodySprite.BodySprite);

        nameTextMaster!.color = Color.clear;
        colorBindText!.color = Color.clear;
    }

    public void UnCamo()
    {
        cosmetics!.SetHat(cosmicInfo.OutfitInfo.HatId, cosmicInfo.ColorInfo);
        cosmetics.SetVisor(cosmicInfo.OutfitInfo.VisorId, cosmicInfo.ColorInfo);
        cosmetics.SetSkin(cosmicInfo.OutfitInfo.SkinId, cosmicInfo.ColorInfo);
        cosmetics.SetColor(cosmicInfo.ColorInfo);

        nameTextMaster!.color = Color.white;
        colorBindText!.color = Color.white;
    } */

    private SpriteRenderer CreateBodyImage(PlayerCosmicInfo info)
    {
        var spriteRenderer = Object.Instantiate<SpriteRenderer>(info.Cosmetics.currentBodySprite.BodySprite, body!.transform);

        spriteRenderer.flipX = info.FlipX;
        spriteRenderer.transform.localScale = Scale;

        return spriteRenderer;
    }

    private CosmeticsLayer CreateCosmetics(SpriteRenderer playerImage, PlayerCosmicInfo info)
    {
        var cosmeticsLayer = Object.Instantiate<CosmeticsLayer>(AmongUsClient.Instance.PlayerPrefab.cosmetics, body!.transform);
        var basePayerBodySprite = info.Cosmetics.currentBodySprite;

        var playerBodySprite = new PlayerBodySprite
        {
            BodySprite = playerImage,
            Type = basePayerBodySprite.Type,
            flippedCosmeticOffset = basePayerBodySprite.flippedCosmeticOffset,
            LongModeParts = new(info.Cosmetics.currentBodySprite.LongModeParts.Length),
        };

        for (int i = 0; i < info.Cosmetics.currentBodySprite.LongModeParts.Length; ++i)
        {
            var newSprite = Object.Instantiate(
                info.Cosmetics.currentBodySprite.LongModeParts[i],
                cosmeticsLayer.transform);

            playerBodySprite.LongModeParts[i] = newSprite;
        }

        cosmeticsLayer.currentBodySprite = playerBodySprite;
        cosmeticsLayer.hat.Parent = playerImage;
        cosmeticsLayer.hat.transform.localPosition = PosOffset;
        cosmeticsLayer.visor.transform.localPosition = PosOffset;
        cosmeticsLayer.petParent = body.transform;
        cosmeticsLayer.transform.localScale = Scale;
        cosmeticsLayer.ResetCosmetics();

        return cosmeticsLayer;
    }

    private void DecorateDummy(CosmeticsLayer cosmetics, PlayerCosmicInfo cosmicInfo, PlayerControl playerRef)
    {
        int colorId = cosmicInfo.ColorInfo;
        bool flipX = cosmicInfo.FlipX;

        cosmetics.SetNameMask(true);
        cosmetics.SetHat(cosmicInfo.OutfitInfo.HatId, colorId);
        cosmetics.SetVisor(cosmicInfo.OutfitInfo.VisorId, colorId);
        cosmetics.SetSkin(cosmicInfo.OutfitInfo.SkinId, colorId);
        cosmetics.SetFlipX(flipX);

        var emptyPet = body!.transform.Find(DefaultPetName);
        if (emptyPet != null)
        {
            Object.Destroy(emptyPet.gameObject);
        }

        string petId = cosmicInfo.OutfitInfo.PetId;

        if (petId != PetData.EmptyId)
        {
            var preBehaviourPrefab = ShipStatus.Instance.CosmeticsCache.GetPet(petId);

            var petBehaviour = Object.Instantiate<PetBehaviour>(preBehaviourPrefab, body.transform);
            petBehaviour.SetCrewmateColor(colorId);
            petBehaviour.transform.localPosition = Vector2.zero + (flipX ? (Vector2.right * UnityEngine.Random.RandomRange(0, 0.2f)) : (Vector2.left * UnityEngine.Random.RandomRange(0, 0.2f))) + (Vector2.down * UnityEngine.Random.RandomRange(-0.05f, 0.15f));
            petBehaviour.transform.localScale = Scale;
            petBehaviour.FlipX = flipX;
            RemovePet(playerRef);

            DestroyAllCollider(petBehaviour.gameObject);

            if (playerRef.HasModifier<GiantModifier>())
            {
                petBehaviour.transform.localScale *= 0.7f;
                petBehaviour.transform.localPosition += Vector3.down * 0.1f;
            }
            else if (playerRef.HasModifier<MiniModifier>())
            {
                petBehaviour.transform.localScale /= 0.7f;
                petBehaviour.transform.localPosition += Vector3.down * 0.1f;
            }
            if (playerRef.HasModifier<ShyModifier>())
            {
                var transparency = OptionGroupSingleton<ShyOptions>.Instance.FinalTransparency / 100;
                foreach (var pet in petBehaviour.GetComponentsInParent<SpriteRenderer>())
                {
                    pet.color = pet.color.SetAlpha(transparency);
                }
                foreach (var pet in petBehaviour.GetComponentsInChildren<SpriteRenderer>())
                {
                    pet.color = pet.color.SetAlpha(transparency);
                }
            }
        }

        cosmetics.SetColor(colorId);

        cosmetics.skin.transform.localPosition = cosmicInfo.Cosmetics.skin.transform.localPosition;
        cosmetics.hat.transform.localPosition = PosOffset;
        cosmetics.visor.transform.localPosition = PosOffset;
    }

    private void CreateNameTextParentObj(PlayerControl player, GameObject parent, PlayerCosmicInfo info)
    {
        var baseParentTrans = player.gameObject.transform.FindChild("Names");
        if (baseParentTrans == null)
        {
            return;
        }

        var baseObject = baseParentTrans.gameObject;

        var nameObj = Object.Instantiate<GameObject>(baseObject, parent.transform);
        nameObj.transform.localScale = player.gameObject.transform.localScale;
        nameObj.transform.localPosition = baseObject.transform.localPosition;
        nameObj.transform.localPosition -= new Vector3(0f, 0.247f, 0f);
        if (player.HasModifier<GiantModifier>()) nameObj.transform.localScale *= 0.7f;
        else if (player.HasModifier<MiniModifier>()) nameObj.transform.localScale /= 0.7f;

        var nameText = nameObj.transform.FindChild(NameTextObjName).GetComponent<TextMeshPro>();
        var baseNameText = baseObject.transform.FindChild(NameTextObjName).GetComponent<TextMeshPro>();

        colorBindTextObj = nameObj.transform.FindChild(ColorBindTextName).gameObject;
        var colorBindText = colorBindTextObj.GetComponent<TextMeshPro>();

        var baseColorBindText = baseObject.transform.FindChild(ColorBindTextName).GetComponent<TextMeshPro>();

            var transparency = OptionGroupSingleton<ShyOptions>.Instance.FinalTransparency / 100;
        if (nameText != null && baseNameText != null)
        {
            ChangeDummyName(nameText, baseNameText, info);
            if (player.HasModifier<ShyModifier>()) nameText.GetComponent<TextMeshPro>().color = nameText.GetComponent<TextMeshPro>().color.SetAlpha(transparency);
        }

        if (colorBindText != null && baseColorBindText != null)
        {
            UpdateColorName(colorBindText, baseColorBindText, info.ColorInfo);
            if (player.HasModifier<ShyModifier>()) colorBindText.GetComponent<TextMeshPro>().color = colorBindText.GetComponent<TextMeshPro>().color.SetAlpha(transparency);
        }

        RemoveRoleInfo(nameObj);
    }

    private static void ChangeDummyName(TextMeshPro nameText, TextMeshPro baseNameText, PlayerCosmicInfo info)
    {
        FitTextMeshPro(nameText, baseNameText);

        nameText.text = info.OutfitInfo.PlayerName;
        nameText.color = Palette.White;

        // nameTextMaster = nameText;
    }

    private static void RemoveRoleInfo(GameObject nameTextObjct)
    {
        Transform info = nameTextObjct.transform.FindChild("Info");
        if (info != null)
        {
            Object.Destroy(info.gameObject);
        }
    }

    private static void UpdateColorName(TextMeshPro colorText, TextMeshPro baseColorText, int colorId)
    {
        char[] array = TranslationController.Instance.GetString(Palette.ColorNames[colorId], Array.Empty<Il2CppSystem.Object>()).ToCharArray();

        if (array.Length != 0)
        {
            array[0] = char.ToUpper(array[0], CultureInfo.InvariantCulture);
            for (int i = 1; i < array.Length; i++)
            {
                array[i] = char.ToLower(array[i], CultureInfo.InvariantCulture);
            }
        }

        FitTextMeshPro(colorText, baseColorText);

        colorText.text = new string(array);
    }

    private static void DestroyAllCollider(GameObject obj)
    {
        DestroyCollider<Collider2D>(obj);
        DestroyCollider<PolygonCollider2D>(obj);
        DestroyCollider<BoxCollider2D>(obj);
        DestroyCollider<CircleCollider2D>(obj);
    }

    private static void DestroyCollider<T>(GameObject obj) where T : Collider2D
    {
        T component = obj.GetComponent<T>();
        if (component != null)
        {
            Object.Destroy(component);
        }
    }

    private static void FitTextMeshPro(TextMeshPro a, TextMeshPro b)
    {
        a.transform.localPosition = b.transform.localPosition;
        a.transform.localScale = b.transform.localScale;
        a.fontSize = a.fontSizeMax = a.fontSizeMin = b.fontSizeMax = b.fontSizeMin = b.fontSize;
    }

    public static void ClearAll()
    {
        FakePlayers.Do(x => x.Destroy());
        FakePlayers.Clear();
    }

    public void Destroy()
    {
        DataManager.Settings.Accessibility.OnChangedEvent -= new Action(SwitchColorName);

        Dispose();
    }

    public void Dispose()
    {
        Dispose(true);
    }

    public void Dispose(bool disposing)
    {
        if (disposing)
        {
            if (body != null)
                Object.Destroy(body);

            if (colorBindTextObj != null)
                Object.Destroy(colorBindTextObj);

            if (rend != null)
                Object.Destroy(rend);
        }
    }
    public static void RemovePet(PlayerControl pc)
    {
        if (pc == null || !pc.Data.IsDead) return;
        if (pc.CurrentOutfit.PetId == "") return;

        pc.SetPet("");
    }
}

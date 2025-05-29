using MiraAPI.Utilities.Assets;
using Reactor.Utilities;
using UnityEngine;

namespace TownOfUs.Assets;

public static class TouAssets
{
    private const string ShortPath = "TownOfUs.Resources";

    public static readonly AssetBundle MainBundle = AssetBundleManager.Load("tou-assets");

    public static LoadableAsset<GameObject> WikiPrefab { get; } = new LoadableBundleAsset<GameObject>("IngameWiki", MainBundle);
    public static readonly LoadableAsset<GameObject> RoleSelectionGame = new LoadableBundleAsset<GameObject>("SelectRoleGame", MainBundle);
    public static LoadableAsset<GameObject> FirstRoundShield { get; } = new LoadableBundleAsset<GameObject>("FirstRoundShield", MainBundle);
    public static LoadableAsset<GameObject> ClericBarrier { get; } = new LoadableBundleAsset<GameObject>("ClericBarrier", MainBundle);
    public static LoadableAsset<GameObject> MedicShield { get; } = new LoadableBundleAsset<GameObject>("MedicShield", MainBundle);

    public static LoadableAsset<GameObject> MeetingDeathPrefab { get; } = new LoadableBundleAsset<GameObject>("DeathAnimation", MainBundle);
    public static LoadableAsset<GameObject> MayorRevealPrefab { get; } = new LoadableBundleAsset<GameObject>("MayorReveal", MainBundle);
    public static LoadableAsset<AnimationClip> MeetingDeathAnim1 { get; } =
        new LoadableBundleAsset<AnimationClip>("DeathMeetingShotFront", MainBundle);
    public static LoadableAsset<AnimationClip> MeetingDeathBloodAnim1 { get; } =
        new LoadableBundleAsset<AnimationClip>("DeathMeetingShotFrontBlood", MainBundle);
    public static LoadableAsset<AnimationClip> MeetingDeathAnim2 { get; } =
        new LoadableBundleAsset<AnimationClip>("DeathMeetingShotRight", MainBundle);
    public static LoadableAsset<AnimationClip> MeetingDeathBloodAnim2 { get; } =
        new LoadableBundleAsset<AnimationClip>("DeathMeetingShotRightBlood", MainBundle);
    public static LoadableAsset<AnimationClip> MeetingDeathAnim3 { get; } =
        new LoadableBundleAsset<AnimationClip>("DeathMeetingBody", MainBundle);
    public static LoadableAsset<AnimationClip> MeetingDeathBloodAnim3 { get; } =
        new LoadableBundleAsset<AnimationClip>("DeathMeetingBodyBlood", MainBundle);

    public static LoadableAsset<GameObject> ScatterUI { get; } = new LoadableBundleAsset<GameObject>("ScatterUI", MainBundle);

    public static LoadableAsset<Sprite> Banner { get; } = new LoadableResourceAsset($"{ShortPath}.Banner.png");

    public static LoadableAsset<Sprite> WikiButton { get; } = new LoadableResourceAsset($"{ShortPath}.WikiButton.png");
    public static LoadableAsset<Sprite> WikiButtonActive { get; } = new LoadableResourceAsset($"{ShortPath}.WikiButtonActive.png");
    public static LoadableAsset<Sprite> ZoomPlus { get; } = new LoadableResourceAsset($"{ShortPath}.Plus.png");
    public static LoadableAsset<Sprite> ZoomMinus { get; } = new LoadableResourceAsset($"{ShortPath}.Minus.png");
    public static LoadableAsset<Sprite> ZoomPlusActive { get; } = new LoadableResourceAsset($"{ShortPath}.PlusActive.png");
    public static LoadableAsset<Sprite> ZoomMinusActive { get; } = new LoadableResourceAsset($"{ShortPath}.MinusActive.png");

    public static LoadableAsset<Sprite> BarryButtonSprite { get; } = new LoadableResourceAsset($"{ShortPath}.BarryButton.png");
    public static LoadableAsset<Sprite> BroadcastSprite { get; } = new LoadableResourceAsset($"{ShortPath}.BroadcastButton.png");
    public static LoadableAsset<Sprite> DisperseSprite { get; } = new LoadableResourceAsset($"{ShortPath}.DisperseButton.png");
    public static LoadableAsset<Sprite> VitalsSprite { get; } = new LoadableResourceAsset($"{ShortPath}.VitalsButton.png");
    public static LoadableAsset<Sprite> CameraSprite { get; } = new LoadableResourceAsset($"{ShortPath}.CamButton.png");

    public static LoadableAsset<Sprite> KillSprite { get; } = new LoadableResourceAsset($"{ShortPath}.KillButton.png");
    public static LoadableAsset<Sprite> RangeSprite { get; } = new LoadableResourceAsset($"{ShortPath}.Range.png");

    public static LoadableAsset<Sprite> HysteriaSprite { get; } = new LoadableResourceAsset($"{ShortPath}.Hysteria.png", 300);
    public static LoadableAsset<Sprite> HysteriaCleanSprite { get; } = new LoadableResourceAsset($"{ShortPath}.HysteriaClean.png", 300);
    public static LoadableAsset<Sprite> ShootMeetingSprite { get; } = new LoadableResourceAsset($"{ShortPath}.Shoot.png", 300);
    public static LoadableAsset<Sprite> BlackmailLetterSprite { get; } = new LoadableResourceAsset($"{ShortPath}.BlackmailLetter.png");
    public static LoadableAsset<Sprite> BlackmailOverlaySprite { get; } = new LoadableResourceAsset($"{ShortPath}.BlackmailOverlay.png");
    public static LoadableAsset<Sprite> FootprintSprite { get; } = new LoadableResourceAsset($"{ShortPath}.Footprint.png");
    public static LoadableAsset<Sprite> CircleSprite { get; } = new LoadableResourceAsset($"{ShortPath}.Circle.png", 512);
    public static LoadableAsset<Sprite> SwapActive { get; } = new LoadableResourceAsset($"{ShortPath}.SwapActive.png", 300);
    public static LoadableAsset<Sprite> SwapInactive { get; } = new LoadableResourceAsset($"{ShortPath}.SwapDisabled.png", 300);
    public static LoadableAsset<Sprite> RevealButtonSprite { get; } = new LoadableResourceAsset($"{ShortPath}.Reveal.png", 300);
    public static LoadableAsset<Sprite> RevealCleanSprite { get; } = new LoadableResourceAsset($"{ShortPath}.RevealClean.png", 300);
    public static LoadableAsset<Sprite> Guess { get; } = new LoadableResourceAsset($"{ShortPath}.Guess.png");
    public static LoadableAsset<Sprite> InJailSprite { get; } = new LoadableResourceAsset($"{ShortPath}.InJail.png");
    public static LoadableAsset<Sprite> JailCellSprite { get; } = new LoadableResourceAsset($"{ShortPath}.JailCell.png");
    public static LoadableAsset<Sprite> ImitateSelectSprite { get; } = new LoadableResourceAsset($"{ShortPath}.ImitateSelect.png", 300);
    public static LoadableAsset<Sprite> ImitateDeselectSprite { get; } = new LoadableResourceAsset($"{ShortPath}.ImitateDeselect.png", 300);
    public static LoadableAsset<Sprite> ExecuteSprite { get; } = new LoadableResourceAsset($"{ShortPath}.Execute.png", 254);
    public static LoadableAsset<Sprite> ExecuteCleanSprite { get; } = new LoadableResourceAsset($"{ShortPath}.ExecuteClean.png", 254);
    public static LoadableAsset<Sprite> Hacked { get; } = new LoadableResourceAsset($"{ShortPath}.Hacked.png");
    public static LoadableAsset<Sprite> BarricadeVentSprite { get; } = new LoadableResourceAsset($"{ShortPath}.BarricadeVent.png", 200);
    public static LoadableAsset<Sprite> BarricadeFungleSprite { get; } = new LoadableResourceAsset($"{ShortPath}.BarricadePlant.png", 200);
    public static LoadableAsset<Sprite> LighterSprite { get; } = new LoadableResourceAsset($"{ShortPath}.Lighter.png");
    public static LoadableAsset<Sprite> DarkerSprite { get; } = new LoadableResourceAsset($"{ShortPath}.Darker.png");

    public static LoadableAsset<Sprite> ArrowSprite { get; } = new LoadableResourceAsset($"{ShortPath}.Arrow.png", 145);
    public static LoadableAsset<Sprite> BasicArrowSprite { get; } = new LoadableResourceAsset($"{ShortPath}.Arrow-OLD.png");
    public static LoadableAsset<Sprite> CrimeSceneSprite { get; } = new LoadableResourceAsset($"{ShortPath}.CrimeScene.png");
    public static LoadableAsset<Sprite> ScreenFlash { get; } = new LoadableResourceAsset($"{ShortPath}.ScreenFlash.png");
    public static LoadableAsset<Sprite> AbilityCounterPlayerSprite { get; } = new LoadableResourceAsset($"{ShortPath}.AbilityCounterPlayer.png");
    public static LoadableAsset<Sprite> AbilityCounterVentSprite { get; } = new LoadableResourceAsset($"{ShortPath}.AbilityCounterVent.png");
    public static LoadableAsset<Sprite> AbilityCounterBodySprite { get; } = new LoadableResourceAsset($"{ShortPath}.AbilityCounterBody.png");
    public static LoadableAsset<Sprite> GameSummarySprite { get; } = new LoadableResourceAsset($"{ShortPath}.GameSummaryButton.png");

    public static void Initialize()
    {
        AuAvengersAnims.Initialize();
    }
}

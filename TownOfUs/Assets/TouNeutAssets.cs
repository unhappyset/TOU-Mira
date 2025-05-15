using MiraAPI.Utilities.Assets;
using UnityEngine;

namespace TownOfUs.Assets;

public static class TouNeutAssets
{
    private const string ShortPath = "TownOfUs.Resources";
    // private const string BannerPath = $"{ShortPath}.RoleBanners"; // Commenting until it is used, so that the warnings stop screaming
    private const string ButtonPath = $"{ShortPath}.NeutButtons";

    // THIS FILE SHOULD ONLY HOLD BUTTONS AND ROLE BANNERS, EVERYTHING ELSE BELONGS IN TouAssets.cs
    public static LoadableAsset<Sprite> RememberButtonSprite { get; } = new LoadableResourceAsset($"{ButtonPath}.RememberButton.png");
    public static LoadableAsset<Sprite> ProtectSprite { get; } = new LoadableResourceAsset($"{ButtonPath}.ProtectButton.png");
    public static LoadableAsset<Sprite> GuardSprite { get; } = new LoadableResourceAsset($"{ButtonPath}.GuardButton.png");
    public static LoadableAsset<Sprite> BribeSprite { get; } = new LoadableResourceAsset($"{ButtonPath}.BribeButton.png");
    public static LoadableAsset<Sprite> VestSprite { get; } = new LoadableResourceAsset($"{ButtonPath}.VestButton.png");

    public static LoadableAsset<Sprite> Observe { get; } = new LoadableResourceAsset($"{ButtonPath}.ObserveButton.png");

    public static LoadableAsset<Sprite> DouseButtonSprite { get; } = new LoadableResourceAsset($"{ButtonPath}.DouseButton.png");
    public static LoadableAsset<Sprite> IgniteButtonSprite { get; } = new LoadableResourceAsset($"{ButtonPath}.IgniteButton.png");
    public static LoadableAsset<Sprite> HackSprite { get; } = new LoadableResourceAsset($"{ButtonPath}.HackButton.png");
    public static LoadableAsset<Sprite> MimicSprite { get; } = new LoadableResourceAsset($"{ButtonPath}.MimicButton.png");
    public static LoadableAsset<Sprite> InfectSprite { get; } = new LoadableResourceAsset($"{ButtonPath}.InfectButton.png");
    public static LoadableAsset<Sprite> ReapSprite { get; } = new LoadableResourceAsset($"{ButtonPath}.ReapButton.png");
    public static LoadableAsset<Sprite> BiteSprite { get; } = new LoadableResourceAsset($"{ButtonPath}.BiteButton.png");
    public static LoadableAsset<Sprite> RampageSprite { get; } = new LoadableResourceAsset($"{ButtonPath}.RampageButton.png");
}
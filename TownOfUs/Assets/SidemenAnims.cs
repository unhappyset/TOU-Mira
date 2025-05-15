using MiraAPI.Utilities.Assets;
using Reactor.Utilities;
using UnityEngine;

namespace TownOfUs.Assets;

public static class SidemenAnims
{
    public static AssetBundle TrapperShaderBundle { get; } = AssetBundleManager.Load(typeof(SidemenAnims).Assembly, "trappershader");
    public static AssetBundle SoundVisionBundle { get; } = AssetBundleManager.Load(typeof(SidemenAnims).Assembly, "soundvision"); // unused?

    // bomb visualizer thing
    public static LoadableAsset<Material> BombMaterial { get; private set; }
    public static LoadableAsset<Material> IgniteMaterial { get; private set; }
    public static LoadableAsset<Material> TrapMaterial { get; private set; }

    public static void Initialize()
    {
        BombMaterial = new LoadableBundleAsset<Material>("bomb", TrapperShaderBundle);
        IgniteMaterial = new LoadableBundleAsset<Material>("arsonisttrap", TrapperShaderBundle);
        TrapMaterial = new LoadableBundleAsset<Material>("trap", TrapperShaderBundle);
    }
}

using Il2CppInterop.Runtime.Attributes;
using MiraAPI.Utilities.Assets;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfUs.Modules.Wiki;

public interface IWikiDiscoverable
{
    [HideFromIl2Cpp] public List<CustomButtonWikiDescription> Abilities => [];

    public string SecondTabName => "Abilities";

    public string GetAdvancedDescription()
    {
        return MiscUtils.AppendOptionsText(GetType());
    }
}

public record struct CustomButtonWikiDescription(string name, string description, LoadableAsset<Sprite> icon);
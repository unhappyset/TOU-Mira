using Reactor.Utilities.Attributes;
using UnityEngine;

namespace TownOfUs.Modules.RainbowMod;

[RegisterInIl2Cpp]
public sealed class RainbowBehaviour(IntPtr cppPtr) : MonoBehaviour(cppPtr)
{
    public Renderer Renderer;
    public int Id;

    public void Update()
    {
        if (Renderer == null) return;

        if (RainbowUtils.IsRainbow(Id)) RainbowUtils.SetRainbow(Renderer);
    }

    public void AddRend(Renderer rend, int id)
    {
        Renderer = rend;
        Id = id;
    }
}
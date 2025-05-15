using UnityEngine;

namespace TownOfUs.Modifiers.Impostor;

public sealed class ScavengerArrowModifier(PlayerControl owner, Color color) : ArrowTargetModifier(owner, color, 0)
{
    public override string ModifierName => "Scavenger Arrow";
}

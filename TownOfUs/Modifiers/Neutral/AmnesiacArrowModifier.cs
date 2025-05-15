using UnityEngine;

namespace TownOfUs.Modifiers.Crewmate;

public sealed class AmnesiacArrowModifier(DeadBody deadBody, Color color) : ArrowDeadBodyModifier(deadBody, color, 0)
{
    public override string ModifierName => "Amnesiac Arrow";
}

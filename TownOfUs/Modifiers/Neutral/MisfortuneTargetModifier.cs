using MiraAPI.Modifiers;

namespace TownOfUs.Modifiers.Neutral;

public sealed class MisfortuneTargetModifier : BaseModifier
{
    public override string ModifierName => "Misfortunate (Can Be Spooked/Tormented/Haunted)";
    public override bool HideOnUi => true;
}
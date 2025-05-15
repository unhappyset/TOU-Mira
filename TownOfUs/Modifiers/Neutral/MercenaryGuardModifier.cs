using MiraAPI.Modifiers;

namespace TownOfUs.Modifiers.Neutral;

public sealed class MercenaryGuardModifier(PlayerControl mercenary) : BaseModifier
{
    public override string ModifierName => "Mercenary Guard";
    public override bool HideOnUi => true;
    public PlayerControl Mercenary { get; } = mercenary;

    public override void OnDeath(DeathReason reason)
    {
        ModifierComponent!.RemoveModifier(this);
    }
}

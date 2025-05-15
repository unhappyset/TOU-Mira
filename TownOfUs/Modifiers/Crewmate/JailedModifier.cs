using MiraAPI.Modifiers;

namespace TownOfUs.Modifiers.Crewmate;

public sealed class JailedModifier(byte jailorId) : BaseModifier
{
    public override string ModifierName => "Jailed";
    public override bool HideOnUi => true;
    public byte JailorId { get; } = jailorId;

    public override void OnDeath(DeathReason reason)
    {
        ModifierComponent!.RemoveModifier(this);
    }
}

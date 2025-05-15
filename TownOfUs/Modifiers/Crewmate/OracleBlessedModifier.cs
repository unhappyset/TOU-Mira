using MiraAPI.Modifiers;

namespace TownOfUs.Modifiers.Crewmate;

public sealed class OracleBlessedModifier(PlayerControl oracle) : BaseModifier
{
    public override string ModifierName => "Blessed";
    public override bool HideOnUi => true;
    public PlayerControl Oracle { get; } = oracle;

    public bool SavedFromExile { get; set; }

    public override void OnDeath(DeathReason reason)
    {
        ModifierComponent!.RemoveModifier(this);
    }
}

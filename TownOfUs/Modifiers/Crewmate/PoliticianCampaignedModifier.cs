using MiraAPI.Modifiers;

namespace TownOfUs.Modifiers.Crewmate;

public sealed class PoliticianCampaignedModifier(PlayerControl politician) : BaseModifier
{
    public override string ModifierName => "Campaigned";
    public override bool HideOnUi => true;

    public PlayerControl Politician { get; } = politician;

    public override void OnDeath(DeathReason reason)
    {
        ModifierComponent!.RemoveModifier(this);
    }
}

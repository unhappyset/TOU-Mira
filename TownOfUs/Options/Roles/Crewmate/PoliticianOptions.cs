using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.Utilities;
using TownOfUs.Roles.Crewmate;

namespace TownOfUs.Options.Roles.Crewmate;

public sealed class PoliticianOptions : AbstractOptionGroup<PoliticianRole>
{
    public override string GroupName => "Politician";

    [ModdedNumberOption("Campaign Cooldown", 10f, 60f, 2.5f, MiraNumberSuffixes.Seconds)]
    public float CampaignCooldown { get; set; } = 25f;
    [ModdedToggleOption("Mayor Keeps Outfit Throughout The Rounds")]
    public bool MayorKeepsOutfit { get; set; } = true;
}

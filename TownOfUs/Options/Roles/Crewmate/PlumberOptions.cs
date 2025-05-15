using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.Utilities;
using TownOfUs.Roles.Crewmate;

namespace TownOfUs.Options.Roles.Crewmate;

public sealed class PlumberOptions : AbstractOptionGroup<PlumberRole>
{
    public override string GroupName => "Plumber";

    [ModdedNumberOption("Flush Cooldown", 10f, 60f, 2.5f, MiraNumberSuffixes.Seconds, "0.0")]
    public float FlushCooldown { get; set; } = 25f;

    [ModdedNumberOption("Maximum Number Of Barricades", 1f, 15f, 1f, MiraNumberSuffixes.None, "0")]
    public float MaxBarricades { get; set; } = 5f;
}

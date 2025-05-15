using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.Utilities;
using TownOfUs.Roles.Crewmate;

namespace TownOfUs.Options.Roles.Crewmate;

public sealed class DetectiveOptions : AbstractOptionGroup<DetectiveRole>
{
    public override string GroupName => "Detective";

    [ModdedNumberOption("Examine Cooldown", 10f, 60f, 2.5f, MiraNumberSuffixes.Seconds)]
    public float ExamineCooldown { get; set; } = 25f;

    [ModdedToggleOption("Show Detective Reports")]
    public bool DetectiveReportOn { get; set; } = true;

    [ModdedNumberOption("Time Where Detective Will Have Role", 0f, 60f, 2.5f, MiraNumberSuffixes.Seconds)]
    public float DetectiveRoleDuration { get; set; } = 15f;

    [ModdedNumberOption("Time Where Detective Will Have Faction", 0f, 60f, 2.5f, MiraNumberSuffixes.Seconds)]
    public float DetectiveFactionDuration { get; set; } = 30f;
}

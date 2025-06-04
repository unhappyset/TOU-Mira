using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.Utilities;
using TownOfUs.Roles.Crewmate;

namespace TownOfUs.Options.Roles.Crewmate;

public sealed class EngineerOptions : AbstractOptionGroup<EngineerTouRole>
{
    public override string GroupName => "Engineer";

    [ModdedNumberOption("Maximum Number Of Fixes", 1f, 15f, 1f, MiraNumberSuffixes.None, "0")]
    public float MaxFixes { get; set; } = 5f;

    [ModdedNumberOption("Fix Delay", 0f, 5f, 0.5f, MiraNumberSuffixes.Seconds)]
    public float FixDelay { get; set; } = 0.5f;
}

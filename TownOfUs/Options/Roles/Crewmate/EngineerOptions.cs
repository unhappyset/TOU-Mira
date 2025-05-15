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
}

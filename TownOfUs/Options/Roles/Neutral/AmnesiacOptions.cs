using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.Utilities;
using TownOfUs.Roles.Neutral;

namespace TownOfUs.Options.Roles.Neutral;

public sealed class AmnesiacOptions : AbstractOptionGroup<AmnesiacRole>
{
    public override string GroupName => "Amnesiac";

    [ModdedToggleOption("Amnesiac Gets Arrows Pointing To Dead Bodies")]
    public bool RememberArrows { get; set; } = false;

    [ModdedNumberOption("Time After Death Arrow Appears", 0f, 15f, 1f, MiraNumberSuffixes.Seconds, "0")]
    public float RememberArrowDelay { get; set; } = 5f;

}

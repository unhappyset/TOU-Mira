using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using TownOfUs.Roles.Neutral;

namespace TownOfUs.Options.Roles.Neutral;

public sealed class PhantomOptions : AbstractOptionGroup<PhantomTouRole>
{
    public override string GroupName => "Phantom";

    [ModdedNumberOption("Tasks Left Before Clickable", 1, 15, 1)]
    public float NumTasksLeftBeforeClickable { get; set; } = 3f;

    [ModdedToggleOption("Winning Ends The Game")]
    public bool WinEndsGame { get; set; } = true;
}

using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.Utilities;
using TownOfUs.Roles.Crewmate;

namespace TownOfUs.Options.Roles.Crewmate;

public sealed class TrackerOptions : AbstractOptionGroup<TrackerTouRole>
{
    public override string GroupName => "Tracker";

    [ModdedNumberOption("Track Cooldown", 1f, 30f, 1f, MiraNumberSuffixes.Seconds)]
    public float TrackCooldown { get; set; } = 20f;

    [ModdedNumberOption("Maximum Number Of Tracks", 1f, 15f, 1f, MiraNumberSuffixes.None, "0")]
    public float MaxTracks { get; set; } = 5f;

    [ModdedNumberOption("Arrow Update Interval", 0.5f, 15f, 0.5f, MiraNumberSuffixes.Seconds)]
    public float UpdateInterval { get; set; } = 5f;

    [ModdedToggleOption("Tracker Arrows Make Sound On Death")]
    public bool SoundOnDeactivate { get; set; } = true;

    [ModdedToggleOption("Tracker Arrows Reset After Each Round")]
    public bool ResetOnNewRound { get; set; } = true;
}

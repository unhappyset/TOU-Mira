using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.Utilities;
using TownOfUs.Roles.Crewmate;

namespace TownOfUs.Options.Roles.Crewmate;

public sealed class LookoutOptions : AbstractOptionGroup<LookoutRole>
{
    public override string GroupName => "Lookout";

    [ModdedNumberOption("Watch Cooldown", 1f, 30f, 1f, MiraNumberSuffixes.Seconds)]
    public float WatchCooldown { get; set; } = 20f;

    [ModdedNumberOption("Maximum Number Of Players That Can Be Watched", 1f, 15f, 1f, MiraNumberSuffixes.None, "0")]
    public float MaxWatches { get; set; } = 5;

    [ModdedToggleOption("Lookout Watches Reset After Each Round")]
    public bool LoResetOnNewRound { get; set; } = true;

    [ModdedToggleOption("Get More Uses From Completing Tasks")]
    public bool TaskUses { get; set; } = false;
}

using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.Utilities;
using TownOfUs.Roles.Impostor;

namespace TownOfUs.Options.Roles.Impostor;

public sealed class JanitorOptions : AbstractOptionGroup<JanitorRole>
{
    public override string GroupName => "Janitor";

    [ModdedNumberOption("Clean Uses Per Game", 0f, 15f, 5f, MiraNumberSuffixes.None, "0", zeroInfinity: true)]
    public float MaxClean { get; set; } = 0f;
    [ModdedNumberOption("Clean Cooldown", 10f, 60f, 2.5f, MiraNumberSuffixes.Seconds)]
    public float CleanCooldown { get; set; } = 40f;
    [ModdedNumberOption("Clean Delay", 0f, 60f, 2.5f, MiraNumberSuffixes.Seconds)]
    public float CleanDelay { get; set; } = 2.5f;
    [ModdedToggleOption("Janitor Can Kill With Teammate")]
    public bool JanitorKill { get; set; } = true;
}

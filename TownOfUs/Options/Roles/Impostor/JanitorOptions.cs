using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.Utilities;
using TownOfUs.Roles.Impostor;

namespace TownOfUs.Options.Roles.Impostor;

public sealed class JanitorOptions : AbstractOptionGroup<JanitorRole>
{
    public override string GroupName => "Janitor";

    [ModdedNumberOption("Clean Cooldown", 10f, 60f, 2.5f, MiraNumberSuffixes.Seconds)]
    public float CleanCooldown { get; set; } = 40f;
    [ModdedToggleOption("Janitor Can Kill")]
    public bool JanitorKill { get; set; } = true;
}

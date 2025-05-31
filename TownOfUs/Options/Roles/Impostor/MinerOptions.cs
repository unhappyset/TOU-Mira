using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.Utilities;
using TownOfUs.Roles.Impostor;

namespace TownOfUs.Options.Roles.Impostor;

public sealed class MinerOptions : AbstractOptionGroup<MinerRole>
{
    public override string GroupName => "Miner";

    [ModdedNumberOption("Mine Cooldown", 10f, 60f, 2.5f, MiraNumberSuffixes.Seconds)]
    public float MineCooldown { get; set; } = 25f;

    [ModdedNumberOption("Mine Delay", 0f, 10f, 0.5f, MiraNumberSuffixes.Seconds)]
    public float MineDelay { get; set; } = 3f;
    [ModdedToggleOption("Miner Can Kill")]
    public bool MinerKill { get; set; } = true;
}

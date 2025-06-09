using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.Utilities;
using TownOfUs.Roles.Impostor;

namespace TownOfUs.Options.Roles.Impostor;

public sealed class MinerOptions : AbstractOptionGroup<MinerRole>
{
    public override string GroupName => "Miner";

    [ModdedNumberOption("Number Of Miner Vents Per Game", 0f, 30f, 5f, MiraNumberSuffixes.None, "0", zeroInfinity: true)]
    public float MaxMines { get; set; } = 0f;

    [ModdedNumberOption("Mine Cooldown", 10f, 60f, 2.5f, MiraNumberSuffixes.Seconds)]
    public float MineCooldown { get; set; } = 25f;

    [ModdedNumberOption("Mine Delay", 0f, 10f, 0.5f, MiraNumberSuffixes.Seconds)]
    public float MineDelay { get; set; } = 3f;

    [ModdedEnumOption("Mine Visiblity", typeof(MineVisiblityOptions), ["Immediate", "After Use"])]
    public MineVisiblityOptions MineVisibility { get; set; } = MineVisiblityOptions.Immediate;

    [ModdedToggleOption("Miner Can Kill")]
    public bool MinerKill { get; set; } = true;
}

public enum MineVisiblityOptions
{
    Immediate,
    AfterUse,
}

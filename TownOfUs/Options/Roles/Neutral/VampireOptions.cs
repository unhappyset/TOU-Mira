using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.Utilities;
using TownOfUs.Roles.Neutral;

namespace TownOfUs.Options.Roles.Neutral;

public sealed class VampireOptions : AbstractOptionGroup<VampireRole>
{
    public override string GroupName => "Vampire";

    [ModdedNumberOption("Bite Cooldown", 10f, 60f, 2.5f, MiraNumberSuffixes.Seconds)]
    public float BiteCooldown { get; set; } = 25f;

    [ModdedNumberOption("Maximum Number Of Vampires Per Game", 2, 5, 1, MiraNumberSuffixes.None, "0")]
    public float MaxVampires { get; set; } = 2;

    [ModdedToggleOption("Vampires Have Impostor Vision")]
    public bool HasVision { get; set; } = true;

    [ModdedToggleOption("New Vampires Can Assassinate")]
    public bool CanGuessAsNewVamp { get; set; } = true;

    [ModdedToggleOption("Can Convert Neutral Benign Roles")]
    public bool CanConvertNeutralBenign { get; set; } = true;

    [ModdedToggleOption("Can Convert Neutral Evil Roles")]
    public bool CanConvertNeutralEvil { get; set; } = true;

    [ModdedToggleOption("Can Convert Lovers")]
    public bool CanConvertLovers { get; set; } = false;

    [ModdedToggleOption("New Vampires Can Convert")]
    public bool CanConvertAsNewVamp { get; set; } = true;

    [ModdedToggleOption("Vampires Can Vent")]
    public bool CanVent { get; set; } = false;
}

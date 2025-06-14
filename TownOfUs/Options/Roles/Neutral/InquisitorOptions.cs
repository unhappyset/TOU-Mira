using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.Utilities;
using TownOfUs.Roles.Neutral;

namespace TownOfUs.Options.Roles.Neutral;

public sealed class InquisitorOptions : AbstractOptionGroup<InquisitorRole>
{
    public override string GroupName => "Inquisitor";

    [ModdedNumberOption("Vanquish Cooldown", 10f, 60f, 2.5f, MiraNumberSuffixes.Seconds)]
    public float VanquishCooldown { get; set; } = 25f;

    [ModdedNumberOption("Inquire Cooldown", 10f, 60f, 2.5f, MiraNumberSuffixes.Seconds)]
    public float InquireCooldown { get; set; } = 25f;

    [ModdedToggleOption("Inquisitor Continues Game")]
    public bool StallGame { get; set; } = true;

    [ModdedToggleOption("Inquisitor Can't Inquire")]
    public bool CantInquire { get; set; } = false;

    [ModdedNumberOption("Max Number Of Inquires", 1f, 15f, 1f, MiraNumberSuffixes.None, "0")]
    public float MaxUses { get; set; } = 5f;

    [ModdedNumberOption("Amount of Heretics Needed", 3f, 5f, 1f, MiraNumberSuffixes.None, "0")]
    public float AmountOfHeretics { get; set; } = 3f;
}

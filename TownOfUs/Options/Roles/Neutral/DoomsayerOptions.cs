using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.Utilities;
using TownOfUs.Roles.Neutral;

namespace TownOfUs.Options.Roles.Neutral;

public sealed class DoomsayerOptions : AbstractOptionGroup<DoomsayerRole>
{
    public override string GroupName => "Doomsayer";

    [ModdedNumberOption("Observe Cooldown", 1f, 30f, 1f, MiraNumberSuffixes.Seconds)]
    public float ObserveCooldown { get; set; } = 20f;

    [ModdedNumberOption("Number Of Guesses Needed To Win", 2f, 5f, 1f, MiraNumberSuffixes.None, "0")]
    public float DoomsayerGuessesToWin { get; set; } = 3f;

    [ModdedToggleOption("Doomsayer Guesses All Roles At Once")]
    public bool DoomsayerGuessAllAtOnce { get; set; } = false;

    [ModdedToggleOption("Doomsayer Can't Observe")]
    public bool CantObserve { get; set; } = false;

    [ModdedToggleOption("Winning Ends The Game")]
    public bool WinEndsGame { get; set; } = true;
}

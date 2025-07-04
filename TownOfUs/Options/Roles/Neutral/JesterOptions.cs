using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.Utilities;
using TownOfUs.Roles.Neutral;

namespace TownOfUs.Options.Roles.Neutral;

public sealed class JesterOptions : AbstractOptionGroup<JesterRole>
{
    public override string GroupName => "Jester";

    [ModdedToggleOption("Jester Can Button")]
    public bool CanButton { get; set; } = true;

    [ModdedToggleOption("Jester Can Hide In Vents")]
    public bool CanVent { get; set; } = true;

    [ModdedToggleOption("Jester Has Impostor Vision")]
    public bool ImpostorVision { get; set; } = true;

    [ModdedToggleOption("Jester Scatter Mechanic Enabled")]
    public bool ScatterOn { get; set; } = true;

    [ModdedNumberOption("Jester Scatter Timer", 10f, 60f, 2.5f, MiraNumberSuffixes.Seconds, "0.0")]
    public float ScatterTimer { get; set; } = 25f;

    [ModdedEnumOption("Jester Win", typeof(JestWinOptions), ["Ends Game", "Haunts", "Nothing"])]
    public JestWinOptions JestWin { get; set; } = JestWinOptions.EndsGame;
}

public enum JestWinOptions
{
    EndsGame,
    Haunts,
    Nothing
}
using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;

namespace TownOfUs.Options;

public sealed class GameTimerOptions : AbstractOptionGroup
{
    public override string GroupName => "Game Timer";
    public override uint GroupPriority => 6;

    [ModdedToggleOption("Game Timer")]
    public bool GameTimerEnabled { get; set; } = false;
    
    public ModdedNumberOption GameTimeLimit { get; } = new ModdedNumberOption("Game Time Limit", 3f, 1f, 12f, 0.5f, MiraNumberSuffixes.None, "0.0m")
    {
        Visible = () => OptionGroupSingleton<GameTimerOptions>.Instance.GameTimerEnabled,
    };
}
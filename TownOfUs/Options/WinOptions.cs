using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;

namespace TownOfUs.Options;

public sealed class WinOptions : AbstractOptionGroup
{
    public override string GroupName => "Game Timer";
    public override uint GroupPriority => 6;

    [ModdedToggleOption("Game Timer")]
    public bool GameTimerEnabled { get; set; } = false;
    
    public ModdedNumberOption GameTimeLimit { get; } = new ModdedNumberOption("Game Time Limit", 300f, 60f, 1200f, 15f, MiraNumberSuffixes.Seconds)
    {
        Visible = () => OptionGroupSingleton<WinOptions>.Instance.GameTimerEnabled,
    };
}
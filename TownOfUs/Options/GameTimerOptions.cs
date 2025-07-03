using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;

namespace TownOfUs.Options;

public sealed class GameTimerOptions : AbstractOptionGroup
{
    public override string GroupName => "End Game Timer";
    public override uint GroupPriority => 3;

    [ModdedToggleOption("Game Timer")]
    public bool GameTimerEnabled { get; set; } = false;
    public ModdedEnumOption TimerEndOption { get; } = new ModdedEnumOption("On Timer End", 1, typeof(GameTimerType), ["Impostor Win", "Game Draw"])
    {
        Visible = () => OptionGroupSingleton<GameTimerOptions>.Instance.GameTimerEnabled,
    };

    public ModdedNumberOption GameTimeLimit { get; } = new ModdedNumberOption("Game Time Limit", 3f, 1f, 30f, 0.5f, MiraNumberSuffixes.None, "0.0m")
    {
        Visible = () => OptionGroupSingleton<GameTimerOptions>.Instance.GameTimerEnabled,
    };
}

public enum GameTimerType
{
    Impostors,
    GameDraw,
}
using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.GameOptions.OptionTypes;
using TownOfUs.Roles.Neutral;

namespace TownOfUs.Options.Roles.Neutral;

public sealed class ExecutionerOptions : AbstractOptionGroup<ExecutionerRole>
{
    public override string GroupName => "Executioner";

    [ModdedEnumOption("On Target Death, Executioner Becomes", typeof(BecomeOptions))]
    public BecomeOptions OnTargetDeath { get; set; } = BecomeOptions.Jester;

    [ModdedToggleOption("Executioner Can Call Meeting")]
    public bool CanButton { get; set; } = true;
    
    [ModdedEnumOption("Executioner Win", typeof(ExeWinOptions), ["Ends Game", "Torments", "Nothing"])]
    public ExeWinOptions ExeWin { get; set; } = ExeWinOptions.EndsGame;

}
public enum ExeWinOptions
{
    EndsGame,
    Torments,
    Nothing
}

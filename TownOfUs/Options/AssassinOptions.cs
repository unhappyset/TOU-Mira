using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;

namespace TownOfUs.Options;

public sealed class AssassinOptions : AbstractOptionGroup
{
    public override string GroupName => "Assassin Options";
    public override uint GroupPriority => 5;

    [ModdedNumberOption("Number Of Impostor Assassins", 0, 4, 1, MiraNumberSuffixes.None, "0")]
    public float NumberOfImpostorAssassins { get; set; } = 1;

    [ModdedNumberOption("Number Of Neutral Assassins", 0, 5, 1, MiraNumberSuffixes.None, "0")]
    public float NumberOfNeutralAssassins { get; set; } = 1;

    [ModdedToggleOption("Amnesiac Turned Impostor Gets Ability")]
    public bool AmneTurnImpAssassin { get; set; } = false;

    [ModdedToggleOption("Amnesiac Turned Neutral Killing Gets Ability")]
    public bool AmneTurnNeutAssassin { get; set; } = false;

    [ModdedToggleOption("Traitor Gets Ability")]
    public bool TraitorCanAssassin { get; set; } = false;

    [ModdedNumberOption("Number Of Assassin Kills", 1, 15, 1, MiraNumberSuffixes.None, "0")]
    public float AssassinKills { get; set; } = 1;

    [ModdedToggleOption("Assassin Can Kill More Than Once Per Meeting")]
    public bool AssassinMultiKill { get; set; } = false;

    [ModdedToggleOption("Assassin Can Guess \"Crewmate\"")]
    public bool AssassinCrewmateGuess { get; set; } = false;

    [ModdedToggleOption("Assassin Can Guess Crew Investigative Roles")]
    public bool AssassinGuessInvest { get; set; } = true;

    [ModdedToggleOption("Assassin Can Guess Neutral Benign Roles")]
    public bool AssassinGuessNeutralBenign { get; set; } = false;

    [ModdedToggleOption("Assassin Can Guess Neutral Evil Roles")]
    public bool AssassinGuessNeutralEvil { get; set; } = false;

    [ModdedToggleOption("Assassin Can Guess Neutral Killing Roles")]
    public bool AssassinGuessNeutralKilling { get; set; } = false;

    [ModdedToggleOption("Assassin Can Guess Impostor Roles")]
    public bool AssassinGuessImpostors { get; set; } = false;

    [ModdedToggleOption("Assassin Can Guess Crewmate Modifiers")]
    public bool AssassinGuessCrewModifiers { get; set; } = false;

    public ModdedToggleOption AssassinGuessInvModifier { get; } = new ModdedToggleOption("Assassin Can Guess Investigator Modifier", false)
    {
        Visible = () => OptionGroupSingleton<AssassinOptions>.Instance.AssassinGuessCrewModifiers,
    };

    public ModdedToggleOption AssassinGuessSpyModifier { get; } = new ModdedToggleOption("Assassin Can Guess Spy Modifier", false)
    {
        Visible = () => OptionGroupSingleton<AssassinOptions>.Instance.AssassinGuessCrewModifiers,
    };

    [ModdedToggleOption("Assassin Can Guess Lovers")]
    public bool AssassinGuessLovers { get; set; } = false;

}

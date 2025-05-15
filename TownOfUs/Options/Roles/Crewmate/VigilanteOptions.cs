using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using TownOfUs.Roles.Crewmate;

namespace TownOfUs.Options.Roles.Crewmate;

public sealed class VigilanteOptions : AbstractOptionGroup<VigilanteRole>
{
    public override string GroupName => "Vigilante";

    [ModdedNumberOption("Number Of Vigilante Kills", 1f, 15f)]
    public float VigilanteKills { get; set; } = 1f;

    [ModdedToggleOption("Vigilante Can Kill More Than Once Per Meeting")]
    public bool VigilanteMultiKill { get; set; } = false;

    [ModdedToggleOption("Vigilante Can Guess Neutral Benign Roles")]
    public bool VigilanteGuessNeutralBenign { get; set; } = false;

    [ModdedToggleOption("Vigilante Can Guess Neutral Evil Roles")]
    public bool VigilanteGuessNeutralEvil { get; set; } = false;

    [ModdedToggleOption("Vigilante Can Guess Neutral Killing Roles")]
    public bool VigilanteGuessNeutralKilling { get; set; } = true;

    [ModdedToggleOption("Vigilante Can Guess Impostor Modifiers")]
    public bool VigilanteGuessImpMods { get; set; } = false;

    [ModdedToggleOption("Vigilante Can Guess Lovers")]
    public bool VigilanteGuessLovers { get; set; } = false;
}

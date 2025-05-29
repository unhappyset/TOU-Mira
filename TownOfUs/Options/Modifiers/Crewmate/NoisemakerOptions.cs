using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.Utilities;
using TownOfUs.Modifiers.Game.Crewmate;
using UnityEngine;

namespace TownOfUs.Options.Modifiers.Crewmate;

public sealed class NoisemakerOptions : AbstractOptionGroup<NoisemakerModifier>
{
    public override string GroupName => "Noisemaker";
    public override uint GroupPriority => 34;
    public override Color GroupColor => TownOfUsColors.Noisemaker;

    [ModdedToggleOption("Impostors Get Alert")]
    public bool ImpostorsAlerted { get; set; } = true;

    [ModdedToggleOption("NKs Get Alert")]
    public bool NeutsAlerted { get; set; } = true;

    [ModdedNumberOption("Alert Duration", 1f, 20f, 1f, MiraNumberSuffixes.Seconds)]
    public float AlertDuration { get; set; } = 10f;
}

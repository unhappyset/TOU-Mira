using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;
using UnityEngine;

namespace TownOfUs.Options.Modifiers;

public sealed class CrewmateModifierOptions : AbstractOptionGroup
{
    public override string GroupName => "Crewmate Modifiers";
    public override Color GroupColor => Palette.CrewmateRoleHeaderBlue;
    public override bool ShowInModifiersMenu => true;
    public override uint GroupPriority => 2;

    [ModdedNumberOption("Aftermath Amount", 0, 5, 1)]
    public float AftermathAmount { get; set; } = 0;
    public ModdedNumberOption AftermathChance { get; } = new ModdedNumberOption("Aftermath Chance", 0, 0, 100f, 10f, MiraNumberSuffixes.Percent)
    {
        Visible = () => OptionGroupSingleton<CrewmateModifierOptions>.Instance.AftermathAmount > 0,
    };
    
    [ModdedNumberOption("Bait Amount", 0, 5, 1)]
    public float BaitAmount { get; set; } = 0;
    public ModdedNumberOption BaitChance { get; } = new ModdedNumberOption("Bait Chance", 0, 0, 100f, 10f, MiraNumberSuffixes.Percent)
    {
        Visible = () => OptionGroupSingleton<CrewmateModifierOptions>.Instance.BaitAmount > 0,
    };

    [ModdedNumberOption("Celebrity Chance", 0, 100f, 10f, MiraNumberSuffixes.Percent)]
    public float CelebrityChance { get; set; } = 0;

    [ModdedNumberOption("Diseased Amount", 0, 5, 1)]
    public float DiseasedAmount { get; set; } = 0;
    public ModdedNumberOption DiseasedChance { get; } = new ModdedNumberOption("Diseased Chance", 0, 0, 100f, 10f, MiraNumberSuffixes.Percent)
    {
        Visible = () => OptionGroupSingleton<CrewmateModifierOptions>.Instance.DiseasedAmount > 0,
    };

    [ModdedNumberOption("Frosty Amount", 0, 5, 1)]
    public float FrostyAmount { get; set; } = 0;
    public ModdedNumberOption FrostyChance { get; } = new ModdedNumberOption("Frosty Chance", 0, 0, 100f, 10f, MiraNumberSuffixes.Percent)
    {
        Visible = () => OptionGroupSingleton<CrewmateModifierOptions>.Instance.FrostyAmount > 0,
    };

    [ModdedNumberOption("Investigator Amount", 0, 5, 1)]
    public float InvestigatorAmount { get; set; } = 0;
    public ModdedNumberOption InvestigatorChance { get; } = new ModdedNumberOption("Investigator Chance", 0, 0, 100f, 10f, MiraNumberSuffixes.Percent)
    {
        Visible = () => OptionGroupSingleton<CrewmateModifierOptions>.Instance.InvestigatorAmount > 0,
    };

    [ModdedNumberOption("Multitasker Amount", 0, 5, 1)]
    public float MultitaskerAmount { get; set; } = 0;
    public ModdedNumberOption MultitaskerChance { get; } = new ModdedNumberOption("Multitasker Chance", 0, 0, 100f, 10f, MiraNumberSuffixes.Percent)
    {
        Visible = () => OptionGroupSingleton<CrewmateModifierOptions>.Instance.MultitaskerAmount > 0,
    };

    [ModdedNumberOption("Spy Amount", 0, 5, 1)]
    public float SpyAmount { get; set; } = 0;
    public ModdedNumberOption SpyChance { get; } = new ModdedNumberOption("Spy Chance", 0, 0, 100f, 10f, MiraNumberSuffixes.Percent)
    {
        Visible = () => OptionGroupSingleton<CrewmateModifierOptions>.Instance.SpyAmount > 0,
    };

    [ModdedNumberOption("Taskmaster Amount", 0, 5, 1)]
    public float TaskmasterAmount { get; set; } = 0;
    public ModdedNumberOption TaskmasterChance { get; } = new ModdedNumberOption("Taskmaster Chance", 0, 0, 100f, 10f, MiraNumberSuffixes.Percent)
    {
        Visible = () => OptionGroupSingleton<CrewmateModifierOptions>.Instance.TaskmasterAmount > 0,
    };

    [ModdedNumberOption("Torch Amount", 0, 5, 1)]
    public float TorchAmount { get; set; } = 0;
    public ModdedNumberOption TorchChance { get; } = new ModdedNumberOption("Torch Chance", 0, 0, 100f, 10f, MiraNumberSuffixes.Percent)
    {
        Visible = () => OptionGroupSingleton<CrewmateModifierOptions>.Instance.TorchAmount > 0,
    };
}

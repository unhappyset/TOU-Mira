using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;

namespace TownOfUs.Options;

public sealed class RoleOptions : AbstractOptionGroup
{
    public override string GroupName => "Role";
    public override uint GroupPriority => 3;

    public static readonly string[] OptionStrings =
    [
        "Common <color=#66FFFFFF>Crew</color>",
        "Random <color=#66FFFFFF>Crew</color>",
        "<color=#66FFFFFF>Crew</color> Investigative",
        "<color=#66FFFFFF>Crew</color> Killing",
        "<color=#66FFFFFF>Crew</color> Protective",
        "<color=#66FFFFFF>Crew</color> Power",
        "<color=#66FFFFFF>Crew</color> Support",
        "Special <color=#66FFFFFF>Crew</color>",
        "Non-<color=#FF0000FF>Imp</color>",
        "Common <color=#999999FF>Neutral</color>",
        "Random <color=#999999FF>Neutral</color>",
        "<color=#999999FF>Neutral</color> Benign",
        "<color=#999999FF>Neutral</color> Evil",
        "<color=#999999FF>Neutral</color> Killing",
        "Common <color=#FF0000FF>Imp</color>",
        "Random <color=#FF0000FF>Imp</color>",
        "<color=#FF0000FF>Imp</color> Concealing",
        "<color=#FF0000FF>Imp</color> Killing",
        "<color=#FF0000FF>Imp</color> Support",
        "Any"
    ];

    [ModdedToggleOption("Reduce Impostor Streak")]
    public bool LastImpostorBias { get; set; } = false;

    public ModdedNumberOption ImpostorBiasPercent { get; } = new("Reduction Chance", 15f, 0f, 100f, 5f, MiraNumberSuffixes.Percent)
    {
        Visible = () => OptionGroupSingleton<RoleOptions>.Instance.LastImpostorBias
    };

    [ModdedToggleOption("Role List Enabled")]
    public bool RoleListEnabled { get; set; } = true;

    public ModdedEnumOption Slot1 { get; } = new("Slot 1", (int)RoleListOption.CrewCommon, typeof(RoleListOption), OptionStrings)
    {
        Visible = () => OptionGroupSingleton<RoleOptions>.Instance.RoleListEnabled
    };

    public ModdedEnumOption Slot2 { get; } = new("Slot 2", (int)RoleListOption.CrewCommon, typeof(RoleListOption), OptionStrings)
    {
        Visible = () => OptionGroupSingleton<RoleOptions>.Instance.RoleListEnabled
    };

    public ModdedEnumOption Slot3 { get; } = new("Slot 3", (int)RoleListOption.CrewCommon, typeof(RoleListOption), OptionStrings)
    {
        Visible = () => OptionGroupSingleton<RoleOptions>.Instance.RoleListEnabled
    };

    public ModdedEnumOption Slot4 { get; } = new("Slot 4", (int)RoleListOption.ImpCommon, typeof(RoleListOption), OptionStrings)
    {
        Visible = () => OptionGroupSingleton<RoleOptions>.Instance.RoleListEnabled
    };

    public ModdedEnumOption Slot5 { get; } = new("Slot 5", (int)RoleListOption.CrewCommon, typeof(RoleListOption), OptionStrings)
    {
        Visible = () => OptionGroupSingleton<RoleOptions>.Instance.RoleListEnabled
    };

    public ModdedEnumOption Slot6 { get; } = new("Slot 6", (int)RoleListOption.CrewCommon, typeof(RoleListOption), OptionStrings)
    {
        Visible = () => OptionGroupSingleton<RoleOptions>.Instance.RoleListEnabled
    };

    public ModdedEnumOption Slot7 { get; } = new("Slot 7", (int)RoleListOption.CrewCommon, typeof(RoleListOption), OptionStrings)
    {
        Visible = () => OptionGroupSingleton<RoleOptions>.Instance.RoleListEnabled
    };

    public ModdedEnumOption Slot8 { get; } = new("Slot 8", (int)RoleListOption.CrewCommon, typeof(RoleListOption), OptionStrings)
    {
        Visible = () => OptionGroupSingleton<RoleOptions>.Instance.RoleListEnabled
    };

    public ModdedEnumOption Slot9 { get; } = new("Slot 9", (int)RoleListOption.ImpCommon, typeof(RoleListOption), OptionStrings)
    {
        Visible = () => OptionGroupSingleton<RoleOptions>.Instance.RoleListEnabled
    };

    public ModdedEnumOption Slot10 { get; } = new("Slot 10", (int)RoleListOption.CrewCommon, typeof(RoleListOption), OptionStrings)
    {
        Visible = () => OptionGroupSingleton<RoleOptions>.Instance.RoleListEnabled
    };

    public ModdedEnumOption Slot11 { get; } = new("Slot 11", (int)RoleListOption.CrewCommon, typeof(RoleListOption), OptionStrings)
    {
        Visible = () => OptionGroupSingleton<RoleOptions>.Instance.RoleListEnabled
    };

    public ModdedEnumOption Slot12 { get; } = new("Slot 12", (int)RoleListOption.CrewCommon, typeof(RoleListOption), OptionStrings)
    {
        Visible = () => OptionGroupSingleton<RoleOptions>.Instance.RoleListEnabled
    };

    public ModdedEnumOption Slot13 { get; } = new("Slot 13", (int)RoleListOption.CrewCommon, typeof(RoleListOption), OptionStrings)
    {
        Visible = () => OptionGroupSingleton<RoleOptions>.Instance.RoleListEnabled
    };

    public ModdedEnumOption Slot14 { get; } = new("Slot 14", (int)RoleListOption.ImpCommon, typeof(RoleListOption), OptionStrings)
    {
        Visible = () => OptionGroupSingleton<RoleOptions>.Instance.RoleListEnabled
    };

    public ModdedEnumOption Slot15 { get; } = new("Slot 15", (int)RoleListOption.CrewCommon, typeof(RoleListOption), OptionStrings)
    {
        Visible = () => OptionGroupSingleton<RoleOptions>.Instance.RoleListEnabled
    };

    public ModdedNumberOption MinNeutralBenign { get; } = new("Min Neutral Benign", 0f, 0f, 3f, 1f, MiraNumberSuffixes.None, "0")
    {
        Visible = () => !OptionGroupSingleton<RoleOptions>.Instance.RoleListEnabled
    };

    public ModdedNumberOption MaxNeutralBenign { get; } = new("Max Neutral Benign", 0f, 0f, 3f, 1f, MiraNumberSuffixes.None, "0")
    {
        Visible = () => !OptionGroupSingleton<RoleOptions>.Instance.RoleListEnabled
    };

    public ModdedNumberOption MinNeutralEvil { get; } = new("Min Neutral Evil", 0f, 0f, 3f, 1f, MiraNumberSuffixes.None, "0")
    {
        Visible = () => !OptionGroupSingleton<RoleOptions>.Instance.RoleListEnabled
    };

    public ModdedNumberOption MaxNeutralEvil { get; } = new("Max Neutral Evil", 0f, 0f, 3f, 1f, MiraNumberSuffixes.None, "0")
    {
        Visible = () => !OptionGroupSingleton<RoleOptions>.Instance.RoleListEnabled
    };

    public ModdedNumberOption MinNeutralKiller { get; } = new("Min Neutral Killer", 0f, 0f, 5f, 1f, MiraNumberSuffixes.None, "0")
    {
        Visible = () => !OptionGroupSingleton<RoleOptions>.Instance.RoleListEnabled
    };

    public ModdedNumberOption MaxNeutralKiller { get; } = new("Max Neutral Killer", 0f, 0f, 5f, 1f, MiraNumberSuffixes.None, "0")
    {
        Visible = () => !OptionGroupSingleton<RoleOptions>.Instance.RoleListEnabled
    };
}

public enum RoleListOption
{
    CrewCommon,
    CrewRandom,
    CrewInvest,
    CrewKilling,
    CrewProtective,
    CrewPower,
    CrewSupport,
    CrewSpecial,
    NonImp,
    NeutCommon,
    NeutRandom,
    NeutBenign,
    NeutEvil,
    NeutKilling,
    ImpCommon,
    ImpRandom,
    ImpConceal,
    ImpKilling,
    ImpSupport,
    Any
}
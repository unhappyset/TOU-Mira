using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;
using TownOfUs.Roles.Crewmate;

namespace TownOfUs.Options.Roles.Crewmate;

public sealed class MedicOptions : AbstractOptionGroup<MedicRole>
{
    public override string GroupName => "Medic";

    [ModdedEnumOption("Show Shielded Player", typeof(MedicOption),
        ["Medic", "Shielded", "Shielded + Medic", "Everyone", "No One"])]
    public MedicOption ShowShielded { get; set; } = MedicOption.ShieldedAndMedic;

    [ModdedEnumOption("Who Gets Murder Attempt Indicator", typeof(MedicOption),
        ["Medic", "Shielded", "Shielded + Medic", "Everyone", "No One"])]
    public MedicOption WhoGetsNotification { get; set; } = MedicOption.Medic;

    [ModdedToggleOption("Allow Medic To Give Shield Away Next Round")]
    public bool ChangeTarget { get; set; } = true;

    [ModdedToggleOption("Shield Breaks On Murder Attempt")]
    public bool ShieldBreaks { get; set; } = false;

    [ModdedToggleOption("Show Medic Reports")]
    public bool ShowReports { get; set; } = true;

    public ModdedNumberOption MedicReportNameDuration { get; } = new("Time Where Medic Will Have Name", 0f, 0f, 60f,
        2.5f, MiraNumberSuffixes.Seconds)
    {
        Visible = () => OptionGroupSingleton<MedicOptions>.Instance.ShowReports
    };

    public ModdedNumberOption MedicReportColorDuration { get; } = new("Time Where Medic Will Have Color Type", 15, 0f,
        60f, 2.5f, MiraNumberSuffixes.Seconds)
    {
        Visible = () => OptionGroupSingleton<MedicOptions>.Instance.ShowReports
    };
}

public enum MedicOption
{
    Medic,
    Shielded,
    ShieldedAndMedic,
    Everyone,
    Nobody
}
using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;
using TownOfUs.Modules.Localization;
using TownOfUs.Roles.Crewmate;

namespace TownOfUs.Options.Roles.Crewmate;

public sealed class MedicOptions : AbstractOptionGroup<MedicRole>
{
    public override string GroupName => TouLocale.Get(TouNames.Medic, "Medic Options");

    [ModdedEnumOption("Show Shielded Player", typeof(MedicOption), ["Self", "Shielded", "Shielded + Self", "Everyone", "No One"])]
    public MedicOption ShowShielded { get; set; } = MedicOption.ShieldedAndMedic;

    [ModdedEnumOption("Who Gets Murder Attempt Indicator", typeof(MedicOption), ["Self", "Shielded", "Shielded + Self", "Everyone", "No One"])]
    public MedicOption WhoGetsNotification { get; set; } = MedicOption.Medic;

    [ModdedToggleOption("Allow Giving Shield Away Next Round")]
    public bool ChangeTarget { get; set; } = true;

    [ModdedToggleOption("Shield Breaks On Murder Attempt")]
    public bool ShieldBreaks { get; set; } = false;

    [ModdedToggleOption("Show Reports in Chat")]
    public bool ShowReports { get; set; } = true;

    public ModdedNumberOption MedicReportNameDuration { get; } = new ModdedNumberOption($"Time Where {TouLocale.Get(TouNames.Medic, "Medic")} Will Have Name", 0f, 0f, 60f, 2.5f, MiraNumberSuffixes.Seconds)
    {
        Visible = () => OptionGroupSingleton<MedicOptions>.Instance.ShowReports,
    };

    public ModdedNumberOption MedicReportColorDuration { get; } = new ModdedNumberOption($"Time Where {TouLocale.Get(TouNames.Medic, "Medic")} Medic Will Have Color Type", 15, 0f, 60f, 2.5f, MiraNumberSuffixes.Seconds)
    {
        Visible = () => OptionGroupSingleton<MedicOptions>.Instance.ShowReports,
    };
}

public enum MedicOption
{
    Medic,
    Shielded,
    ShieldedAndMedic,
    Everyone,
    Nobody,
}

using MiraAPI.Modifiers;
using MiraAPI.Modifiers.Types;
using MiraAPI.PluginLoading;

namespace TownOfUs.Modifiers.Game;

[MiraIgnore]
public abstract class TouGameModifier : GameModifier
{
    public virtual ModifierFaction FactionType => ModifierFaction.Universal;

    public override bool HideOnUi => false;

    public override int GetAmountPerGame() => 1;

    public override bool IsModifierValidOn(RoleBehaviour role) => !role.Player.GetModifierComponent().HasModifier<TouGameModifier>(true);
}
public enum ModifierFaction
{
    Alliance,
    Universal,
    Crewmate,
    Neutral,
    Impostor,
    CrewmateAlliance,
    CrewmateUtility,
    CrewmateVisibility,
    CrewmatePostmortem,
    CrewmatePassive,
    NeutralAlliance,
    NeutralUtility,
    NeutralVisibility,
    NeutralPostmortem,
    NeutralPassive,
    ImpostorAlliance,
    ImpostorUtility,
    ImpostorVisibility,
    ImpostorPostmortem,
    ImpostorPassive,
    UniversalUtility,
    UniversalVisibility,
    UniversalPostmortem,
    UniversalPassive,
    KillerUtility,
    KillerVisibility,
    KillerPostmortem,
    KillerPassive,
    NonCrewmate,
    NonCrewUtility,
    NonCrewVisibility,
    NonCrewPostmortem,
    NonCrewPassive,
    NonNeutral,
    NonNeutUtility,
    NonNeutVisibility,
    NonNeutPostmortem,
    NonNeutPassive,
    NonImpostor,
    NonImpUtility,
    NonImpVisibility,
    NonImpPostmortem,
    NonImpPassive,
    External,
    Other,
}

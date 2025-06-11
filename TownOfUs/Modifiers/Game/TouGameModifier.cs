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
    NeutralAlliance,
    ImpostorAlliance,
    NonCrewmate,
    NonNeutral,
    NonImpostor,
    External,
    Other,
}

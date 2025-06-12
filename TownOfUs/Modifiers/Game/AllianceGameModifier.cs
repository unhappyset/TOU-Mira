using MiraAPI.Modifiers;
using MiraAPI.Modifiers.Types;
using MiraAPI.PluginLoading;

namespace TownOfUs.Modifiers.Game;

[MiraIgnore]
public abstract class AllianceGameModifier : GameModifier
{
    public virtual string IntroInfo => $"Alliance: {ModifierName}";
    public virtual string Symbol => "?";
    public virtual float IntroSize => 4f;
    public virtual int CustomAmount => GetAmountPerGame();
    public virtual int CustomChance => GetAssignmentChance();
    public virtual ModifierFaction FactionType => ModifierFaction.Alliance;

    public override bool HideOnUi => false;

    public override int GetAmountPerGame() => 1;

    public override bool IsModifierValidOn(RoleBehaviour role) => !role.Player.GetModifierComponent().HasModifier<AllianceGameModifier>(true);
}

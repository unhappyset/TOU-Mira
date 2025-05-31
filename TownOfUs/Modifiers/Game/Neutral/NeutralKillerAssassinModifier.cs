using MiraAPI.GameOptions;
using TownOfUs.Options;
using TownOfUs.Roles;

namespace TownOfUs.Modifiers.Game.Neutral;

public sealed class NeutralKillerAssassinModifier : AssassinModifier
{
    public override string ModifierName => "Assassin (Neutral)";
    public override int GetAmountPerGame() => (int)OptionGroupSingleton<AssassinOptions>.Instance.NumberOfNeutralAssassins;

    public override bool IsModifierValidOn(RoleBehaviour role)
    {
        return role is ITownOfUsRole { RoleAlignment: RoleAlignment.NeutralKilling };
    }
}

using MiraAPI.GameOptions;
using TownOfUs.Options;
using TownOfUs.Roles;

namespace TownOfUs.Modifiers.Game.Impostor;

public sealed class NeutralKillerAssassinModifier : AssassinModifier
{
    public override int GetAmountPerGame() => (int)OptionGroupSingleton<AssassinOptions>.Instance.NumberOfNeutralAssassins;

    public override bool IsModifierValidOn(RoleBehaviour role)
    {
        return role is ITownOfUsRole { RoleAlignment: RoleAlignment.NeutralKilling };
    }
}

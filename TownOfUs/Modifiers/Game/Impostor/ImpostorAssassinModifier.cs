using MiraAPI.GameOptions;
using TownOfUs.Options;

namespace TownOfUs.Modifiers.Game.Impostor;

public sealed class ImpostorAssassinModifier : AssassinModifier
{
    public override int GetAmountPerGame() => (int)OptionGroupSingleton<AssassinOptions>.Instance.NumberOfImpostorAssassins;

    public override bool IsModifierValidOn(RoleBehaviour role)
    {
        return role.TeamType == RoleTeamTypes.Impostor;
    }
}

using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using TownOfUs.Options.Modifiers;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfUs.Modifiers.Game.Impostor;

public sealed class ImpostorDoubleShotModifier : DoubleShotModifier
{
    public override string ModifierName => "Double Shot";
    public override Color FreeplayFileColor => new Color32(255, 25, 25, 255);

    public override int GetAssignmentChance()
    {
        return (int)OptionGroupSingleton<ImpostorModifierOptions>.Instance.DoubleShotChance;
    }

    public override int GetAmountPerGame()
    {
        return (int)OptionGroupSingleton<ImpostorModifierOptions>.Instance.DoubleShotAmount;
    }

    public override bool IsModifierValidOn(RoleBehaviour role)
    {
        if (
            role.Player.IsImpostor()
            && role.Player.GetModifierComponent().HasModifier<ImpostorAssassinModifier>(true)
            && base.IsModifierValidOn(role)
        )
        {
            return true;
        }

        return false;
    }
}
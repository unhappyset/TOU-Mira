﻿using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using TownOfUs.Options.Modifiers;
using TownOfUs.Roles;

namespace TownOfUs.Modifiers.Game.Neutral;

public sealed class NeutralKillerDoubleShotModifier : DoubleShotModifier, IWikiDiscoverable
{
    public override string ModifierName => TouLocale.Get("TouModifierDoubleShot", "Double Shot");
    public override bool ShowInFreeplay => true;

    public bool IsHiddenFromList => true;

    // YES this is scuffed, a better solution will be used at a later time
    public uint FakeTypeId =>
        ModifierManager.GetModifierTypeId(ModifierManager.Modifiers.FirstOrDefault(x =>
            x is TouGameModifier touGameMod && touGameMod.LocaleKey == "DoubleShot")!.GetType()) ??
        throw new InvalidOperationException("Modifier is not registered.");

    public override int GetAssignmentChance()
    {
        return (int)OptionGroupSingleton<NeutralModifierOptions>.Instance.DoubleShotChance;
    }

    public override int GetAmountPerGame()
    {
        return (int)OptionGroupSingleton<NeutralModifierOptions>.Instance.DoubleShotAmount;
    }

    public override bool IsModifierValidOn(RoleBehaviour role)
    {
        if (
            role is ITownOfUsRole { RoleAlignment: RoleAlignment.NeutralKilling }
            && role.Player.GetModifierComponent().HasModifier<NeutralKillerAssassinModifier>(true)
            && !role.Player.GetModifierComponent().HasModifier<TouGameModifier>(true)
        )
        {
            return true;
        }

        return false;
    }
}
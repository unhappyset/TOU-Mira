using MiraAPI.Modifiers;
using MiraAPI.Roles;
using TownOfUs.Roles.Crewmate;
using TownOfUs.Utilities;

namespace TownOfUs.Modifiers.Crewmate;

public sealed class ImitatorCacheModifier : BaseModifier
{
    public override string ModifierName => "Imitator";
    public override bool HideOnUi => true;

    public override void OnDeath(DeathReason reason)
    {
        ModifierComponent?.RemoveModifier(this);
    }

    public override void OnDeactivate()
    {
        if (Player == null || Player.IsRole<ImitatorRole>()) return;

        Player.RpcChangeRole(RoleId.Get<ImitatorRole>(), false);
    }
}

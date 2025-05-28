using AmongUs.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Roles;
using TownOfUs.Roles.Crewmate;
using TownOfUs.Utilities;

namespace TownOfUs.Modifiers.Crewmate;

public sealed class ImitatorCacheModifier(RoleBehaviour oldRole) : BaseModifier, ICachedRole
{
    public override string ModifierName => "Imitator";
    public override bool HideOnUi => true;
    public bool ShowCurrentRoleFirst => true;
    public RoleBehaviour CachedRole => RoleManager.Instance.GetRole((RoleTypes)RoleId.Get<ImitatorRole>());
    public RoleBehaviour OldRole => oldRole;

    public override void OnDeath(DeathReason reason)
    {
        ModifierComponent?.RemoveModifier(this);
    }

    public override void OnDeactivate()
    {
        if (Player == null || Player.IsRole<ImitatorRole>()) return;

        Player.ChangeRole(RoleId.Get<ImitatorRole>(), false);
        if (Player.Data.Role is ImitatorRole imitator) imitator.OldRole = OldRole;
    }
}

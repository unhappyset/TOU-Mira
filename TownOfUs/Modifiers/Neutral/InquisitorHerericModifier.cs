using MiraAPI.Modifiers;

namespace TownOfUs.Modifiers.Neutral;

public sealed class InquisitorHereticModifier(byte ownerId) : BaseModifier
{
    public override string ModifierName => "Inquisitor Heretic";
    public override bool HideOnUi => true;

    public byte OwnerId { get; } = ownerId;
    public RoleBehaviour TargetRole { get; set; }

    public override void OnActivate()
    {
        TargetRole = Player.Data.Role;
    }
}
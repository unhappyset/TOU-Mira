using MiraAPI.Modifiers;
using TownOfUs.Roles.Neutral;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfUs.Modifiers.Neutral;

public sealed class ArsonistDousedModifier(byte arsonistId) : BaseModifier
{
    public override string ModifierName => "Doused";
    public override bool HideOnUi => true;
    public byte ArsonistId { get; } = arsonistId;

    public override void OnDeath(DeathReason reason)
    {
        ModifierComponent!.RemoveModifier(this);
    }

    public override void FixedUpdate()
    {
        if (PlayerControl.LocalPlayer.IsRole<ArsonistRole>())
            Player?.cosmetics.SetOutline(true, new Il2CppSystem.Nullable<Color>(Color.yellow));
    }

    public override void OnDeactivate()
    {
        Player.cosmetics.SetOutline(false, new Il2CppSystem.Nullable<Color>(Color.yellow));
    }
}

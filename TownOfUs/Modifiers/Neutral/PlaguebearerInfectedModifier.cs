using MiraAPI.Modifiers;
using TownOfUs.Roles.Neutral;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfUs.Modifiers.Neutral;

public sealed class PlaguebearerInfectedModifier(byte plaguebearerId) : BaseModifier
{
    public override string ModifierName => "Infected";
    public override bool HideOnUi => true;

    public byte PlagueBearerId { get; } = plaguebearerId;

    private Color color = new(0.9f, 1f, 0.7f, 1f);

    public override void FixedUpdate()
    {
        if (PlayerControl.LocalPlayer.IsRole<PlaguebearerRole>() && Player != PlayerControl.LocalPlayer)
            Player?.cosmetics.SetOutline(true, new Il2CppSystem.Nullable<Color>(color));
    }

    public override void OnDeactivate()
    {
        Player?.cosmetics.SetOutline(false, new Il2CppSystem.Nullable<Color>(color));
    }
}

using MiraAPI.Modifiers;
using Reactor.Utilities.Extensions;
using TownOfUs.Utilities;

namespace TownOfUs.Modifiers.Neutral;

public sealed class MercenaryBribedModifier(PlayerControl mercenary) : BaseModifier
{
    public override string ModifierName => "Mercenary Bribed";
    public override bool HideOnUi => true;
    public PlayerControl Mercenary { get; } = mercenary;

    public bool alerted;

    public override void OnDeath(DeathReason reason)
    {
        ModifierComponent!.RemoveModifier(this);
    }

    public override void OnMeetingStart()
    {
        if (!Player.AmOwner) return;
        if (alerted) return;

        var title = $"<color=#{TownOfUsColors.Mercenary.ToHtmlStringRGBA()}>Mercenary Feedback</color>";
        MiscUtils.AddFakeChat(Player.Data, title, "You have been bribed by a Mercenary!", true, true);

        alerted = true;
    }
}

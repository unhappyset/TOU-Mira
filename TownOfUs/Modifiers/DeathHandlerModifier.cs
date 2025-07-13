using MiraAPI.Modifiers;

namespace TownOfUs.Modifiers;

public sealed class DeathHandlerModifier : BaseModifier
{
    public override string ModifierName => "Death Handler";
    public override bool HideOnUi => true;
    public override bool ShowInFreeplay => false;
    // This will determine if another mira event should be able to modify the information
    public bool LockInfo { get; set; }
    // This will determine if symbols or anything are shown
    public bool DiedThisRound { get; set; } = true;
    // This will specify how the player died such as; Suicide, Prosecuted, Ejected, Rampaged, Reaped, etc.
    public string CauseOfDeath { get; set; } = "Suicide";
    // This is set up by the game itself and will display in the lobby
    public int RoundOfDeath { get; set; } = -1;
    // This will specify who killed the player, if any, such as; By Innersloth
    public string KilledBy { get; set; } = string.Empty;
}
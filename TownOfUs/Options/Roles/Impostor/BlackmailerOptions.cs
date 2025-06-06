using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.Utilities;
using TownOfUs.Roles.Impostor;

namespace TownOfUs.Options.Roles.Impostor;

public sealed class BlackmailerOptions : AbstractOptionGroup<BlackmailerRole>
{
    public override string GroupName => "Blackmailer";

    [ModdedNumberOption("Blackmail Cooldown", 1f, 30f, suffixType: MiraNumberSuffixes.Seconds)]
    public float BlackmailCooldown { get; set; } = 20f;

    [ModdedNumberOption("Max Players Alive Until Voting", 1f, 15f)]
    public float MaxAliveForVoting { get; set; } = 10f;
    [ModdedToggleOption("Blackmail Same Person Twice In A Row")]
    public bool BlackmailInARow { get; set; } = false;

    [ModdedToggleOption("Only Target Sees Blackmail")]
    public bool OnlyTargetSeesBlackmail { get; set; } = false;
    [ModdedToggleOption("Blackmailer Can Kill")]
    public bool BlackmailerKill { get; set; } = true;
}

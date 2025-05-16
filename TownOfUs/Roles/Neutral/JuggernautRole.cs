using AmongUs.GameOptions;
using Il2CppInterop.Runtime.Attributes;
using MiraAPI.GameOptions;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using System.Globalization;
using System.Text;
using TownOfUs.Modules.Wiki;
using TownOfUs.Options.Roles.Neutral;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfUs.Roles.Neutral;

public sealed class JuggernautRole(IntPtr cppPtr) : NeutralRole(cppPtr), ITownOfUsRole, IWikiDiscoverable, IDoomable
{
    public string RoleName => "Juggernaut";
    public string RoleDescription => "Your Power Grows With Every Kill";
    public string RoleLongDescription => "With each kill your kill cooldown decreases";
    public Color RoleColor => TownOfUsColors.Juggernaut;
    public ModdedRoleTeams Team => ModdedRoleTeams.Custom;
    public RoleAlignment RoleAlignment => RoleAlignment.NeutralKilling;
    public DoomableType DoomHintType => DoomableType.Relentless;
    public CustomRoleConfiguration Configuration => new(this)
    {
        CanUseVent = OptionGroupSingleton<JuggernautOptions>.Instance.CanVent,
        IntroSound = TouAudio.WarlockIntroSound,
        Icon = TouRoleIcons.Juggernaut,
        MaxRoleCount = 1,
        GhostRole = (RoleTypes)RoleId.Get<NeutralGhostRole>(),
    };

    public bool HasImpostorVision => true;

    public int KillCount { get; set; }

    [HideFromIl2Cpp]
    public StringBuilder SetTabText()
    {
        var stringB = ITownOfUsRole.SetNewTabText(this);

        stringB.Append(CultureInfo.InvariantCulture, $"\n<b>Kill Count:</b> {KillCount}");

        return stringB;
    }

    public override bool DidWin(GameOverReason gameOverReason)
    {
        return WinConditionMet();
    }

    public override bool CanUse(IUsable usable)
    {
        if (!GameManager.Instance.LogicUsables.CanUse(usable, Player))
        {
            return false;
        }
        Console console = usable.TryCast<Console>()!;
        return (console == null) || console.AllowImpostor;
    }

    public bool WinConditionMet()
    {
        if (Player.HasDied()) return false;

        var result = Helpers.GetAlivePlayers().Count <= 2 && MiscUtils.KillersAliveCount == 1;
        return result;
    }

    public string GetAdvancedDescription()
    {
        return "The Juggernaut is a Neutral Killing role that wins by being the last killer alive. For each kill they make, their kill cooldown gets reduced." + MiscUtils.AppendOptionsText(GetType());
    }
}

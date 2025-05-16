using System.Text;
using AmongUs.GameOptions;
using Il2CppInterop.Runtime.Attributes;
using MiraAPI.GameOptions;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using TownOfUs.Modules.Wiki;
using TownOfUs.Options.Roles.Neutral;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfUs.Roles.Neutral;

public sealed class GlitchRole(IntPtr cppPtr) : NeutralRole(cppPtr), ITownOfUsRole, IWikiDiscoverable, IDoomable
{
    public string RoleName => "Glitch";
    public string RoleDescription => "Murder, Mimic, Hack... Data Lost";
    public string RoleLongDescription => "Murder everyone to win with your abilities!";
    public Color RoleColor => TownOfUsColors.Glitch;
    public ModdedRoleTeams Team => ModdedRoleTeams.Custom;
    public RoleAlignment RoleAlignment => RoleAlignment.NeutralKilling;
    public DoomableType DoomHintType => DoomableType.Perception;
    public CustomRoleConfiguration Configuration => new(this)
    {
        CanUseVent = OptionGroupSingleton<GlitchOptions>.Instance.CanVent,
        IntroSound = TouAudio.GlitchSound,
        MaxRoleCount = 1,
        Icon = TouRoleIcons.Glitch,
        GhostRole = (RoleTypes)RoleId.Get<NeutralGhostRole>(),
    };

    public bool HasImpostorVision => true;

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
    [HideFromIl2Cpp]
    public StringBuilder SetTabText()
    {
        return ITownOfUsRole.SetNewTabText(this);
    }

    public string GetAdvancedDescription()
    {
        return "The Glitch is a Neutral Killing role that wins by being the last killer alive. They can Mimic into another player or they can hack a player." + MiscUtils.AppendOptionsText(GetType());
    }

    [HideFromIl2Cpp]
    public List<CustomButtonWikiDescription> Abilities { get; } = [
        new("Mimic",
            "Mimic the appearance of another player, taking on their whole look.",
            TouNeutAssets.MimicSprite),
        new("Hack",
            "Disable a player's abilities.",
            TouNeutAssets.HackSprite) 
    ];
}

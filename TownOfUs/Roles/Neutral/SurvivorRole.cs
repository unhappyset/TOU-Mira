using System.Text;
using AmongUs.GameOptions;
using Il2CppInterop.Runtime.Attributes;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Patches.Stubs;
using MiraAPI.Roles;
using TownOfUs.Modifiers;
using TownOfUs.Modules.Wiki;
using TownOfUs.Options.Roles.Neutral;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfUs.Roles.Neutral;

public sealed class SurvivorRole(IntPtr cppPtr) : NeutralRole(cppPtr), ITownOfUsRole, IWikiDiscoverable, IDoomable
{
    public string RoleName => "Survivor";
    public string RoleDescription => "Do Whatever It Takes To Live";
    public string RoleLongDescription => "Stay alive to win with any faction remaining";
    public Color RoleColor => TownOfUsColors.Survivor;
    public ModdedRoleTeams Team => ModdedRoleTeams.Custom;
    public RoleAlignment RoleAlignment => RoleAlignment.NeutralBenign;
    public DoomableType DoomHintType => DoomableType.Protective;
    public CustomRoleConfiguration Configuration => new(this)
    {
        IntroSound = TouAudio.ToppatIntroSound,
        Icon = TouRoleIcons.Survivor,
        GhostRole = (RoleTypes)RoleId.Get<NeutralGhostRole>(),
    };

    [HideFromIl2Cpp]
    public List<CustomButtonWikiDescription> Abilities { get; } = [
        new("Vest",
            "Put on a Vest protecting you from attacks.",
            TouNeutAssets.VestSprite)
    ];

    [HideFromIl2Cpp]
    public StringBuilder SetTabText()
    {
        return ITownOfUsRole.SetNewTabText(this);
    }

    public string GetAdvancedDescription()
    {
        return "The Survivor is a Neutral Benign role that just needs to survive till the end of the game." + MiscUtils.AppendOptionsText(GetType());
    }

    public override void Initialize(PlayerControl player)
    {
        RoleBehaviourStubs.Initialize(this, player);

        if (Player.AmOwner && OptionGroupSingleton<SurvivorOptions>.Instance.ScatterOn)
        {
            Player.AddModifier<ScatterModifier>(OptionGroupSingleton<SurvivorOptions>.Instance.ScatterTimer);
        }
    }

    public override void Deinitialize(PlayerControl targetPlayer)
    {
        RoleBehaviourStubs.Deinitialize(this, targetPlayer);

        if (Player.AmOwner && OptionGroupSingleton<SurvivorOptions>.Instance.ScatterOn)
        {
            Player.RemoveModifier<ScatterModifier>();
        }
    }

    public override bool DidWin(GameOverReason gameOverReason)
    {
        return !Player.HasDied();
    }
}

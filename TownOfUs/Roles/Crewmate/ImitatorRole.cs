using System.Text;
using Il2CppInterop.Runtime.Attributes;
using MiraAPI.Modifiers;
using MiraAPI.Roles;
using TownOfUs.Modifiers.Crewmate;
using TownOfUs.Modules.Wiki;
using TownOfUs.Patches.Stubs;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfUs.Roles.Crewmate;

public sealed class ImitatorRole(IntPtr cppPtr) : CrewmateRole(cppPtr), ITownOfUsRole, IWikiDiscoverable, IDoomable
{
    public string RoleName => "Imitator";
    public string RoleDescription => "Use Dead Roles To Benefit The Crew";
    public string RoleLongDescription => "Use the true-hearted dead to benefit the crew once more";
    public Color RoleColor => TownOfUsColors.Imitator;
    public ModdedRoleTeams Team => ModdedRoleTeams.Crewmate;
    public RoleAlignment RoleAlignment => RoleAlignment.CrewmateSupport;
    public DoomableType DoomHintType => DoomableType.Perception;
    public CustomRoleConfiguration Configuration => new(this)
    {
        Icon = TouRoleIcons.Imitator,
        IntroSound = TouAudio.SpyIntroSound,
    };
    public override void Initialize(PlayerControl player)
    {
        RoleStubs.RoleBehaviourInitialize(this, player);
        player.AddModifier<ImitatorCacheModifier>();
    }

    [HideFromIl2Cpp]
    public StringBuilder SetTabText()
    {
        return ITownOfUsRole.SetNewTabText(this);
    }

    public string GetAdvancedDescription()
    {
        return "The Imitator is a Crewmate Support role that can select a dead crewmate to imitate their role." +
            "They will become their role and abilities until they change targets. " +
            "If there are multiple living imitators and the Mayor is dead, none of the Imitators will be able to select the Mayor."
            + MiscUtils.AppendOptionsText(GetType());
    }
}

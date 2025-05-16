using System.Text;
using Il2CppInterop.Runtime.Attributes;
using MiraAPI.Roles;
using TownOfUs.Modules.Wiki;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfUs.Roles.Crewmate;

public sealed class LookoutRole(IntPtr cppPtr) : CrewmateRole(cppPtr), ITownOfUsRole, IWikiDiscoverable, IDoomable
{
    public string RoleName => "Lookout";
    public string RoleDescription => "Keep Your Eyes Wide Open";
    public string RoleLongDescription => "Watch other crewmates to see what roles interact with them";
    public Color RoleColor => TownOfUsColors.Lookout;
    public ModdedRoleTeams Team => ModdedRoleTeams.Crewmate;
    public RoleAlignment RoleAlignment => RoleAlignment.CrewmateInvestigative;
    public DoomableType DoomHintType => DoomableType.Hunter;
    public CustomRoleConfiguration Configuration => new(this)
    {
        Icon = TouRoleIcons.Lookout,
        IntroSound = TouAudio.QuestionSound,
    };
    public override bool IsAffectedByComms => false;

    [HideFromIl2Cpp]
    public StringBuilder SetTabText()
    {
        return ITownOfUsRole.SetNewTabText(this);
    }

    public string GetAdvancedDescription() 
    {
        return "The Lookout is a Crewmate Investigative role that can watch other players during rounds. During meetings they will see all roles who interact with each watched player."
            + MiscUtils.AppendOptionsText(GetType());
    }

    [HideFromIl2Cpp]
    public List<CustomButtonWikiDescription> Abilities { get; } = [
        new("Watch", 
            "Watch a player or multiple, the next meeting you will know which players interacted with the watched ones.",
            TouCrewAssets.WatchSprite)
    ];
}

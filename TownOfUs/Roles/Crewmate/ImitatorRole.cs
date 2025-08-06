using System.Text;
using Il2CppInterop.Runtime.Attributes;
using MiraAPI.Modifiers;
using MiraAPI.Patches.Stubs;
using MiraAPI.Roles;
using TownOfUs.Modifiers.Crewmate;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfUs.Roles.Crewmate;

public sealed class ImitatorRole(IntPtr cppPtr) : CrewmateRole(cppPtr), ITownOfUsRole, IWikiDiscoverable, IDoomable
{
    public DoomableType DoomHintType => DoomableType.Perception;
    public string RoleName => TouLocale.Get("TouRoleImitator", "Imitator");
    public string RoleDescription => "Use Dead Roles To Benefit The Crew";
    public string RoleLongDescription => "Use the true-hearted dead to benefit the crew once more";
    public Color RoleColor => TownOfUsColors.Imitator;
    public ModdedRoleTeams Team => ModdedRoleTeams.Crewmate;
    public RoleAlignment RoleAlignment => RoleAlignment.CrewmateSupport;

    public CustomRoleConfiguration Configuration => new(this)
    {
        Icon = TouRoleIcons.Imitator,
        IntroSound = TouAudio.SpyIntroSound
    };

    [HideFromIl2Cpp]
    public StringBuilder SetTabText()
    {
        return ITownOfUsRole.SetNewTabText(this);
    }

    public string GetAdvancedDescription()
    {
        return $"The {RoleName} is a Crewmate Support role that can select a dead crewmate to imitate their role. " +
               "They will become their role and abilities until they change targets. " +
               $"Certain roles are innacessible if there are multiple living {RoleName.ToLowerInvariant()}s."
               + MiscUtils.AppendOptionsText(GetType());
    }

    [HideFromIl2Cpp]
    public List<CustomButtonWikiDescription> Abilities { get; } =
    [
        new("Crewmate Imitation",
            $"All crewmate roles are available besides Imitator, Politician, Mayor and Crewmate. Prosecutor and Jailor are limited, as they can only be selected if no other Imitators exist. Jailor and Prosecutor cannot use their meeting abilities, and Vigi does not get safe shots.",
            TouCrewAssets.InspectSprite),
        new("Neutral Counterparts",
            $"{TouLocale.Get("TouRoleAmnesiac", "Amnesiac")} ⇨ {TouLocale.Get("TouRoleMedic", "Medic")} | "
            + "Doom ⇨ Vigi | "
            + "Exe ⇨ Snitch\n"
            + $"{TouLocale.Get("TouRoleGlitch", "Glitch")} ⇨ {TouLocale.Get("TouRoleSheriff", "Sheriff")} | "
            + "GA ⇨ Cleric | "
            + "Inquis ⇨ Oracle\n"
            + $"{TouLocale.Get("TouRoleJester", "Jester")} ⇨ Plumber | "
            + "Merc ⇨ Warden\n"
            + "Pb/Pest ⇨ Aurial | "
            + "SC ⇨ Medium | "
            + "WW ⇨ Hunter",
            TouNeutAssets.GuardSprite),
        new("Impostor Counterparts",
            $"{TouLocale.Get("TouRoleBomber", "Bomber")} ⇨ {TouLocale.Get("TouRoleTrapper", "Trapper")} | "
            + $"Escapist ⇨ {TouLocale.Get("TouRoleTransporter", "Transporter")}\n"
            + "Hypnotist ⇨ Lookout | "
            + "Janitor ⇨ Detective\n"
            + $"Miner ⇨ {TouLocale.Get("TouRoleEngineer", "Engineer")} | "
            + "Scavenger ⇨ Tracker\n"
            + "Undertaker ⇨ Altruist | "
            + $"Warlock ⇨ {TouLocale.Get("TouRoleVeteran", "Veteran")}",
            TouImpAssets.DragSprite),
    ];

    public string SecondTabName => "Role Guide";

    public override void Initialize(PlayerControl player)
    {
        RoleBehaviourStubs.Initialize(this, player);
        player.AddModifier<ImitatorCacheModifier>();
    }
}
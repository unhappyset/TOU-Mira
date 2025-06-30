using System.Text;
using AmongUs.GameOptions;
using Il2CppInterop.Runtime.Attributes;
using HarmonyLib;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Roles;
using Reactor.Networking.Attributes;
using Reactor.Utilities;
using TownOfUs.Modifiers.Crewmate;
using TownOfUs.Modules;
using TownOfUs.Options;
using TownOfUs.Utilities;
using UnityEngine;
using TownOfUs.Modifiers.Neutral;
using TownOfUs.Roles.Crewmate;
using TownOfUs.Modules.Wiki;
using TownOfUs.Modifiers.Game.Impostor;
using TownOfUs.Modifiers.Game.Neutral;
using TownOfUs.Events.TouEvents;
using MiraAPI.Events;
using MiraAPI.Patches.Stubs;

namespace TownOfUs.Roles.Neutral;

public sealed class AmnesiacRole(IntPtr cppPtr) : NeutralRole(cppPtr), ITownOfUsRole, IWikiDiscoverable, IDoomable, ICrewVariant
{
    public string RoleName => "Amnesiac";
    public string RoleDescription => "Remember A Role Of A Deceased Player";
    public string RoleLongDescription => "Find a dead body to remember and become their role";
    public RoleBehaviour CrewVariant => RoleManager.Instance.GetRole((RoleTypes)RoleId.Get<MysticRole>());
    public Color RoleColor => TownOfUsColors.Amnesiac;
    public ModdedRoleTeams Team => ModdedRoleTeams.Custom;
    public RoleAlignment RoleAlignment => RoleAlignment.NeutralBenign;
    public DoomableType DoomHintType => DoomableType.Death;
    public CustomRoleConfiguration Configuration => new(this)
    {
        IntroSound = TouAudio.MediumIntroSound,
        GhostRole = (RoleTypes)RoleId.Get<NeutralGhostRole>(),
        Icon = TouRoleIcons.Amnesiac,
    };

    public override void Deinitialize(PlayerControl targetPlayer)
    {
        RoleBehaviourStubs.Deinitialize(this, targetPlayer);

        if (Player.HasModifier<AmnesiacArrowModifier>())
        {
            var mods = Player.GetModifiers<AmnesiacArrowModifier>();

            mods.Do([HideFromIl2Cpp] (x) => Player.RemoveModifier(x.UniqueId));
        }
    }

    public override bool DidWin(GameOverReason gameOverReason)
    {
        return false;
    }

    [MethodRpc((uint)TownOfUsRpc.Remember, SendImmediately = true)]
    public static void RpcRemember(PlayerControl player, PlayerControl target)
    {
        if (player.Data.Role is not AmnesiacRole)
        {
            Logger<TownOfUsPlugin>.Error("RpcRemember - Invalid amnesiac");
            return;
        }

        var touAbilityEvent = new TouAbilityEvent(AbilityType.AmnesiacPreRemember, player, target);
        MiraEventManager.InvokeEvent(touAbilityEvent);

        var roleWhenAlive = target.GetRoleWhenAlive();

        player.ChangeRole((ushort)roleWhenAlive.Role);
        if (player.Data.Role is InquisitorRole inquis)
        {
            inquis.Targets = ModifierUtils.GetPlayersWithModifier<InquisitorHereticModifier>().ToList();
            inquis.TargetRoles = ModifierUtils.GetActiveModifiers<InquisitorHereticModifier>().Select(x => x.TargetRole).OrderBy(x => x.NiceName).ToList();
        }
        if (player.Data.Role is MayorRole mayor)
            {
                mayor.Revealed = true;
            }

        if (target.IsImpostor() && OptionGroupSingleton<AssassinOptions>.Instance.AmneTurnImpAssassin)
        {
            player.AddModifier<ImpostorAssassinModifier>();
        }
        else if (target.IsNeutral() && target.Is(RoleAlignment.NeutralKilling) && OptionGroupSingleton<AssassinOptions>.Instance.AmneTurnNeutAssassin)
        {
            player.AddModifier<NeutralKillerAssassinModifier>();
        }
        
        if (target.Data.Role is not VampireRole && target.Data.Role.MaxCount <= PlayerControl.AllPlayerControls.ToArray().Count(x => x.Data.Role.Role == target.Data.Role.Role))
        {
            if (target.IsCrewmate())
            {
                target.ChangeRole((ushort)RoleTypes.Crewmate);
            }
            else if (target.IsImpostor())
            {
                target.ChangeRole((ushort)RoleTypes.Impostor);
            }
            else if (target.IsNeutral() && player.Data.Role is ITownOfUsRole touRole)
            {
                switch (touRole.RoleAlignment)
                {
                    default:
                        target.ChangeRole(RoleId.Get<SurvivorRole>());
                        break;
                    case RoleAlignment.NeutralEvil:
                        target.ChangeRole(RoleId.Get<JesterRole>());
                        break;
                    case RoleAlignment.NeutralKilling:
                        target.ChangeRole(RoleId.Get<MercenaryRole>());
                        player.AddModifier<MercenaryBribedModifier>(target)!.alerted = true;
                        break;
                }
            }
            else
            {
                target.ChangeRole(RoleId.Get<SurvivorRole>());
            }
        }
        var touAbilityEvent2 = new TouAbilityEvent(AbilityType.AmnesiacPostRemember, player, target);
        MiraEventManager.InvokeEvent(touAbilityEvent2);
    }

    [HideFromIl2Cpp]
    public StringBuilder SetTabText()
    {
        return ITownOfUsRole.SetNewTabText(this);
    }

    public string GetAdvancedDescription()
    {
        return "The Amnesiac is a Neutral Benign role that gains access to a new role from remembering a dead body’s role. Use the role you remember to win the game." + MiscUtils.AppendOptionsText(GetType());
    }

    [HideFromIl2Cpp]
    public List<CustomButtonWikiDescription> Abilities { get; } = [
        new("Remember",
            "Remember the role of a dead body. If the dead body's role is a unique role, you will remember the base faction's role instead.",
            TouNeutAssets.RememberButtonSprite)    
    ];
}

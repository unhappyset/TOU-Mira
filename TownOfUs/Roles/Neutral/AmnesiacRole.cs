using System.Text;
using AmongUs.GameOptions;
using HarmonyLib;
using Il2CppInterop.Runtime.Attributes;
using MiraAPI.Events;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Patches.Stubs;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using Reactor.Networking.Attributes;
using Reactor.Utilities;
using TownOfUs.Events.TouEvents;
using TownOfUs.Modifiers.Crewmate;
using TownOfUs.Modifiers.Game.Impostor;
using TownOfUs.Modifiers.Game.Neutral;
using TownOfUs.Modifiers.Neutral;
using TownOfUs.Modules;
using TownOfUs.Modules.Wiki;
using TownOfUs.Options;
using TownOfUs.Roles.Crewmate;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfUs.Roles.Neutral;

public sealed class AmnesiacRole(IntPtr cppPtr)
    : NeutralRole(cppPtr), ITownOfUsRole, IWikiDiscoverable, IDoomable, ICrewVariant
{
    public RoleBehaviour CrewVariant => RoleManager.Instance.GetRole((RoleTypes)RoleId.Get<MysticRole>());
    public DoomableType DoomHintType => DoomableType.Death;
    public string RoleName => "Amnesiac";
    public string RoleDescription => "Remember A Role Of A Deceased Player";
    public string RoleLongDescription => "Find a dead body to remember and become their role";
    public Color RoleColor => TownOfUsColors.Amnesiac;
    public ModdedRoleTeams Team => ModdedRoleTeams.Custom;
    public RoleAlignment RoleAlignment => RoleAlignment.NeutralBenign;

    public CustomRoleConfiguration Configuration => new(this)
    {
        IntroSound = TouAudio.MediumIntroSound,
        GhostRole = (RoleTypes)RoleId.Get<NeutralGhostRole>(),
        Icon = TouRoleIcons.Amnesiac
    };

    [HideFromIl2Cpp]
    public StringBuilder SetTabText()
    {
        return ITownOfUsRole.SetNewTabText(this);
    }

    public string GetAdvancedDescription()
    {
        return
            "The Amnesiac is a Neutral Benign role that gains access to a new role from remembering a dead body’s role. Use the role you remember to win the game." +
            MiscUtils.AppendOptionsText(GetType());
    }

    [HideFromIl2Cpp]
    public List<CustomButtonWikiDescription> Abilities { get; } =
    [
        new("Remember",
            "Remember the role of a dead body. If the dead body's role is a unique role, you will remember the base faction's role instead.",
            TouNeutAssets.RememberButtonSprite)
    ];

    public override void Deinitialize(PlayerControl targetPlayer)
    {
        RoleBehaviourStubs.Deinitialize(this, targetPlayer);

        if (Player.HasModifier<AmnesiacArrowModifier>())
        {
            var mods = Player.GetModifiers<AmnesiacArrowModifier>();

            mods.Do([HideFromIl2Cpp](x) => Player.RemoveModifier(x.UniqueId));
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

        var roleWhenAlive = target.GetRoleWhenAlive();

        if (roleWhenAlive is AmnesiacRole)
        {
            if (player.AmOwner)
            {
                var notif1 = Helpers.CreateAndShowNotification(
                    $"<b>{target.CachedPlayerData.PlayerName} was an {TownOfUsColors.Amnesiac.ToTextColor()}Amnesiac</color>, so their role cannot be picked up.</b>",
                    Color.white, new Vector3(0f, 1f, -20f), spr: TouRoleIcons.Amnesiac.LoadAsset());
                notif1.Text.SetOutlineThickness(0.35f);
            }

            return;
        }

        var touAbilityEvent = new TouAbilityEvent(AbilityType.AmnesiacPreRemember, player, target);
        MiraEventManager.InvokeEvent(touAbilityEvent);

        player.ChangeRole((ushort)roleWhenAlive.Role);
        if (player.Data.Role is InquisitorRole inquis)
        {
            inquis.Targets = ModifierUtils.GetPlayersWithModifier<InquisitorHereticModifier>().ToList();
            inquis.TargetRoles = ModifierUtils.GetActiveModifiers<InquisitorHereticModifier>().Select(x => x.TargetRole)
                .OrderBy(x => x.NiceName).ToList();
        }

        if (player.Data.Role is MayorRole mayor)
        {
            mayor.Revealed = true;
        }

        if (player.AmOwner)
        {
            var notif1 = Helpers.CreateAndShowNotification(
                $"<b>You remembered that you were like {target.Data.PlayerName}, the {player.Data.Role.TeamColor.ToTextColor()}{player.Data.Role.NiceName}</color>.</b>",
                Color.white, new Vector3(0f, 1f, -20f), spr: TouRoleIcons.Amnesiac.LoadAsset());
            notif1.Text.SetOutlineThickness(0.35f);
        }

        if (target.IsImpostor() && OptionGroupSingleton<AssassinOptions>.Instance.AmneTurnImpAssassin)
        {
            player.AddModifier<ImpostorAssassinModifier>();
        }
        else if (target.IsNeutral() && target.Is(RoleAlignment.NeutralKilling) &&
                 OptionGroupSingleton<AssassinOptions>.Instance.AmneTurnNeutAssassin)
        {
            player.AddModifier<NeutralKillerAssassinModifier>();
        }

        if (target.Data.Role is not VampireRole && target.Data.Role.MaxCount <= PlayerControl.AllPlayerControls
                .ToArray().Count(x => x.Data.Role.Role == target.Data.Role.Role))
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

        // TODO: Fix Amnesiac breaking unique roles for Amne/Imitator
        /* if (target.Data.Role is not VampireRole && target.Data.Role.MaxCount <= PlayerControl.AllPlayerControls.ToArray().Count(x => x.Data.Role.Role == target.Data.Role.Role))
        {
            var newRole = RoleManager.Instance.GetRole((RoleTypes)RoleId.Get<SurvivorRole>());
            if (target.IsCrewmate())
            {
                newRole = RoleManager.Instance.GetRole(RoleTypes.Crewmate);
            }
            else if (target.IsImpostor())
            {
                newRole = RoleManager.Instance.GetRole(RoleTypes.Impostor);
            }
            else if (target.IsNeutral() && player.Data.Role is ITownOfUsRole touRole)
            {
                switch (touRole.RoleAlignment)
                {
                    default:
                        newRole = RoleManager.Instance.GetRole((RoleTypes)RoleId.Get<SurvivorRole>());
                        break;
                    case RoleAlignment.NeutralEvil:
                        newRole = RoleManager.Instance.GetRole((RoleTypes)RoleId.Get<JesterRole>());
                        break;
                    case RoleAlignment.NeutralKilling:
                        newRole = RoleManager.Instance.GetRole((RoleTypes)RoleId.Get<MercenaryRole>());

                        player.AddModifier<MercenaryBribedModifier>(target)!.alerted = true;
                        break;
                }
            }

            var roleBehaviour = UnityEngine.Object.Instantiate(newRole, target.Data.gameObject.transform);
            GameHistory.RegisterRole(target, roleBehaviour);
            target.Data.RoleWhenAlive = new Il2CppSystem.Nullable<RoleTypes>(roleBehaviour.Role);

            if (target.Data.Role is NeutralGhostRole) target.ChangeRole(RoleId.Get<NeutralGhostRole>(), false);
        } */

        var touAbilityEvent2 = new TouAbilityEvent(AbilityType.AmnesiacPostRemember, player, target);
        MiraEventManager.InvokeEvent(touAbilityEvent2);
    }
}
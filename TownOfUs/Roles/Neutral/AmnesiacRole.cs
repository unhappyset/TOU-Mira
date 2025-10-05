﻿using System.Text;
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
using TownOfUs.Interfaces;
using TownOfUs.Modifiers;
using TownOfUs.Modifiers.Game.Impostor;
using TownOfUs.Modifiers.Game.Neutral;
using TownOfUs.Modifiers.Neutral;
using TownOfUs.Modules;
using TownOfUs.Options;
using TownOfUs.Options.Roles.Neutral;
using TownOfUs.Roles.Crewmate;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfUs.Roles.Neutral;

public sealed class AmnesiacRole(IntPtr cppPtr)
    : NeutralRole(cppPtr), ITownOfUsRole, IWikiDiscoverable, IDoomable, ICrewVariant, IGuessable
{
    public RoleBehaviour CrewVariant => RoleManager.Instance.GetRole((RoleTypes)RoleId.Get<MysticRole>());
    public DoomableType DoomHintType => DoomableType.Death;
    public string LocaleKey => "Amnesiac";
    public string RoleName => TouLocale.Get($"TouRole{LocaleKey}");
    public string RoleDescription => TouLocale.GetParsed($"TouRole{LocaleKey}IntroBlurb");
    public string RoleLongDescription => TouLocale.GetParsed($"TouRole{LocaleKey}TabDescription");

    public string GetAdvancedDescription()
    {
        return
            TouLocale.GetParsed($"TouRole{LocaleKey}WikiDescription") +
            MiscUtils.AppendOptionsText(GetType());
    }

    [HideFromIl2Cpp]
    public List<CustomButtonWikiDescription> Abilities
    {
        get
        {
            return new List<CustomButtonWikiDescription>
            {
                new(TouLocale.GetParsed($"TouRole{LocaleKey}Remember", "Remember"),
                    TouLocale.GetParsed($"TouRole{LocaleKey}RememberWikiDescription"),
                    TouNeutAssets.RememberButtonSprite)
            };
        }
    }

    public Color RoleColor => TownOfUsColors.Amnesiac;
    public ModdedRoleTeams Team => ModdedRoleTeams.Custom;

    public RoleAlignment RoleAlignment => RoleAlignment.NeutralBenign;

    // This is so the role can be guessed without requiring it to be enabled normally
    public bool CanBeGuessed =>
        (MiscUtils.GetPotentialRoles()
             .Contains(RoleManager.Instance.GetRole((RoleTypes)RoleId.Get<GuardianAngelTouRole>())) &&
         OptionGroupSingleton<GuardianAngelOptions>.Instance.OnTargetDeath is BecomeOptions.Amnesiac)
        || (MiscUtils.GetPotentialRoles()
                .Contains(RoleManager.Instance.GetRole((RoleTypes)RoleId.Get<ExecutionerRole>())) &&
            OptionGroupSingleton<ExecutionerOptions>.Instance.OnTargetDeath is BecomeOptions.Amnesiac);

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

    [MethodRpc((uint)TownOfUsRpc.Remember)]
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
                notif1.AdjustNotification();
            }

            return;
        }

        var touAbilityEvent = new TouAbilityEvent(AbilityType.AmnesiacPreRemember, player, target);
        MiraEventManager.InvokeEvent(touAbilityEvent);

        player.ChangeRole((ushort)roleWhenAlive.Role);
        if (player.Data.Role is InquisitorRole inquis)
        {
            inquis.Targets = ModifierUtils.GetPlayersWithModifier<InquisitorHereticModifier>().Where(x => x != player)
                .ToList();
            inquis.TargetRoles = ModifierUtils.GetActiveModifiers<InquisitorHereticModifier>()
                .Where(x => x.Player != player)
                .Select([HideFromIl2Cpp](x) => x.TargetRole).OrderBy([HideFromIl2Cpp](x) => x.GetRoleName()).ToList();
        }
        else if (player.Data.Role is PlaguebearerRole || player.Data.Role is PestilenceRole)
        {
            ModifierUtils.GetActiveModifiers<PlaguebearerInfectedModifier>()
                .Do(x => x.ModifierComponent?.RemoveModifier(x));
        }
        else if (player.Data.Role is ArsonistRole)
        {
            ModifierUtils.GetActiveModifiers<ArsonistDousedModifier>().Do(x => x.ModifierComponent?.RemoveModifier(x));
        }
        else if (player.Data.Role is MayorRole mayor)
        {
            mayor.Revealed = false;
        }
        else if (player.Data.Role is GuardianAngelTouRole ga)
        {
            var gaTarget = ModifierUtils.GetPlayersWithModifier<GuardianAngelTargetModifier>()
                .FirstOrDefault(x => x.PlayerId == target.PlayerId);

            if (gaTarget != null && gaTarget.TryGetModifier<GuardianAngelTargetModifier>(out var gaMod))
            {
                ga.Target = gaTarget;
                gaMod.OwnerId = player.PlayerId;
            }
        }
        else if (player.Data.Role is ExecutionerRole exe)
        {
            var exeTarget = ModifierUtils.GetPlayersWithModifier<ExecutionerTargetModifier>()
                .FirstOrDefault(x => x.PlayerId == target.PlayerId);

            if (exeTarget != null && exeTarget.TryGetModifier<ExecutionerTargetModifier>(out var exeMod))
            {
                exe.Target = exeTarget;
                exeMod.OwnerId = player.PlayerId;
            }
        }
        else if (player.Data.Role is VampireRole)
        {
            if (target.HasModifier<VampireBittenModifier>())
            {
                // Makes the amne stay with the bitten modifier
                player.AddModifier<VampireBittenModifier>();
            }
            else
            {
                // Makes the og vampire a bitten vampire so to speak, yes it makes it more confusing, but that's how it is, deal with it - Atony
                target.AddModifier<VampireBittenModifier>();
            }
        }

        if (player.AmOwner)
        {
            var notif1 = Helpers.CreateAndShowNotification(
                $"<b>You remembered that you were like {target.Data.PlayerName}, the {player.Data.Role.TeamColor.ToTextColor()}{player.Data.Role.GetRoleName()}</color>.</b>",
                Color.white, new Vector3(0f, 1f, -20f), spr: TouRoleIcons.Amnesiac.LoadAsset());
            notif1.AdjustNotification();
        }

        if (roleWhenAlive is not VampireRole && (roleWhenAlive.MaxCount <= 1 || (roleWhenAlive.MaxCount <= PlayerControl
                .AllPlayerControls
                .ToArray().Count(x => x.Data.Role.Role == roleWhenAlive.Role))))
        {
            if (target.IsCrewmate())
            {
                target.ChangeRole((ushort)RoleTypes.Crewmate);
            }
            else if (target.IsImpostor())
            {
                target.ChangeRole((ushort)RoleTypes.Impostor);
            }
            /*else if (target.IsNeutral() && player.Data.Role is ITownOfUsRole touRole)
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
            }*/
            else
            {
                target.ChangeRole(RoleId.Get<MercenaryRole>());
                player.AddModifier<MercenaryBribedModifier>(target)!.alerted = true;

                if (!target.HasModifier<BasicGhostModifier>())
                {
                    target.AddModifier<BasicGhostModifier>();
                }
            }
        }

        if (player.IsImpostor() && OptionGroupSingleton<AssassinOptions>.Instance.AmneTurnImpAssassin)
        {
            player.AddModifier<ImpostorAssassinModifier>();
        }
        else if (player.IsNeutral() && player.Is(RoleAlignment.NeutralKilling) &&
                 OptionGroupSingleton<AssassinOptions>.Instance.AmneTurnNeutAssassin)
        {
            player.AddModifier<NeutralKillerAssassinModifier>();
        }

        var touAbilityEvent2 = new TouAbilityEvent(AbilityType.AmnesiacPostRemember, player, target);
        MiraEventManager.InvokeEvent(touAbilityEvent2);
    }
}
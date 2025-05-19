using Il2CppInterop.Runtime.Attributes;
using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Modifiers;
using MiraAPI.Modifiers.Types;
using MiraAPI.Roles;
using Reactor.Networking.Attributes;
using Reactor.Utilities;
using System.Collections;
using System.Text;
using TownOfUs.Buttons.Crewmate;
using TownOfUs.Modifiers.Crewmate;
using TownOfUs.Modules;
using TownOfUs.Modules.Anims;
using TownOfUs.Modules.Wiki;
using TownOfUs.Options.Roles.Crewmate;
using TownOfUs.Patches.Stubs;
using TownOfUs.Utilities;
using UnityEngine;
using Object = UnityEngine.Object;

namespace TownOfUs.Roles.Crewmate;

public sealed class AltruistRole(IntPtr cppPtr) : CrewmateRole(cppPtr), ITownOfUsRole, IWikiDiscoverable, IDoomable
{
    public string RoleName => "Altruist";
    public string RoleDescription => "Revive Dead Crewmates";
    public string RoleLongDescription => "Revive dead crewmates in groups";
    public Color RoleColor => TownOfUsColors.Altruist;
    public ModdedRoleTeams Team => ModdedRoleTeams.Crewmate;
    public RoleAlignment RoleAlignment => RoleAlignment.CrewmateProtective;
    public DoomableType DoomHintType => DoomableType.Death;
    public CustomRoleConfiguration Configuration => new(this)
    {
        IntroSound = TouAudio.AltruistReviveSound,
        Icon = TouRoleIcons.Altruist,
    };

    public override bool IsAffectedByComms => false;

    public override void OnMeetingStart()
    {
        RoleStubs.RoleBehaviourOnMeetingStart(this);

        Logger<TownOfUsPlugin>.Error($"AltruistRole.OnMeetingStart");

        ClearArrows();
    }

    public override void OnVotingComplete()
    {
        RoleStubs.RoleBehaviourOnVotingComplete(this);

        CustomButtonSingleton<AltruistReviveButton>.Instance.RevivedInRound = false;
    }

    public override void OnDeath(DeathReason reason)
    {
        RoleStubs.RoleBehaviourOnDeath(this, reason);

        ClearArrows();
    }

    public override void Deinitialize(PlayerControl targetPlayer)
    {
        RoleStubs.RoleBehaviourDeinitialize(this, targetPlayer);

        ClearArrows();
    }

    [HideFromIl2Cpp]
    public static void ClearArrows()
    {
        Logger<TownOfUsPlugin>.Error($"AltruistRole.ClearArrows");

        if (PlayerControl.LocalPlayer.IsImpostor() || PlayerControl.LocalPlayer.Is(RoleAlignment.NeutralKilling))
        {
            Logger<TownOfUsPlugin>.Error($"AltruistRole.ClearArrows BadGuys Only");

            foreach (var playerWithArrow in ModifierUtils.GetPlayersWithModifier<AltruistArrowModifier>(x => x.Owner == PlayerControl.LocalPlayer))
            {
                playerWithArrow.RemoveModifier<AltruistArrowModifier>();
            }
        }
    }

    [HideFromIl2Cpp]
    public IEnumerator CoRevivePlayer(PlayerControl dead, float waitfor = 1f)
    {
        var roleWhenAlive = dead.GetRoleWhenAlive();

        //if (roleWhenAlive == null)
        //{
        //    Logger<TownOfUsPlugin>.Error($"CoRevivePlayer - Dead player {dead.PlayerId} does not have a role when alive, cannot revive");
        //    yield break; // cannot revive if no role when alive
        //}

        Player.moveable = false;
        Player.NetTransform.Halt();

        var body = FindObjectsOfType<DeadBody>()
            .FirstOrDefault(b => b.ParentId == dead.PlayerId);
        var position = new Vector2(Player.transform.localPosition.x, Player.transform.localPosition.y);

        if (body != null)
        {
            position = new Vector2(body.transform.localPosition.x, body.transform.localPosition.y + 0.3636f);
            if (OptionGroupSingleton<AltruistOptions>.Instance.HideAtBeginningOfRevive)
                Destroy(body.gameObject);
        }

        yield return new WaitForSeconds(waitfor);
        if (MeetingHud.Instance) yield break;

        Player.moveable = true;

        GameHistory.ClearMurder(dead);

        dead.Revive();

            dead.transform.position = new Vector2(position.x, position.y);
            if (dead.AmOwner) PlayerControl.LocalPlayer.NetTransform.RpcSnapTo(new Vector2(position.x, position.y));

        if (ModCompatibility.IsSubmerged() && PlayerControl.LocalPlayer.PlayerId == dead.PlayerId)
        {
            ModCompatibility.ChangeFloor(dead.transform.position.y > -7);
        }

        // return player from ghost role back to what they were when alive
        dead.ChangeRole((ushort)roleWhenAlive!.Role, false);

        if (dead.Data.Role is IAnimated animated)
        {
            animated.IsVisible = true;
            animated.SetVisible();
        }
        foreach (var button in CustomButtonManager.Buttons.Where(x => x.Enabled(dead.Data.Role)).OfType<IAnimated>())
        {
            button.IsVisible = true;
            button.SetVisible();
        }
        foreach (var modifier in dead.GetModifiers<GameModifier>().Where(x => x is IAnimated))
        {
            var animatedMod = modifier as IAnimated;
            if (animatedMod != null)
            {
                animatedMod.IsVisible = true;
                animatedMod.SetVisible();
            }
        }

        dead.RemainingEmergencies = 0;

        Player.RemainingEmergencies = 0;

        body = Object.FindObjectsOfType<DeadBody>()
            .FirstOrDefault(b => b.ParentId == dead.PlayerId);
        if (!OptionGroupSingleton<AltruistOptions>.Instance.HideAtBeginningOfRevive && body != null) Object.Destroy(body.gameObject);

        if (PlayerControl.LocalPlayer.IsImpostor() || PlayerControl.LocalPlayer.Is(RoleAlignment.NeutralKilling))
        {
            if (Player.HasModifier<AltruistArrowModifier>())
            {
                Player.RemoveModifier<AltruistArrowModifier>();
            }

            if (!dead.HasModifier<AltruistArrowModifier>() && dead != PlayerControl.LocalPlayer)
            {
                dead.AddModifier<AltruistArrowModifier>(PlayerControl.LocalPlayer, Color.white);
            }
        }
    }

    [MethodRpc((uint)TownOfUsRpc.AltruistRevive, SendImmediately = true)]
    public static void RpcRevive(PlayerControl dead, PlayerControl target)
    {
        if (dead.Data.Role is not AltruistRole)
        {
            Logger<TownOfUsPlugin>.Error("RpcRevive - Invalid altruist");
            return;
        }

        var role = dead.GetRole<AltruistRole>();

        if (role == null) return;

        if (PlayerControl.LocalPlayer.IsImpostor() || PlayerControl.LocalPlayer.Is(RoleAlignment.NeutralKilling))
        {
            Coroutines.Start(MiscUtils.CoFlash(TownOfUsColors.Altruist));

            if (!dead.HasModifier<AltruistArrowModifier>())
            {
                dead.AddModifier<AltruistArrowModifier>(PlayerControl.LocalPlayer, TownOfUsColors.Impostor);
            }
        }

        Coroutines.Start(role.CoRevivePlayer(target, OptionGroupSingleton<AltruistOptions>.Instance.ReviveDuration));
    }

    [HideFromIl2Cpp]
    public StringBuilder SetTabText()
    {
        return ITownOfUsRole.SetNewTabText(this);
    }
    
    public string GetAdvancedDescription()
    {
        return "The Altruist is a Crewmate Protective role can revive dead players in groups. However, their location and the revived players' locations will be revealed to all Impostors." + MiscUtils.AppendOptionsText(GetType());
    }

    [HideFromIl2Cpp]
    public List<CustomButtonWikiDescription> Abilities { get; } = [
        new("Revive",
            "Revive a group of dead bodies near you. You will be frozen during the revival and you will be unable to move until the revival is complete." +
            " Impostors will also have an arrow pointing towards you during the revival, so be cautious.",
            TouCrewAssets.ReviveSprite)
    ];
}

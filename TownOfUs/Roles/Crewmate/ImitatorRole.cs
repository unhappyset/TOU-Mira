using System.Text;
using AmongUs.GameOptions;
using Il2CppInterop.Runtime.Attributes;
using MiraAPI.Modifiers;
using MiraAPI.Roles;
using TownOfUs.Modifiers.Crewmate;
using TownOfUs.Modules;
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
    public RoleBehaviour OldRole { get; set; }
    public CustomRoleConfiguration Configuration => new(this)
    {
        Icon = TouRoleIcons.Imitator,
        IntroSound = CustomRoleUtils.GetIntroSound(RoleTypes.Noisemaker),
    };

    private NetworkedPlayerInfo? _selectedPlr;

    private MeetingMenu? _meetingMenu;

    public override void Initialize(PlayerControl player)
    {
        RoleStubs.RoleBehaviourInitialize(this, player);

        if (Player.AmOwner)
        {
            _meetingMenu = new MeetingMenu(
                this,
                Click,
                MeetingAbilityType.Toggle,
                TouAssets.ImitateSelectSprite,
                TouAssets.ImitateDeselectSprite,
                IsExempt,
                activeColor: Color.white)
            {
                Position = new Vector3(-0.40f, 0f, -3f),
            };
        }

        if (!MeetingHud.Instance) return;

        HudManager.Instance.SetHudActive(Player, this, false);
        OnMeetingStart();
    }

    public override void OnMeetingStart()
    {
        RoleStubs.RoleBehaviourOnMeetingStart(this);

        if (Player.AmOwner)
        {
            _meetingMenu!.GenButtons(MeetingHud.Instance, Player.AmOwner && !Player.HasDied() && !Player.HasModifier<JailedModifier>());
        }
    }

    public override void OnVotingComplete()
    {
        RoleStubs.RoleBehaviourOnVotingComplete(this);

        if (Player.AmOwner)
        {
            _meetingMenu!.HideButtons();
        }
    }

    public void UpdateRole()
    {
        if (_selectedPlr == null || Player.Data.IsDead || !_selectedPlr.IsDead)
        {
            _selectedPlr = null;
            return;
        }

        var roleWhenAlive = _selectedPlr.Object.GetRoleWhenAlive();

        Player.RpcChangeRole((ushort)roleWhenAlive!.Role, false);
        Player.RpcAddModifier<ImitatorCacheModifier>(roleWhenAlive!);

        _selectedPlr = null;
    }

    public override void Deinitialize(PlayerControl targetPlayer)
    {
        _selectedPlr = null;

        if (Player.AmOwner && _meetingMenu != null)
        {
            _meetingMenu?.Dispose();
            _meetingMenu = null!;
        }

        RoleStubs.RoleBehaviourDeinitialize(this, targetPlayer);
    }

    public void Click(PlayerVoteArea voteArea, MeetingHud __)
    {
        var player = GameData.Instance.GetPlayerById(voteArea.TargetPlayerId);

        if (_selectedPlr == player)
        {
            _selectedPlr = null;
            _meetingMenu!.Actives[voteArea.TargetPlayerId] = false;
            return;
        }

        if (_selectedPlr != null)
        {
            _meetingMenu!.Actives[_selectedPlr.PlayerId] = false;
            _selectedPlr = null;
        }

        _meetingMenu!.Actives[voteArea.TargetPlayerId] = true;
        _selectedPlr = player;
    }

    private bool IsExempt(PlayerVoteArea voteArea)
    {
        var player = GameData.Instance.GetPlayerById(voteArea.TargetPlayerId);

        if (player != null && !player.Object.IsCrewmate()) return true;
        if (player != null && player.Object.GetRoleWhenAlive() is MayorRole && PlayerControl.AllPlayerControls.ToArray().Any(x => x.Data.Role is ImitatorRole && !x.Data.IsDead && !x != Player)) return true;
        if (player != null && player.Object.GetRoleWhenAlive() is ImitatorRole) return true;

        return voteArea.TargetPlayerId == Player.PlayerId || Player.Data.IsDead || !voteArea!.AmDead;
    }

    [HideFromIl2Cpp]
    public StringBuilder SetTabText()
    {
        return ITownOfUsRole.SetNewTabText(this);
    }

    public string GetAdvancedDescription()
    {
        return "The Imitator is a Crewmate Support role that can select a dead crewmate to imitate their role." +
        "They will become their role and abilities for 1 round. " +
        "In the next meeting the Imitator may reselect them or choose a different person. " +
        "If there are multiple living imitators and the Mayor is dead, none of the Imitators will be able to select the Mayor.";
    }
}

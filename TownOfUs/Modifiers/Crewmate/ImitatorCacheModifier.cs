using AmongUs.GameOptions;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Roles;
using Reactor.Networking.Attributes;
using TownOfUs.Modules;
using TownOfUs.Options.Roles.Crewmate;
using TownOfUs.Roles.Crewmate;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfUs.Modifiers.Crewmate;

public sealed class ImitatorCacheModifier() : BaseModifier, ICachedRole
{
    public override string ModifierName => "Imitator";
    public override bool HideOnUi => true;
    public bool ShowCurrentRoleFirst => true;
    public RoleBehaviour CachedRole => RoleManager.Instance.GetRole((RoleTypes)RoleId.Get<ImitatorRole>());
    public RoleBehaviour OldRole { get; set; }
    private NetworkedPlayerInfo? _selectedPlr;
    private NetworkedPlayerInfo? _prevSelectedPlr;
    public bool ChangedSelectedPlayer { get; set; } = true;

    private MeetingMenu? _meetingMenu;
    public override void OnActivate()
    {
        base.OnActivate();

        if (Player.AmOwner)
        {
            _meetingMenu = new MeetingMenu(
                Player.Data.Role,
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
    }
    public override void OnMeetingStart()
    {
        if (Player.AmOwner)
        {
            // _selectedPlr = null;
            _meetingMenu!.GenButtons(MeetingHud.Instance, Player.AmOwner && !Player.HasDied() && !Player.HasModifier<JailedModifier>());
            if (_selectedPlr != null) _meetingMenu!.Actives[_selectedPlr.PlayerId] = true;
        }
    }
    public void OnVotingComplete()
    {
        if (Player.AmOwner)
        {
            _meetingMenu!.HideButtons();
        }
    }

    public override void OnDeactivate()
    {
        _selectedPlr = null;

        if (Player.AmOwner && _meetingMenu != null)
        {
            _meetingMenu?.Dispose();
            _meetingMenu = null!;
        }

        // if (Player == null || Player.IsRole<ImitatorRole>()) return;

        // Player.ChangeRole(RoleId.Get<ImitatorRole>(), false);
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
        ChangedSelectedPlayer = _prevSelectedPlr != _selectedPlr;
    }

    private bool IsExempt(PlayerVoteArea voteArea)
    {
        var player = GameData.Instance.GetPlayerById(voteArea.TargetPlayerId);
        var opts = OptionGroupSingleton<ImitatorOptions>.Instance;
        if (player != null && player.Object.GetRoleWhenAlive() is ICrewVariant && player.Object.IsNeutral() && opts.ImitateNeutrals) return voteArea.TargetPlayerId == Player.PlayerId || Player.Data.IsDead || !voteArea!.AmDead;
        if (player != null && player.Object.GetRoleWhenAlive() is ICrewVariant && player.Object.IsImpostor() && opts.ImitateImpostors) return voteArea.TargetPlayerId == Player.PlayerId || Player.Data.IsDead || !voteArea!.AmDead;
        

        if (player != null && !player.Object.IsCrewmate()) return true;
        if (player != null && player.Object.GetRoleWhenAlive() is MayorRole && PlayerControl.AllPlayerControls.ToArray().Any(x => x.Data.Role is ImitatorRole && !x.Data.IsDead && !x != Player)) return true;
        if (player != null && player.Object.GetRoleWhenAlive() is ImitatorRole) return true;

        return voteArea.TargetPlayerId == Player.PlayerId || Player.Data.IsDead || !voteArea!.AmDead;
    }
    public void UpdateRole()
    {
        if (!ChangedSelectedPlayer) return;
        if (_prevSelectedPlr != null) return;
        if (_selectedPlr == null || Player.Data.IsDead || !_selectedPlr.IsDead)
        {
            _selectedPlr = null;
            _prevSelectedPlr = null;
            if (Player == null || Player.IsRole<ImitatorRole>()) return;

            Player.RpcChangeRole(RoleId.Get<ImitatorRole>(), false);
            return;
        }

        var roleWhenAlive = _selectedPlr.Object.GetRoleWhenAlive();
        if (roleWhenAlive is ICrewVariant crewType) roleWhenAlive = crewType.CrewVariant;

        if (Player.Data.Role.GetType() != roleWhenAlive!.GetType()) Player.RpcChangeRole((ushort)roleWhenAlive!.Role, false);
        RpcUpdateImitation(Player, roleWhenAlive!);

        _prevSelectedPlr = _selectedPlr;
    }
    [MethodRpc((uint)TownOfUsRpc.UpdateImitation, SendImmediately = true)]
    public static void RpcUpdateImitation(PlayerControl player, RoleBehaviour role)
    {
        if (player.TryGetModifier<ImitatorCacheModifier>(out var mod))
        {
            mod.OldRole = role;
        }
    }

    public override void OnDeath(DeathReason reason)
    {
        ModifierComponent?.RemoveModifier(this);
    }
}

using AmongUs.GameOptions;
using Il2CppSystem.Text;
using MiraAPI.Roles;
using Reactor.Utilities;
using TownOfUs.Modules;
using UnityEngine;

namespace TownOfUs.Roles.Neutral;

public class NeutralGhostRole(IntPtr cppPtr) : RoleBehaviour(cppPtr), ITownOfUsRole
{
    public virtual string RoleName => "Neutral Ghost";
    public virtual string RoleDescription => string.Empty;
    public virtual string RoleLongDescription => string.Empty;
    public virtual Color RoleColor => TownOfUsColors.Neutral;
    public ModdedRoleTeams Team => ModdedRoleTeams.Custom;
    public virtual RoleAlignment RoleAlignment => RoleAlignment.NeutralBenign;
    public virtual CustomRoleConfiguration Configuration => new(this)
    {
        TasksCountForProgress = false,
        HideSettings = true,
        RoleHintType = RoleHintType.None,
    };

    public override bool IsDead => true;
    public override bool IsAffectedByComms => false;

    private Minigame _hauntMenu = null!;

    public void Awake()
    {
        var crewGhost = RoleManager.Instance.GetRole(RoleTypes.CrewmateGhost).Cast<CrewmateGhostRole>();
        _hauntMenu = crewGhost.HauntMenu;
        Ability = crewGhost.Ability;
    }

    public override void AppendTaskHint(StringBuilder taskStringBuilder)
    {
        // remove default task hint
    }

    public override bool CanUse(IUsable console)
    {
        return console.TryCast<Console>();
    }

    // reimplement haunt minigame
    public override void UseAbility()
    {
        if (HudManager.Instance.Chat.IsOpenOrOpening)
        {
            return;
        }
        if (Minigame.Instance)
        {
            if (Minigame.Instance.TryCast<HauntMenuMinigame>())
            {
                Minigame.Instance.Close();
            }
            return;
        }
        var minigame = Instantiate(_hauntMenu, HudManager.Instance.AbilityButton.transform, false);
        minigame.transform.SetLocalZ(-5f);
        minigame.Begin(null);
        HudManager.Instance.AbilityButton.SetDisabled();
    }

    public override bool DidWin(GameOverReason gameOverReason)
    {
        var role = Player.GetRoleWhenAlive();

        var win = role!.DidWin(gameOverReason);

        Logger<TownOfUsPlugin>.Message($"NeutralGhostRole.DidWin - role: {role.NiceName} DidWin: {win}");
        
        if (role is JesterRole && win)
        {
            Logger<TownOfUsPlugin>.Info($"Jester - Player: {Player.Data.PlayerName}");
            Player.Data.IsDead = false;
        }

        return win;
    }

    public virtual bool WinConditionMet()
    {
        var role = Player.GetRoleWhenAlive();

        return role is ITownOfUsRole tRole && tRole.WinConditionMet();
    }
}

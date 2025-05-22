using System.Text;
using AmongUs.GameOptions;
using Il2CppInterop.Runtime.Attributes;
using MiraAPI.GameOptions;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using Reactor.Utilities;
using Reactor.Utilities.Extensions;
using TownOfUs.Modules.Wiki;
using TownOfUs.Options.Roles.Crewmate;
using TownOfUs.Patches.Stubs;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfUs.Roles.Crewmate;

public sealed class SnitchRole(IntPtr cppPtr) : CrewmateRole(cppPtr), ITownOfUsRole, IWikiDiscoverable, IDoomable
{
    public string RoleName => "Snitch";
    public string RoleDescription => "Find the <color=#FF0000FF>Impostors</color>!";
    public string RoleLongDescription => CompletedAllTasks ? "Find the Impostors!" : "Complete all your tasks to discover the Impostors.";
    public Color RoleColor => TownOfUsColors.Snitch;
    public ModdedRoleTeams Team => ModdedRoleTeams.Crewmate;
    public RoleAlignment RoleAlignment => RoleAlignment.CrewmateInvestigative;
    public DoomableType DoomHintType => DoomableType.Insight;
    public CustomRoleConfiguration Configuration => new(this)
    {
        Icon = TouRoleIcons.Snitch,
        IntroSound = CustomRoleUtils.GetIntroSound(RoleTypes.Noisemaker),
    };

    private Dictionary<byte, ArrowBehaviour>? _snitchArrows;
    public ArrowBehaviour? SnitchRevealArrow { get; private set; }
    public bool CompletedAllTasks { get; private set; }
    public bool OnLastTask { get; private set; }

    public void CheckTaskRequirements()
    {
        var completedTasks = Player.myTasks.ToArray().Count(t => t.IsComplete);

        OnLastTask = (Player.myTasks.Count - completedTasks) <= (int)OptionGroupSingleton<SnitchOptions>.Instance.TaskRemainingWhenRevealed;

        if (IsTargetOfSnitch(PlayerControl.LocalPlayer) && OnLastTask)
        {
            CreateRevealingArrow();
        }

        if (OnLastTask && Player.AmOwner)
        {
            Coroutines.Start(MiscUtils.CoFlash(TownOfUsColors.Snitch, alpha: 0.5f));
        }

        CompletedAllTasks = completedTasks == Player.myTasks.Count;

        if (CompletedAllTasks && IsTargetOfSnitch(PlayerControl.LocalPlayer))
        {
            Coroutines.Start(MiscUtils.CoFlash(TownOfUsColors.Snitch, alpha: 0.5f));
        }

        if (CompletedAllTasks && Player.AmOwner)
        {
            CreateSnitchArrows();
        }
    }

    public static bool IsTargetOfSnitch(PlayerControl player)
    {
        if (player == null || player.Data == null || player.Data.Role == null)
        {
            return false;
        }

        return (player.IsImpostor() && !player.IsTraitor()) || (player.IsTraitor() && OptionGroupSingleton<SnitchOptions>.Instance.SnitchSeesTraitor) || (player.Is(RoleAlignment.NeutralKilling) && OptionGroupSingleton<SnitchOptions>.Instance.SnitchNeutralRoles);
    }

    public static bool SnitchVisibilityFlag(PlayerControl player)
    {
        var snitchRevealed = PlayerControl.LocalPlayer.Data.Role is SnitchRole snitch && snitch.CompletedAllTasks;
        var showSnitch = IsTargetOfSnitch(PlayerControl.LocalPlayer) && player.Data.Role is SnitchRole snitch2 && snitch2.OnLastTask;

        if (MeetingHud.Instance != null && !OptionGroupSingleton<SnitchOptions>.Instance.SnitchSeesImpostorsMeetings)
        {
            snitchRevealed = false;
        }

        return (snitchRevealed && IsTargetOfSnitch(player)) || showSnitch;
    }

    public override void OnDeath(DeathReason reason)
    {
        RoleStubs.RoleBehaviourOnDeath(this, reason);

        ClearArrows();
    }

    public void RemoveArrowForPlayer(byte playerId)
    {
        if (_snitchArrows != null && _snitchArrows.TryGetValue(playerId, out var arrow))
        {
            arrow.gameObject.Destroy();
            _snitchArrows.Remove(playerId);
        }
    }

    public void ClearArrows()
    {
        if (_snitchArrows != null && _snitchArrows.Count > 0)
        {
            _snitchArrows.ToList().ForEach(arrow => arrow.Value.gameObject.Destroy());
            _snitchArrows.Clear();
        }

        if (SnitchRevealArrow != null)
        {
            SnitchRevealArrow.gameObject.Destroy();
        }
    }

    private void CreateRevealingArrow()
    {
        if (SnitchRevealArrow != null)
        {
            return;
        }

        PlayerNameColor.Set(Player);
        Coroutines.Start(MiscUtils.CoFlash(TownOfUsColors.Snitch, alpha: 0.5f));
        SnitchRevealArrow = MiscUtils.CreateArrow(Player.transform, TownOfUsColors.Snitch);
    }

    private void CreateSnitchArrows()
    {
        if (_snitchArrows != null)
        {
            return;
        }

        Coroutines.Start(MiscUtils.CoFlash(TownOfUsColors.Snitch, alpha: 0.5f));
        _snitchArrows = new();

        var imps = Helpers.GetAlivePlayers().Where(plr => plr.Data.Role.IsImpostor && !plr.IsTraitor());
        var traitor = Helpers.GetAlivePlayers().FirstOrDefault(plr => plr.IsTraitor());
        imps.ToList().ForEach(imp =>
        {
            _snitchArrows.Add(imp.PlayerId, MiscUtils.CreateArrow(imp.transform, TownOfUsColors.Impostor));
            PlayerNameColor.Set(imp);
        });

        if (OptionGroupSingleton<SnitchOptions>.Instance.SnitchSeesTraitor && traitor != null)
        {
            _snitchArrows.Add(traitor.PlayerId, MiscUtils.CreateArrow(traitor.transform, TownOfUsColors.Impostor));
            PlayerNameColor.Set(traitor);
        }

        if (OptionGroupSingleton<SnitchOptions>.Instance.SnitchNeutralRoles)
        {
            var neutrals = MiscUtils.GetRoles(RoleAlignment.NeutralKilling).Where(role => !role.Player.Data.IsDead && !role.Player.Data.Disconnected);
            neutrals.ToList().ForEach(neutral =>
            {
                _snitchArrows.Add(neutral.Player.PlayerId, MiscUtils.CreateArrow(neutral.Player.transform, TownOfUsColors.Neutral));
                PlayerNameColor.Set(neutral.Player);
            });
        }
    }
    public void AddSnitchTraitorArrows()
    {
        var completedTasks = Player.myTasks.ToArray().Count(t => t.IsComplete);

        OnLastTask = (Player.myTasks.Count - completedTasks) <= (int)OptionGroupSingleton<SnitchOptions>.Instance.TaskRemainingWhenRevealed;

        if (PlayerControl.LocalPlayer.IsTraitor() && OptionGroupSingleton<SnitchOptions>.Instance.SnitchSeesTraitor && OnLastTask)
        {
            CreateRevealingArrow();
        }

        CompletedAllTasks = completedTasks == Player.myTasks.Count;

        if (CompletedAllTasks && Player.AmOwner)
        {
            var traitor = Helpers.GetAlivePlayers().FirstOrDefault(plr => plr.IsTraitor());
            if (_snitchArrows == null || traitor == null || (_snitchArrows.TryGetValue(traitor.PlayerId, out var arrow) && arrow != null))
            {
                return;
            }
            if (OptionGroupSingleton<SnitchOptions>.Instance.SnitchSeesTraitor && traitor != null)
            {
                _snitchArrows.Add(traitor.PlayerId, MiscUtils.CreateArrow(traitor.transform, TownOfUsColors.Impostor));
                PlayerNameColor.Set(traitor);
            }
        }
    }

    private void FixedUpdate()
    {
        if (Player == null || Player.Data.Role is not SnitchRole) return;

        if (SnitchRevealArrow != null && SnitchRevealArrow.target != SnitchRevealArrow.transform.parent.position)
        {
            SnitchRevealArrow.target = Player.transform.position;
        }

        if (_snitchArrows != null && _snitchArrows.Count > 0 && Player.AmOwner)
        {
            _snitchArrows.ToList().ForEach(arrow => arrow.Value.target = arrow.Value.transform.parent.position);
        }
    }

    [HideFromIl2Cpp]
    public StringBuilder SetTabText()
    {
        return ITownOfUsRole.SetNewTabText(this);
    }
    
    public string GetAdvancedDescription()
    {
        return
            "The Snitch is a Crewmate Investigative role that can reveal the Impostors to themselves by finishing all their tasks. " +
            "Upon completing all tasks, the Impostors will be revealed to the Snitch with an arrow and their red name. " +
            $"The Snitch will be revealed to the Impostors when they have {OptionGroupSingleton<SnitchOptions>.Instance.TaskRemainingWhenRevealed} task(s) remaining."
            + MiscUtils.AppendOptionsText(GetType());
    }
}

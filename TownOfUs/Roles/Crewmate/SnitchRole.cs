﻿using System.Globalization;
using System.Text;
using AmongUs.GameOptions;
using HarmonyLib;
using Il2CppInterop.Runtime.Attributes;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Patches.Stubs;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using Reactor.Utilities;
using Reactor.Utilities.Extensions;
using TownOfUs.Modifiers.Crewmate;
using TownOfUs.Modifiers.Game.Alliance;
using TownOfUs.Options.Roles.Crewmate;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfUs.Roles.Crewmate;

public sealed class SnitchRole(IntPtr cppPtr) : CrewmateRole(cppPtr), ITownOfUsRole, IWikiDiscoverable, IDoomable
{
    private Dictionary<byte, ArrowBehaviour>? _snitchArrows;
    [HideFromIl2Cpp] public ArrowBehaviour? SnitchRevealArrow { get; private set; }
    public bool CompletedAllTasks => TaskStage is TaskStage.CompletedTasks;
    public bool OnLastTask => TaskStage is TaskStage.Revealed or TaskStage.CompletedTasks;
    public TaskStage TaskStage { get; private set; } = TaskStage.Unrevealed;

    private void FixedUpdate()
    {
        if (Player == null || Player.Data.Role is not SnitchRole)
        {
            return;
        }

        if (SnitchRevealArrow != null && SnitchRevealArrow.target != SnitchRevealArrow.transform.parent.position)
        {
            SnitchRevealArrow.target = Player.transform.position;
        }

        if (_snitchArrows != null && _snitchArrows.Count > 0 && Player.AmOwner)
        {
            _snitchArrows.ToList().ForEach(arrow => arrow.Value.target = arrow.Value.transform.parent.position);
        }
    }

    public DoomableType DoomHintType => DoomableType.Insight;
    public string LocaleKey => "Snitch";
    public string RoleName => TouLocale.Get($"TouRole{LocaleKey}");
    public string RoleDescription => TouLocale.GetParsed($"TouRole{LocaleKey}IntroBlurb");
    public string RoleLongDescription => TouLocale.GetParsed($"TouRole{LocaleKey}TabDescription");

    public string GetAdvancedDescription()
    {
        return
            TouLocale.GetParsed($"TouRole{LocaleKey}WikiDescription") +
            MiscUtils.AppendOptionsText(GetType());
    }

    public Color RoleColor => TownOfUsColors.Snitch;
    public ModdedRoleTeams Team => ModdedRoleTeams.Crewmate;
    public RoleAlignment RoleAlignment => RoleAlignment.CrewmateInvestigative;

    public CustomRoleConfiguration Configuration => new(this)
    {
        Icon = TouRoleIcons.Snitch,
        IntroSound = TouAudio.ToppatIntroSound
    };

    [HideFromIl2Cpp]
    public StringBuilder SetTabText()
    {
        var stringB = new StringBuilder();
        stringB.AppendLine(CultureInfo.InvariantCulture,
            $"{RoleColor.ToTextColor()}{TouLocale.Get("YouAreA")}<b> {RoleName}.</b></color>");
        stringB.AppendLine(CultureInfo.InvariantCulture,
            $"<size=60%>{TouLocale.Get("Alignment")}: <b>{MiscUtils.GetParsedRoleAlignment(RoleAlignment, true)}</b></size>");
        stringB.Append("<size=70%>");

        var desc = CompletedAllTasks ? "CompletedTasks" : string.Empty;
        if (PlayerControl.LocalPlayer.HasModifier<EgotistModifier>())
        {
            desc += "Ego";
        }

        var text = TouLocale.GetParsed($"TouRole{LocaleKey}TabDescription{desc}");

        stringB.AppendLine(CultureInfo.InvariantCulture, $"{text}");

        return stringB;
    }

    public void CheckTaskRequirements()
    {
        var realTasks = Player.myTasks.ToArray()
            .Where(x => !PlayerTask.TaskIsEmergency(x) && !x.TryCast<ImportantTextTask>()).ToList();

        var completedTasks = realTasks.Count(t => t.IsComplete);
        var tasksRemaining = realTasks.Count - completedTasks;

        if (TaskStage is TaskStage.Unrevealed && tasksRemaining <=
            (int)OptionGroupSingleton<SnitchOptions>.Instance.TaskRemainingWhenRevealed)
        {
            TaskStage = TaskStage.Revealed;
            if (Player.AmOwner)
            {
                Coroutines.Start(MiscUtils.CoFlash(TownOfUsColors.Snitch, alpha: 0.5f));
                var text = "The impostors know of your whereabouts!";
                if (Player.HasModifier<EgotistModifier>())
                {
                    text = "The impostors know of your whereabouts, and know you're the Egotist!";
                }

                var notif1 = Helpers.CreateAndShowNotification(
                    $"<b>{TownOfUsColors.Snitch.ToTextColor()}{text}</color></b>", Color.white,
                    new Vector3(0f, 1f, -20f),
                    spr: TouRoleIcons.Snitch.LoadAsset());

                notif1.AdjustNotification();
            }
            else if (IsTargetOfSnitch(PlayerControl.LocalPlayer))
            {
                CreateRevealingArrow();
                Coroutines.Start(MiscUtils.CoFlash(TownOfUsColors.Snitch, alpha: 0.5f));
                var text = "The Snitch is getting closer to reveal you!";
                if (Player.HasModifier<EgotistModifier>())
                {
                    text = "The Snitch is an Egotist, who will help you overthrow the crewmates!";
                }

                var notif1 = Helpers.CreateAndShowNotification(
                    $"<b>{TownOfUsColors.Snitch.ToTextColor()}{text}</color></b>", Color.white,
                    new Vector3(0f, 1f, -20f),
                    spr: TouRoleIcons.Snitch.LoadAsset());

                notif1.AdjustNotification();
            }
        }

        if (completedTasks == realTasks.Count)
        {
            TaskStage = TaskStage.CompletedTasks;
            if (Player.AmOwner)
            {
                CreateSnitchArrows();
                var text = "You have revealed the impostors!";
                if (Player.HasModifier<EgotistModifier>())
                {
                    text = "You have revealed the impostors, who can help your win condition!";
                }

                var notif1 = Helpers.CreateAndShowNotification(
                    $"<b>{TownOfUsColors.Snitch.ToTextColor()}{text}</color></b>", Color.white,
                    new Vector3(0f, 1f, -20f),
                    spr: TouRoleIcons.Snitch.LoadAsset());

                notif1.AdjustNotification();
            }
            else if (IsTargetOfSnitch(PlayerControl.LocalPlayer))
            {
                Coroutines.Start(MiscUtils.CoFlash(TownOfUsColors.Snitch, alpha: 0.5f));
                var text = "The Snitch knows what you are now!";
                if (Player.HasModifier<EgotistModifier>())
                {
                    text = "The Snitch can now help you as the Egotist!";
                }

                var notif1 = Helpers.CreateAndShowNotification(
                    $"<b>{TownOfUsColors.Snitch.ToTextColor()}{text}</color></b>", Color.white,
                    new Vector3(0f, 1f, -20f), spr: TouRoleIcons.Snitch.LoadAsset());

                notif1.AdjustNotification();
            }
        }

        if (TownOfUsPlugin.IsDevBuild)
        {
            Logger<TownOfUsPlugin>.Error(
                $"Snitch Stage for '{Player.Data.PlayerName}': {TaskStage.ToDisplayString()} - ({completedTasks} / {realTasks.Count})");
        }
    }

    public static bool IsTargetOfSnitch(PlayerControl player)
    {
        if (player == null || player.Data == null || player.Data.Role == null)
        {
            return false;
        }

        return (player.IsImpostor() && !player.IsTraitor()) ||
               (player.IsTraitor() && OptionGroupSingleton<SnitchOptions>.Instance.SnitchSeesTraitor) ||
               (player.Is(RoleAlignment.NeutralKilling) &&
                OptionGroupSingleton<SnitchOptions>.Instance.SnitchNeutralRoles);
    }

    public override void Initialize(PlayerControl player)
    {
        RoleBehaviourStubs.Initialize(this, player);

        ClearArrows();
        // incase amne becomes snitch or smth
        CheckTaskRequirements();
    }

    public override void Deinitialize(PlayerControl targetPlayer)
    {
        RoleBehaviourStubs.Deinitialize(this, targetPlayer);

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

        ModifierUtils.GetActiveModifiers<SnitchImpostorRevealModifier>()
            .Do(x => x.ModifierComponent?.RemoveModifier(x));
        ModifierUtils.GetActiveModifiers<SnitchPlayerRevealModifier>().Do(x => x.ModifierComponent?.RemoveModifier(x));
    }

    private void CreateRevealingArrow()
    {
        if (SnitchRevealArrow != null)
        {
            return;
        }

        Player.AddModifier<SnitchPlayerRevealModifier>(
            RoleManager.Instance.GetRole((RoleTypes)RoleId.Get<SnitchRole>()));
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
        _snitchArrows = new Dictionary<byte, ArrowBehaviour>();
        var imps = Helpers.GetAlivePlayers().Where(plr => plr.Data.Role.IsImpostor && !plr.IsTraitor());
        var traitor = Helpers.GetAlivePlayers().FirstOrDefault(plr => plr.IsTraitor());
        imps.ToList().ForEach(imp =>
        {
            _snitchArrows.Add(imp.PlayerId, MiscUtils.CreateArrow(imp.transform, TownOfUsColors.Impostor));
            PlayerNameColor.Set(imp);
            imp.AddModifier<SnitchImpostorRevealModifier>();
        });

        if (OptionGroupSingleton<SnitchOptions>.Instance.SnitchSeesTraitor && traitor != null)
        {
            _snitchArrows.Add(traitor.PlayerId, MiscUtils.CreateArrow(traitor.transform, TownOfUsColors.Impostor));
            PlayerNameColor.Set(traitor);
            traitor.AddModifier<SnitchImpostorRevealModifier>();
        }

        if (OptionGroupSingleton<SnitchOptions>.Instance.SnitchNeutralRoles)
        {
            var neutrals = MiscUtils.GetRoles(RoleAlignment.NeutralKilling)
                .Where(role => !role.Player.Data.IsDead && !role.Player.Data.Disconnected);
            neutrals.ToList().ForEach(neutral =>
            {
                _snitchArrows.Add(neutral.Player.PlayerId,
                    MiscUtils.CreateArrow(neutral.Player.transform, TownOfUsColors.Neutral));
                PlayerNameColor.Set(neutral.Player);
                neutral.Player.AddModifier<SnitchImpostorRevealModifier>();
            });
        }
    }

    public void AddSnitchTraitorArrows()
    {
        if (PlayerControl.LocalPlayer.IsTraitor() && OnLastTask)
        {
            CreateRevealingArrow();
        }

        if (CompletedAllTasks && Player.AmOwner)
        {
            var traitor = Helpers.GetAlivePlayers().FirstOrDefault(plr => plr.IsTraitor());
            if (_snitchArrows == null || traitor == null ||
                (_snitchArrows.TryGetValue(traitor.PlayerId, out var arrow) && arrow != null))
            {
                return;
            }

            if (OptionGroupSingleton<SnitchOptions>.Instance.SnitchSeesTraitor && traitor != null)
            {
                _snitchArrows.Add(traitor.PlayerId, MiscUtils.CreateArrow(traitor.transform, TownOfUsColors.Impostor));
                PlayerNameColor.Set(traitor);
                Player.AddModifier<SnitchImpostorRevealModifier>();
            }
        }
    }
}

public enum TaskStage
{
    Unrevealed,
    Revealed,
    CompletedTasks
}
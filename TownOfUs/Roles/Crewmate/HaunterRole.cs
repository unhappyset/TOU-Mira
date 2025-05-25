using System.Text;
using Il2CppInterop.Runtime.Attributes;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Patches.Stubs;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using Reactor.Utilities;
using TownOfUs.Modifiers.Crewmate;
using TownOfUs.Modules.Wiki;
using TownOfUs.Options.Roles.Crewmate;
using TownOfUs.Utilities;
using TownOfUs.Utilities.Appearances;
using UnityEngine;

namespace TownOfUs.Roles.Crewmate;

public sealed class HaunterRole(IntPtr cppPtr) : CrewmateGhostRole(cppPtr), ITownOfUsRole, IGhostRole, IWikiDiscoverable
{
    public string RoleName => "Haunter";
    public string RoleDescription => string.Empty;
    public string RoleLongDescription => "Complete all your tasks without getting caught to reveal impostors!";
    public Color RoleColor => TownOfUsColors.Haunter;
    public ModdedRoleTeams Team => ModdedRoleTeams.Crewmate;
    public RoleAlignment RoleAlignment => RoleAlignment.CrewmateInvestigative;
    public CustomRoleConfiguration Configuration => new(this)
    {
        Icon = TouRoleIcons.Haunter,
        TasksCountForProgress = false,
        HideSettings = false,
    };

    public bool Setup { get; set; }
    public bool Caught { get; set; }
    public bool Faded { get; set; }
    public bool CanBeClicked { get; set; }
    public bool Revealed { get; private set; }
    public bool CompletedAllTasks { get; private set; }
    public bool GhostActive => Setup && !Caught;

    public override void UseAbility()
    {
        if (GhostActive) return;

        base.UseAbility();
    }

    public override void Initialize(PlayerControl player)
    {
        RoleBehaviourStubs.Initialize(this, player);
        MiscUtils.AdjustGhostTasks(player);
    }

    public override bool CanUse(IUsable console)
    {
        var validUsable = console.TryCast<Console>() ||
            console.TryCast<DoorConsole>() ||
            console.TryCast<OpenDoorConsole>() ||
            console.TryCast<DeconControl>() ||
            console.TryCast<PlatformConsole>() ||
            console.TryCast<Ladder>() ||
            console.TryCast<ZiplineConsole>();

        return GhostActive && validUsable;
    }

    public bool CanCatch()
    {
        var options = OptionGroupSingleton<HaunterOptions>.Instance;

        if (options.HaunterCanBeClickedBy == HaunterRoleClickableType.ImpsOnly && !PlayerControl.LocalPlayer.IsImpostor())
            return false;

        if (options.HaunterCanBeClickedBy == HaunterRoleClickableType.NonCrew && !(PlayerControl.LocalPlayer.IsImpostor() || PlayerControl.LocalPlayer.Is(RoleAlignment.NeutralKilling)))
            return false;

        return true;
    }

    public void Spawn()
    {
        Setup = true;

        // Logger<TownOfUsPlugin>.Error($"Setup HaunterRole '{Player.Data.PlayerName}'");
        Player.gameObject.layer = LayerMask.NameToLayer("Players");

        Player.gameObject.GetComponent<PassiveButton>().OnClick = new UnityEngine.UI.Button.ButtonClickedEvent();
        Player.gameObject.GetComponent<PassiveButton>().OnClick.AddListener((Action)(() => Player.OnClick()));
        Player.gameObject.GetComponent<BoxCollider2D>().enabled = true;

        if (Player.AmOwner)
        {
            Player.SpawnAtRandomVent();
            Player.MyPhysics.ResetMoveState();

            HudManager.Instance.SetHudActive(false);
            HudManager.Instance.SetHudActive(true);
            HudManager.Instance.AbilityButton.SetDisabled();
            Patches.HudManagerPatches.ZoomButton.SetActive(false);
        }
    }

    public void FadeUpdate(HudManager instance)
    {
        if (!Caught && Setup)
        {
            Player.GhostFade();
            Faded = true;
        }
        else if (Faded)
        {
            Player.ResetAppearance();
            Player.cosmetics.ToggleNameVisible(true);

            Player.cosmetics.currentBodySprite.BodySprite.color = Color.white;
            Player.gameObject.layer = LayerMask.NameToLayer("Ghost");
            Player.MyPhysics.ResetMoveState();

            Faded = false;

            // Logger<TownOfUsPlugin>.Message($"HaunterRole.FadeUpdate UnFaded");
        }
    }

    public void Clicked()
    {
        // Logger<TownOfUsPlugin>.Message($"HaunterRole.Clicked");
        Caught = true;
        Player.Exiled();

        if (Player.AmOwner)
        {
            HudManager.Instance.AbilityButton.SetEnabled();
        }

        Player.RemoveModifier<HaunterArrowModifier>();
    }

    public void CheckTaskRequirements()
    {
        if (Caught) return;

        var completedTasks = Player.myTasks.ToArray().Count(t => t.IsComplete);
        var tasksRemaining = Player.myTasks.Count - completedTasks;

        CanBeClicked = tasksRemaining <= (int)OptionGroupSingleton<HaunterOptions>.Instance.NumTasksLeftBeforeClickable;

        if (!CompletedAllTasks && completedTasks == Player.myTasks.Count)
        {
            CompletedAllTasks = true;

            if (Player.AmOwner || IsTargetOfHaunter(PlayerControl.LocalPlayer))
            {
                Coroutines.Start(MiscUtils.CoFlash(Color.white));
            }
        }

        if (!Revealed && tasksRemaining == (int)OptionGroupSingleton<HaunterOptions>.Instance.NumTasksLeftBeforeAlerted)
        {
            // Logger<TownOfUsPlugin>.Error($"CheckTaskRequirements Revealed");
            Revealed = true;

            if (Player.AmOwner)
            {
                Coroutines.Start(MiscUtils.CoFlash(RoleColor));
                var notif1 = Helpers.CreateAndShowNotification($"<b>{TownOfUsColors.Haunter.ToTextColor()}You have alerted the Killers!</b></color>", Color.white, new Vector3(0f, 1f, -20f), spr: TouRoleIcons.Haunter.LoadAsset());
                notif1.Text.SetOutlineThickness(0.35f);
            }
            else if (IsTargetOfHaunter(PlayerControl.LocalPlayer))
            {
                // Logger<TownOfUsPlugin>.Error($"CheckTaskRequirements IsTargetOfHaunter");
                Coroutines.Start(MiscUtils.CoFlash(RoleColor));

                Player.AddModifier<HaunterArrowModifier>(PlayerControl.LocalPlayer, RoleColor);
                var notif1 = Helpers.CreateAndShowNotification($"<b>{TownOfUsColors.Haunter.ToTextColor()}A Haunter is loose, catch them before they reveal you!</b></color>", Color.white, new Vector3(0f, 1f, -20f), spr: TouRoleIcons.Haunter.LoadAsset());
                notif1.Text.SetOutlineThickness(0.35f);
            }
        }
    }

    public static bool IsTargetOfHaunter(PlayerControl player)
    {
        if (player == null || player.Data == null || player.Data.Role == null)
        {
            return false;
        }

        return player.IsImpostor() || (player.Is(RoleAlignment.NeutralKilling) && OptionGroupSingleton<HaunterOptions>.Instance.RevealNeutralRoles);
    }

    public static bool HaunterVisibilityFlag(PlayerControl player)
    {
        var haunter = MiscUtils.GetRole<HaunterRole>();

        if (haunter == null) return false;

        return IsTargetOfHaunter(player) && haunter.CompletedAllTasks && !player.AmOwner;
    }

    [HideFromIl2Cpp]
    public StringBuilder SetTabText()
    {
        return ITownOfUsRole.SetNewTabText(this);
    }
    
    public string GetAdvancedDescription()
    {
        return
            "The Haunter is a Crewmate Ghost who can do tasks. They will appear as a transparent player. " +
            "If they finish all their tasks, all alive players will see who the Impostors are. " +
            "However, if an Impostor clicks them first, they will become a normal ghost. " +
            "Impostors get a warning shortly before and when the Haunter finishes their tasks. "
            + MiscUtils.AppendOptionsText(GetType());
    }
}

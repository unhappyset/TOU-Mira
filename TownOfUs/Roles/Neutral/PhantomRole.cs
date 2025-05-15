using System.Text;
using Il2CppInterop.Runtime.Attributes;
using MiraAPI.GameOptions;
using MiraAPI.Patches.Stubs;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using TownOfUs.Modules.Wiki;
using TownOfUs.Options.Roles.Neutral;
using TownOfUs.Utilities;
using TownOfUs.Utilities.Appearances;
using UnityEngine;

namespace TownOfUs.Roles.Neutral;

public sealed class PhantomTouRole(IntPtr cppPtr) : NeutralGhostRole(cppPtr), ITownOfUsRole, IGhostRole, IWikiDiscoverable
{
    public override string RoleName => "Phantom";
    public override string RoleDescription => string.Empty;
    public override string RoleLongDescription => "Complete all your tasks without being caught!";
    public override Color RoleColor => TownOfUsColors.Phantom;
    public override RoleAlignment RoleAlignment => RoleAlignment.NeutralEvil;
    public override CustomRoleConfiguration Configuration => new(this)
    {
        Icon = TouRoleIcons.Phantom,
        HideSettings = false,
    };

    public bool Setup { get; set; }
    public bool Caught { get; set; }
    public bool Faded { get; set; }
    public bool CanBeClicked { get; set; }
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

    public override bool DidWin(GameOverReason gameOverReason)
    {
        return CompletedAllTasks;
    }

    public override bool WinConditionMet()
    {
        return OptionGroupSingleton<PhantomOptions>.Instance.WinEndsGame && CompletedAllTasks;
    }

    public bool CanCatch() => true;

    public void Spawn()
    {
        Setup = true;

        // Logger<TownOfUsPlugin>.Error($"Setup PhantomTouRole '{Player.Data.PlayerName}'");
        Player.gameObject.layer = LayerMask.NameToLayer("Players");

        Player.gameObject.GetComponent<PassiveButton>().OnClick = new UnityEngine.UI.Button.ButtonClickedEvent();
        Player.gameObject.GetComponent<PassiveButton>().OnClick.AddListener((Action)(() => Player.OnClick()));
        Player.gameObject.GetComponent<BoxCollider2D>().enabled = true;

        if (Player.AmOwner)
        {
            Player.SpawnAtRandomVent();
            Player.MyPhysics.ResetMoveState();

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

            // Logger<TownOfUsPlugin>.Message($"PhantomTouRole.FadeUpdate UnFaded");
        }
    }

    public void Clicked()
    {
        // Logger<TownOfUsPlugin>.Message($"PhantomTouRole.Clicked");
        Caught = true;
        Player.Exiled();

        if (Player.AmOwner)
        {
            HudManager.Instance.AbilityButton.SetEnabled();
        }
    }

    public void CheckTaskRequirements()
    {
        if (Caught) return;

        var completedTasks = Player.myTasks.ToArray().Count(t => t.IsComplete);
        var tasksRemaining = Player.myTasks.Count - completedTasks;

        CanBeClicked = tasksRemaining <= (int)OptionGroupSingleton<PhantomOptions>.Instance.NumTasksLeftBeforeClickable;
        if (tasksRemaining == (int)OptionGroupSingleton<PhantomOptions>.Instance.NumTasksLeftBeforeClickable && Player.AmOwner)
        {
            var notif1 = Helpers.CreateAndShowNotification($"<b>{TownOfUsColors.Phantom.ToTextColor()}You are now clickable by players!</b></color>", Color.white, new Vector3(0f, 1f, -20f), spr: TouRoleIcons.Phantom.LoadAsset());
            notif1.Text.SetOutlineThickness(0.35f);
        }
        CompletedAllTasks = completedTasks == Player.myTasks.Count;
    }
    [HideFromIl2Cpp]
    public StringBuilder SetTabText()
    {
        return ITownOfUsRole.SetNewTabText(this);
    }

    public string GetAdvancedDescription() 
    {
        return "The Phantom is a Neutral Ghost role that wins the game by finishing their tasks before a alive player has clicked on them." + MiscUtils.AppendOptionsText(GetType());
    }
}

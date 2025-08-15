using System.Text;
using AmongUs.GameOptions;
using Il2CppInterop.Runtime.Attributes;
using MiraAPI.Patches.Stubs;
using MiraAPI.Roles;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfUs.Roles.Other;

public sealed class SpectatorRole(IntPtr cppPtr) : RoleBehaviour(cppPtr), ITownOfUsRole, IWikiDiscoverable
{
    public static readonly List<byte> TrackedSpectators = [];
    public static readonly List<PlayerControl> TrackedPlayers = [];
    public static bool FixedCam;
    private static int CurrentTarget;
    private bool ShowHud;
    private bool ShowShadows;

    public string RoleName => TouLocale.Get(TouNames.Spectator, "Spectator");
    public string RoleDescription => "Watch the game unfold!";
    public string RoleLongDescription => "Never participating, watch as the chaos unfolds in-game!";
    public Color RoleColor => TownOfUsColors.Spectator;
    public ModdedRoleTeams Team => ModdedRoleTeams.Custom;
    public RoleAlignment RoleAlignment => RoleAlignment.GameOutlier;

    public override bool IsDead => true;

    public CustomRoleConfiguration Configuration => new(this)
    {
        IntroSound = CustomRoleUtils.GetIntroSound(RoleTypes.Noisemaker),
        GhostRole = (RoleTypes)RoleId.Get<SpectatorRole>(),
        Icon = TouRoleIcons.Spectator,
    };

    public override void Initialize(PlayerControl player)
    {
        RoleBehaviourStubs.Initialize(this, player);

        if (!Player.AmOwner)
            return;

        ShowHud = false;
        FixedCam = true;
        ShowShadows = false;

        HudManager.Instance.PlayerCam.SetTarget(Player);
        HudManager.Instance.ShadowQuad.gameObject.SetActive(ShowShadows);
        HudManager.Instance.SetHudActive(ShowHud);
    }

    [HideFromIl2Cpp]
    public StringBuilder SetTabText()
    {
        return ITownOfUsRole.SetNewTabText(this);
    }

    public void Update()
    {
        if (!Player.AmOwner || LobbyBehaviour.Instance || MeetingHud.Instance)
            return;

        if (Input.GetKeyDown(KeyCode.F1))
            FixedCam = !FixedCam;

        if (Input.GetKeyDown(KeyCode.K))
            HudManager.Instance.PlayerCam.SetTarget(Player);

        if (Input.GetKeyDown(KeyCode.I))
        {
            ShowHud = !ShowHud;
            HudManager.Instance.SetHudActive(ShowHud);
        }

        if (Input.GetKeyDown(KeyCode.M))
        {
            ShowShadows = !ShowShadows;
            HudManager.Instance.ShadowQuad.gameObject.SetActive(ShowShadows);
        }

        if (Input.GetKeyDown(KeyCode.J))
            {
                CurrentTarget--;

                if (CurrentTarget <= -1)
                    CurrentTarget = TrackedPlayers.Count - 1;

                HudManager.Instance.PlayerCam.SetTarget(TrackedPlayers[CurrentTarget]);
            }

        if (Input.GetKeyDown(KeyCode.L))
        {
            CurrentTarget++;

            if (CurrentTarget >= TrackedPlayers.Count)
                CurrentTarget = 0;

            HudManager.Instance.PlayerCam.SetTarget(TrackedPlayers[CurrentTarget]);
        }
    }

    public void OnDestroy()
    {
        ShowHud = true;
        FixedCam = false;
        ShowShadows = true;

        if (!Player.AmOwner)
            return;

        HudManager.Instance.PlayerCam.SetTarget(Player);
        HudManager.Instance.ShadowQuad.gameObject.SetActive(ShowShadows);
        HudManager.Instance.SetHudActive(ShowHud);
    }

    public string GetAdvancedDescription()
    {
        return $"The {RoleName} is a Game Outlier role that does not win, and instead can watch the game unfold before them." + MiscUtils.AppendOptionsText(GetType());
    }

    public override bool CanUse(IUsable console) => false;

    public override bool DidWin(GameOverReason gameOverReason) => false;

    public static void InitList()
    {
        foreach (var player in PlayerControl.AllPlayerControls)
        {
            if (!TrackedSpectators.Contains(player.PlayerId))
                TrackedPlayers.Add(player);
        }
    }
}
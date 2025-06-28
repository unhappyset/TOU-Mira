using HarmonyLib;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Roles;
using TownOfUs.Modifiers;
using TownOfUs.Modifiers.Crewmate;
using TownOfUs.Modifiers.Game.Universal;
using TownOfUs.Modifiers.Impostor.Venerer;
using TownOfUs.Modifiers.Neutral;
using TownOfUs.Modules;
using TownOfUs.Modules.Wiki;
using TownOfUs.Options;
using TownOfUs.Options.Roles.Crewmate;
using TownOfUs.Roles;
using TownOfUs.Roles.Crewmate;
using TownOfUs.Roles.Neutral;
using TownOfUs.Utilities;
using TownOfUs.Utilities.Appearances;
using UnityEngine;
using UnityEngine.Events;
using Color = UnityEngine.Color;
using TMPro;
using System.Text;
using TownOfUs.Patches.Options;
using Reactor.Utilities;
using System.Collections;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace TownOfUs.Patches;

[HarmonyPatch]
public static class HudManagerPatches
{
    public static GameObject ZoomButton;
    private static GameObject WikiButton;
    private static GameObject RoleList;
    public static GameObject TeamChatButton;

    private static bool Zooming;

    private static IEnumerator CoResizeUI()
    {
        while (!HudManager.Instance) yield return null;
        yield return new WaitForSeconds(0.01f);
        ResizeUI(TownOfUsPlugin.ButtonUIFactor.Value);
    }

    public static void ResizeUI(float scaleFactor)
    {
        foreach (var aspect in HudManager.Instance.transform.FindChild("Buttons").GetComponentsInChildren<AspectPosition>(true))
        {
            if (aspect.gameObject == null) continue;
            if (aspect.gameObject.transform.parent.name == "TopRight") continue;
            if (aspect.gameObject.transform.parent.transform.parent.name == "TopRight") continue;
            aspect.gameObject.SetActive(!aspect.isActiveAndEnabled);
            aspect.DistanceFromEdge *= new Vector2(scaleFactor, scaleFactor);
            aspect.gameObject.SetActive(!aspect.isActiveAndEnabled);
        }
        
        foreach (var button in HudManager.Instance.GetComponentsInChildren<ActionButton>(true))
        {
            if (button.gameObject == null) continue;
            
            button.gameObject.SetActive(!button.isActiveAndEnabled);
            button.gameObject.transform.localScale *= scaleFactor;
            button.gameObject.SetActive(!button.isActiveAndEnabled);
        }

        foreach (var arrange in HudManager.Instance.transform.FindChild("Buttons").GetComponentsInChildren<GridArrange>(true))
        {
            if (arrange.gameObject == null || arrange.gameObject.transform == null) continue;

            arrange.gameObject.SetActive(!arrange.isActiveAndEnabled);
            arrange.CellSize = new Vector2(scaleFactor, scaleFactor);
            arrange.gameObject.SetActive(!arrange.isActiveAndEnabled);
            if (arrange.gameObject.transform.childCount != 0) arrange.ArrangeChilds();
        }
    }

    private static void AdjustCameraSize(float size)
    {
        Camera.main!.orthographicSize = size;
        foreach (var cam in Camera.allCameras) cam.orthographicSize = Camera.main!.orthographicSize;
        ResolutionManager.ResolutionChanged.Invoke((float)Screen.width / Screen.height, Screen.width, Screen.height, Screen.fullScreen);
        
        if (size <= 3f)
        {
            Zooming = false;
            HudManager.Instance.ShadowQuad.gameObject.SetActive(!PlayerControl.LocalPlayer.Data.IsDead);
        }
        else
        {
            Zooming = true;
            HudManager.Instance.ShadowQuad.gameObject.SetActive(false);
        }
        
        ZoomButton.transform.Find("Inactive").GetComponent<SpriteRenderer>().sprite = Zooming ? TouAssets.ZoomPlus.LoadAsset() : TouAssets.ZoomMinus.LoadAsset();
        ZoomButton.transform.Find("Active").GetComponent<SpriteRenderer>().sprite = Zooming ? TouAssets.ZoomPlusActive.LoadAsset() : TouAssets.ZoomMinusActive.LoadAsset();
    }

    private static void ButtonClickZoom()
    {
        if (MeetingHud.Instance || ExileController.Instance)
        {
            ZoomButton.SetActive(false);
            return;
        }
        
        AdjustCameraSize(!Zooming ? 12f : 3f);
    }

    private static void ScrollZoom(bool zoomOut = false)
    {
        if (MeetingHud.Instance || ExileController.Instance)
        {
            ZoomButton.SetActive(false);
            return;
        }

        var size = Camera.main!.orthographicSize;
        size = zoomOut ? size * 1.25f : size / 1.25f;
        size = Mathf.Clamp(size, 3, 15);
        
        AdjustCameraSize(size);
    }

    public static void ResetZoom()
    {
        ZoomButton.SetActive(false);
        
        AdjustCameraSize(3f);
    }

    private static void CheckForScrollZoom()
    {
        var scrollWheel = Input.GetAxis("Mouse ScrollWheel");
        var axisRaw = ConsoleJoystick.player.GetAxisRaw(55);

        if (scrollWheel > 0 || axisRaw > 0)
        {
            ScrollZoom();
        }
        else if (scrollWheel < 0 || axisRaw < 0)
        {
            ScrollZoom(true);
        }
    }

    private static void UpdateTeamChat()
    {
        var genOpt = OptionGroupSingleton<GeneralOptions>.Instance;

        var isValid = MeetingHud.Instance &&
            (PlayerControl.LocalPlayer.IsJailed() || PlayerControl.LocalPlayer.Data.Role is JailorRole ||
            (PlayerControl.LocalPlayer.IsImpostor() && genOpt is { FFAImpostorMode: false, ImpostorChat.Value: true }) ||
            (PlayerControl.LocalPlayer.Data.Role is VampireRole && genOpt.VampireChat));

        if (!TeamChatButton) return;
        
        TeamChatButton.SetActive(isValid);
        var aspectPosition = TeamChatButton.GetComponentInChildren<AspectPosition>();
        var distanceFromEdge = aspectPosition.DistanceFromEdge;
        distanceFromEdge.x = HudManager.Instance.Chat.isActiveAndEnabled ? 2.73f : 2.15f;
        distanceFromEdge.y = 0.485f;
        aspectPosition.DistanceFromEdge = distanceFromEdge;
        aspectPosition.AdjustPosition();
        TeamChatButton.transform.Find("Selected").gameObject.SetActive(false);

        if (!TeamChatPatches.TeamChatActive) return;

        TeamChatButton.transform.Find("Inactive").gameObject.SetActive(false);
        TeamChatButton.transform.Find("Active").gameObject.SetActive(false);
        TeamChatButton.transform.Find("Selected").gameObject.SetActive(true);
    }

    private static void UpdateCamouflageComms()
    {
        var genOpt = OptionGroupSingleton<GeneralOptions>.Instance;

        // Camo comms
        if (!genOpt.CamouflageComms) return;

        if (!ShipStatus.Instance.Systems.TryGetValue(SystemTypes.Comms, out var commsSystem) || commsSystem == null)
            return;

        var isActive = false;
        if (ShipStatus.Instance.Type == ShipStatus.MapType.Hq || ShipStatus.Instance.Type == ShipStatus.MapType.Fungle)
        {
            var hqSystem = commsSystem.Cast<HqHudSystemType>();
            if (hqSystem != null) isActive = hqSystem.IsActive;
        }
        else
        {
            var hudSystem = commsSystem.Cast<HudOverrideSystemType>();
            if (hudSystem != null) isActive = hudSystem.IsActive;
        }

        foreach (var player in PlayerControl.AllPlayerControls)
        {
            var appearanceType = player.GetAppearanceType();
            if (isActive)
            {
                if (appearanceType != TownOfUsAppearances.Swooper)
                {
                    player.SetCamouflage(true);
                }
            }
            else
            {
                if (appearanceType == TownOfUsAppearances.Camouflage && !player.HasModifier<VenererCamouflageModifier>() && !PlayerControl.LocalPlayer.IsHysteria())
                {
                    player.SetCamouflage(false);
                }
            }
        }
    }

    private static void UpdateColorNameText()
    {
        if (MeetingHud.Instance)
        {
            foreach (var colorBlindName in MeetingHud.Instance.playerStates.Select(playerVA => playerVA.ColorBlindName))
            {
                colorBlindName.text = colorBlindName.text.ToTitleCase();
            }
        }
        else
        {
            foreach (var player in PlayerControl.AllPlayerControls)
            {
                player.cosmetics.colorBlindText.text = player.cosmetics.colorBlindText.text.ToTitleCase();
            }
        }
    }

    private static void UpdateRoleNameText()
    {
        var genOpt = OptionGroupSingleton<GeneralOptions>.Instance;
        var taskOpt = OptionGroupSingleton<TaskTrackingOptions>.Instance;

        if (MeetingHud.Instance)
        {
            foreach (var playerVA in MeetingHud.Instance.playerStates)
            {
                var player = MiscUtils.PlayerById(playerVA.TargetPlayerId);
                playerVA.ColorBlindName.transform.localPosition = new Vector3(-0.93f, -0.2f, -0.1f);

                if (player == null) continue;

                var playerName = player.GetDefaultAppearance().PlayerName ?? "Unknown";
                var playerColor = Color.white;

                if (PlayerControl.LocalPlayer.IsImpostor() && player.IsImpostor() && PlayerControl.LocalPlayer != player && !genOpt.FFAImpostorMode)
                    playerColor = Color.red;

                playerColor = playerColor.UpdateTargetColor(player);
                playerName = playerName.UpdateTargetSymbols(player);
                playerName = playerName.UpdateProtectionSymbols(player);
                playerName = playerName.UpdateAllianceSymbols(player);
                playerName = playerName.UpdateStatusSymbols(player);

                var role = player.Data.Role;

                if (role == null) continue;

                var color = role.TeamColor;

                if (HaunterRole.HaunterVisibilityFlag(player))
                    playerColor = color;

                color = Color.white;

                var roleName = "";

                if (player.AmOwner ||
                    (PlayerControl.LocalPlayer.IsImpostor() && player.IsImpostor() && genOpt is { ImpsKnowRoles.Value: true, FFAImpostorMode: false }) ||
                    (PlayerControl.LocalPlayer.GetRoleWhenAlive() && role is VampireRole) ||
                    SnitchRole.SnitchVisibilityFlag(player, true) ||
                    (PlayerControl.LocalPlayer.HasDied() && genOpt.TheDeadKnow) ||
                    GuardianAngelTouRole.GASeesRoleVisibilityFlag(player) ||
                    SleuthModifier.SleuthVisibilityFlag(player) ||
                    MayorRole.MayorVisibilityFlag(player))
                {
                    color = role.TeamColor;
                    roleName = $"<size=80%>{color.ToTextColor()}{player.Data.Role.NiceName}</color></size>";
                    if (!player.HasModifier<VampireBittenModifier>() && player.Data.Role is VampireRole) roleName += "<size=80%><color=#FFFFFF> (<color=#A22929>OG</color>)</color></size>";

                    var cachedMod = player.GetModifiers<BaseModifier>().FirstOrDefault(x => x is ICachedRole);
                    if (cachedMod is ICachedRole cache && cache.Visible && player.Data.Role.GetType() != cache.CachedRole.GetType())
                    {
                        roleName = cache.ShowCurrentRoleFirst ? $"<size=80%>{color.ToTextColor()}{player.Data.Role.NiceName}</color> ({cache.CachedRole.TeamColor.ToTextColor()}{cache.CachedRole.NiceName}</color>)</size>" :
                            $"<size=80%>{cache.CachedRole.TeamColor.ToTextColor()}{cache.CachedRole.NiceName}</color> ({color.ToTextColor()}{player.Data.Role.NiceName}</color>)</size>";
                    }
                    // Guardian Angel here is vanilla's GA, NOT Town of Us GA
                    if (player.Data.IsDead && role is GuardianAngelRole gaRole) roleName = $"<size=80%>{gaRole.TeamColor.ToTextColor()}{gaRole?.NiceName}</color></size>";
                    if (SleuthModifier.SleuthVisibilityFlag(player) || (player.Data.IsDead && role is not PhantomTouRole or GuardianAngelRole or HaunterRole))
                    {
                        var roleWhenAlive = player.GetRoleWhenAlive();
                        color = roleWhenAlive.TeamColor;

                        roleName = $"<size=80%>{color.ToTextColor()}{roleWhenAlive.NiceName}</color></size>";
                        if (!player.HasModifier<VampireBittenModifier>() && roleWhenAlive is VampireRole) roleName += "<size=80%><color=#FFFFFF> (<color=#A22929>OG</color>)</color></size>";
                    }
                }

                if (((taskOpt.ShowTaskInMeetings && player.AmOwner) || (PlayerControl.LocalPlayer.HasDied() && taskOpt.ShowTaskDead)) && (player.IsCrewmate() || player.Data.Role is PhantomTouRole))
                {
                    var completed = player.myTasks.ToArray().Count(x => x.IsComplete);
                    var totalTasks = player.myTasks.ToArray().Count(x => !PlayerTask.TaskIsEmergency(x) && !x.TryCast<ImportantTextTask>());
                    roleName += $" <size=80%>{Color.yellow.ToTextColor()}({completed}/{totalTasks})</color></size>";
                }

                if (player.TryGetModifier<OracleConfessModifier>(out var confess, x => x.ConfessToAll))
                {
                    var accuracy = OptionGroupSingleton<OracleOptions>.Instance.RevealAccuracyPercentage;
                    var revealText = confess.RevealedFaction switch
                    {
                        ModdedRoleTeams.Crewmate => $"\n<size=75%>{Palette.CrewmateBlue.ToTextColor()}({accuracy}% Crew) </color></size>",
                        ModdedRoleTeams.Custom => $"\n<size=75%>{TownOfUsColors.Neutral.ToTextColor()}({accuracy}% Neut) </color></size>",
                        ModdedRoleTeams.Impostor => $"\n<size=75%>{TownOfUsColors.ImpSoft.ToTextColor()}({accuracy}% Imp) </color></size>",
                        _ => string.Empty,
                    };

                    playerName += revealText;
                }
                if (SnitchRole.SnitchVisibilityFlag(player)) playerColor = TownOfUsColors.Impostor;

                if (player?.Data?.Disconnected == true)
                {
                    if (!((PlayerControl.LocalPlayer.IsImpostor() && player.IsImpostor() && genOpt is { ImpsKnowRoles.Value: true, FFAImpostorMode: false }) ||
                    (PlayerControl.LocalPlayer.GetRoleWhenAlive() is VampireRole && role is VampireRole) ||
                    SnitchRole.SnitchVisibilityFlag(player, true) ||
                    !TutorialManager.InstanceExists && ((PlayerControl.LocalPlayer.HasDied() && genOpt.TheDeadKnow) ||
                    GuardianAngelTouRole.GASeesRoleVisibilityFlag(player) ||
                    SleuthModifier.SleuthVisibilityFlag(player) ||
                    MayorRole.MayorVisibilityFlag(player))))
                    {
                        roleName = "";
                        color = Color.white;
                        playerColor = Color.white;
                    }

                    var dash = "";
                    if (!string.IsNullOrEmpty(roleName))
                    {
                        dash = " - ";
                    }

                    roleName = $"{roleName}<size=80%>{dash}Disconnected</size>";
                }

                if (!string.IsNullOrEmpty(roleName))
                {
                    if (TownOfUsPlugin.ColorPlayerName.Value) playerName = $"{roleName}\n{color.ToTextColor()}{playerName}</color>";
                    else playerName = $"{roleName}\n{playerName}";
                }

                playerVA.NameText.text = playerName;
                playerVA.NameText.color = playerColor;
            }
        }
        else
        {
            var body = Object.FindObjectsOfType<DeadBody>().FirstOrDefault(x => x.ParentId == PlayerControl.LocalPlayer.PlayerId && !TutorialManager.InstanceExists);
            var fakePlayer = FakePlayer.FakePlayers.FirstOrDefault(x => x?.PlayerId == PlayerControl.LocalPlayer.PlayerId && !TutorialManager.InstanceExists);

            foreach (var player in PlayerControl.AllPlayerControls)
            {
                var playerName = player.GetAppearance().PlayerName ?? "Unknown";
                var playerColor = Color.white;

                if (PlayerControl.LocalPlayer.IsImpostor() && player.IsImpostor() && PlayerControl.LocalPlayer != player && !genOpt.FFAImpostorMode)
                    playerColor = Color.red;

                playerColor = playerColor.UpdateTargetColor(player, body || fakePlayer?.body);
                playerName = playerName.UpdateTargetSymbols(player, body || fakePlayer?.body);
                playerName = playerName.UpdateProtectionSymbols(player, body || fakePlayer?.body);
                playerName = playerName.UpdateAllianceSymbols(player, body || fakePlayer?.body);
                playerName = playerName.UpdateStatusSymbols(player, body || fakePlayer?.body);

                var role = player.Data.Role;
                var color = Color.white;

                if (role == null) continue;

                var roleName = "";

                if (player.AmOwner ||
                    (PlayerControl.LocalPlayer.IsImpostor() && player.IsImpostor() && genOpt is { ImpsKnowRoles.Value: true, FFAImpostorMode: false }) ||
                    (PlayerControl.LocalPlayer.GetRoleWhenAlive() is VampireRole && role is VampireRole) ||
                    SnitchRole.SnitchVisibilityFlag(player, true) ||
                    (PlayerControl.LocalPlayer.HasDied() && genOpt.TheDeadKnow && !body && !fakePlayer?.body) ||
                    GuardianAngelTouRole.GASeesRoleVisibilityFlag(player) ||
                    MayorRole.MayorVisibilityFlag(player))
                {
                    color = role.TeamColor;
                    roleName = $"<size=80%>{color.ToTextColor()}{player.Data.Role.NiceName}</color></size>";
                    if (!player.HasModifier<VampireBittenModifier>() && player.Data.Role is VampireRole) roleName += "<size=80%><color=#FFFFFF> (<color=#A22929>OG</color>)</color></size>";

                    var cachedMod = player.GetModifiers<BaseModifier>().FirstOrDefault(x => x is ICachedRole);
                    if (cachedMod is ICachedRole cache && cache.Visible && player.Data.Role.GetType() != cache.CachedRole.GetType())
                    {
                        roleName = cache.ShowCurrentRoleFirst ? $"<size=80%>{color.ToTextColor()}{player.Data.Role.NiceName}</color> ({cache.CachedRole.TeamColor.ToTextColor()}{cache.CachedRole.NiceName}</color>)</size>" 
                            : $"<size=80%>{cache.CachedRole.TeamColor.ToTextColor()}{cache.CachedRole.NiceName}</color> ({color.ToTextColor()}{player.Data.Role.NiceName}</color>)</size>";
                    }
                    // Guardian Angel here is vanilla's GA, NOT Town of Us GA
                    if (player.Data.IsDead && role is GuardianAngelRole gaRole) roleName = $"<size=80%>{gaRole.TeamColor.ToTextColor()}{gaRole.NiceName}</color></size>";
                    if (SleuthModifier.SleuthVisibilityFlag(player) || (player.Data.IsDead && role is not PhantomTouRole or GuardianAngelRole or HaunterRole))
                    {
                        var roleWhenAlive = player.GetRoleWhenAlive();
                        color = roleWhenAlive.TeamColor;

                        roleName = $"<size=80%>{color.ToTextColor()}{roleWhenAlive.NiceName}</color></size>";
                        if (!player.HasModifier<VampireBittenModifier>() && roleWhenAlive is VampireRole) roleName += "<size=80%><color=#FFFFFF> (<color=#A22929>OG</color>)</color></size>";
                    }
                }

                if (((taskOpt.ShowTaskRound && player.AmOwner) || (PlayerControl.LocalPlayer.HasDied() && taskOpt.ShowTaskDead && !body && !fakePlayer?.body)) && (player.IsCrewmate() || player.Data.Role is PhantomTouRole))
                {
                    var completed = player.myTasks.ToArray().Count(x => x.IsComplete);
                    var totalTasks = player.myTasks.ToArray().Count(x => !PlayerTask.TaskIsEmergency(x) && !x.TryCast<ImportantTextTask>());

                    roleName += $" <size=80%>{Color.yellow.ToTextColor()}({completed}/{totalTasks})</color></size>";
                }

                if (player.AmOwner && player.TryGetModifier<ScatterModifier>(out var scatter) && !player.HasDied())
                {
                    roleName += $" - {scatter.GetDescription()}";
                }

                if (player.AmOwner && player.Data.Role is IGhostRole { GhostActive: true })
                {
                    playerColor = Color.clear;
                }
                if (SnitchRole.SnitchVisibilityFlag(player)) playerColor = TownOfUsColors.Impostor;

                if (!string.IsNullOrEmpty(roleName))
                {
                    playerName = TownOfUsPlugin.ColorPlayerName.Value ? $"{roleName}\n{color.ToTextColor()}{playerName}</color>" : $"{roleName}\n{playerName}";
                }

                player.cosmetics.nameText.text = playerName;
                player.cosmetics.nameText.color = playerColor;
                player.cosmetics.nameText.transform.localPosition = new Vector3(0f, 0.15f, -0.5f);
            }
        }
    }

    private static void UpdateGhostRoles(HudManager instance)
    {
        foreach (var phantom in CustomRoleUtils.GetActiveRolesOfType<PhantomTouRole>())
        {
            if (phantom.Player.Data != null && phantom.Player.Data.Disconnected) continue;
            phantom.FadeUpdate(instance);
        }

        foreach (var haunter in CustomRoleUtils.GetActiveRolesOfType<HaunterRole>())
        {
            if (haunter.Player.Data != null && haunter.Player.Data.Disconnected) continue;
            haunter.FadeUpdate(instance);
        }
    }

    private static string GetRoleForSlot(int slotValue)
    {
        var roleListText = RoleOptions.OptionStrings.ToList();
        if (slotValue >= 0 && slotValue < roleListText.Count)
        {
            return roleListText[slotValue];
        }
        else
        {
            return "<color=#696969>Unknown</color>";
        }
    }

    private static void UpdateRoleList(HudManager instance)
    {
        if (!LobbyBehaviour.Instance)
        {
            if (RoleList) RoleList.SetActive(false);
            return;
        }

        if (!RoleList)
        {
            var pingTracker = UnityEngine.Object.FindObjectOfType<PingTracker>(true);
            RoleList = UnityEngine.Object.Instantiate(pingTracker.gameObject, instance.transform);
            RoleList.name = "RoleListText";
            //RoleList.GetComponent<AspectPosition>().DistanceFromEdge = new Vector3(-4.9f, 5.9f);
            RoleList.SetActive(false);
            var pos = RoleList.gameObject.GetComponent<AspectPosition>();
            pos.Alignment = AspectPosition.EdgeAlignments.LeftTop;
            pos.DistanceFromEdge = new Vector3(0.43f, 0.1f, 1f);
        }
        else
        {
            RoleList.SetActive(false);
            var objText = RoleList.GetComponent<TextMeshPro>();
            var rolelistBuilder = new StringBuilder();

            var players = GameData.Instance.PlayerCount;
            int maxSlots = players < 15 ? players : 15;

            var list = OptionGroupSingleton<RoleOptions>.Instance;
            if (list.RoleListEnabled)
            {
                for (int i = 0; i < maxSlots; i++)
                {
                    int slotValue = i switch
                    {
                        0 => list.Slot1,
                        1 => list.Slot2,
                        2 => list.Slot3,
                        3 => list.Slot4,
                        4 => list.Slot5,
                        5 => list.Slot6,
                        6 => list.Slot7,
                        7 => list.Slot8,
                        8 => list.Slot9,
                        9 => list.Slot10,
                        10 => list.Slot11,
                        11 => list.Slot12,
                        12 => list.Slot13,
                        13 => list.Slot14,
                        14 => list.Slot15,
                        _ => -1
                    };

                    rolelistBuilder.AppendLine(GetRoleForSlot(slotValue));
                    objText.text = $"<color=#FFD700>Set Role List:</color>\n{rolelistBuilder}";
                }
            }
            else
            {
                rolelistBuilder.AppendLine(System.Globalization.CultureInfo.InvariantCulture, $"<color=#999999>Neutral</color> Benigns: {list.MinNeutralBenign.Value} Min, {list.MaxNeutralBenign.Value} Max");
                rolelistBuilder.AppendLine(System.Globalization.CultureInfo.InvariantCulture, $"<color=#999999>Neutral</color> Evils: {list.MinNeutralEvil.Value} Min, {list.MaxNeutralEvil.Value} Max");
                rolelistBuilder.AppendLine(System.Globalization.CultureInfo.InvariantCulture, $"<color=#999999>Neutral</color> Killers: {list.MinNeutralKiller.Value} Min, {list.MaxNeutralKiller.Value} Max");
                objText.text = $"<color=#FFD700>Neutral Faction List:</color>\n{rolelistBuilder}";
            }

            objText.alignment = TextAlignmentOptions.TopLeft;
            objText.verticalAlignment = VerticalAlignmentOptions.Top;
            objText.fontSize = objText.fontSizeMin = objText.fontSizeMax = 3f;

            RoleList.SetActive(true);
        }
    }

    private static void CreateZoomButton(HudManager instance)
    {
        var isChatButtonVisible = HudManager.Instance.Chat.isActiveAndEnabled;

        if (!ZoomButton)
        {
            ZoomButton = Object.Instantiate(instance.MapButton.gameObject, instance.MapButton.transform.parent);
            ZoomButton.GetComponent<PassiveButton>().OnClick = new();
            ZoomButton.GetComponent<PassiveButton>().OnClick.AddListener(new Action(ButtonClickZoom));
            ZoomButton.name = "Zoom";
            ZoomButton.transform.Find("Background").localPosition = Vector3.zero;
            ZoomButton.transform.Find("Inactive").GetComponent<SpriteRenderer>().sprite =
                TouAssets.ZoomMinus.LoadAsset();
            ZoomButton.transform.Find("Active").GetComponent<SpriteRenderer>().sprite =
                TouAssets.ZoomMinusActive.LoadAsset();
        }

        if (ZoomButton)
        {
            var aspectPosition = ZoomButton.GetComponentInChildren<AspectPosition>();
            var distanceFromEdge = aspectPosition.DistanceFromEdge;
            distanceFromEdge.x = isChatButtonVisible ? 2.73f : 2.15f;
            distanceFromEdge.y = 0.485f;
            aspectPosition.DistanceFromEdge = distanceFromEdge;
            aspectPosition.AdjustPosition();
        }
    }

    private static void CreateTeamChatButton(HudManager instance)
    {
        if (TeamChatButton) return;

        TeamChatButton = Object.Instantiate(instance.MapButton.gameObject, instance.MapButton.transform.parent);
        TeamChatButton.GetComponent<PassiveButton>().OnClick = new Button.ButtonClickedEvent();
        TeamChatButton.GetComponent<PassiveButton>().OnClick.AddListener(new Action(TeamChatPatches.ToggleTeamChat));
        TeamChatButton.name = "FactionChat";
        TeamChatButton.transform.Find("Background").localPosition = Vector3.zero;
        TeamChatButton.transform.Find("Inactive").GetComponent<SpriteRenderer>().sprite = TouAssets.TeamChatInactive.LoadAsset();
        TeamChatButton.transform.Find("Active").GetComponent<SpriteRenderer>().sprite = TouAssets.TeamChatActive.LoadAsset();
        TeamChatButton.transform.Find("Selected").GetComponent<SpriteRenderer>().sprite = TouAssets.TeamChatSelected.LoadAsset();
    }

    private static void CreateWikiButton(HudManager instance)
    {
        var isChatButtonVisible = HudManager.Instance.Chat.isActiveAndEnabled;

        if (!WikiButton)
        {
            WikiButton = Object.Instantiate(instance.MapButton.gameObject, instance.MapButton.transform.parent);
            WikiButton.GetComponent<PassiveButton>().OnClick = new();
            WikiButton.GetComponent<PassiveButton>().OnClick.AddListener((UnityAction)(() =>
            {
                if (Minigame.Instance)
                {
                    return;
                }

                IngameWikiMinigame.Create().Begin(null);
            }));

            WikiButton.transform.Find("Background").localPosition = Vector3.zero;
            WikiButton.transform.Find("Inactive").GetComponent<SpriteRenderer>().sprite =
                TouAssets.WikiButton.LoadAsset();
            WikiButton.transform.Find("Active").GetComponent<SpriteRenderer>().sprite =
                TouAssets.WikiButtonActive.LoadAsset();
        }

        if (WikiButton)
        {
            var aspectPosition = WikiButton.GetComponentInChildren<AspectPosition>();
            var distanceFromEdge = aspectPosition.DistanceFromEdge;
            distanceFromEdge.x = isChatButtonVisible ? 2.73f : 2.15f;
            
            if (((ModCompatibility.IsWikiButtonOffset && !ZoomButton.active) || ZoomButton.active) && (!MeetingHud.Instance) && Minigame.Instance == null && (PlayerJoinPatch.SentOnce || TutorialManager.InstanceExists)) distanceFromEdge.x += 0.84f;
            
            if (TeamChatButton.active) distanceFromEdge.x += 0.84f;

            distanceFromEdge.y = 0.485f;
            WikiButton.SetActive(true);
            aspectPosition.DistanceFromEdge = distanceFromEdge;
            aspectPosition.AdjustPosition();
        }
    }

    [HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
    [HarmonyPostfix]
    public static void HudManagerUpdatePatch(HudManager __instance)
    {
        CreateZoomButton(__instance);
        CreateTeamChatButton(__instance);
        CreateWikiButton(__instance);

        UpdateRoleList(__instance);

        if (PlayerControl.LocalPlayer != null)
        {
            UpdateColorNameText();
        }

        if (PlayerControl.LocalPlayer == null ||
            PlayerControl.LocalPlayer.Data == null ||
            PlayerControl.LocalPlayer.Data.Role == null ||
            !ShipStatus.Instance ||
            (AmongUsClient.Instance.GameState != InnerNet.InnerNetClient.GameStates.Started &&
             !TutorialManager.InstanceExists))
        {
            return;
        }
            
        // TERRIBLE FOR PERFORMANCE (FindObjectsOfType is very costly)
        var body = Object.FindObjectsOfType<DeadBody>().FirstOrDefault(x => x.ParentId == PlayerControl.LocalPlayer.PlayerId);
        var fakePlayer = FakePlayer.FakePlayers.FirstOrDefault(x => x?.PlayerId == PlayerControl.LocalPlayer.PlayerId);

        if (((PlayerControl.LocalPlayer.Data.IsDead && !body && !fakePlayer?.body && (PlayerControl.LocalPlayer.Data.Role is IGhostRole { Caught: true } || PlayerControl.LocalPlayer.Data.Role is not IGhostRole)) || TutorialManager.InstanceExists)
            && Input.GetAxis("Mouse ScrollWheel") != 0 && !MeetingHud.Instance && Minigame.Instance == null && !HudManager.Instance.Chat.IsOpenOrOpening)
        {
            CheckForScrollZoom();
        }

        UpdateTeamChat();
        UpdateCamouflageComms();
        UpdateRoleNameText();
        UpdateGhostRoles(__instance);
    }

    [HarmonyPatch(typeof(HudManager), nameof(HudManager.Start))]
    [HarmonyPostfix]
    public static void HudManagerStartPatch(HudManager __instance)
    {
        Coroutines.Start(CoResizeUI());
    }
}

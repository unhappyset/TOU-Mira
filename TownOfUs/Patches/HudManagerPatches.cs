using HarmonyLib;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Roles;
using TownOfUs.Modifiers;
using TownOfUs.Modifiers.Crewmate;
using TownOfUs.Modifiers.Game.Universal;
using TownOfUs.Modifiers.Game.Alliance;
using TownOfUs.Modifiers.Impostor;
using TownOfUs.Modifiers.Impostor.Venerer;
using TownOfUs.Modifiers.Neutral;
using TownOfUs.Modules;
using TownOfUs.Modules.Wiki;
using TownOfUs.Options;
using TownOfUs.Options.Roles.Crewmate;
using TownOfUs.Options.Roles.Neutral;
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
using TownOfUs.Roles.Impostor;

namespace TownOfUs.Patches;

[HarmonyPatch]
public static class HudManagerPatches
{
    public static GameObject ZoomButton;
    public static GameObject WikiButton;
    public static GameObject RoleList;
    public static GameObject TeamChatButton;
    public static bool Zooming;
    public static void ResizeUI(float scaleFactor)
    {
        foreach (AspectPosition aspect in HudManager.Instance.transform.FindChild("Buttons").GetComponentsInChildren<AspectPosition>(true))
        {
            if (aspect.gameObject.transform.parent.name == "TopRight") continue;
            if (aspect.gameObject.transform.parent.transform.parent.name == "TopRight") continue;
            var wasActive = aspect.isActiveAndEnabled;
            aspect.gameObject.SetActive(false);
            aspect.DistanceFromEdge *= new Vector2(scaleFactor, scaleFactor);
            aspect.gameObject.SetActive(wasActive);
        }
        
        foreach (ActionButton button in HudManager.Instance.GetComponentsInChildren<ActionButton>(true))
        {
            var wasActive = button.isActiveAndEnabled;
            button.gameObject.SetActive(false);
            button.gameObject.transform.localScale *= scaleFactor;
            button.gameObject.SetActive(wasActive);
        }
        foreach (GridArrange arrange in HudManager.Instance.transform.FindChild("Buttons").GetComponentsInChildren<GridArrange>(true))
        {
            var wasActive = arrange.isActiveAndEnabled;
            arrange.gameObject.SetActive(false);
            arrange.CellSize *= new Vector2(scaleFactor, scaleFactor);
            arrange.gameObject.SetActive(wasActive);
        }
    }
    public static void Zoom()
    {
        Zooming = !Zooming;
        var size = Zooming ? 12f : 3f;
        ZoomButton.transform.Find("Inactive").GetComponent<SpriteRenderer>().sprite = Zooming ? TouAssets.ZoomPlus.LoadAsset() : TouAssets.ZoomMinus.LoadAsset();
        ZoomButton.transform.Find("Active").GetComponent<SpriteRenderer>().sprite = Zooming ? TouAssets.ZoomPlusActive.LoadAsset() : TouAssets.ZoomMinusActive.LoadAsset();

        Camera.main.orthographicSize = size;
        HudManager.Instance.UICamera.orthographicSize = size;
        HudManager.Instance.ShadowQuad.transform.localScale = new Vector3(10.6667f, 6f, 0f) * (size / 3f);
        if (GameObject.Find("ShadowCamera").TryGetComponent<Camera>(out var shadowCam)) shadowCam.orthographicSize = size;
        ResolutionManager.ResolutionChanged.Invoke((float)Screen.width / Screen.height, Screen.width, Screen.height, Screen.fullScreen);
    }
    public static void ScrollZoom(bool zoomOut = false)
    {
        var size = Camera.main.orthographicSize;
        if (zoomOut) size *= 1.25f;
        else size /= 1.25f;
        if (size != 3f) Zooming = true;
        if (size <= 3f)
        {
            Zooming = false;
            size = 3f;
        }
        else if (size >= 12f)
        {
            Zooming = true;
        }
        if (size >= 15f) size = 15f;
        ZoomButton.transform.Find("Inactive").GetComponent<SpriteRenderer>().sprite = Zooming ? TouAssets.ZoomPlus.LoadAsset() : TouAssets.ZoomMinus.LoadAsset();
        ZoomButton.transform.Find("Active").GetComponent<SpriteRenderer>().sprite = Zooming ? TouAssets.ZoomPlusActive.LoadAsset() : TouAssets.ZoomMinusActive.LoadAsset();

        Camera.main.orthographicSize = size;
        HudManager.Instance.UICamera.orthographicSize = size;
        HudManager.Instance.ShadowQuad.transform.localScale = new Vector3(10.6667f, 6f, 0f) * (size / 3f);
        if (GameObject.Find("ShadowCamera").TryGetComponent<Camera>(out var shadowCam)) shadowCam.orthographicSize = size;
        ResolutionManager.ResolutionChanged.Invoke((float)Screen.width / Screen.height, Screen.width, Screen.height, Screen.fullScreen);
    }
    public static void ResetZoom()
    {
        Zooming = false;
        var size = 3f;
        ZoomButton.transform.Find("Inactive").GetComponent<SpriteRenderer>().sprite = TouAssets.ZoomMinus.LoadAsset();
        ZoomButton.transform.Find("Active").GetComponent<SpriteRenderer>().sprite = TouAssets.ZoomMinusActive.LoadAsset();
        ZoomButton.SetActive(false);

        Camera.main.orthographicSize = size;
        HudManager.Instance.UICamera.orthographicSize = size;
        HudManager.Instance.ShadowQuad.transform.localScale = new Vector3(10.6667f, 6f, 0f);
        if (GameObject.Find("ShadowCamera").TryGetComponent<Camera>(out var shadowCam)) shadowCam.orthographicSize = size;
        ResolutionManager.ResolutionChanged.Invoke((float)Screen.width / Screen.height, Screen.width, Screen.height, Screen.fullScreen);
    }

    public static void CheckForScrollZoom()
    {
        if (Input.GetAxis("Mouse ScrollWheel") > 0)
        {
            ScrollZoom();
        }
        else if (Input.GetAxis("Mouse ScrollWheel") < 0)
        {
            ScrollZoom(true);
        }
    }

    [HarmonyPatch(typeof(HudManager), nameof(HudManager.Start))]
    [HarmonyPostfix]
    public static void HudManagerStartPatch(HudManager __instance)
    {
        ResizeUI(TownOfUsPlugin.ButtonUIFactor.Value);
    }
    [HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
    [HarmonyPostfix]
    public static void HudManagerUpdatePatch(HudManager __instance)
    {
        var isChatButtonVisible = HudManager.Instance.Chat.isActiveAndEnabled;

        if (!ZoomButton)
        {
            ZoomButton =
                UnityEngine.Object.Instantiate(__instance.MapButton.gameObject, __instance.MapButton.transform.parent);
            ZoomButton.GetComponent<PassiveButton>().OnClick = new();
            ZoomButton.GetComponent<PassiveButton>().OnClick.AddListener(new Action(Zoom));
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

        if (!TeamChatButton)
        {
            TeamChatButton =
                UnityEngine.Object.Instantiate(__instance.MapButton.gameObject, __instance.MapButton.transform.parent);
            TeamChatButton.GetComponent<PassiveButton>().OnClick = new();
            TeamChatButton.GetComponent<PassiveButton>().OnClick.AddListener(new Action(TeamChatPatches.ToggleTeamChat));
            TeamChatButton.name = "FactionChat";
            TeamChatButton.transform.Find("Background").localPosition = Vector3.zero;
            TeamChatButton.transform.Find("Inactive").GetComponent<SpriteRenderer>().sprite = TouAssets.TeamChatInactive.LoadAsset();
            TeamChatButton.transform.Find("Active").GetComponent<SpriteRenderer>().sprite = TouAssets.TeamChatActive.LoadAsset();
            TeamChatButton.transform.Find("Selected").GetComponent<SpriteRenderer>().sprite = TouAssets.TeamChatSelected.LoadAsset();
        }

        if (!WikiButton)
        {
            WikiButton =
                UnityEngine.Object.Instantiate(__instance.MapButton.gameObject, __instance.MapButton.transform.parent);
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
            if (((ModCompatibility.IsWikiButtonOffset && !ZoomButton.active) || ZoomButton.active) && (MeetingHud.Instance == null) && Minigame.Instance == null && (PlayerJoinPatch.SentOnce || TutorialManager.InstanceExists)) distanceFromEdge.x += 0.84f;
            if (TeamChatButton.active) distanceFromEdge.x += 0.84f;
            distanceFromEdge.y = 0.485f;
            WikiButton.SetActive(true);
            aspectPosition.DistanceFromEdge = distanceFromEdge;
            aspectPosition.AdjustPosition();
        }

        UpdateRoleList(__instance);
        if (PlayerControl.LocalPlayer != null) UpdateColorNameText(__instance);

        if (PlayerControl.LocalPlayer == null ||
            PlayerControl.LocalPlayer.Data == null ||
            PlayerControl.LocalPlayer.Data.Role == null ||
            !ShipStatus.Instance ||
            (AmongUsClient.Instance.GameState != InnerNet.InnerNetClient.GameStates.Started &&
             !TutorialManager.InstanceExists))
        {
            return;
        }
            var body = GameObject.FindObjectsOfType<DeadBody>().FirstOrDefault(x => x.ParentId == PlayerControl.LocalPlayer.PlayerId);
            var fakePlayer = FakePlayer.FakePlayers.FirstOrDefault(x => x?.PlayerId == PlayerControl.LocalPlayer.PlayerId);

        if (((PlayerControl.LocalPlayer.Data.IsDead && !body && !fakePlayer?.body && (PlayerControl.LocalPlayer.Data.Role is IGhostRole { Caught: true } || PlayerControl.LocalPlayer.Data.Role is not IGhostRole)) || TutorialManager.InstanceExists)
            && Input.GetAxis("Mouse ScrollWheel") != 0 && MeetingHud.Instance == null && Minigame.Instance == null && !HudManager.Instance.Chat.IsOpenOrOpening)
        {
            CheckForScrollZoom();
        }
        UpdateTeamChat(__instance);
        UpdateCamouflageComms(__instance);
        UpdateRoleNameText(__instance);
        UpdateGhostRoles(__instance);
    }

    public static void UpdateTeamChat(HudManager instance)
    {
        var genOpt = OptionGroupSingleton<GeneralOptions>.Instance;

        var isValid = MeetingHud.Instance &&
            ((PlayerControl.LocalPlayer.HasDied() && genOpt.TheDeadKnow && (genOpt is { FFAImpostorMode: false, ImpostorChat.Value: true } || genOpt.VampireChat)) ||
            (PlayerControl.LocalPlayer.IsImpostor() && genOpt is { FFAImpostorMode: false, ImpostorChat.Value: true }) ||
            (PlayerControl.LocalPlayer.Data.Role is VampireRole && genOpt.VampireChat));

        if (TeamChatButton)
        {
            TeamChatButton.SetActive(isValid);
            var aspectPosition = TeamChatButton.GetComponentInChildren<AspectPosition>();
            var distanceFromEdge = aspectPosition.DistanceFromEdge;
            distanceFromEdge.x = HudManager.Instance.Chat.isActiveAndEnabled ? 2.73f : 2.15f;
            distanceFromEdge.y = 0.485f;
            aspectPosition.DistanceFromEdge = distanceFromEdge;
            aspectPosition.AdjustPosition();
            TeamChatButton.transform.Find("Selected").gameObject.SetActive(false);
            if (TeamChatPatches.TeamChatActive)
            { 
                TeamChatButton.transform.Find("Inactive").gameObject.SetActive(false);
                TeamChatButton.transform.Find("Active").gameObject.SetActive(false);
                TeamChatButton.transform.Find("Selected").gameObject.SetActive(true);
            }
        }
    }
    public static void UpdateCamouflageComms(HudManager instance)
    {
        var genOpt = OptionGroupSingleton<GeneralOptions>.Instance;

        // Camo comms
        if (genOpt.CamouflageComms)
        {
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
    }

    public static void UpdateColorNameText(HudManager instance)
    {
        if (MeetingHud.Instance != null)
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
    public static void UpdateRoleNameText(HudManager instance)
    {
        var genOpt = OptionGroupSingleton<GeneralOptions>.Instance;
        var taskopt = OptionGroupSingleton<TaskTrackingOptions>.Instance;

        if (MeetingHud.Instance != null)
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

                if (player.HasModifier<SeerGoodRevealModifier>() && PlayerControl.LocalPlayer.IsRole<SeerRole>())
                    playerColor = Color.green;
                else if (player.HasModifier<SeerEvilRevealModifier>() && PlayerControl.LocalPlayer.IsRole<SeerRole>())
                    playerColor = Color.red;

                if (player.HasModifier<PoliticianCampaignedModifier>(x => x.Politician == PlayerControl.LocalPlayer) && PlayerControl.LocalPlayer.IsRole<PoliticianRole>())
                    playerColor = Color.cyan;

                if ((player.HasModifier<ExecutionerTargetModifier>(x => x.OwnerId == PlayerControl.LocalPlayer.PlayerId) && PlayerControl.LocalPlayer.IsRole<ExecutionerRole>())
                || (player.HasModifier<ExecutionerTargetModifier>() && PlayerControl.LocalPlayer.HasDied() && genOpt.TheDeadKnow))
                    playerName += "<color=#643B1F> X</color>";

                if (player.HasModifier<InquisitorHereticModifier>() && PlayerControl.LocalPlayer.HasDied() && (genOpt.TheDeadKnow || PlayerControl.LocalPlayer.GetRoleWhenAlive() is InquisitorRole))
                    playerName += "<color=#D94291> $</color>";

                if (player.HasModifier<MercenaryBribedModifier>(x => x.Mercenary == PlayerControl.LocalPlayer) && PlayerControl.LocalPlayer.IsRole<MercenaryRole>())
                {
                    playerColor = Color.green;

                    if (player.Is(RoleAlignment.NeutralEvil) || player.IsRole<AmnesiacRole>() || player.IsRole<MercenaryRole>())
                        playerColor = Color.red;
                }

                if ((player.HasModifier<GuardianAngelTargetModifier>(x => x.OwnerId == PlayerControl.LocalPlayer.PlayerId) && PlayerControl.LocalPlayer.IsRole<GuardianAngelTouRole>())
                    || (player.HasModifier<GuardianAngelTargetModifier>() && ((PlayerControl.LocalPlayer.HasDied() && genOpt.TheDeadKnow)
                    || (player.AmOwner && OptionGroupSingleton<GuardianAngelOptions>.Instance.GATargetKnows))))
                {
                    playerName += "<color=#B3FFFF> ★</color>";
                }

                if ((player.HasModifier<MedicShieldModifier>(x => x.Medic == PlayerControl.LocalPlayer) && PlayerControl.LocalPlayer.IsRole<MedicRole>())
                    || (player.HasModifier<MedicShieldModifier>() && ((PlayerControl.LocalPlayer.HasDied() && genOpt.TheDeadKnow)
                    || (player.AmOwner && player.TryGetModifier<MedicShieldModifier>(out var med) && med.VisibleSymbol))))
                {
                    playerName += "<color=#006600> +</color>";
                }

                if ((player.HasModifier<ClericBarrierModifier>(x => x.Cleric == PlayerControl.LocalPlayer) && PlayerControl.LocalPlayer.IsRole<ClericRole>())
                    || (player.HasModifier<ClericBarrierModifier>() && ((PlayerControl.LocalPlayer.HasDied() && genOpt.TheDeadKnow)
                    || (player.AmOwner && player.TryGetModifier<ClericBarrierModifier>(out var cleric) && cleric.VisibleSymbol))))
                {
                    playerName += "<color=#00FFB3> Ω</color>";
                }

                if ((player.HasModifier<WardenFortifiedModifier>(x => x.Warden == PlayerControl.LocalPlayer) && PlayerControl.LocalPlayer.IsRole<WardenRole>())
                    || (player.HasModifier<WardenFortifiedModifier>() && ((PlayerControl.LocalPlayer.HasDied() && genOpt.TheDeadKnow)
                    || (player.AmOwner && player.TryGetModifier<WardenFortifiedModifier>(out var warden) && warden.VisibleSymbol))))
                {
                    playerName += "<color=#9900FF> π</color>";
                }

                if (player.HasModifier<LoverModifier>() && (PlayerControl.LocalPlayer.HasModifier<LoverModifier>() || (PlayerControl.LocalPlayer.HasDied() && genOpt.TheDeadKnow)))
                    playerName += "<color=#FF66CC> ♥</color>";

                if ((player.HasModifier<PlaguebearerInfectedModifier>(x => x.PlagueBearerId == PlayerControl.LocalPlayer.PlayerId) && PlayerControl.LocalPlayer.IsRole<PlaguebearerRole>())
                || (player.HasModifier<PlaguebearerInfectedModifier>() && PlayerControl.LocalPlayer.HasDied() && genOpt.TheDeadKnow))
                    playerName += "<color=#E6FFB3> ¥</color>";

                if ((player.HasModifier<ArsonistDousedModifier>(x => x.ArsonistId == PlayerControl.LocalPlayer.PlayerId) && PlayerControl.LocalPlayer.IsRole<ArsonistRole>())
                || (player.HasModifier<ArsonistDousedModifier>() && PlayerControl.LocalPlayer.HasDied() && genOpt.TheDeadKnow))
                    playerName += "<color=#FF4D00> Δ</color>";

                if ((player.HasModifier<BlackmailedModifier>(x => x.BlackMailerId == PlayerControl.LocalPlayer.PlayerId) && PlayerControl.LocalPlayer.IsRole<BlackmailerRole>())
                || (player.HasModifier<BlackmailedModifier>() && PlayerControl.LocalPlayer.IsImpostor() && genOpt.ImpsKnowRoles && !genOpt.FFAImpostorMode)
                || (player.HasModifier<BlackmailedModifier>() && PlayerControl.LocalPlayer.HasDied() && genOpt.TheDeadKnow))
                    playerName += "<color=#2A1119> M</color>";

                if ((player.HasModifier<HypnotisedModifier>(x => x.Hypnotist == PlayerControl.LocalPlayer) && PlayerControl.LocalPlayer.IsRole<HypnotistRole>())
                || (player.HasModifier<HypnotisedModifier>() && PlayerControl.LocalPlayer.IsImpostor() && genOpt.ImpsKnowRoles && !genOpt.FFAImpostorMode)
                || (player.HasModifier<HypnotisedModifier>() && PlayerControl.LocalPlayer.HasDied() && genOpt.TheDeadKnow))
                    playerName += "<color=#D53F42> @</color>";

                var role = player.Data.Role;
                var color = role.TeamColor;

                if (HaunterRole.HaunterVisibilityFlag(player))
                    playerColor = color;

                if (role == null) continue;

                var roleName = "";

                if (player.AmOwner ||
                    (PlayerControl.LocalPlayer.IsImpostor() && player.IsImpostor() && genOpt is { ImpsKnowRoles.Value: true, FFAImpostorMode: false }) ||
                    (PlayerControl.LocalPlayer.Data.Role is VampireRole && role is VampireRole) ||
                    SnitchRole.SnitchVisibilityFlag(player, true) ||
                    !TutorialManager.InstanceExists && ((PlayerControl.LocalPlayer.HasDied() && genOpt.TheDeadKnow) ||
                    GuardianAngelTouRole.GASeesRoleVisibilityFlag(player) ||
                    SleuthModifier.SleuthVisibilityFlag(player) ||
                    MayorRole.MayorVisibilityFlag(player)))
                {
                    roleName = $"<size=80%>{color.ToTextColor()}{player.Data.Role.NiceName}</color></size>";
                    if (!player.HasModifier<VampireBittenModifier>() && player.Data.Role is VampireRole) roleName += "<size=80%><color=#FFFFFF> (<color=#A22929>OG</color>)</color></size>";

                    var cachedMod = player.GetModifiers<BaseModifier>().FirstOrDefault(x => x is ICachedRole) as ICachedRole;
                    if (cachedMod != null && player.Data.Role != cachedMod.CachedRole)
                    {
                        if (cachedMod.ShowCurrentRoleFirst) roleName = $"<size=80%>{color.ToTextColor()}{player.Data.Role.NiceName}</color> ({cachedMod.CachedRole.TeamColor.ToTextColor()}{cachedMod.CachedRole.NiceName}</color>)</size>";
                        else roleName = $"<size=80%>{cachedMod.CachedRole.TeamColor.ToTextColor()}{cachedMod.CachedRole.NiceName}</color> ({color.ToTextColor()}{player.Data.Role.NiceName}</color>)</size>";
                    }
                    // Guardian Angel here is vanilla's GA, NOT Town of Us GA
                    if (player.Data.IsDead && role is GuardianAngelRole gaRole) roleName = $"<size=80%>{gaRole.TeamColor.ToTextColor()}{gaRole?.NiceName}</color></size>";
                    if (SleuthModifier.SleuthVisibilityFlag(player) || (player.Data.IsDead && role is not PhantomTouRole or GuardianAngelRole or HaunterRole))
                    {
                        var roleWhenAlive = player.GetRoleWhenAlive();
                        color = roleWhenAlive!.TeamColor;

                        roleName = $"<size=80%>{color.ToTextColor()}{roleWhenAlive?.NiceName}</color></size>";
                        if (!player.HasModifier<VampireBittenModifier>() && roleWhenAlive is VampireRole) roleName += "<size=80%><color=#FFFFFF> (<color=#A22929>OG</color>)</color></size>";
                    }
                }

                if (((taskopt.ShowTaskInMeetings && player.AmOwner) || (PlayerControl.LocalPlayer.HasDied() && taskopt.ShowTaskDead)) && (player.IsCrewmate() || player.Data.Role is PhantomTouRole))
                {
                    var completed = player.myTasks.ToArray().Count(x => x.IsComplete);
                    var totaltasks = player.myTasks.ToArray().Count(x => !PlayerTask.TaskIsEmergency(x) && !x.TryCast<ImportantTextTask>());
                    roleName += $" <size=80%>{Color.yellow.ToTextColor()}({completed}/{totaltasks})</color></size>";
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
                    if (!PlayerControl.LocalPlayer.HasDied() || !genOpt.TheDeadKnow)
                    {
                        roleName = "";
                    }

                    var dash = "";
                    if (!string.IsNullOrEmpty(roleName))
                    {
                        dash = " - ";
                    }

                    roleName = $"{roleName}<size=80%>{dash}Disconnected</size>";
                    playerColor = Color.white;
                }

                if (!string.IsNullOrEmpty(roleName))
                {
                    if (TownOfUsPlugin.ColorPlayerName.Value) playerName = $"{roleName}\n{color.ToTextColor()}{playerName}</color>";
                    else  playerName = $"{roleName}\n{playerName}";
                }

                playerVA.NameText.text = playerName;
                playerVA.NameText.color = playerColor;
            }
        }
        else
        {
            var body = GameObject.FindObjectsOfType<DeadBody>().FirstOrDefault(x => x.ParentId == PlayerControl.LocalPlayer.PlayerId);
            var fakePlayer = FakePlayer.FakePlayers.FirstOrDefault(x => x?.PlayerId == PlayerControl.LocalPlayer.PlayerId);

            foreach (var player in PlayerControl.AllPlayerControls)
            {
                var playerName = player.GetAppearance().PlayerName ?? "Unknown";
                var playerColor = Color.white;

                if (PlayerControl.LocalPlayer.IsImpostor() && player.IsImpostor() && PlayerControl.LocalPlayer != player && !genOpt.FFAImpostorMode)
                    playerColor = Color.red;

                if (player.HasModifier<EclipsalBlindModifier>() && PlayerControl.LocalPlayer.IsImpostor())
                    playerColor = Color.black;

                if (player.HasModifier<GrenadierFlashModifier>() && !player.IsImpostor() && PlayerControl.LocalPlayer.IsImpostor())
                    playerColor = Color.black;

                if (player.HasModifier<SeerGoodRevealModifier>() && PlayerControl.LocalPlayer.IsRole<SeerRole>())
                    playerColor = Color.green;
                else if (player.HasModifier<SeerEvilRevealModifier>() && PlayerControl.LocalPlayer.IsRole<SeerRole>())
                    playerColor = Color.red;

                if (player.HasModifier<PoliticianCampaignedModifier>(x => x.Politician == PlayerControl.LocalPlayer) && PlayerControl.LocalPlayer.IsRole<PoliticianRole>())
                    playerColor = Color.cyan;

                if ((player.HasModifier<ExecutionerTargetModifier>(x => x.OwnerId == PlayerControl.LocalPlayer.PlayerId) && PlayerControl.LocalPlayer.IsRole<ExecutionerRole>())
                    || (player.HasModifier<ExecutionerTargetModifier>() && PlayerControl.LocalPlayer.HasDied() && genOpt.TheDeadKnow && !body && !fakePlayer?.body))
                    playerName += "<color=#643B1F> X</color>";
                    
                if (player.HasModifier<InquisitorHereticModifier>() && PlayerControl.LocalPlayer.HasDied() && (genOpt.TheDeadKnow || PlayerControl.LocalPlayer.GetRoleWhenAlive() is InquisitorRole))
                    playerName += "<color=#D94291> $</color>";

                if (player.HasModifier<MercenaryBribedModifier>(x => x.Mercenary == PlayerControl.LocalPlayer) && PlayerControl.LocalPlayer.IsRole<MercenaryRole>())
                {
                    playerColor = Color.green;

                    if (player.Is(RoleAlignment.NeutralEvil) || player.IsRole<AmnesiacRole>() || player.IsRole<MercenaryRole>())
                        playerColor = Color.red;
                }

                if ((player.HasModifier<GuardianAngelTargetModifier>(x => x.OwnerId == PlayerControl.LocalPlayer.PlayerId) && PlayerControl.LocalPlayer.IsRole<GuardianAngelTouRole>())
                    || (player.HasModifier<GuardianAngelTargetModifier>() && ((PlayerControl.LocalPlayer.HasDied() && genOpt.TheDeadKnow && !body && !fakePlayer?.body)
                    || (player.AmOwner && OptionGroupSingleton<GuardianAngelOptions>.Instance.GATargetKnows))))
                {
                    playerName += player.HasModifier<GuardianAngelProtectModifier>() ? "<color=#FFD900> ★</color>" : "<color=#B3FFFF> ★</color>";
                }

                if ((player.HasModifier<MedicShieldModifier>(x => x.Medic == PlayerControl.LocalPlayer) && PlayerControl.LocalPlayer.IsRole<MedicRole>())
                    || (player.HasModifier<MedicShieldModifier>() && ((PlayerControl.LocalPlayer.HasDied() && genOpt.TheDeadKnow && !body && !fakePlayer?.body)
                    || (player.AmOwner && player.TryGetModifier<MedicShieldModifier>(out var med) && med.VisibleSymbol))))
                {
                    playerName += "<color=#006600> +</color>";
                }

                if ((player.HasModifier<ClericBarrierModifier>(x => x.Cleric == PlayerControl.LocalPlayer) && PlayerControl.LocalPlayer.IsRole<ClericRole>())
                    || (player.HasModifier<ClericBarrierModifier>() && ((PlayerControl.LocalPlayer.HasDied() && genOpt.TheDeadKnow && !body && !fakePlayer?.body)
                    || (player.AmOwner && player.TryGetModifier<ClericBarrierModifier>(out var cleric) && cleric.VisibleSymbol))))
                {
                    playerName += "<color=#00FFB3> Ω</color>";
                }

                if ((player.HasModifier<WardenFortifiedModifier>(x => x.Warden == PlayerControl.LocalPlayer) && PlayerControl.LocalPlayer.IsRole<WardenRole>())
                    || (player.HasModifier<WardenFortifiedModifier>() && ((PlayerControl.LocalPlayer.HasDied() && genOpt.TheDeadKnow && !body && !fakePlayer?.body)
                    || (player.AmOwner && player.TryGetModifier<WardenFortifiedModifier>(out var warden) && warden.VisibleSymbol))))
                {
                    playerName += "<color=#9900FF> π</color>";
                }

                if (player.HasModifier<LoverModifier>() && (PlayerControl.LocalPlayer.HasModifier<LoverModifier>() || (PlayerControl.LocalPlayer.HasDied() && genOpt.TheDeadKnow && !body && !fakePlayer?.body)))
                    playerName += "<color=#FF66CC> ♥</color>";

                if ((player.HasModifier<PlaguebearerInfectedModifier>(x => x.PlagueBearerId == PlayerControl.LocalPlayer.PlayerId) && PlayerControl.LocalPlayer.IsRole<PlaguebearerRole>())
                || (player.HasModifier<PlaguebearerInfectedModifier>() && PlayerControl.LocalPlayer.HasDied() && genOpt.TheDeadKnow && !body && !fakePlayer?.body))
                    playerName += "<color=#E6FFB3> ¥</color>";

                if ((player.HasModifier<ArsonistDousedModifier>(x => x.ArsonistId == PlayerControl.LocalPlayer.PlayerId) && PlayerControl.LocalPlayer.IsRole<ArsonistRole>())
                || (player.HasModifier<ArsonistDousedModifier>() && PlayerControl.LocalPlayer.HasDied() && genOpt.TheDeadKnow && !body && !fakePlayer?.body))
                    playerName += "<color=#FF4D00> Δ</color>";

                if ((player.HasModifier<BlackmailedModifier>(x => x.BlackMailerId == PlayerControl.LocalPlayer.PlayerId) && PlayerControl.LocalPlayer.IsRole<BlackmailerRole>())
                || (player.HasModifier<BlackmailedModifier>() && PlayerControl.LocalPlayer.IsImpostor() && genOpt.ImpsKnowRoles && !genOpt.FFAImpostorMode)
                || (player.HasModifier<BlackmailedModifier>() && PlayerControl.LocalPlayer.HasDied() && genOpt.TheDeadKnow && !body && !fakePlayer?.body))
                    playerName += "<color=#2A1119> M</color>";

                if ((player.HasModifier<HypnotisedModifier>(x => x.Hypnotist == PlayerControl.LocalPlayer) && PlayerControl.LocalPlayer.IsRole<HypnotistRole>())
                || (player.HasModifier<HypnotisedModifier>() && PlayerControl.LocalPlayer.IsImpostor() && genOpt.ImpsKnowRoles && !genOpt.FFAImpostorMode)
                || (player.HasModifier<HypnotisedModifier>() && PlayerControl.LocalPlayer.HasDied() && genOpt.TheDeadKnow && !body && !fakePlayer?.body))
                    playerName += "<color=#D53F42> @</color>";

                var role = player.Data.Role;
                var color = role.TeamColor;

                if (role == null) continue;

                var roleName = "";

                if (player.AmOwner ||
                    (PlayerControl.LocalPlayer.IsImpostor() && player.IsImpostor() && genOpt is { ImpsKnowRoles.Value: true, FFAImpostorMode: false }) ||
                    (PlayerControl.LocalPlayer.Data.Role is VampireRole && role is VampireRole) ||
                    SnitchRole.SnitchVisibilityFlag(player, true) ||
                    !TutorialManager.InstanceExists && ((PlayerControl.LocalPlayer.HasDied() && genOpt.TheDeadKnow && !body && !fakePlayer?.body) ||
                    GuardianAngelTouRole.GASeesRoleVisibilityFlag(player) ||
                    MayorRole.MayorVisibilityFlag(player)))
                {
                    roleName = $"<size=80%>{color.ToTextColor()}{player.Data.Role.NiceName}</color></size>";
                    if (!player.HasModifier<VampireBittenModifier>() && player.Data.Role is VampireRole) roleName += "<size=80%><color=#FFFFFF> (<color=#A22929>OG</color>)</color></size>";

                    var cachedMod = player.GetModifiers<BaseModifier>().FirstOrDefault(x => x is ICachedRole) as ICachedRole;
                    if (cachedMod != null && player.Data.Role != cachedMod.CachedRole)
                    {
                        if (cachedMod.ShowCurrentRoleFirst) roleName = $"<size=80%>{color.ToTextColor()}{player.Data.Role.NiceName}</color> ({cachedMod.CachedRole.TeamColor.ToTextColor()}{cachedMod.CachedRole.NiceName}</color>)</size>";
                        else roleName = $"<size=80%>{cachedMod.CachedRole.TeamColor.ToTextColor()}{cachedMod.CachedRole.NiceName}</color> ({color.ToTextColor()}{player.Data.Role.NiceName}</color>)</size>";
                    }
                    // Guardian Angel here is vanilla's GA, NOT Town of Us GA
                    if (player.Data.IsDead && role is GuardianAngelRole gaRole) roleName = $"<size=80%>{gaRole.TeamColor.ToTextColor()}{gaRole.NiceName}</color></size>";
                    if (SleuthModifier.SleuthVisibilityFlag(player) || (player.Data.IsDead && role is not PhantomTouRole or GuardianAngelRole or HaunterRole))
                    {
                        var roleWhenAlive = player.GetRoleWhenAlive();
                        color = roleWhenAlive!.TeamColor;

                        roleName = $"<size=80%>{color.ToTextColor()}{roleWhenAlive?.NiceName}</color></size>";
                        if (!player.HasModifier<VampireBittenModifier>() && roleWhenAlive is VampireRole) roleName += "<size=80%><color=#FFFFFF> (<color=#A22929>OG</color>)</color></size>";
                    }
                }

                if (((taskopt.ShowTaskRound && player.AmOwner) || (PlayerControl.LocalPlayer.HasDied() && taskopt.ShowTaskDead && !body && !fakePlayer?.body)) && (player.IsCrewmate() || player.Data.Role is PhantomTouRole))
                {
                    var completed = player.myTasks.ToArray().Count(x => x.IsComplete);
                    var totaltasks = player.myTasks.ToArray().Count(x => !PlayerTask.TaskIsEmergency(x) && !x.TryCast<ImportantTextTask>());

                    roleName += $" <size=80%>{Color.yellow.ToTextColor()}({completed}/{totaltasks})</color></size>";
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
                    if (TownOfUsPlugin.ColorPlayerName.Value) playerName = $"{roleName}\n{color.ToTextColor()}{playerName}</color>";
                    else  playerName = $"{roleName}\n{playerName}";
                }

                player.cosmetics.nameText.text = playerName;
                player.cosmetics.nameText.color = playerColor;
                player.cosmetics.nameText.transform.localPosition = new Vector3(0f, 0.15f, -0.5f);
            }
        }
    }

    public static void UpdateGhostRoles(HudManager instance)
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
    public static void UpdateRoleList(HudManager instance)
    {
        if (RoleList != null) RoleList.SetActive(false);
        if (!LobbyBehaviour.Instance) return;

        if (RoleList == null)
        {
            var pingTracker = UnityEngine.Object.FindObjectOfType<PingTracker>(true);
            RoleList = UnityEngine.Object.Instantiate(pingTracker.gameObject, instance.transform);
            RoleList.name = "RoleListText";
            RoleList.GetComponent<AspectPosition>().DistanceFromEdge = new Vector3(-4.9f, 5.9f);
        }
        else
        {
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
}

using HarmonyLib;
using MiraAPI.GameOptions;
using MiraAPI.Hud;
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
using TownOfUs.Roles.Impostor;
using TownOfUs.Roles.Neutral;
using TownOfUs.Utilities;
using TownOfUs.Utilities.Appearances;
using UnityEngine;
using UnityEngine.Events;
using Color = UnityEngine.Color;

namespace TownOfUs.Patches;

[HarmonyPatch]
public static class HudManagerPatches
{
    public static GameObject ZoomButton;
    public static GameObject WikiButton;

    public static bool Zooming;
    public static void Zoom()
    {
        Zooming = !Zooming;
        var size = Zooming ? 12f : 3f;
        ZoomButton.transform.Find("Inactive").GetComponent<SpriteRenderer>().sprite = Zooming ? TouAssets.ZoomPlus.LoadAsset() : TouAssets.ZoomMinus.LoadAsset();
        ZoomButton.transform.Find("Active").GetComponent<SpriteRenderer>().sprite = Zooming ? TouAssets.ZoomPlusActive.LoadAsset() : TouAssets.ZoomMinusActive.LoadAsset();
        Camera.main.orthographicSize = size;

        foreach (var cam in Camera.allCameras)
        {
            if (cam?.gameObject.name == "UI Camera")
                cam.orthographicSize = size;
        }

        // TODO: figure out a replacement for this as it breaks chat
        //ResolutionManager.ResolutionChanged.Invoke((float)Screen.width / Screen.height, Screen.width, Screen.height, Screen.fullScreen);
        List<GameObject> uiList =
        [
            HudManager.Instance.UICamera.gameObject,
            HudManager.Instance.TaskStuff.gameObject,
            HudManager.Instance.MapButton.gameObject,
            HudManager.Instance.KillButton.gameObject,
            HudManager.Instance.ImpostorVentButton.gameObject,
            HudManager.Instance.AdminButton.gameObject,
            HudManager.Instance.UseButton.gameObject,
            HudManager.Instance.PetButton.gameObject,
            HudManager.Instance.ReportButton.gameObject,
            HudManager.Instance.AbilityButton.gameObject,
            HudManager.Instance.SabotageButton.gameObject
        ];

        foreach (var obj in uiList)
        {
            if (obj.active)
            {
                //Logger<TownOfUsPlugin>.Message($"OBJECT ACTIVE: {obj.name}");
                if (obj.TryGetComponent<ActionButton>(out var button) && button.gameObject.active)
                {
                    //Logger<TownOfUsPlugin>.Message($"OBJECT (BUTTON) ACTIVE: {obj.name}");
                    button.gameObject.SetActive(false);
                    button.gameObject.SetActive(true);
                }
                obj.SetActive(false);
                obj.SetActive(true);
            }
        }

        foreach (var button in CustomButtonManager.Buttons.Where(x => x.Button != null && x.Button.gameObject.active).Select(x => x.Button!.gameObject))
        {
            button.SetActive(false);
            button.SetActive(true);
        }
    }

    public static void CheckForScrollZoom()
    {
        if (Input.GetAxis("Mouse ScrollWheel") > 0)
        {
            Zooming = true;
        }
        else if (Input.GetAxis("Mouse ScrollWheel") < 0)
        {
            Zooming = false;
        }
        Zoom();
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
            if (ZoomButton.active && MeetingHud.Instance == null && Minigame.Instance == null && (PlayerJoinPatch.SentOnce || TutorialManager.InstanceExists)) distanceFromEdge.x += 0.84f;
            distanceFromEdge.y = 0.485f;
            WikiButton.SetActive(true);
            aspectPosition.DistanceFromEdge = distanceFromEdge;
            aspectPosition.AdjustPosition();
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

        if (PlayerControl.LocalPlayer.Data.IsDead && (PlayerControl.LocalPlayer.Data.Role is IGhostRole { Caught: true } || PlayerControl.LocalPlayer.Data.Role is not IGhostRole)
            && Input.GetAxis("Mouse ScrollWheel") != 0 && MeetingHud.Instance == null && Minigame.Instance == null)
        {
            CheckForScrollZoom();
        }

        UpdateCamouflageComms(__instance);
        UpdateRoleNameText(__instance);
        UpdateGhostRoles(__instance);
        ResetZoom(__instance);
    }


    public static void ResetZoom(HudManager instance)
    {
        if (MeetingHud.Instance != null && !Zooming)
        {
            Zooming = true;
            Zoom();
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
                    playerName += "<color=#8C4005FF> X</color>";

                if (player.HasModifier<MercenaryBribedModifier>(x => x.Mercenary == PlayerControl.LocalPlayer) && PlayerControl.LocalPlayer.IsRole<MercenaryRole>())
                {
                    playerColor = Color.green;

                    if (player.Is(RoleAlignment.NeutralEvil) || player.IsRole<AmnesiacRole>() || player.IsRole<MercenaryRole>())
                        playerColor = Color.red;
                }

                if ((player.HasModifier<GuardianAngelTargetModifier>(x => x.OwnerId == PlayerControl.LocalPlayer.PlayerId) && PlayerControl.LocalPlayer.IsRole<GuardianAngelTouRole>())
                    || (player.HasModifier<GuardianAngelTargetModifier>() && ((PlayerControl.LocalPlayer.HasDied() && genOpt.TheDeadKnow)
                    || (PlayerControl.LocalPlayer == player && OptionGroupSingleton<GuardianAngelOptions>.Instance.GATargetKnows))))
                {
                    playerName += "<color=#B3FFFFFF> ★</color>";
                }

                if ((player.HasModifier<MedicShieldModifier>(x => x.Medic == PlayerControl.LocalPlayer) && PlayerControl.LocalPlayer.IsRole<MedicRole>())
                    || (player.HasModifier<MedicShieldModifier>() && ((PlayerControl.LocalPlayer.HasDied() && genOpt.TheDeadKnow)
                    || (PlayerControl.LocalPlayer == player && player.TryGetModifier<MedicShieldModifier>(out var med) && med.VisibleSymbol))))
                {
                    playerName += "<color=#006600FF> +</color>";
                }

                if ((player.HasModifier<ClericBarrierModifier>(x => x.Cleric == PlayerControl.LocalPlayer) && PlayerControl.LocalPlayer.IsRole<ClericRole>())
                    || (player.HasModifier<ClericBarrierModifier>() && ((PlayerControl.LocalPlayer.HasDied() && genOpt.TheDeadKnow)
                    || (PlayerControl.LocalPlayer == player && player.TryGetModifier<ClericBarrierModifier>(out var cleric) && cleric.VisibleSymbol))))
                {
                    playerName += "<color=#00FFB3FF> Ω</color>";
                }

                if ((player.HasModifier<WardenFortifiedModifier>(x => x.Warden == PlayerControl.LocalPlayer) && PlayerControl.LocalPlayer.IsRole<WardenRole>())
                    || (player.HasModifier<WardenFortifiedModifier>() && ((PlayerControl.LocalPlayer.HasDied() && genOpt.TheDeadKnow)
                    || (PlayerControl.LocalPlayer == player && player.TryGetModifier<WardenFortifiedModifier>(out var warden) && warden.VisibleSymbol))))
                {
                    playerName += "<color=#9900FFFF> π</color>";
                }

                if (player.HasModifier<LoverModifier>() && (PlayerControl.LocalPlayer.HasModifier<LoverModifier>() || (PlayerControl.LocalPlayer.HasDied() && genOpt.TheDeadKnow)))
                    playerName += "<color=#FF66CCFF> ♥</color>";

                if ((player.HasModifier<PlaguebearerInfectedModifier>(x => x.PlagueBearerId == PlayerControl.LocalPlayer.PlayerId) && PlayerControl.LocalPlayer.IsRole<PlaguebearerRole>())
                || (player.HasModifier<PlaguebearerInfectedModifier>() && PlayerControl.LocalPlayer.HasDied() && genOpt.TheDeadKnow))
                    playerName += "<color=#E6FFB3FF> ¥</color>";

                if ((player.HasModifier<ArsonistDousedModifier>(x => x.ArsonistId == PlayerControl.LocalPlayer.PlayerId) && PlayerControl.LocalPlayer.IsRole<ArsonistRole>())
                || (player.HasModifier<ArsonistDousedModifier>() && PlayerControl.LocalPlayer.HasDied() && genOpt.TheDeadKnow))
                    playerName += "<color=#FF4D00FF> Δ</color>";

                var role = player.Data.Role;
                var color = role.TeamColor;

                if (HaunterRole.HaunterVisibilityFlag(player))
                    playerColor = color;

                if (role == null) continue;

                var roleName = "";

                if (player.AmOwner ||
                    (PlayerControl.LocalPlayer.HasDied() && genOpt.TheDeadKnow) ||
                    (PlayerControl.LocalPlayer.IsImpostor() && player.IsImpostor() && genOpt is { ImpsKnowRoles: true, FFAImpostorMode: false }) ||
                    (PlayerControl.LocalPlayer.Data.Role is VampireRole && role is VampireRole) ||
                    SnitchRole.SnitchVisibilityFlag(player) ||
                    GuardianAngelTouRole.GASeesRoleVisibilityFlag(player) ||
                    SleuthModifier.SleuthVisibilityFlag(player) ||
                    MayorRole.MayorVisibilityFlag(player))
                {
                    roleName = $"<size=80%>{color.ToTextColor()}{player.Data.Role.NiceName}</color></size>";

                    if (player.HasModifier<TraitorCacheModifier>() && role is not TraitorRole) roleName = $"<size=80%>{color.ToTextColor()}Traitor ({player.Data.Role.NiceName})</color></size>";

                    if (SleuthModifier.SleuthVisibilityFlag(player) || (player.Data.IsDead && role is not PhantomTouRole or HaunterRole))
                    {
                        var roleWhenAlive = player.GetRoleWhenAlive();
                        color = roleWhenAlive!.TeamColor;

                        roleName = $"<size=80%>{color.ToTextColor()}{roleWhenAlive?.NiceName}</color></size>";
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
                    var revealText = confess!.RevealedFaction switch
                    {
                        ModdedRoleTeams.Crewmate => $" {TownOfUsColors.Crewmate.ToTextColor()}({accuracy}% Crew) </color>",
                        ModdedRoleTeams.Custom => $" {TownOfUsColors.Neutral.ToTextColor()}({accuracy}% Neut) </color>",
                        ModdedRoleTeams.Impostor => $" {TownOfUsColors.Impostor.ToTextColor()}({accuracy}% Imp) </color>",
                        _ => string.Empty,
                    };

                    playerName += revealText;
                }

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
                    playerName = $"{roleName}\n{playerName}";
                }

                playerVA.NameText.text = playerName;
                playerVA.NameText.color = playerColor;
            }
        }
        else
        {
            var body = GameObject.FindObjectsOfType<DeadBody>().FirstOrDefault(x => x.ParentId == PlayerControl.LocalPlayer.PlayerId);

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
                    || (player.HasModifier<ExecutionerTargetModifier>() && PlayerControl.LocalPlayer.HasDied() && genOpt.TheDeadKnow && !body))
                    playerName += "<color=#8C4005FF> X</color>";

                if (player.HasModifier<MercenaryBribedModifier>(x => x.Mercenary == PlayerControl.LocalPlayer) && PlayerControl.LocalPlayer.IsRole<MercenaryRole>())
                {
                    playerColor = Color.green;

                    if (player.Is(RoleAlignment.NeutralEvil) || player.IsRole<AmnesiacRole>() || player.IsRole<MercenaryRole>())
                        playerColor = Color.red;
                }

                if ((player.HasModifier<GuardianAngelTargetModifier>(x => x.OwnerId == PlayerControl.LocalPlayer.PlayerId) && PlayerControl.LocalPlayer.IsRole<GuardianAngelTouRole>())
                    || (player.HasModifier<GuardianAngelTargetModifier>() && ((PlayerControl.LocalPlayer.HasDied() && genOpt.TheDeadKnow && !body)
                    || (PlayerControl.LocalPlayer == player && OptionGroupSingleton<GuardianAngelOptions>.Instance.GATargetKnows))))
                {
                    playerName += player.HasModifier<GuardianAngelProtectModifier>() ? "<color=#FFD900FF> ★</color>": "<color=#B3FFFFFF> ★</color>";
                }

                if ((player.HasModifier<MedicShieldModifier>(x => x.Medic == PlayerControl.LocalPlayer) && PlayerControl.LocalPlayer.IsRole<MedicRole>())
                    || (player.HasModifier<MedicShieldModifier>() && ((PlayerControl.LocalPlayer.HasDied() && genOpt.TheDeadKnow && !body)
                    || (PlayerControl.LocalPlayer == player && player.TryGetModifier<MedicShieldModifier>(out var med) && med.VisibleSymbol))))
                {
                    playerName += "<color=#006600FF> +</color>";
                }

                if ((player.HasModifier<ClericBarrierModifier>(x => x.Cleric == PlayerControl.LocalPlayer) && PlayerControl.LocalPlayer.IsRole<ClericRole>())
                    || (player.HasModifier<ClericBarrierModifier>() && ((PlayerControl.LocalPlayer.HasDied() && genOpt.TheDeadKnow && !body)
                    || (PlayerControl.LocalPlayer == player && player.TryGetModifier<ClericBarrierModifier>(out var cleric) && cleric.VisibleSymbol))))
                {
                    playerName += "<color=#00FFB3FF> Ω</color>";
                }

                if ((player.HasModifier<WardenFortifiedModifier>(x => x.Warden == PlayerControl.LocalPlayer) && PlayerControl.LocalPlayer.IsRole<WardenRole>())
                    || (player.HasModifier<WardenFortifiedModifier>() && ((PlayerControl.LocalPlayer.HasDied() && genOpt.TheDeadKnow && !body)
                    || (PlayerControl.LocalPlayer == player && player.TryGetModifier<WardenFortifiedModifier>(out var warden) && warden.VisibleSymbol))))
                {
                    playerName += "<color=#9900FFFF> π</color>";
                }

                if (player.HasModifier<LoverModifier>() && (PlayerControl.LocalPlayer.HasModifier<LoverModifier>() || (PlayerControl.LocalPlayer.HasDied() && genOpt.TheDeadKnow && !body)))
                    playerName += "<color=#FF66CCFF> ♥</color>";

                if ((player.HasModifier<PlaguebearerInfectedModifier>(x => x.PlagueBearerId == PlayerControl.LocalPlayer.PlayerId) && PlayerControl.LocalPlayer.IsRole<PlaguebearerRole>())
                || (player.HasModifier<PlaguebearerInfectedModifier>() && PlayerControl.LocalPlayer.HasDied() && genOpt.TheDeadKnow))
                    playerName += "<color=#E6FFB3FF> ¥</color>";

                if ((player.HasModifier<ArsonistDousedModifier>(x => x.ArsonistId == PlayerControl.LocalPlayer.PlayerId) && PlayerControl.LocalPlayer.IsRole<ArsonistRole>())
                || (player.HasModifier<ArsonistDousedModifier>() && PlayerControl.LocalPlayer.HasDied() && genOpt.TheDeadKnow))
                    playerName += "<color=#FF4D00FF> Δ</color>";

                var role = player.Data.Role;
                var color = role.TeamColor;

                if (role == null) continue;

                var roleName = "";

                if (player.AmOwner ||
                    (PlayerControl.LocalPlayer.HasDied() && genOpt.TheDeadKnow && !body) ||
                    (PlayerControl.LocalPlayer.IsImpostor() && player.IsImpostor() && genOpt is { ImpsKnowRoles: true, FFAImpostorMode: false }) ||
                    (PlayerControl.LocalPlayer.Data.Role is VampireRole && role is VampireRole) ||
                    SnitchRole.SnitchVisibilityFlag(player) ||
                    GuardianAngelTouRole.GASeesRoleVisibilityFlag(player) ||
                    MayorRole.MayorVisibilityFlag(player))
                {
                    roleName = $"<size=80%>{color.ToTextColor()}{player.Data.Role.NiceName}</color></size>";

                    var cachedMod = player.GetModifiers<BaseModifier>().FirstOrDefault(x => x is ICachedRole) as ICachedRole;
                    if (cachedMod != null)
                    {
                        if (cachedMod.ShowCurrentRoleFirst) roleName = $"<size=80%>{color.ToTextColor()}{player.Data.Role.NiceName}</color> ({cachedMod.CachedRole.TeamColor.ToTextColor()}{cachedMod.CachedRole.NiceName}</color>)</size>";
                        else roleName = $"<size=80%>{cachedMod.CachedRole.TeamColor.ToTextColor()}{cachedMod.CachedRole.NiceName}</color> ({color.ToTextColor()}{player.Data.Role.NiceName}</color>)</size>";
                    }

                    if (player.Data.IsDead && role is not PhantomTouRole or HaunterRole)
                    {
                        var roleWhenAlive = player.GetRoleWhenAlive();
                        color = roleWhenAlive!.TeamColor;

                        roleName = $"<size=80%>{color.ToTextColor()}{roleWhenAlive?.NiceName}</color></size>";
                    }
                }

                if (((taskopt.ShowTaskRound && player.AmOwner) || (PlayerControl.LocalPlayer.HasDied() && taskopt.ShowTaskDead && !body)) && (player.IsCrewmate() || player.Data.Role is PhantomTouRole))
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

                if (!string.IsNullOrEmpty(roleName))
                {
                    playerName = $"{roleName}\n{playerName}";
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
}

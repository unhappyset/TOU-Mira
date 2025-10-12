﻿using System.Collections;
using System.Globalization;
using HarmonyLib;
using MiraAPI.Events;
using System.Text;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.Events.Vanilla.Meeting;
using MiraAPI.Events.Vanilla.Meeting.Voting;
using MiraAPI.Events.Vanilla.Player;
using MiraAPI.Events.Vanilla.Usables;
using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Modifiers;
using MiraAPI.Modifiers.ModifierDisplay;
using MiraAPI.Modifiers.Types;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using PowerTools;
using Reactor.Utilities;
using Reactor.Utilities.Extensions;
using TMPro;
using TownOfUs.Buttons;
using TownOfUs.Buttons.Crewmate;
using TownOfUs.Buttons.Impostor;
using TownOfUs.Buttons.Modifiers;
using TownOfUs.Buttons.Neutral;
using TownOfUs.Events.TouEvents;
using TownOfUs.Modifiers;
using TownOfUs.Modifiers.Game;
using TownOfUs.Modifiers.Game.Universal;
using TownOfUs.Modifiers.Neutral;
using TownOfUs.Modules;
using TownOfUs.Modules.Anims;
using TownOfUs.Options;
using TownOfUs.Options.Modifiers.Universal;
using TownOfUs.Options.Roles.Crewmate;
using TownOfUs.Options.Roles.Impostor;
using TownOfUs.Options.Roles.Neutral;
using TownOfUs.Patches;
using TownOfUs.Patches.Misc;
using TownOfUs.Roles;
using TownOfUs.Roles.Crewmate;
using TownOfUs.Roles.Impostor;
using TownOfUs.Roles.Other;
using TownOfUs.Utilities;
using UnityEngine;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace TownOfUs.Events;

public static class TownOfUsEventHandlers
{
    internal static TextMeshPro ModifierText;

    public static void RunModChecks()
    {
        var option = OptionGroupSingleton<GeneralOptions>.Instance.ModifierReveal;
        var modifier = PlayerControl.LocalPlayer.GetModifiers<AllianceGameModifier>().FirstOrDefault();
        var uniModifier = PlayerControl.LocalPlayer.GetModifiers<UniversalGameModifier>().FirstOrDefault();

        if (modifier != null && option is ModReveal.Alliance)
        {
            ModifierText.text = $"<size={modifier.IntroSize}>{modifier.IntroInfo}</size>";

            ModifierText.color = MiscUtils.GetModifierColour(modifier);
        }
        else if (uniModifier != null && option is ModReveal.Universal)
        {
            ModifierText.text =
                $"<size=4><color=#FFFFFF>{TouLocale.Get("Modifier")}: </color>{uniModifier.ModifierName}</size>";

            ModifierText.color = MiscUtils.GetModifierColour(uniModifier);
        }
        else
        {
            ModifierText.text = string.Empty;
        }
    }

    [RegisterEvent(1000)]
    public static void IntroRoleRevealEventHandler(IntroRoleRevealEvent @event)
    {
        var instance = @event.IntroCutscene;

        if (ModCompatibility.IsSubmerged())
        {
            Coroutines.Start(ModCompatibility.WaitMeeting(ModCompatibility.ResetTimers));
        }

        if (PlayerControl.LocalPlayer.Data.Role is ITownOfUsRole custom)
        {
            instance.RoleText.text = custom.RoleName;
            if (instance.YouAreText.transform.TryGetComponent<TextTranslatorTMP>(out var tmp))
            {
                tmp.defaultStr = custom.YouAreText;
                tmp.TargetText = StringNames.None;
                tmp.ResetText();
            }

            instance.RoleBlurbText.text = custom.RoleDescription;
        }

        var teamModifier = PlayerControl.LocalPlayer.GetModifiers<TouGameModifier>().FirstOrDefault();
        if (teamModifier != null && OptionGroupSingleton<GeneralOptions>.Instance.TeamModifierReveal)
        {
            var color = MiscUtils.GetModifierColour(teamModifier);

            instance.RoleBlurbText.text =
                $"<size={teamModifier.IntroSize}>\n</size>{instance.RoleBlurbText.text}\n<size={teamModifier.IntroSize}><color=#{color.ToHtmlStringRGBA()}>{teamModifier.IntroInfo}</color></size>";
        }
    }

    [RegisterEvent]
    public static void IntroBeginEventHandler(IntroBeginEvent @event)
    {
        var cutscene = @event.IntroCutscene;
        Coroutines.Start(CoChangeModifierText(cutscene));
    }

    public static IEnumerator CoChangeModifierText(IntroCutscene cutscene)
    {
        yield return new WaitForSeconds(0.01f);

        ModifierText =
            Object.Instantiate(cutscene.RoleText, cutscene.RoleText.transform.parent, false);

        if (PlayerControl.LocalPlayer.Data.Role is ITownOfUsRole custom)
        {
            cutscene.RoleText.text = custom.RoleName;
            cutscene.YouAreText.text = custom.YouAreText;
            cutscene.RoleBlurbText.text = custom.RoleDescription;
        }

        var teamModifier = PlayerControl.LocalPlayer.GetModifiers<TouGameModifier>().FirstOrDefault();
        if (teamModifier != null && OptionGroupSingleton<GeneralOptions>.Instance.TeamModifierReveal)
        {
            var color = MiscUtils.GetModifierColour(teamModifier);

            cutscene.RoleBlurbText.text =
                $"<size={teamModifier.IntroSize}>\n</size>{cutscene.RoleBlurbText.text}\n<size={teamModifier.IntroSize}><color=#{color.ToHtmlStringRGBA()}>{teamModifier.IntroInfo}</color></size>";
        }

        RunModChecks();

        ModifierText.transform.position =
            cutscene.transform.position - new Vector3(0f, 1.6f, -10f);
        ModifierText.gameObject.SetActive(true);
        ModifierText.color.SetAlpha(0.8f);
    }

    [RegisterEvent]
    public static void IntroEndEventHandler(IntroEndEvent @event)
    {
        HudManager.Instance.SetHudActive(false);
        HudManager.Instance.SetHudActive(true);

        var genOpt = OptionGroupSingleton<GeneralOptions>.Instance;

        if (genOpt.StartCooldownMode is not StartCooldownType.NoButtons)
        {
            var minCooldown = Math.Min(genOpt.StartCooldownMin, genOpt.StartCooldownMax);
            var maxCooldown = Math.Max(genOpt.StartCooldownMin, genOpt.StartCooldownMax);
            foreach (var button in
                     CustomButtonManager.Buttons.Where(x => x.Enabled(PlayerControl.LocalPlayer.Data.Role)))
            {
                if (button is FakeVentButton)
                {
                    continue;
                }

                switch (genOpt.StartCooldownMode)
                {
                    case StartCooldownType.AllButtons:
                        button.SetTimer(genOpt.GameStartCd);
                        break;
                    default:
                        if (button.Cooldown >= minCooldown && button.Cooldown <= maxCooldown)
                        {
                            button.SetTimer(genOpt.GameStartCd);
                        }
                        else
                        {
                            button.SetTimer(button.Cooldown);
                        }

                        break;
                }
            }

            if (PlayerControl.LocalPlayer.IsImpostor())
            {
                PlayerControl.LocalPlayer.SetKillTimer(genOpt.GameStartCd);
            }
        }

        var modsTab = ModifierDisplayComponent.Instance;
        if (modsTab != null && !modsTab.IsOpen && PlayerControl.LocalPlayer.GetModifiers<GameModifier>()
                .Any(x => !x.HideOnUi && x.GetDescription() != string.Empty))
        {
            modsTab.ToggleTab();
        }

        var panelThing = HudManager.Instance.TaskStuff.transform.FindChild("RolePanel");
        if (panelThing != null)
        {
            var panel = panelThing.gameObject.GetComponent<TaskPanelBehaviour>();
            var role = PlayerControl.LocalPlayer.Data.Role as ICustomRole;
            if (role == null)
            {
                return;
            }

            panel.open = true;

            var tabText = panel.tab.gameObject.GetComponentInChildren<TextMeshPro>();
            var ogPanel = HudManager.Instance.TaskStuff.transform.FindChild("TaskPanel").gameObject
                .GetComponent<TaskPanelBehaviour>();
            if (tabText.text != role.RoleName)
            {
                tabText.text = role.RoleName;
            }

            var y = ogPanel.taskText.textBounds.size.y + 1;
            panel.closedPosition = new Vector3(ogPanel.closedPosition.x, ogPanel.open ? y + 0.2f : 2f,
                ogPanel.closedPosition.z);
            panel.openPosition = new Vector3(ogPanel.openPosition.x, ogPanel.open ? y : 2f, ogPanel.openPosition.z);

            panel.SetTaskText(role.SetTabText().ToString());
        }
    }

    [RegisterEvent]
    public static void StartMeetingEventHandler(StartMeetingEvent @event)
    {
        foreach (var mod in ModifierUtils.GetActiveModifiers<MisfortuneTargetModifier>())
        {
            mod.ModifierComponent?.RemoveModifier(mod);
        }

        var exeButton = CustomButtonSingleton<ExeTormentButton>.Instance;
        var jestButton = CustomButtonSingleton<JesterHauntButton>.Instance;
        var phantomButton = CustomButtonSingleton<PhantomSpookButton>.Instance;
        if (exeButton.Show || jestButton.Show || phantomButton.Show)
        {
            PlayerControl.LocalPlayer.RpcRemoveModifier<IndirectAttackerModifier>();
        }

        exeButton.Show = false;
        jestButton.Show = false;
        phantomButton.Show = false;
    }

    [RegisterEvent(-100)]
    public static void BetaBeforeMurderHandler(BeforeMurderEvent @event)
    {
        var killer = @event.Source;
        var victim = @event.Target;
        Logger<TownOfUsPlugin>.Error(
            $"{killer.Data.PlayerName} ({killer.Data.Role.GetRoleName()}) is attempting to kill {victim.Data.PlayerName} ({victim.Data.Role.GetRoleName()}) | Meeting: {MeetingHud.Instance != null}");
    }

    [RegisterEvent(-100)]
    public static void BetaAfterMurderHandler(AfterMurderEvent @event)
    {
        var killer = @event.Source;
        var victim = @event.Target;
        Logger<TownOfUsPlugin>.Error(
            $"{killer.Data.PlayerName} ({killer.Data.Role.GetRoleName()}) successfully killed {victim.Data.PlayerName} ({victim.GetRoleWhenAlive().GetRoleName()}) | Meeting: {MeetingHud.Instance != null}");
    }

    [RegisterEvent]
    public static void RoundStartHandler(RoundStartEvent @event)
    {
        if (!@event.TriggeredByIntro)
        {
            return; // Only run when game starts.
        }

        if (FirstDeadPatch.PlayerNames.Count > 0)
        {
            var stringB = new StringBuilder();
            stringB.Append(CultureInfo.InvariantCulture, $"List Of Players That Died In Order: ");
            foreach (var playername in FirstDeadPatch.PlayerNames)
            {
                stringB.Append(CultureInfo.InvariantCulture, $"{playername}, ");
            }

            stringB = stringB.Remove(stringB.Length - 2, 2);

            Logger<TownOfUsPlugin>.Warning(stringB.ToString());
        }

        FirstDeadPatch.PlayerNames = [];

        HudManager.Instance.SetHudActive(false);
        HudManager.Instance.SetHudActive(true);
        CustomButtonSingleton<InquisitorVanquishButton>.Instance.Usable =
            OptionGroupSingleton<InquisitorOptions>.Instance.FirstRoundUse;

        CustomButtonSingleton<WatchButton>.Instance.ExtraUses = 0;
        CustomButtonSingleton<WatchButton>.Instance.SetUses((int)OptionGroupSingleton<LookoutOptions>.Instance
            .MaxWatches);
        CustomButtonSingleton<TrackerTrackButton>.Instance.ExtraUses = 0;
        CustomButtonSingleton<TrackerTrackButton>.Instance.SetUses((int)OptionGroupSingleton<TrackerOptions>.Instance
            .MaxTracks);
        CustomButtonSingleton<TrapperTrapButton>.Instance.ExtraUses = 0;
        CustomButtonSingleton<TrapperTrapButton>.Instance.SetUses((int)OptionGroupSingleton<TrapperOptions>.Instance
            .MaxTraps);

        CustomButtonSingleton<HunterStalkButton>.Instance.ExtraUses = 0;
        CustomButtonSingleton<HunterStalkButton>.Instance.SetUses((int)OptionGroupSingleton<HunterOptions>.Instance
            .StalkUses);
        CustomButtonSingleton<SheriffShootButton>.Instance.Usable =
            OptionGroupSingleton<SheriffOptions>.Instance.FirstRoundUse;
        CustomButtonSingleton<VeteranAlertButton>.Instance.ExtraUses = 0;
        CustomButtonSingleton<VeteranAlertButton>.Instance.SetUses((int)OptionGroupSingleton<VeteranOptions>.Instance
            .MaxNumAlerts);

        CustomButtonSingleton<SpellslingerHexButton>.Instance.SetUses((int)OptionGroupSingleton<SpellslingerOptions>
            .Instance
            .MaxHexes);

        CustomButtonSingleton<JailorJailButton>.Instance.ExecutedACrew = false;

        var engiVent = CustomButtonSingleton<EngineerVentButton>.Instance;
        engiVent.ExtraUses = 0;
        engiVent.SetUses((int)OptionGroupSingleton<EngineerOptions>.Instance.MaxVents);
        if ((int)OptionGroupSingleton<EngineerOptions>.Instance.MaxVents == 0)
        {
            engiVent.Button?.usesRemainingText.gameObject.SetActive(false);
            engiVent.Button?.usesRemainingSprite.gameObject.SetActive(false);
        }
        else
        {
            engiVent.Button?.usesRemainingText.gameObject.SetActive(true);
            engiVent.Button?.usesRemainingSprite.gameObject.SetActive(true);
        }

        var medicShield = CustomButtonSingleton<MedicShieldButton>.Instance;
        medicShield.SetUses(OptionGroupSingleton<MedicOptions>.Instance.ChangeTarget
            ? (int)OptionGroupSingleton<MedicOptions>.Instance.MedicShieldUses
            : 0);
        if ((int)OptionGroupSingleton<MedicOptions>.Instance.MedicShieldUses == 0 ||
            !OptionGroupSingleton<MedicOptions>.Instance.ChangeTarget)
        {
            medicShield.Button?.usesRemainingText.gameObject.SetActive(false);
            medicShield.Button?.usesRemainingSprite.gameObject.SetActive(false);
        }
        else
        {
            medicShield.Button?.usesRemainingText.gameObject.SetActive(true);
            medicShield.Button?.usesRemainingSprite.gameObject.SetActive(true);
        }

        CustomButtonSingleton<PlumberBlockButton>.Instance.ExtraUses = 0;
        CustomButtonSingleton<PlumberBlockButton>.Instance.SetUses((int)OptionGroupSingleton<PlumberOptions>.Instance
            .MaxBarricades);
        CustomButtonSingleton<TransporterTransportButton>.Instance.ExtraUses = 0;
        CustomButtonSingleton<TransporterTransportButton>.Instance.SetUses((int)OptionGroupSingleton<TransporterOptions>
            .Instance.MaxNumTransports);

        CustomButtonSingleton<WarlockKillButton>.Instance.Charge = 0f;
        CustomButtonSingleton<WarlockKillButton>.Instance.BurstActive = false;

        CustomButtonSingleton<BarryButton>.Instance.Usable =
            OptionGroupSingleton<ButtonBarryOptions>.Instance.FirstRoundUse || TutorialManager.InstanceExists;
        CustomButtonSingleton<SatelliteButton>.Instance.Usable =
            OptionGroupSingleton<SatelliteOptions>.Instance.FirstRoundUse || TutorialManager.InstanceExists;
        CustomButtonSingleton<BomberPlantButton>.Instance.Usable =
            OptionGroupSingleton<BomberOptions>.Instance.CanBombFirstRound || TutorialManager.InstanceExists;
    }

    [RegisterEvent]
    public static void ChangeRoleHandler(ChangeRoleEvent @event)
    {
        if (!PlayerControl.LocalPlayer)
        {
            return;
        }

        var player = @event.Player;
        if (!MeetingHud.Instance && player.AmOwner)
        {
            foreach (var button in CustomButtonManager.Buttons)
            {
                if (button is TownOfUsTargetButton<PlayerControl> touPlayerButton && touPlayerButton.Target != null)
                {
                    touPlayerButton.Target.cosmetics.currentBodySprite.BodySprite.SetOutline(null);
                }
                else if (button is TownOfUsTargetButton<DeadBody> touBodyButton && touBodyButton.Target != null)
                {
                    touBodyButton.Target.bodyRenderers.Do(x => x.SetOutline(null));
                }
                else if (button is TownOfUsTargetButton<Vent> touVentButton && touVentButton.Target != null)
                {
                    touVentButton.Target.SetOutline(false, true, player.Data.Role.TeamColor);
                }
            }

            HudManager.Instance.SetHudActive(false);
            HudManager.Instance.SetHudActive(true);
        }
    }

    [RegisterEvent]
    public static void SetRoleHandler(SetRoleEvent @event)
    {
        GameHistory.RegisterRole(@event.Player, @event.Player.Data.Role);
    }

    [RegisterEvent]
    public static void ClearBodiesAndResetPlayersEventHandler(RoundStartEvent @event)
    {
        Object.FindObjectsOfType<DeadBody>().ToList().ForEach(x => x.gameObject.Destroy());

        foreach (var player in PlayerControl.AllPlayerControls)
        {
            player.MyPhysics.ResetAnimState();
            player.MyPhysics.ResetMoveState();
        }

        FakePlayer.ClearAll();
    }

    [RegisterEvent]
    public static void EjectionEventHandler(EjectionEvent @event)
    {
        var exiled = @event.ExileController?.initData?.networkedPlayer?.Object;
        if (exiled == null)
        {
            return;
        }

        if (exiled.AmOwner)
        {
            HudManager.Instance.SetHudActive(false);

            if (!MeetingHud.Instance)
            {
                HudManager.Instance.SetHudActive(true);
            }
        }

        if (exiled.Data.Role is IAnimated animated)
        {
            animated.IsVisible = false;
            animated.SetVisible();
        }

        foreach (var button in CustomButtonManager.Buttons.Where(x => x.Enabled(exiled.Data.Role)).OfType<IAnimated>())
        {
            button.IsVisible = false;
            button.SetVisible();
        }

        foreach (var modifier in exiled.GetModifiers<GameModifier>().Where(x => x is IAnimated))
        {
            var animatedMod = modifier as IAnimated;
            if (animatedMod != null)
            {
                animatedMod.IsVisible = false;
                animatedMod.SetVisible();
            }
        }
    }

    [RegisterEvent]
    public static void AfterMurderEventHandler(AfterMurderEvent murderEvent)
    {
        var source = murderEvent.Source;
        var target = murderEvent.Target;

        GameHistory.AddMurder(source, target);

        if (SpellslingerRole.EveryoneHexed() && PlayerControl.LocalPlayer.Data.Role is SpellslingerRole)
        {
            CustomButtonSingleton<SpellslingerHexButton>.Instance.SetActive(false, PlayerControl.LocalPlayer.Data.Role);
        }

        if (target.AmOwner)
        {
            HudManager.Instance.SetHudActive(false);

            if (!MeetingHud.Instance)
            {
                HudManager.Instance.SetHudActive(true);
                HudManager.Instance.Chat.chatButton.gameObject.SetActive(false);
            }
        }

        if (target.Data.Role is IAnimated animated)
        {
            animated.IsVisible = false;
            animated.SetVisible();
        }

        foreach (var button in CustomButtonManager.Buttons.Where(x => x.Enabled(target.Data.Role)).OfType<IAnimated>())
        {
            button.IsVisible = false;
            button.SetVisible();
        }

        foreach (var modifier in target.GetModifiers<GameModifier>().Where(x => x is IAnimated))
        {
            var animatedMod = modifier as IAnimated;
            if (animatedMod != null)
            {
                animatedMod.IsVisible = false;
                animatedMod.SetVisible();
            }
        }

        if (source.IsImpostor() && source.AmOwner && source != target && !MeetingHud.Instance)
        {
            switch (source.Data.Role)
            {
                case AmbusherRole:
                    var ambushButton = CustomButtonSingleton<AmbusherAmbushButton>.Instance;
                    ambushButton.ResetCooldownAndOrEffect();
                    break;
                case BomberRole:
                    var bombButton = CustomButtonSingleton<BomberPlantButton>.Instance;
                    bombButton.ResetCooldownAndOrEffect();
                    break;
                case JanitorRole:
                    if (OptionGroupSingleton<JanitorOptions>.Instance.ResetCooldowns)
                    {
                        var cleanButton = CustomButtonSingleton<JanitorCleanButton>.Instance;
                        cleanButton.ResetCooldownAndOrEffect();
                    }

                    break;
            }
        }

        // here we're adding support for kills during a meeting
        if (MeetingHud.Instance)
        {
            HandleMeetingMurder(MeetingHud.Instance, source, target);
        }
        else
        {
            var body = Object.FindObjectsOfType<DeadBody>().FirstOrDefault(x => x.ParentId == target.PlayerId);

            if (target.HasModifier<MiniModifier>() && body != null)
            {
                body.transform.localScale *= 0.7f;
            }

            if (target.HasModifier<GiantModifier>() && body != null)
            {
                body.transform.localScale /= 0.7f;
            }

            if (target.AmOwner)
            {
                if (Minigame.Instance != null)
                {
                    Minigame.Instance.Close();
                    Minigame.Instance.Close();
                }

                if (MapBehaviour.Instance != null)
                {
                    MapBehaviour.Instance.Close();
                    MapBehaviour.Instance.Close();
                }
            }
        }
    }

    [RegisterEvent]
    public static void PlayerCanUseEventHandler(PlayerCanUseEvent @event)
    {
        if (!PlayerControl.LocalPlayer || !PlayerControl.LocalPlayer.Data ||
            !PlayerControl.LocalPlayer.Data.Role)
        {
            return;
        }

        if (PlayerControl.LocalPlayer.GetModifiers<DisabledModifier>().Any(x => !x.CanUseAbilities))
        {
            @event.Cancel();
        }

        // Prevent last 2 players from venting (or however many are set up)
        if (@event.IsVent)
        {
            if (PlayerControl.LocalPlayer.HasModifier<GlitchHackedModifier>())
            {
                if (PlayerControl.LocalPlayer.inVent)
                {
                    PlayerControl.LocalPlayer.GetModifier<GlitchHackedModifier>()!.ShowHacked();
                    PlayerControl.LocalPlayer.MyPhysics.RpcExitVent(Vent.currentVent.Id);
                    PlayerControl.LocalPlayer.MyPhysics.ExitAllVents();
                }

                @event.Cancel();
            }
            else if (HudManager.Instance.Chat.IsOpenOrOpening || MeetingHud.Instance)
            {
                @event.Cancel();
            }

            var aliveCount = PlayerControl.AllPlayerControls.ToArray().Count(x => !x.HasDied());
            var minimum = (int)OptionGroupSingleton<GeneralOptions>.Instance.PlayerCountWhenVentsDisable;

            if (PlayerControl.LocalPlayer.inVent && aliveCount <= minimum &&
                PlayerControl.LocalPlayer.Data.Role is not IGhostRole)
            {
                PlayerControl.LocalPlayer.MyPhysics.RpcExitVent(Vent.currentVent.Id);
                PlayerControl.LocalPlayer.MyPhysics.ExitAllVents();
            }

            if (aliveCount <= minimum)
            {
                @event.Cancel();
            }
        }
    }

    [RegisterEvent]
    public static void PlayerJoinEventHandler(PlayerJoinEvent @event)
    {
        Coroutines.Start(CoSendSpecData());
    }

    public static IEnumerator CoSendSpecData()
    {
        while (!AmongUsClient.Instance)
        {
            yield return null;
        }

        while (!PlayerControl.LocalPlayer)
        {
            yield return null;
        }

        if (!PlayerControl.LocalPlayer.IsHost())
        {
            yield break;
        }

        ChatPatches.RpcSetSpectatorList(SpectatorRole.TrackedSpectators);
    }

    [RegisterEvent]
    public static void PlayerLeaveEventHandler(PlayerLeaveEvent @event)
    {
        if (!MeetingHud.Instance)
        {
            return;
        }

        var player = @event.ClientData.Character;

        if (!player)
        {
            return;
        }

        var pva = MeetingHud.Instance.playerStates.First(x => x.TargetPlayerId == player.PlayerId);

        if (!pva)
        {
            return;
        }

        pva.AmDead = true;
        pva.Overlay.gameObject.SetActive(true);
        pva.Overlay.color = Color.white;
        pva.XMark.gameObject.SetActive(false);
        pva.XMark.transform.localScale = Vector3.one;

        MeetingMenu.Instances.Do(x => x.HideSingle(player.PlayerId));
    }

    private static IEnumerator CoHideHud()
    {
        yield return new WaitForSeconds(0.01f);
        HudManager.Instance.SetHudActive(false);
        HudManager.Instance.AbilityButton.SetDisabled();
        HudManager.Instance.SabotageButton.SetDisabled();
        HudManager.Instance.UseButton.SetDisabled();
    }

    private static IEnumerator CoAnimateDeath(PlayerVoteArea voteArea)
    {
        var animDic = new Dictionary<AnimationClip, AnimationClip>
        {
            { TouAssets.MeetingDeathBloodAnim1.LoadAsset(), TouAssets.MeetingDeathAnim1.LoadAsset() },
            { TouAssets.MeetingDeathBloodAnim2.LoadAsset(), TouAssets.MeetingDeathAnim2.LoadAsset() },
            { TouAssets.MeetingDeathBloodAnim3.LoadAsset(), TouAssets.MeetingDeathAnim3.LoadAsset() }
        };
        var trueAnim = animDic.Random();
        var animation = Object.Instantiate(TouAssets.MeetingDeathPrefab.LoadAsset(), voteArea.transform);
        animation.transform.localPosition = new Vector3(-0.8f, 0, 0);
        animation.transform.localScale = new Vector3(0.375f, 0.375f, 1f);
        animation.gameObject.layer = animation.transform.GetChild(0).gameObject.layer = voteArea.gameObject.layer;

        var animationRend = animation.GetComponent<SpriteRenderer>();
        animationRend.material = voteArea.PlayerIcon.cosmetics.currentBodySprite.BodySprite.material;

        voteArea.Overlay.gameObject.SetActive(false);
        animation.gameObject.SetActive(false);

        Coroutines.Start(MiscUtils.CoFlash(Palette.ImpostorRed, 0.5f, 0.15f));
        var seconds = Random.RandomRange(0.4f, 1.1f);
        // if there's less than 6 players alive, animation will play instantly
        if (Helpers.GetAlivePlayers().Count <= 5)
        {
            seconds = 0.01f;
        }

        yield return new WaitForSeconds(seconds);

        voteArea.PlayerIcon.gameObject.SetActive(false);
        animation.gameObject.SetActive(true);
        var bodysAnim = animation.GetComponent<SpriteAnim>();

        var bloodAnim = animation.transform.GetChild(0).GetComponent<SpriteAnim>();

        bloodAnim.Play(trueAnim.Key);
        bodysAnim.Play(trueAnim.Value);

        bodysAnim.SetSpeed(1.05f);
        bloodAnim.SetSpeed(1.05f);
        var bodyAnimLength = bodysAnim.m_currAnim.length;

        yield return new WaitForSeconds(0.1f);
        SoundManager.Instance.PlaySound(voteArea.GetPlayer()!.KillSfx, false);

        yield return new WaitForSeconds(bodyAnimLength - 0.25f);
        voteArea.Overlay.gameObject.SetActive(true);
        animation.gameObject.Destroy();
        voteArea.XMark.gameObject.SetActive(true);
        SoundManager.Instance.PlaySound(MeetingHud.Instance.MeetingIntro.PlayerDeadSound, false);
        Coroutines.Start(MiscUtils.BetterBloop(voteArea.XMark.transform));
        voteArea.Overlay.gameObject.SetActive(true);
    }

    private static void HandleMeetingMurder(MeetingHud instance, PlayerControl source, PlayerControl target)
    {
        var timer = (int)OptionGroupSingleton<GeneralOptions>.Instance.AddedMeetingDeathTimer;
        if (timer > 0 && timer <= 15)
        {
            instance.discussionTimer -= timer;
        }
        else if (timer >= 15)
        {
            instance.discussionTimer -= 15f;
        }

        // To handle murders during a meeting
        var targetVoteArea = instance.playerStates.First(x => x.TargetPlayerId == target.PlayerId);

        if (!targetVoteArea)
        {
            return;
        }

        if (targetVoteArea.DidVote)
        {
            targetVoteArea.UnsetVote();
        }

        targetVoteArea.AmDead = true;
        targetVoteArea.Overlay.gameObject.SetActive(true);
        targetVoteArea.Overlay.color = Color.white;
        targetVoteArea.XMark.gameObject.SetActive(false);
        targetVoteArea.XMark.transform.localScale = Vector3.one;

        if (Minigame.Instance != null)
        {
            Minigame.Instance.Close();
            Minigame.Instance.Close();
        }

        targetVoteArea.Overlay.gameObject.SetActive(false);
        if (target.GetRoleWhenAlive() is MayorRole mayor && mayor.Revealed)
        {
            MayorRole.DestroyReveal(targetVoteArea);
        }

        Coroutines.Start(CoAnimateDeath(targetVoteArea));

        // hide meeting menu buttons on the victim's screen
        if (target.AmOwner)
        {
            MeetingMenu.Instances.Do(x => x.HideButtons());
            Coroutines.Start(CoHideHud());
        }
        // hide meeting menu button for victim
        else if (!source.AmOwner && !target.AmOwner)
        {
            MeetingMenu.Instances.Do(x => x.HideSingle(target.PlayerId));
            if (PlayerControl.LocalPlayer.Data.Role is SwapperRole swapperRole)
            {
                if (swapperRole.Swap1 == targetVoteArea)
                {
                    swapperRole.Swap1 = null;
                }
                else if (swapperRole.Swap2 == targetVoteArea)
                {
                    swapperRole.Swap2 = null;
                }
            }
        }

        foreach (var pva in instance.playerStates)
        {
            if (pva.VotedFor != target.PlayerId || pva.AmDead)
            {
                continue;
            }

            pva.UnsetVote();

            var voteAreaPlayer = MiscUtils.PlayerById(pva.TargetPlayerId);

            if (voteAreaPlayer == null)
            {
                continue;
            }

            var voteData = voteAreaPlayer.GetVoteData();
            var votes = voteData.Votes.RemoveAll(x => x.Suspect == target.PlayerId);
            voteData.VotesRemaining += votes;

            if (!voteAreaPlayer.AmOwner)
            {
                continue;
            }

            instance.ClearVote();
        }

        instance.SetDirtyBit(1U);

        if (AmongUsClient.Instance.AmHost)
        {
            instance.CheckForEndVoting();
        }
    }

    [RegisterEvent]
    public static void VotingCompleteHandler(VotingCompleteEvent @event)
    {
        if (Minigame.Instance)
        {
            Minigame.Instance.Close();
            Minigame.Instance.Close();
        }
    }
}
﻿using System.Collections;
using System.Globalization;
using System.Text;
using AmongUs.GameOptions;
using Il2CppInterop.Runtime.Attributes;
using MiraAPI.Events;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Patches.Stubs;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using Reactor.Networking.Attributes;
using Reactor.Utilities;
using Reactor.Utilities.Extensions;
using TownOfUs.Events.TouEvents;
using TownOfUs.Modifiers.Crewmate;
using TownOfUs.Modules;
using TownOfUs.Options.Roles.Crewmate;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfUs.Roles.Crewmate;

public sealed class PlumberRole(IntPtr cppPtr) : CrewmateRole(cppPtr), ITownOfUsRole, IWikiDiscoverable, IDoomable
{
    public override bool IsAffectedByComms => false;

    [HideFromIl2Cpp] public List<int> FutureBlocks { get; set; } = [];

    // Blocked vent, remaining rounds
    [HideFromIl2Cpp] public static List<KeyValuePair<int, int>> VentsBlocked { get; set; } = [];


    // Barricade object, remaining rounds
    [HideFromIl2Cpp] public static List<KeyValuePair<GameObject, int>> Barricades { get; set; } = [];

    public DoomableType DoomHintType => DoomableType.Trickster;
    public string LocaleKey => "Plumber";
    public string RoleName => TouLocale.Get($"TouRole{LocaleKey}");
    public string RoleDescription => TouLocale.GetParsed($"TouRole{LocaleKey}IntroBlurb");
    public string RoleLongDescription => TouLocale.GetParsed($"TouRole{LocaleKey}TabDescription");

    public string GetAdvancedDescription()
    {
        return
            TouLocale.GetParsed($"TouRole{LocaleKey}WikiDescription") +
            MiscUtils.AppendOptionsText(GetType());
    }

    [HideFromIl2Cpp]
    public List<CustomButtonWikiDescription> Abilities
    {
        get
        {
            return new List<CustomButtonWikiDescription>
            {
                new(TouLocale.GetParsed($"TouRole{LocaleKey}Flush", "Flush"),
                    TouLocale.GetParsed($"TouRole{LocaleKey}FlushWikiDescription"),
                    TouCrewAssets.FlushSprite),
                new(TouLocale.GetParsed($"TouRole{LocaleKey}Block", "Block"),
                    TouLocale.GetParsed($"TouRole{LocaleKey}BlockWikiDescription"),
                    TouCrewAssets.BlockSprite)
            };
        }
    }

    public Color RoleColor => TownOfUsColors.Plumber;
    public ModdedRoleTeams Team => ModdedRoleTeams.Crewmate;
    public RoleAlignment RoleAlignment => RoleAlignment.CrewmateSupport;

    public CustomRoleConfiguration Configuration => new(this)
    {
        IntroSound = CustomRoleUtils.GetIntroSound(RoleTypes.Engineer),
        Icon = TouRoleIcons.Plumber
    };

    public void LobbyStart()
    {
        Clear();
    }

    [HideFromIl2Cpp]
    public StringBuilder SetTabText()
    {
        var stringB = ITownOfUsRole.SetNewTabText(this);
        var duration = (int)OptionGroupSingleton<PlumberOptions>.Instance.BarricadeRoundDuration;
        var text = duration == 0 ? "Barricades Stay Forever." : $"Barricades Stay For {duration} Round(s)";
        stringB.Append(CultureInfo.InvariantCulture,
            $"\n<b><size=60%>Note: {text}</size></b>");
        if (VentsBlocked.Count > 0 || FutureBlocks.Count > 0)
        {
            stringB.Append(CultureInfo.InvariantCulture,
                $"\n<b>Vents List:</b>");

            if (VentsBlocked.Count > 0)
            {
                foreach (var ventPair in VentsBlocked)
                {
                    var vent = Helpers.GetVentById(ventPair.Key);
                    if (vent == null)
                    {
                        continue;
                    }

                    var text2 = duration == 0 ? string.Empty : $": {ventPair.Value} Round(s) Remaining";
                    stringB.Append(CultureInfo.InvariantCulture,
                        $"\n{MiscUtils.GetRoomName(vent.transform.position)} Vent{text2}");
                }
            }

            if (FutureBlocks.Count > 0)
            {
                foreach (var ventId in FutureBlocks)
                {
                    var vent = Helpers.GetVentById(ventId);
                    if (vent == null)
                    {
                        continue;
                    }

                    stringB.Append(CultureInfo.InvariantCulture,
                        $"\n<color=#BFBFBF>{MiscUtils.GetRoomName(vent.transform.position)} Vent: Preparing...</color>");
                }
            }
        }

        return stringB;
    }

    public override void Initialize(PlayerControl player)
    {
        RoleBehaviourStubs.Initialize(this, player);
        if (TutorialManager.InstanceExists)
        {
            Clear();
        }
    }

    public override void Deinitialize(PlayerControl targetPlayer)
    {
        RoleBehaviourStubs.Deinitialize(this, targetPlayer);

        SubClear();
    }

    public void SubClear()
    {
        FutureBlocks.Clear();
    }

    public void Clear()
    {
        if (Barricades.Count > 0)
        {
            foreach (var barricade in Barricades.Select(x => x.Key))
            {
                if (barricade == null)
                {
                    continue;
                }

                Destroy(barricade);
            }
        }

        FutureBlocks.Clear();
        VentsBlocked.Clear();
        Barricades.Clear();
    }

    public static void ClearAll()
    {
        if (Barricades.Count > 0)
        {
            foreach (var barricade in Barricades.Select(x => x.Key))
            {
                if (barricade == null)
                {
                    continue;
                }

                Destroy(barricade);
            }
        }

        VentsBlocked.Clear();
        Barricades.Clear();
    }

    public void SetupBarricades()
    {
        foreach (var ventId in FutureBlocks)
        {
            VentsBlocked.Add(new(ventId, (int)OptionGroupSingleton<PlumberOptions>.Instance.BarricadeRoundDuration));

            GameObject barricade = new("Barricade");

            var trueVent = Helpers.GetVentById(ventId);

            if (trueVent == null)
            {
                continue;
            }

            barricade.transform.SetParent(trueVent.transform);
            barricade.gameObject.layer = trueVent.gameObject.layer;

            var render = barricade.AddComponent<SpriteRenderer>();
            var spriteList = new List<Sprite>
            {
                TouAssets.BarricadeVentSprite.LoadAsset(),
                TouAssets.BarricadeVentSprite2.LoadAsset(),
                TouAssets.BarricadeVentSprite3.LoadAsset(),
            };
            var trueBarricade = spriteList.Random();
            render.sprite = trueBarricade;

            switch (ShipStatus.Instance.Type)
            {
                case ShipStatus.MapType.Fungle:
                    render.sprite = TouAssets.BarricadeFungleSprite.LoadAsset();
                    barricade.transform.localPosition = new Vector3(0.03f, -0.107f, -0.001f);
                    break;
                case ShipStatus.MapType.Pb:
                    barricade.transform.localPosition = new Vector3(0, 0.05f, -0.001f);
                    barricade.transform.localScale = new Vector3(0.8f, 0.7f, 1f);
                    break;
                default:
                    barricade.transform.localPosition = new Vector3(0, 0, -0.001f);
                    break;
            }

            if (trueVent.gameObject.name == "LowerCentralVent" && ModCompatibility.IsSubmerged())
            {
                barricade.transform.localPosition = new Vector3(0, 0.7f, -0.001f);
                barricade.transform.localScale = new Vector3(1.05f, 1.15f, 1.0625f);
            }

            if (ModCompatibility.IsLevelImpostor())
            {
                switch (ModCompatibility.GetLIVentType(trueVent))
                {
                    case "util-vent3":
                        render.sprite = TouAssets.BarricadeFungleSprite.LoadAsset();
                        barricade.transform.localPosition = new Vector3(0.03f, -0.107f, -0.001f);
                        break;
                    case "util-vent2":
                        barricade.transform.localPosition = new Vector3(0, 0.05f, -0.001f);
                        barricade.transform.localScale = new Vector3(0.8f, 0.7f, 1f);
                        break;
                    default:
                        barricade.transform.localPosition = new Vector3(0, 0, -0.001f);
                        break;
                }
            }

            Barricades.Add(new(barricade, (int)OptionGroupSingleton<PlumberOptions>.Instance.BarricadeRoundDuration));
        }

        FutureBlocks.Clear();
    }

    public static IEnumerator SeeVenter(PlayerControl plumber)
    {
        var playersInVent = PlayerControl.AllPlayerControls.ToArray().Where(x => x.inVent);

        foreach (var player in playersInVent)
        {
            player.AddModifier<PlumberVenterModifier>(plumber, Color.white);
        }

        yield return new WaitForSeconds(1f);

        foreach (var player in ModifierUtils.GetPlayersWithModifier<PlumberVenterModifier>(x => x.Owner == plumber))
        {
            player.RemoveModifier<PlumberVenterModifier>();
        }
    }

    [MethodRpc((uint)TownOfUsRpc.PlumberFlush)]
    public static void RpcPlumberFlush(PlayerControl player)
    {
        if (player.Data.Role is not PlumberRole)
        {
            Logger<TownOfUsPlugin>.Error("RpcPlumberFlush - Invalid Plumber");
            return;
        }

        var touAbilityEvent = new TouAbilityEvent(AbilityType.PlumberFlush, player);
        MiraEventManager.InvokeEvent(touAbilityEvent);

        if (PlayerControl.LocalPlayer.inVent)
        {
            PlayerControl.LocalPlayer.MyPhysics.RpcExitVent(Vent.currentVent.Id);
            PlayerControl.LocalPlayer.MyPhysics.ExitAllVents();

            Coroutines.Start(MiscUtils.CoFlash(TownOfUsColors.Plumber));
        }

        if (!player.AmOwner)
        {
            return;
        }

        var someoneInVent = PlayerControl.AllPlayerControls.ToArray().Any(x => x.inVent);
        if (!someoneInVent)
        {
            return;
        }

        Coroutines.Start(MiscUtils.CoFlash(TownOfUsColors.Plumber));
        Coroutines.Start(SeeVenter(player));
    }

    [MethodRpc((uint)TownOfUsRpc.PlumberBlockVent)]
    public static void RpcPlumberBlockVent(PlayerControl player, int ventId)
    {
        if (player.Data.Role is not PlumberRole plumber)
        {
            Logger<TownOfUsPlugin>.Error("RpcPlumberBlockVent - Invalid Plumber");
            return;
        }

        if (!plumber.FutureBlocks.Contains(ventId))
        {
            plumber.FutureBlocks.Add(ventId);
        }

        var touAbilityEvent = new TouAbilityEvent(AbilityType.PlumberBlock, player, Helpers.GetVentById(ventId));
        MiraEventManager.InvokeEvent(touAbilityEvent);
    }
}
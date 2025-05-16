using System.Collections;
using System.Text;
using AmongUs.GameOptions;
using Il2CppInterop.Runtime.Attributes;
using MiraAPI.Modifiers;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using Reactor.Networking.Attributes;
using Reactor.Utilities;
using TownOfUs.Modifiers.Crewmate;
using TownOfUs.Modules;
using TownOfUs.Modules.Wiki;
using TownOfUs.Patches.Stubs;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfUs.Roles.Crewmate;

public sealed class PlumberRole(IntPtr cppPtr) : CrewmateRole(cppPtr), ITownOfUsRole, IWikiDiscoverable, IDoomable
{
    public string RoleName => "Plumber";
    public string RoleDescription => "Get The Rats Out Of The Sewers";
    public string RoleLongDescription => "Flush the vent system to rid of venters, and\nbarricade vents to block them the next round";
    public Color RoleColor => TownOfUsColors.Plumber;
    public ModdedRoleTeams Team => ModdedRoleTeams.Crewmate;
    public RoleAlignment RoleAlignment => RoleAlignment.CrewmateSupport;
    public DoomableType DoomHintType => DoomableType.Trickster;
    public override bool IsAffectedByComms => false;

    public CustomRoleConfiguration Configuration => new(this)
    {
        IntroSound = CustomRoleUtils.GetIntroSound(RoleTypes.Engineer),
        Icon = TouRoleIcons.Plumber,
    };

    [HideFromIl2Cpp]
    public List<int> FutureBlocks { get; set; } = [];

    [HideFromIl2Cpp]
    public List<int> VentsBlocked { get; set; } = [];

    [HideFromIl2Cpp]
    public List<GameObject> Barricades { get; set; } = [];

    public override void Deinitialize(PlayerControl targetPlayer)
    {
        RoleStubs.RoleBehaviourDeinitialize(this, targetPlayer);

        Clear();
    }

    public void Clear()
    {
        foreach (var barricade in Barricades)
        {
            Destroy(barricade);
        }

        FutureBlocks.Clear();
        VentsBlocked.Clear();
        Barricades.Clear();
    }

    public void LobbyStart()
    {
        Clear();
    }

    public void SetupBarricades()
    {
        foreach (var ventId in FutureBlocks)
        {
            VentsBlocked.Add(ventId);

            GameObject barricade = new("Barricade");

            Vent? trueVent = Helpers.GetVentById(ventId);

            if (trueVent == null) continue;

            barricade.transform.SetParent(trueVent.transform);
            barricade.gameObject.layer = trueVent.gameObject.layer;

            SpriteRenderer render = barricade.AddComponent<SpriteRenderer>();

            switch (ShipStatus.Instance.Type)
            {
                case ShipStatus.MapType.Fungle:
                    render.sprite = TouAssets.BarricadeFungleSprite.LoadAsset();
                    barricade.transform.localPosition = new Vector3(0.03f, -0.107f, -0.001f);
                    break;
                case ShipStatus.MapType.Pb:
                    render.sprite = TouAssets.BarricadeVentSprite.LoadAsset();
                    barricade.transform.localPosition = new Vector3(0, 0.05f, -0.001f);
                    barricade.transform.localScale = new Vector3(0.8f, 0.7f, 1f);
                    break;
                default:
                    render.sprite = TouAssets.BarricadeVentSprite.LoadAsset();
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
                        render.sprite = TouAssets.BarricadeVentSprite.LoadAsset();
                        barricade.transform.localPosition = new Vector3(0, 0.05f, -0.001f);
                        barricade.transform.localScale = new Vector3(0.8f, 0.7f, 1f);
                        break;
                    default:
                        render.sprite = TouAssets.BarricadeVentSprite.LoadAsset();
                        barricade.transform.localPosition = new Vector3(0, 0, -0.001f);
                        break;
                }
            }

            Barricades.Add(barricade);
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

    [MethodRpc((uint)TownOfUsRpc.PlumberFlush, SendImmediately = true)]
    public static void RpcPlumberFlush(PlayerControl player)
    {
        if (player.Data.Role is not PlumberRole)
        {
            Logger<TownOfUsPlugin>.Error("RpcPlumberFlush - Invalid Plumber");
            return;
        }

        if (PlayerControl.LocalPlayer.inVent)
        {
            PlayerControl.LocalPlayer.MyPhysics.RpcExitVent(Vent.currentVent.Id);
            PlayerControl.LocalPlayer.MyPhysics.ExitAllVents();

            Coroutines.Start(MiscUtils.CoFlash(TownOfUsColors.Plumber));
        }

        if (!player.AmOwner) return;
        var someoneInVent = PlayerControl.AllPlayerControls.ToArray().Any(x => x.inVent);
        if (!someoneInVent) return;

        Coroutines.Start(MiscUtils.CoFlash(TownOfUsColors.Plumber));
        Coroutines.Start(SeeVenter(player));
    }

    [MethodRpc((uint)TownOfUsRpc.PlumberBlockVent, SendImmediately = true)]
    public static void RpcPlumberBlockVent(PlayerControl player, int ventId)
    {
        if (player.Data.Role is not PlumberRole plumber)
        {
            Logger<TownOfUsPlugin>.Error("RpcPlumberBlockVent - Invalid Plumber");
            return;
        }

        if (!plumber.FutureBlocks.Contains(ventId))
            plumber.FutureBlocks.Add(ventId);
    }

    [HideFromIl2Cpp]
    public StringBuilder SetTabText()
    {
        return ITownOfUsRole.SetNewTabText(this);
    }
    public string GetAdvancedDescription()
    {
        return
            "The Plumber is a Crewmate Support role that can place Barricades on vents and Flush anyone out of vents.."
               + MiscUtils.AppendOptionsText(GetType());
    }

    [HideFromIl2Cpp]
    public List<CustomButtonWikiDescription> Abilities { get; } = [
        new("Flush",
            $"Flushing the vents makes every vent open and close, kicking out anyone who is actively in a vent. The Plumber also gets an arrow pointing to every flushed player for one second.",
            TouCrewAssets.FlushSprite),
        new("Barricade",
            $"Barricading a vent places a barricade on the vent selected for the next round, preventing players from using it.",
            TouCrewAssets.BarricadeSprite),
    ];
}

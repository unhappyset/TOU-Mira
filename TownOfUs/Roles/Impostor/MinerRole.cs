using System.Text;
using Il2CppInterop.Runtime.Attributes;
using MiraAPI.Roles;
using Reactor.Networking.Attributes;
using Reactor.Utilities;
using TownOfUs.Modules;
using TownOfUs.Modules.Wiki;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfUs.Roles.Impostor;

public sealed class MinerRole(IntPtr cppPtr) : ImpostorRole(cppPtr), ITownOfUsRole, IWikiDiscoverable
{
    public string RoleName => "Miner";
    public string RoleDescription => "From The Top, Make It Drop, That's A Vent";
    public string RoleLongDescription => "Place interconnected vents around the map";
    public Color RoleColor => TownOfUsColors.Impostor;
    public ModdedRoleTeams Team => ModdedRoleTeams.Impostor;
    public RoleAlignment RoleAlignment => RoleAlignment.ImpostorSupport;
    public CustomRoleConfiguration Configuration => new(this)
    {
        Icon = TouRoleIcons.Miner,
        OptionsScreenshot = TouImpAssets.MinerRoleBanner,
        IntroSound = TouAudio.MineSound,
    };

    private readonly List<Vent> _vents = new();

    [MethodRpc((uint)TownOfUsRpc.PlaceVent, SendImmediately = true)]
    public static void RpcPlaceVent(PlayerControl player, int ventId, Vector2 position, float zAxis)
    {
        if (player.Data.Role is not MinerRole)
        {
            Logger<TownOfUsPlugin>.Error("RpcPlaceVent - Invalid miner");
            return;
        }

        var ventPrefab = FindObjectOfType<Vent>();
        var vent = Instantiate(ventPrefab, ventPrefab.transform.parent);

        vent.Id = ventId;
        vent.transform.position = new Vector3(position.x, position.y, zAxis);

        var miner = player.GetRole<MinerRole>();
        if (miner == null) return;

        if (miner._vents.Count > 0)
        {
            var leftVent = miner._vents[^1];
            vent.Left = leftVent;
            leftVent.Right = vent;
        }
        else
        {
            vent.Left = null;
        }

        vent.Right = null;
        vent.Center = null;

        var allVents = ShipStatus.Instance.AllVents.ToList();
        allVents.Add(vent);
        ShipStatus.Instance.AllVents = allVents.ToArray();

        miner._vents.Add(vent);

        if (ModCompatibility.SubLoaded)
        {
            vent.gameObject.layer = 12;
            vent.gameObject.AddSubmergedComponent("ElevatorMover"); // just in case elevator vent is not blocked
            if (vent.gameObject.transform.position.y > -7)
            {
                vent.gameObject.transform.position = new Vector3(vent.gameObject.transform.position.x, vent.gameObject.transform.position.y, 0.03f);
            }
            else
            {
                vent.gameObject.transform.position = new Vector3(vent.gameObject.transform.position.x, vent.gameObject.transform.position.y, 0.0009f);
                vent.gameObject.transform.localPosition = new Vector3(vent.gameObject.transform.localPosition.x, vent.gameObject.transform.localPosition.y, -0.003f);
            }
        }
    }

    [HideFromIl2Cpp]
    public StringBuilder SetTabText()
    {
        return ITownOfUsRole.SetNewTabText(this);
    }

    public string GetAdvancedDescription()
    {
        return "The Miner is an Impostor Support role that can create vents." + MiscUtils.AppendOptionsText(GetType());
    }

    [HideFromIl2Cpp]
    public List<CustomButtonWikiDescription> Abilities { get; } = [
        new("Mine",
            "Place a vent where you are standing. These vents won't connect to already existing vents on the map but with each other.",
            TouImpAssets.MineSprite)    
    ];
}

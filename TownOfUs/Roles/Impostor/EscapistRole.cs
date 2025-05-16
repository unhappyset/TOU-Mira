using System.Text;
using Il2CppInterop.Runtime.Attributes;
using MiraAPI.GameOptions;
using MiraAPI.Roles;
using Reactor.Networking.Attributes;
using Reactor.Utilities;
using TownOfUs.Modules.Wiki;
using TownOfUs.Options.Roles.Impostor;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfUs.Roles.Impostor;

public sealed class EscapistRole(IntPtr cppPtr) : ImpostorRole(cppPtr), ITownOfUsRole, IWikiDiscoverable, IDoomable
{
    public string RoleName => "Escapist";
    public string RoleDescription => "Get Away From Kills With Ease";
    public string RoleLongDescription => "Teleport to get away from the scene of the crime";
    public Color RoleColor => TownOfUsColors.Impostor;
    public ModdedRoleTeams Team => ModdedRoleTeams.Impostor;
    public RoleAlignment RoleAlignment => RoleAlignment.ImpostorConcealing;
    public DoomableType DoomHintType => DoomableType.Protective;
    public CustomRoleConfiguration Configuration => new(this)
    {
        Icon = TouRoleIcons.Escapist,
        IntroSound = TouAudio.TimeLordIntroSound,
        CanUseVent = OptionGroupSingleton<EscapistOptions>.Instance.CanVent,
    };

    public Vector2? MarkedLocation { get; set; }

    [MethodRpc((uint)TownOfUsRpc.Recall, SendImmediately = true)]
    public static void RpcRecall(PlayerControl player)
    {
        if (player.Data.Role is not EscapistRole)
        {
            Logger<TownOfUsPlugin>.Error("RpcRecall - Invalid escapist");
            return;
        }
    }

    [HideFromIl2Cpp]
    public StringBuilder SetTabText()
    {
        return ITownOfUsRole.SetNewTabText(this);
    }

    public string GetAdvancedDescription()
    {
        return "The Escapist is an Impostor Concealing role that can mark a location and then recall (teleport) to that location."
            + MiscUtils.AppendOptionsText(GetType());
    }

    [HideFromIl2Cpp]
    public List<CustomButtonWikiDescription> Abilities { get; } = [
        new("Mark",
            "Mark a location for later use.",
            TouImpAssets.MarkSprite),
        new("Recall",
            "Recall to the marked location, resetting it in the process.",
            TouImpAssets.RecallSprite)
    ];
}

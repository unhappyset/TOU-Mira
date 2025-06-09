using System.Text;
using Il2CppInterop.Runtime.Attributes;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using Reactor.Networking.Attributes;
using Reactor.Networking.Rpc;
using Reactor.Utilities;
using TownOfUs.Modules.Components;
using TownOfUs.Modules.Wiki;
using TownOfUs.Options.Roles.Impostor;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfUs.Roles.Impostor;

public sealed class JanitorRole(IntPtr cppPtr) : ImpostorRole(cppPtr), ITownOfUsRole, IWikiDiscoverable, IDoomable
{
    public string RoleName => "Janitor";
    public string RoleDescription => "Sanitize The Ship";
    public string RoleLongDescription => "Clean bodies to hide kills" + (OptionGroupSingleton<JanitorOptions>.Instance.CleanDelay == 0 ? string.Empty : "\n<b>You must stay next to the body while cleaning.</b>");
    public Color RoleColor => TownOfUsColors.Impostor;
    public ModdedRoleTeams Team => ModdedRoleTeams.Impostor;
    public RoleAlignment RoleAlignment => RoleAlignment.ImpostorSupport;
    public DoomableType DoomHintType => DoomableType.Death;
    public CustomRoleConfiguration Configuration => new(this)
    {
        UseVanillaKillButton = true,
        Icon = TouRoleIcons.Janitor,
        IntroSound = TouAudio.JanitorCleanSound,
    };
    public void FixedUpdate()
    {
        if (Player == null || Player.Data.Role is not JanitorRole || Player.HasDied() || !Player.AmOwner || MeetingHud.Instance != null || (!HudManager.Instance.UseButton.isActiveAndEnabled && !HudManager.Instance.PetButton.isActiveAndEnabled)) return;
        HudManager.Instance.KillButton.ToggleVisible(OptionGroupSingleton<JanitorOptions>.Instance.JanitorKill || (Player != null && Player.GetModifiers<BaseModifier>().Any(x => x is ICachedRole)) || (Player != null && MiscUtils.ImpAliveCount == 1));
    }

    [MethodRpc((uint)TownOfUsRpc.CleanBody, LocalHandling = RpcLocalHandling.Before, SendImmediately = true)]
    public static void RpcCleanBody(PlayerControl player, byte bodyId)
    {
        if (player.Data.Role is not JanitorRole)
        {
            Logger<TownOfUsPlugin>.Error("RpcCleanBody - Invalid Janitor");
            return;
        }

        var body = Helpers.GetBodyById(bodyId);

        if (body != null)
        {
            Coroutines.Start(body.CoClean());
            Coroutines.Start(CrimeSceneComponent.CoClean(body));
        }
    }

    [HideFromIl2Cpp]
    public StringBuilder SetTabText()
    {
        return ITownOfUsRole.SetNewTabText(this);
    }

    public string GetAdvancedDescription()
    {
        return "The Janitor is an Impostor Support role that can clean dead bodies." + MiscUtils.AppendOptionsText(GetType());
    }

    [HideFromIl2Cpp]
    public List<CustomButtonWikiDescription> Abilities { get; } = [
        new("Clean",
            "Clean a dead body, making it disapear and making it unreportable.",
            TouImpAssets.CleanButtonSprite)
    ];
}

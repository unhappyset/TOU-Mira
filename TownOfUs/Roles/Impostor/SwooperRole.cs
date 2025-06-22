using AmongUs.GameOptions;
using System.Text;
using Il2CppInterop.Runtime.Attributes;
using MiraAPI.GameOptions;
using MiraAPI.Roles;
using TownOfUs.Options.Roles.Impostor;
using UnityEngine;
using TownOfUs.Modules.Wiki;
using TownOfUs.Utilities;

namespace TownOfUs.Roles.Impostor;

public sealed class SwooperRole(IntPtr cppPtr) : ImpostorRole(cppPtr), ITownOfUsRole, IWikiDiscoverable, IDoomable
{
    public string RoleName => "Swooper";
    public string RoleDescription => "Turn Invisible Temporarily";
    public string RoleLongDescription => "Turn invisible and sneakily kill";
    public Color RoleColor => TownOfUsColors.Impostor;
    public ModdedRoleTeams Team => ModdedRoleTeams.Impostor;
    public RoleAlignment RoleAlignment => RoleAlignment.ImpostorConcealing;
    public DoomableType DoomHintType => DoomableType.Hunter;
    public CustomRoleConfiguration Configuration => new(this)
    {
        CanUseVent = OptionGroupSingleton<SwooperOptions>.Instance.CanVent,
        Icon = TouRoleIcons.Swooper,
        IntroSound = CustomRoleUtils.GetIntroSound(RoleTypes.Phantom),
    };

    [HideFromIl2Cpp]
    public StringBuilder SetTabText()
    {
        return ITownOfUsRole.SetNewTabText(this);
    }

    public string GetAdvancedDescription()
    {
        return "The Swooper is an Impostor Concealing role that can temporarily turn invisible."
            + MiscUtils.AppendOptionsText(GetType());
    }

    [HideFromIl2Cpp]
    public List<CustomButtonWikiDescription> Abilities { get; } = [
        new("Swoop",
            "Turn invisible to all players except Impostors.",
            TouImpAssets.SwoopSprite),
        new("Unswoop",
            "Cancel your swoop early, or let it finish fully to make yourself visible once again.",
            TouImpAssets.UnswoopSprite)
    ];
}

using System.Text;
using AmongUs.GameOptions;
using Il2CppInterop.Runtime.Attributes;
using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Modifiers;
using MiraAPI.Roles;
using Reactor.Networking.Attributes;
using Reactor.Utilities;
using System.Globalization;
using TownOfUs.Buttons.Neutral;
using TownOfUs.Modifiers.Neutral;
using TownOfUs.Options.Roles.Neutral;
using TownOfUs.Utilities;
using UnityEngine;
using TownOfUs.Modules.Wiki;

namespace TownOfUs.Roles.Neutral;

public sealed class MercenaryRole(IntPtr cppPtr) : NeutralRole(cppPtr), ITownOfUsRole, IWikiDiscoverable, IDoomable
{
    public string RoleName => "Mercenary";
    public string RoleDescription => "Bribe The Crewmates";
    public string RoleLongDescription => "Guard crewmates, and then bribe the winners!";
    public Color RoleColor => TownOfUsColors.Mercenary;
    public ModdedRoleTeams Team => ModdedRoleTeams.Custom;
    public RoleAlignment RoleAlignment => RoleAlignment.NeutralBenign;
    public DoomableType DoomHintType => DoomableType.Insight;
    public CustomRoleConfiguration Configuration => new(this)
    {
        IntroSound = CustomRoleUtils.GetIntroSound(RoleTypes.Crewmate),
        Icon = TouRoleIcons.Mercenary,
        GhostRole = (RoleTypes)RoleId.Get<NeutralGhostRole>(),
    };

    public static int BrideCost => (int)OptionGroupSingleton<MercenaryOptions>.Instance.BribeCost;

    public int Gold { get; set; }
    public bool CanBribe => Gold >= BrideCost;

    public override bool DidWin(GameOverReason gameOverReason)
    {
        var bribed = ModifierUtils.GetPlayersWithModifier<MercenaryBribedModifier>();

        return bribed.Any(x => x.Data.Role.DidWin(gameOverReason));
    }

    [HideFromIl2Cpp]
    public StringBuilder SetTabText()
    {
        var stringB = ITownOfUsRole.SetNewTabText(this);
        var players = ModifierUtils.GetPlayersWithModifier<MercenaryBribedModifier>();

        stringB.Append(CultureInfo.InvariantCulture, $"\n<b>Gold:</b> {Gold}");

        var playerControls = players as PlayerControl[] ?? [.. players];
        if (playerControls.Length != 0)
        {
            stringB.Append($"\n<b>Bribed:</b>");
        }

        foreach (var player in playerControls)
        {
            stringB.Append(CultureInfo.InvariantCulture, $"\n{player.Data.PlayerName}");
        }

        return stringB;
    }

    public void AddPayment()
    {
        Gold++;

        if (CanBribe)
        {
            CustomButtonSingleton<MercenaryBribeButton>.Instance.SetActive(true, this);
        }
    }

    public void Clear()
    {
        Gold = 0;

        CustomButtonSingleton<MercenaryGuardButton>.Instance.SetActive(true, this);
        CustomButtonSingleton<MercenaryBribeButton>.Instance.SetActive(false, this);
    }

    [MethodRpc((uint)TownOfUsRpc.Guarded, SendImmediately = true)]
    public static void RpcGuarded(PlayerControl player)
    {
        if (player.Data.Role is not MercenaryRole)
        {
            Logger<TownOfUsPlugin>.Error("RpcGuarded - Invalid mercenary");
            return;
        }

        if (!player.AmOwner) return;

        var mercenary = player.GetRole<MercenaryRole>();
        mercenary?.AddPayment();
    }
    public string GetAdvancedDescription()
    {
        return
            "The Mercenary is a Neutral Evil role that can only win by bribing players, allowing them to gain multiple win conditions."
               + MiscUtils.AppendOptionsText(GetType());
    }

    [HideFromIl2Cpp]
    public List<CustomButtonWikiDescription> Abilities { get; } = [
        new("Guard",
            $"Guarding a player allows the Mercenary to absorb an ability used on the target. This will grant them gold, for Bribing. If any bribed targets win, the Mercenary will win with them.",
            TouNeutAssets.GuardSprite),
        new("Bribe",
            $"Bribing a player allows the Mercenary to gain their win condition, given that they have gold to spare.",
            TouNeutAssets.BribeSprite),
    ];
}

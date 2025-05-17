using System.Text;
using AmongUs.GameOptions;
using Il2CppInterop.Runtime.Attributes;
using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Modifiers;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using Reactor.Networking.Attributes;
using TownOfUs.Buttons.Neutral;
using TownOfUs.Modifiers.Neutral;
using TownOfUs.Modules.Wiki;
using TownOfUs.Options.Roles.Neutral;
using TownOfUs.Patches.Stubs;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfUs.Roles.Neutral;

public sealed class PlaguebearerRole(IntPtr cppPtr) : NeutralRole(cppPtr), ITownOfUsRole, IWikiDiscoverable, IDoomable
{
    public string RoleName => "Plaguebearer";
    public string RoleDescription => "Infect Everyone To Become <color=#4D4D4DFF>Pestilence</color>";
    public string RoleLongDescription => "Infect everyone to become <color=#4D4D4DFF>Pestilence</color>";
    public Color RoleColor => TownOfUsColors.Plaguebearer;
    public ModdedRoleTeams Team => ModdedRoleTeams.Custom;
    public RoleAlignment RoleAlignment => RoleAlignment.NeutralKilling;
    public DoomableType DoomHintType => DoomableType.Fearmonger;
    public CustomRoleConfiguration Configuration => new(this)
    {
        IntroSound = CustomRoleUtils.GetIntroSound(RoleTypes.Phantom),
        Icon = TouRoleIcons.Plaguebearer,
        MaxRoleCount = 1,
        GhostRole = (RoleTypes)RoleId.Get<NeutralGhostRole>(),
    };
    public override void Initialize(PlayerControl player)
    {
        RoleStubs.RoleBehaviourInitialize(this, player);
        player.AddModifier<PlaguebearerInfectedModifier>(Player.PlayerId);
    }
    public override void Deinitialize(PlayerControl targetPlayer)
    {
        RoleStubs.RoleBehaviourDeinitialize(this, targetPlayer);
        targetPlayer.RemoveModifier<PlaguebearerInfectedModifier>();
    }

    [HideFromIl2Cpp]
    public StringBuilder SetTabText()
    {
        var stringB = ITownOfUsRole.SetNewTabText(this);

        var allInfected = PlayerControl.AllPlayerControls.ToArray().Where(x => !x.HasDied() && x != Player && x.GetModifier<PlaguebearerInfectedModifier>()?.PlagueBearerId == Player.PlayerId);

        if (allInfected.Any())
        {
            stringB.Append("\n<b>Players Infected:</b>");
            foreach (var plr in allInfected)
            {
                stringB.Append(TownOfUsPlugin.Culture, $"\n{Color.white.ToTextColor()}{plr.Data.PlayerName}</color>");
            }
        }

        var notInfected = PlayerControl.AllPlayerControls.ToArray().Where(x => !x.HasDied() && x != Player && !x.HasModifier<PlaguebearerInfectedModifier>());
        stringB.Append(TownOfUsPlugin.Culture, $"\n\n<b>Players Left To Infect: {notInfected.Count()}</b>");

        return stringB;
    }

    public void PlayerControlFixedUpdate(PlayerControl playerControl)
    {
        var allInfected = PlayerControl.AllPlayerControls.ToArray().Where(x => !x.HasDied() && x != Player && x.GetModifier<PlaguebearerInfectedModifier>()?.PlagueBearerId == Player.PlayerId);

        if (allInfected.Count() == Helpers.GetAlivePlayers().Count - 1 && (MeetingHud.Instance == null || Helpers.GetAlivePlayers().Count > 2))
        {
            Player.ChangeRole(RoleId.Get<PestilenceRole>());

            CustomButtonSingleton<PestilenceKillButton>.Instance.SetTimer(OptionGroupSingleton<PlaguebearerOptions>.Instance.PestKillCooldown);

            allInfected.ToList().ForEach(x => x.RemoveModifier<PlaguebearerInfectedModifier>());
        }
    }

    public override bool CanUse(IUsable usable)
    {
        if (!GameManager.Instance.LogicUsables.CanUse(usable, Player))
        {
            return false;
        }
        Console console = usable.TryCast<Console>()!;
        return (console == null) || console.AllowImpostor;
    }

    public override bool DidWin(GameOverReason gameOverReason)
    {
        return WinConditionMet();
    }

    public bool WinConditionMet()
    {
        if (Player.HasDied()) return false;

        var result = Helpers.GetAlivePlayers().Count <= 2 && MiscUtils.KillersAliveCount == 1;

        return result;
    }

    public static void CheckInfected(PlayerControl source, PlayerControl target)
    {
        if (!source.HasModifier<PlaguebearerInfectedModifier>() && source.Data.Role is PlaguebearerRole)
        {
            source.AddModifier<PlaguebearerInfectedModifier>(source.PlayerId);
            target.AddModifier<PlaguebearerInfectedModifier>(source.PlayerId);
        }

        if (source.HasModifier<PlaguebearerInfectedModifier>() && !target.HasModifier<PlaguebearerInfectedModifier>())
        {
            var plaguebearerId = source.GetModifier<PlaguebearerInfectedModifier>()?.PlagueBearerId;

            if (plaguebearerId != null)
            {
                target.AddModifier<PlaguebearerInfectedModifier>((byte)plaguebearerId);
            }
        }

        if (!source.HasModifier<PlaguebearerInfectedModifier>() && target.HasModifier<PlaguebearerInfectedModifier>() && source.Data.Role is not PlaguebearerRole)
        {
            var plaguebearerId = target.GetModifier<PlaguebearerInfectedModifier>()?.PlagueBearerId;

            if (plaguebearerId != null)
            {
                source.AddModifier<PlaguebearerInfectedModifier>((byte)plaguebearerId);
            }
        }
    }

    [MethodRpc((uint)TownOfUsRpc.CheckInfected, SendImmediately = true)]
    public static void RpcCheckInfected(PlayerControl source, PlayerControl target)
    {
        CheckInfected(source, target);
    }

    public string GetAdvancedDescription()
    {
        return "The Plaguebearer is a Neutral Killing role that needs to infect all other players to turn into the Pestilence." + MiscUtils.AppendOptionsText(GetType());
    }

    [HideFromIl2Cpp]
    public List<CustomButtonWikiDescription> Abilities { get; } = [
        new("Infect",
            "Infect a player, causing them to be infected. When a infected player or dead body interacts or get interacted with the infection will spread to all non-infected players.",
            TouNeutAssets.InfectSprite)    
    ];
}

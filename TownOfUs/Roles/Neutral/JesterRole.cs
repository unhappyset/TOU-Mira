using System.Text;
using AmongUs.GameOptions;
using Il2CppInterop.Runtime.Attributes;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Roles;
using Reactor.Networking.Attributes;
using Reactor.Utilities;
using TownOfUs.Modifiers;
using TownOfUs.Modules;
using TownOfUs.Modules.Wiki;
using TownOfUs.Options.Roles.Neutral;
using TownOfUs.Patches;
using TownOfUs.Patches.Stubs;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfUs.Roles.Neutral;

public sealed class JesterRole(IntPtr cppPtr) : NeutralRole(cppPtr), ITownOfUsRole, IWikiDiscoverable, IDoomable
{
    public string RoleName => "Jester";
    public string RoleDescription => "Get voted out!";
    public string RoleLongDescription => "Be as suspicious as possible, and get voted out!";
    public Color RoleColor => TownOfUsColors.Jester;
    public ModdedRoleTeams Team => ModdedRoleTeams.Custom;
    public RoleAlignment RoleAlignment => RoleAlignment.NeutralEvil;
    public DoomableType DoomHintType => DoomableType.Trickster;
    public CustomRoleConfiguration Configuration => new(this)
    {
        CanUseVent = OptionGroupSingleton<JesterOptions>.Instance.CanVent,
        IntroSound = CustomRoleUtils.GetIntroSound(RoleTypes.Noisemaker),
        GhostRole = (RoleTypes)RoleId.Get<NeutralGhostRole>(),
        Icon = TouRoleIcons.Jester,
    };

    public bool Voted { get; set; }
    public bool SentWinMsg { get; set; }

    [HideFromIl2Cpp]
    public List<byte> Voters { get; } = [];

    public bool HasImpostorVision => OptionGroupSingleton<JesterOptions>.Instance.ImpostorVision;

    [HideFromIl2Cpp]
    public StringBuilder SetTabText()
    {
        return ITownOfUsRole.SetNewTabText(this);
    }

    public string GetAdvancedDescription()
    {
        return "The Jester is a Neutral Evil role that wins by getting themselves ejected." + MiscUtils.AppendOptionsText(GetType());
    }

    public override void Initialize(PlayerControl player)
    {
        RoleStubs.RoleBehaviourInitialize(this, player);

        if (!OptionGroupSingleton<JesterOptions>.Instance.CanButton)
        {
            player.RemainingEmergencies = 0;
        }

        if (Player.AmOwner)
        {
            if (OptionGroupSingleton<JesterOptions>.Instance.ScatterOn) 
                Player.AddModifier<ScatterModifier>(OptionGroupSingleton<JesterOptions>.Instance.ScatterTimer);

            HudManager.Instance.ImpostorVentButton.graphic.sprite = TouNeutAssets.JesterVentSprite.LoadAsset();
            HudManager.Instance.ImpostorVentButton.buttonLabelText.SetOutlineColor(TownOfUsColors.Jester);
        }
    }

    public override void Deinitialize(PlayerControl targetPlayer)
    {
        RoleStubs.RoleBehaviourDeinitialize(this, targetPlayer);

        if (Player.AmOwner)
        {
            if (OptionGroupSingleton<JesterOptions>.Instance.ScatterOn) 
                Player.RemoveModifier<ScatterModifier>();

            HudManager.Instance.ImpostorVentButton.graphic.sprite = TouAssets.VentSprite.LoadAsset();
            HudManager.Instance.ImpostorVentButton.buttonLabelText.SetOutlineColor(TownOfUsColors.Impostor);
        }
    }

    public override void OnDeath(DeathReason reason)
    {
        RoleStubs.RoleBehaviourOnDeath(this, reason);

        if (reason == DeathReason.Exile)
            RpcJesterWin(Player);

        //Logger<TownOfUsPlugin>.Error($"JesterRole.OnDeath - Voted: {Voted}");
    }

    public override void OnVotingComplete()
    {
        RoleStubs.RoleBehaviourOnVotingComplete(this);

        Voters.Clear();

        foreach (var state in MeetingHudGetVotesPatch.States)
        {
            if (state.VotedForId == Player.PlayerId)
            {
                Voters.Add(state.VoterId);
            }
        }
    }

    public override bool CanUse(IUsable usable)
    {
        if (!GameManager.Instance.LogicUsables.CanUse(usable, Player))
        {
            return false;
        }
        var console = usable.TryCast<Console>()!;
        return console == null || console.AllowImpostor;
    }

    public override bool DidWin(GameOverReason gameOverReason)
    {
        //Logger<TownOfUsPlugin>.Message($"JesterRole.DidWin - Voted: '{Voted}', Exists: '{GameHistory.DeathHistory.Exists(x => x.Item1 == Player.PlayerId && x.Item2 == DeathReason.Exile)}'");

        return Voted || GameHistory.DeathHistory.Exists(x => x.Item1 == Player.PlayerId && x.Item2 == DeathReason.Exile);
    }

    public bool WinConditionMet()
    {
        if (OptionGroupSingleton<JesterOptions>.Instance.JestWin is not JestWinOptions.EndsGame) return false;

        return Voted || GameHistory.DeathHistory.Exists(x => x.Item1 == Player.PlayerId && x.Item2 == DeathReason.Exile);
    }

    [MethodRpc((uint)TownOfUsRpc.JesterWin, SendImmediately = true)]
    public static void RpcJesterWin(PlayerControl player)
    {
        if (player.Data.Role is not JesterRole)
        {
            Logger<TownOfUsPlugin>.Error("RpcJesterWin - Invalid Jester");
            return;
        }

        var jester = player.GetRole<JesterRole>();
        jester!.Voted = true;
    }
}

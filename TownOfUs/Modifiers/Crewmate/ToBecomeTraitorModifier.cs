using AmongUs.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Modifiers.Types;
using MiraAPI.Roles;
using Reactor.Networking.Attributes;
using TownOfUs.Modifiers.Neutral;
using TownOfUs.Roles.Crewmate;
using TownOfUs.Roles.Impostor;
using TownOfUs.Utilities;

namespace TownOfUs.Modifiers.Crewmate;

public sealed class ToBecomeTraitorModifier : GameModifier, IAssignableTargets
{
    public override string ModifierName => "Possible Traitor";
    public override bool HideOnUi => true;
    public override int GetAmountPerGame() => 0;
    public override int GetAssignmentChance() => 0;
    public int Priority { get; set; } = 3;

    public void Clear()
    {
        AssignTargets();
        ModifierComponent?.RemoveModifier(this);
    }

    public void AssignTargets()
    {
        Random rnd = new();
        var chance = rnd.Next(0, 100);

        if (chance <= GameOptionsManager.Instance.CurrentGameOptions.RoleOptions.GetChancePerGame((RoleTypes)RoleId.Get<TraitorRole>()))
        {
            var filtered = PlayerControl.AllPlayerControls.ToArray()
                .Where(x => x.Is(ModdedRoleTeams.Crewmate) && 
                                    !x.Data.IsDead && 
                                    !x.Data.Disconnected && 
                                    !x.HasModifier<PlayerTargetModifier>() &&
                                    x.Data.Role is not MayorRole).ToList();

            Random rndIndex = new();
            if (filtered.Count == 0) return;
            var randomTarget = filtered[rndIndex.Next(0, filtered.Count)];

            randomTarget.RpcAddModifier<ToBecomeTraitorModifier>();
        }
    }

    [MethodRpc((uint)TownOfUsRpc.SetTraitor, SendImmediately = true)]
    public static void RpcSetTraitor(PlayerControl player)
    {
        if (!player.HasModifier<ToBecomeTraitorModifier>()) return;

        player.RemoveModifier<ToBecomeTraitorModifier>();
        player.ChangeRole(RoleId.Get<TraitorRole>());
        
        if (SnitchRole.IsTargetOfSnitch(player))
        {
            CustomRoleUtils.GetActiveRolesOfType<SnitchRole>().ToList().ForEach(snitch => snitch.AddSnitchTraitorArrows());
        }
    }
}

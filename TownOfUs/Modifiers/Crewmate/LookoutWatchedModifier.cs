using AmongUs.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Roles;
using Reactor.Utilities.Extensions;
using System.Text;
using TownOfUs.Modifiers.Impostor;
using TownOfUs.Roles.Impostor;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfUs.Modifiers.Crewmate;

public sealed class LookoutWatchedModifier(PlayerControl lookout) : BaseModifier
{
    public override string ModifierName => "Watched";
    public override bool HideOnUi => true;

    public PlayerControl Lookout { get; set; } = lookout;
    public List<RoleBehaviour> SeenPlayers { get; set; } = [];

    public override void OnDeath(DeathReason reason)
    {
        ModifierComponent!.RemoveModifier(this);
    }

    public override void FixedUpdate()
    {
        base.FixedUpdate();

        if (Lookout.AmOwner)
        {
            Player?.cosmetics.SetOutline(true, new Il2CppSystem.Nullable<Color>(TownOfUsColors.Lookout));
        }
    }

    public override void OnMeetingStart()
    {
        if (!Lookout.AmOwner) return;
        var title = $"<color=#{TownOfUsColors.Lookout.ToHtmlStringRGBA()}>Lookout Feedback</color>";
        var msg = $"No players interacted with {Player.Data.PlayerName}";

        if (SeenPlayers.Count != 0)
        {
            var message = new StringBuilder($"Roles seen interacting with {Player.Data.PlayerName}:\n");

            SeenPlayers.Shuffle();

            foreach (var role in SeenPlayers)
            {
                message.Append(TownOfUsPlugin.Culture, $" {role.NiceName},");
            }

            message = message.Remove(message.Length - 1, 1);

            var final = message.ToString();

            if (string.IsNullOrWhiteSpace(final))
                return;

            msg = final;
        }

        if (HudManager.Instance)
            MiscUtils.AddFakeChat(PlayerControl.LocalPlayer.Data, title, msg, true, true);

        SeenPlayers.Clear();
    }

    public void SeePlayer(PlayerControl source)
    {
        if (source.HasModifier<TraitorCacheModifier>())
        {
            SeenPlayers.Add(RoleManager.Instance.GetRole((RoleTypes)RoleId.Get<TraitorRole>()));
        }
        else
        {
            SeenPlayers.Add(source.Data.Role);
        }
    }
}

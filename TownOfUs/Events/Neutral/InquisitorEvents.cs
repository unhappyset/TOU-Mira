using HarmonyLib;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.Events.Vanilla.Player;
using MiraAPI.Hud;
using MiraAPI.Modifiers;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using Reactor.Utilities;
using TownOfUs.Buttons.Neutral;
using TownOfUs.Modifiers.Neutral;
using TownOfUs.Roles.Neutral;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfUs.Events.Neutral;

public static class InquisitorEvents
{
    [RegisterEvent]
    public static void AfterMurderEventHandler(AfterMurderEvent @event)
    {
        var source = @event.Source;
        var victim = @event.Target;

        if (PlayerControl.LocalPlayer.Data.Role is InquisitorRole)
        {
            if (victim.HasModifier<InquisitorHereticModifier>() && !victim.AmOwner && !source.AmOwner)
            {
                Coroutines.Start(MiscUtils.CoFlash(TownOfUsColors.Inquisitor, alpha: 0.1f));
                var notif1 = Helpers.CreateAndShowNotification($"<b>{TownOfUsColors.Inquisitor.ToTextColor()}A Heretic has perished!</b></color>", Color.white, spr: TouRoleIcons.Inquisitor.LoadAsset());
                notif1.Text.SetOutlineThickness(0.35f);
                notif1.transform.localPosition = new Vector3(0f, 1f, -20f);
            }
            else if (!victim.HasModifier<InquisitorHereticModifier>() && !victim.AmOwner && source.AmOwner)
            {
                Coroutines.Start(MiscUtils.CoFlash(TownOfUsColors.Inquisitor, alpha: 0.4f));
                var notif1 = Helpers.CreateAndShowNotification($"<b>{TownOfUsColors.Inquisitor.ToTextColor()}{victim.Data.PlayerName} was not a heretic!\nYou can no longer vanquish players.</b></color>", Color.white, spr: TouRoleIcons.Inquisitor.LoadAsset());
                notif1.Text.SetOutlineThickness(0.35f);
                notif1.transform.localPosition = new Vector3(0f, 1f, -20f);
                CustomButtonSingleton<InquisitorVanquishButton>.Instance.Usable = false;
            }
            else if (victim.HasModifier<InquisitorHereticModifier>() && !victim.AmOwner && source.AmOwner)
            {
                Coroutines.Start(MiscUtils.CoFlash(TownOfUsColors.Doomsayer, alpha: 0.4f));
                var notif1 = Helpers.CreateAndShowNotification($"<b>{TownOfUsColors.Inquisitor.ToTextColor()}{victim.Data.PlayerName} was a heretic!</b></color>", Color.white, spr: TouRoleIcons.Inquisitor.LoadAsset());
                notif1.Text.SetOutlineThickness(0.35f);
                notif1.transform.localPosition = new Vector3(0f, 1f, -20f);
            }
        }
        CustomRoleUtils.GetActiveRolesOfType<InquisitorRole>().Do(x => x.CheckTargetDeath(victim));
    }

    [RegisterEvent]
    public static void PlayerDeathEventHandler(PlayerDeathEvent @event)
    {
        if (@event.DeathReason != DeathReason.Exile) return;

        CustomRoleUtils.GetActiveRolesOfType<InquisitorRole>().Do(x => x.CheckTargetDeath(@event.Player));
    }

    [RegisterEvent]
    public static void RoundStartEventHandler(RoundStartEvent @event)
    {
        if (@event.TriggeredByIntro) return;
        if (PlayerControl.LocalPlayer.Data.Role is not InquisitorRole inquis) return;

        if (inquis.TargetsDead && !PlayerControl.LocalPlayer.HasDied())
        {
            PlayerControl.LocalPlayer.RpcPlayerExile();
        }
    }
}

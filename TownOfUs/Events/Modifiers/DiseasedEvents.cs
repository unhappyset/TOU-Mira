using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Modifiers;
using MiraAPI.Utilities;
using TownOfUs.Buttons.Impostor;
using TownOfUs.Buttons.Neutral;
using TownOfUs.Modifiers.Game.Crewmate;
using TownOfUs.Options.Modifiers.Crewmate;
using TownOfUs.Roles.Impostor;
using TownOfUs.Roles.Neutral;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfUs.Events.Modifiers;

public static class DiseasedEvents
{
    [RegisterEvent(10)]
    public static void AfterMurderEventHandler(AfterMurderEvent @event)
    {
        var source = @event.Source;
        var target = @event.Target;

        if (!target.HasModifier<DiseasedModifier>()) return;

        var cdMultiplier = OptionGroupSingleton<DiseasedOptions>.Instance.CooldownMultiplier;
        if (source.AmOwner)
        {
            var notif1 = Helpers.CreateAndShowNotification(
                $"<b>{TownOfUsColors.Diseased.ToTextColor()}{@event.Target.Data.PlayerName} was Diseased, causing your kill cooldown to multiply by {Math.Round(cdMultiplier, 2)}.</color></b>", Color.white, spr: TouModifierIcons.Diseased.LoadAsset());

            notif1.Text.SetOutlineThickness(0.35f);
                notif1.transform.localPosition = new Vector3(0f, 1f, -20f);
        }

        source.SetKillTimer(source.GetKillCooldown() * cdMultiplier);

        // I don't like this, I'd rather have something more dynamic but it should work
        if (source.Data.Role is JuggernautRole)
        {
            var button = CustomButtonSingleton<JuggernautKillButton>.Instance;
            button.SetTimer(button.Cooldown * cdMultiplier);
        }
        else if (source.Data.Role is GlitchRole)
        {
            var button = CustomButtonSingleton<GlitchKillButton>.Instance;
            button.SetTimer(button.Cooldown * cdMultiplier);
        }
        else if (source.Data.Role is WerewolfRole)
        {
            var button = CustomButtonSingleton<WerewolfKillButton>.Instance;
            button.SetTimer(button.Cooldown * cdMultiplier);
        }
        else if (source.Data.Role is VampireRole)
        {
            var button = CustomButtonSingleton<VampireBiteButton>.Instance;
            button.SetTimer(button.Cooldown * cdMultiplier);
        }
        else if (source.Data.Role is SoulCollectorRole)
        {
            var button = CustomButtonSingleton<SoulCollectorReapButton>.Instance;
            button.SetTimer(button.Cooldown * cdMultiplier);
        }
        else if (source.Data.Role is PestilenceRole)
        {
            var button = CustomButtonSingleton<PestilenceKillButton>.Instance;
            button.SetTimer(button.Cooldown * cdMultiplier);
        }
        else if (source.Data.Role is BomberRole)
        {
            var button = CustomButtonSingleton<BomberPlantButton>.Instance;
            button.SetTimer(button.Cooldown * cdMultiplier);
        }
        else if (source.Data.Role is JanitorRole)
        {
            var button = CustomButtonSingleton<JanitorCleanButton>.Instance;
            button.SetTimer(button.Cooldown * cdMultiplier);
        }
        else if (source.Data.Role is WarlockRole)
        {
            var button = CustomButtonSingleton<WarlockKillButton>.Instance;
            button.SetTimer(button.Cooldown * cdMultiplier);
        }
    }
}

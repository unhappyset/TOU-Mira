using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Player;
using MiraAPI.GameOptions;
using MiraAPI.Hud;
using TownOfUs.Buttons.Crewmate;
using TownOfUs.Options.Roles.Crewmate;

namespace TownOfUs.Events.Crewmate;

public static class EngineerEvents
{
    [RegisterEvent]
    public static void CompleteTaskEvent(CompleteTaskEvent @event)
    {
        if (@event.Player.AmOwner && @event.Player.Data.Role is EngineerRole &&
            OptionGroupSingleton<EngineerOptions>.Instance.TaskUses &&
            (int)OptionGroupSingleton<EngineerOptions>.Instance.MaxVents != 0)
        {
            var button = CustomButtonSingleton<EngineerVentButton>.Instance;
            ++button.UsesLeft;
            ++button.ExtraUses;
            button.SetUses(button.UsesLeft);
        }
    }
}
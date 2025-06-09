using HarmonyLib;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Meeting;
using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Roles;
using TownOfUs.Buttons.Impostor;
using TownOfUs.Options.Roles.Impostor;
using TownOfUs.Roles.Impostor;

namespace TownOfUs.Events.Impostor;

public static class MorphlingEvents
{
    [RegisterEvent]
    public static void EjectionEventEventHandler(EjectionEvent @event)
    {
        CustomRoleUtils.GetActiveRolesOfType<MorphlingRole>().Do(x => x.Clear());
        var button = CustomButtonSingleton<MorphlingMorphButton>.Instance;
        button.SetUses((int)OptionGroupSingleton<MorphlingOptions>.Instance.MaxMorphs);
        button.SetTextOutline(button.TextOutlineColor);
        if (button.Button != null) button.Button.usesRemainingSprite.color = button.TextOutlineColor;
    }
}

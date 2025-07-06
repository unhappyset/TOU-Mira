using MiraAPI.GameOptions;
using MiraAPI.Utilities.Assets;
using Reactor.Utilities.Extensions;
using TownOfUs.Modules.Anims;
using TownOfUs.Options;
using TownOfUs.Patches;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfUs.Modifiers;

public sealed class FirstDeadShield : ExcludedGameModifier, IAnimated
{
    public override string ModifierName => "First Death Shield";
    public override LoadableAsset<Sprite>? ModifierIcon => TouModifierIcons.FirstRoundShield;

    public override bool HideOnUi => !TownOfUsPlugin.ShowShieldHud.Value;

    public GameObject? FirstRoundShield { get; set; }
    public bool IsVisible { get; set; } = true;

    public void SetVisible()
    {
    }

    public override int GetAmountPerGame()
    {
        return FirstDeadPatch.PlayerName != null && OptionGroupSingleton<GeneralOptions>.Instance.FirstDeathShield
            ? 1
            : 0;
    }

    public override int GetAssignmentChance()
    {
        return FirstDeadPatch.PlayerName != null && OptionGroupSingleton<GeneralOptions>.Instance.FirstDeathShield
            ? 100
            : 0;
    }

    public override bool IsModifierValidOn(RoleBehaviour role)
    {
        return role.Player.name == FirstDeadPatch.PlayerName;
    }

    public override string GetDescription()
    {
        return !HideOnUi ? "You have protection because you died first last game" : string.Empty;
    }

    public override void OnActivate()
    {
        FirstDeadPatch.PlayerName = null!;
        FirstRoundShield =
            AnimStore.SpawnAnimBody(Player, TouAssets.FirstRoundShield.LoadAsset(), false, -1.1f, -0.225f)!;
    }

    public override void OnDeath(DeathReason reason)
    {
        base.OnDeath(reason);

        FirstRoundShield?.SetActive(false);
        ModifierComponent!.RemoveModifier(this);
    }

    public override void OnDeactivate()
    {
        if (FirstRoundShield?.gameObject != null)
        {
            FirstRoundShield.Destroy();
        }
    }

    public override void Update()
    {
        if (!MeetingHud.Instance && FirstRoundShield?.gameObject != null)
        {
            FirstRoundShield?.SetActive(!Player.IsConcealed() && IsVisible);
        }
        else if (MeetingHud.Instance)
        {
            FirstRoundShield?.SetActive(false);
            ModifierComponent!.RemoveModifier(this);
        }
    }
}
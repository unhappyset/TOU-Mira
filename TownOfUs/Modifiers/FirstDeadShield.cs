using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Modifiers.Types;
using MiraAPI.Utilities.Assets;
using Reactor.Utilities.Extensions;
using TownOfUs.Modules.Anims;
using TownOfUs.Options;
using TownOfUs.Patches;
using UnityEngine;

namespace TownOfUs.Modifiers;

public sealed class FirstDeadShield : GameModifier
{
    public override string ModifierName => "First Death Shield";
    public override LoadableAsset<Sprite>? ModifierIcon => TouModifierIcons.FirstRoundShield;

    public override int GetAmountPerGame() => FirstDeadPatch.PlayerName != null && OptionGroupSingleton<GeneralOptions>.Instance.FirstDeathShield ? 1 : 0;

    public override int GetAssignmentChance() => FirstDeadPatch.PlayerName != null && OptionGroupSingleton<GeneralOptions>.Instance.FirstDeathShield ? 100 : 0;

    public override bool IsModifierValidOn(RoleBehaviour role) => role.Player.name == FirstDeadPatch.PlayerName;
    public override bool HideOnUi => !TownOfUsPlugin.ShowShieldHud.Value;
    public override string GetDescription()
    {
        return !HideOnUi ? "You have protection because you died first last game" : string.Empty;
    }
    public bool Visible = true;

    public GameObject? FirstRoundShield { get; set; }

    public override void OnActivate()
    {
        FirstDeadPatch.PlayerName = null!;
        FirstRoundShield = AnimStore.SpawnAnimBody(Player, TouAssets.FirstRoundShield.LoadAsset(), false, -1.1f, -0.225f)!;
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
    public void SetVisible(bool visible)
    {
        Visible = visible;
    }

    public bool IsConcealed()
    {
        if (Player.HasModifier<ConcealedModifier>() || !Player.Visible || (Player.TryGetModifier<DisabledModifier>(out var mod) && !mod.IsConsideredAlive))
        {
            return true;
        }
        if (!Visible || Player.inVent)
        {
            return true;
        }


        var mushroom = UnityEngine.Object.FindObjectOfType<MushroomMixupSabotageSystem>();
        if (mushroom && mushroom.IsActive)
        {
            return true;
        }

        if (OptionGroupSingleton<GeneralOptions>.Instance.CamouflageComms)
        {
            if (!ShipStatus.Instance.Systems.TryGetValue(SystemTypes.Comms, out var commsSystem) || commsSystem == null)
            {
                return false;
            }

            var isActive = false;
            if (ShipStatus.Instance.Type == ShipStatus.MapType.Hq || ShipStatus.Instance.Type == ShipStatus.MapType.Fungle)
            {
                var hqSystem = commsSystem.Cast<HqHudSystemType>();
                if (hqSystem != null) isActive = hqSystem.IsActive;
            }
            else
            {
                var hudSystem = commsSystem.Cast<HudOverrideSystemType>();
                if (hudSystem != null) isActive = hudSystem.IsActive;
            }

            return isActive;
        }

        return false;
    }

    public override void Update()
    {
        if (!MeetingHud.Instance && FirstRoundShield?.gameObject != null)
        {
            FirstRoundShield?.SetActive(!IsConcealed());
        }
        else if (MeetingHud.Instance)
        {
            FirstRoundShield?.SetActive(false);
            ModifierComponent!.RemoveModifier(this);
            return;
        }
    }
}

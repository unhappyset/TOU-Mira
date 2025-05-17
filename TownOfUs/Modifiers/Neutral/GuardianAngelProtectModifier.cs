using MiraAPI.GameOptions;
using MiraAPI.Roles;
using MiraAPI.Utilities.Assets;
using TownOfUs.Options;
using TownOfUs.Options.Roles.Neutral;
using TownOfUs.Roles.Neutral;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfUs.Modifiers.Neutral;

public sealed class GuardianAngelProtectModifier : BaseShieldModifier
{
    public override float Duration => OptionGroupSingleton<GuardianAngelOptions>.Instance.ProtectDuration;
    public override string ModifierName => "Protected";
    public override LoadableAsset<Sprite>? ModifierIcon => TouRoleIcons.GuardianAngel;
    public override string ShieldDescription => "You are protected by your Guardian Angel!\nYou cannot be killed.";
    public override bool AutoStart => true;
    public override bool HideOnUi
    {
        get
        {
          var showProtect = OptionGroupSingleton<GuardianAngelOptions>.Instance.ShowProtect;
          var showProtectEveryone = showProtect == ProtectOptions.Everyone;
          var showProtectSelf = PlayerControl.LocalPlayer.PlayerId == Player.PlayerId &&
              showProtect is ProtectOptions.Self or ProtectOptions.SelfAndGA;
            return !TownOfUsPlugin.ShowShieldHud.Value && (!showProtectEveryone || !showProtectSelf);
        }
    }

    public override void OnActivate()
    {
        var showProtect = OptionGroupSingleton<GuardianAngelOptions>.Instance.ShowProtect;
        var ga = CustomRoleUtils.GetActiveRolesOfType<GuardianAngelTouRole>().FirstOrDefault(x => x.Target == Player);

        var showProtectEveryone = showProtect == ProtectOptions.Everyone;
        var showProtectSelf = PlayerControl.LocalPlayer.PlayerId == Player.PlayerId &&
            showProtect is ProtectOptions.Self or ProtectOptions.SelfAndGA;
        var showProtectGA = PlayerControl.LocalPlayer.PlayerId == ga?.Player.PlayerId &&
                 showProtect is ProtectOptions.GA or ProtectOptions.SelfAndGA;

        if (showProtectEveryone || showProtectSelf || showProtectGA || (PlayerControl.LocalPlayer.HasDied() && OptionGroupSingleton<GeneralOptions>.Instance.TheDeadKnow))
        {
            RoleEffectAnimation roleEffectAnimation = UnityEngine.Object.Instantiate<RoleEffectAnimation>(DestroyableSingleton<RoleManager>.Instance.protectLoopAnim, Player.gameObject.transform);
            roleEffectAnimation.SetMaterialColor(7); // This is white, if it's not, make sure it is set to white from the int
            roleEffectAnimation.SetMaskLayerBasedOnWhoShouldSee(true);
            roleEffectAnimation.Play(Player, new Action(OnDeactivate), Player.cosmetics.FlipX, RoleEffectAnimation.SoundType.Local, Duration);
        }
    }

    public override void OnDeactivate()
    {
      for (int i = Player.currentRoleAnimations.Count - 1; i >= 0; i--)
      {
        if (Player.currentRoleAnimations[i] != null && Player.currentRoleAnimations[i].effectType == RoleEffectAnimation.EffectType.ProtectLoop)
        {
          UnityEngine.Object.Destroy(Player.currentRoleAnimations[i].gameObject);
          Player.currentRoleAnimations.RemoveAt(i);
        }
      }
    }

    public override void OnDeath(DeathReason reason)
    {
		for (int i = Player.currentRoleAnimations.Count - 1; i >= 0; i--)
		{
			if (Player.currentRoleAnimations[i] != null && Player.currentRoleAnimations[i].effectType == RoleEffectAnimation.EffectType.ProtectLoop)
			{
				UnityEngine.Object.Destroy(Player.currentRoleAnimations[i].gameObject);
				Player.currentRoleAnimations.RemoveAt(i);
			}
		}
    }
}

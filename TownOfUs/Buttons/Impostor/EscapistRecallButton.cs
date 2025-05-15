using System.Collections;
using MiraAPI.GameOptions;
using MiraAPI.Utilities.Assets;
using Reactor.Utilities;
using TownOfUs.Options.Roles.Impostor;
using TownOfUs.Roles.Impostor;
using UnityEngine;

namespace TownOfUs.Buttons.Impostor;

public sealed class EscapistRecallButton : TownOfUsRoleButton<EscapistRole>, IAftermathableButton
{
    public override string Name => "Recall";
    public override string Keybind => "ActionQuaternary";
    public override Color TextOutlineColor => TownOfUsColors.Impostor;
    public override float Cooldown => OptionGroupSingleton<EscapistOptions>.Instance.RecallCooldown + MapCooldown;
    public override LoadableAsset<Sprite> Sprite => TouImpAssets.RecallSprite;

    public override bool Enabled(RoleBehaviour? role) => base.Enabled(role) && Role is not { MarkedLocation: null };

    public override bool CanUse()
    {
        return base.CanUse() && Role is not { MarkedLocation: null };
    }

    protected override void OnClick()
    {
        if (Role.MarkedLocation != null)
        {
            Coroutines.Start(CoRecall(Role.MarkedLocation.Value));
            // TouAudio.PlaySound(TouAudio.EscapistRecallSound);
        }
    }

    private static IEnumerator CoRecall(Vector2 location)
    {
        yield return HudManager.Instance.CoFadeFullScreen(Color.clear, new Color(0.6f, 0.1f, 0.2f, 1f), 11f / 24f);
        PlayerControl.LocalPlayer.NetTransform.RpcSnapTo(location);

        EscapistRole.RpcRecall(PlayerControl.LocalPlayer);
        yield return HudManager.Instance.CoFadeFullScreen(new Color(0.6f, 0.1f, 0.2f, 1f), Color.clear);
    }
}

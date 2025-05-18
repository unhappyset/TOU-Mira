using System.Collections;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using Reactor.Utilities;
using Reactor.Utilities.Extensions;
using TownOfUs.Options.Roles.Crewmate;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfUs.Modifiers.Crewmate;

public sealed class FootstepsModifier : BaseModifier
{
    public override string ModifierName => "Footsteps";
    public override bool HideOnUi => true;

    private Dictionary<GameObject, SpriteRenderer>? _currentSteps;
    private Vector3 _lastPos;
    private Color _footstepColor;

    public override void OnActivate()
    {
        _currentSteps = [];
        _lastPos = Player.transform.position;

        _footstepColor = OptionGroupSingleton<InvestigatorOptions>.Instance.ShowAnonymousFootprints ?
            new Color(0.2f, 0.2f, 0.2f, 1f) : Palette.PlayerColors[Player.CurrentOutfit.ColorId];
    }

    public override void OnDeactivate()
    {
        if (_currentSteps == null)
        {
            return;
        }

        _currentSteps.ToList().ForEach(step => Coroutines.Start(FootstepFadeout(step.Key, step.Value)));
        _currentSteps.Clear();
    }

    public override void FixedUpdate()
    {
        if (_currentSteps == null || Player.HasModifier<ConcealedModifier>() || (Player.TryGetModifier<DisabledModifier>(out var mod) && !mod.IsConsideredAlive) || Vector3.Distance(_lastPos, Player.transform.position) < OptionGroupSingleton<InvestigatorOptions>.Instance.FootprintInterval)
        {
            return;
        }

        if (!OptionGroupSingleton<InvestigatorOptions>.Instance.ShowFootprintVent && ShipStatus.Instance?.AllVents
            .Any(vent => Vector2.Distance(vent.gameObject.transform.position, Player.GetTruePosition()) < 1f) == true)
        {
            return;
        }

        var angle = Mathf.Atan2(Player.MyPhysics.Velocity.y, Player.MyPhysics.Velocity.x) * Mathf.Rad2Deg;

        var footstep = new GameObject("Footstep")
        {
            transform =
            {
                parent = ShipStatus.Instance?.transform,
                position = new Vector3(Player.transform.position.x, Player.transform.position.y, 2.5708f),
                rotation = Quaternion.AngleAxis(angle - 90, Vector3.forward),
            },
        };

        var sprite = footstep.AddComponent<SpriteRenderer>();
        sprite.sprite = TouAssets.FootprintSprite.LoadAsset();
        sprite.color = _footstepColor;
        footstep.layer = LayerMask.NameToLayer("Players");

        footstep.transform.localScale *= new Vector2(1.2f, 1f) * (OptionGroupSingleton<InvestigatorOptions>.Instance.FootprintSize / 10);

        _currentSteps.Add(footstep, sprite);
        _lastPos = Player.transform.position;
        Coroutines.Start(FootstepDisappear(footstep, sprite));
    }

    public override void OnDeath(DeathReason reason)
    {
        ModifierComponent!.RemoveModifier(this);
    }

    public static IEnumerator FootstepFadeout(GameObject obj, SpriteRenderer rend)
    {
        yield return MiscUtils.FadeOut(rend, 0.0001f, 0.05f);
        obj.DestroyImmediate();
    }

    public static IEnumerator FootstepDisappear(GameObject obj, SpriteRenderer rend)
    {
        yield return new WaitForSeconds(OptionGroupSingleton<InvestigatorOptions>.Instance.FootprintDuration);
        yield return FootstepFadeout(obj, rend);
    }
}

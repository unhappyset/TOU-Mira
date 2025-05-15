using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Modifiers.Types;
using TownOfUs.Options.Modifiers.Crewmate;

namespace TownOfUs.Modifiers.Game.Crewmate;

public sealed class FrozenModifier : TimedModifier
{
    public override string ModifierName => "Frozen";
    public override bool HideOnUi => true;
    public override float Duration => OptionGroupSingleton<FrostyOptions>.Instance.ChillDuration;

    private float SpeedCache { get; set; }
    private DateTime ApplicationTime { get; set; }

    public override void OnDeath(DeathReason reason)
    {
        if (Player == null)
        {
            return;
        }

        Player.RemoveModifier(this);
    }

    public override void OnActivate()
    {
        if (Player != null)
        {
            ApplicationTime = DateTime.UtcNow;
            SpeedCache = Player.MyPhysics.Speed;
            Player.MyPhysics.Speed *= OptionGroupSingleton<FrostyOptions>.Instance.ChillStartSpeed;
        }
    }

    public override void OnDeactivate()
    {
        if (Player != null)
        {
            Player.MyPhysics.Speed = SpeedCache;
        }
    }

    public override void FixedUpdate()
    {
        base.FixedUpdate();

        if (Player == null) return;

        var timeSpan = DateTime.UtcNow - ApplicationTime;
        var duration = Duration * 1000f;
        Player.MyPhysics.Speed = SpeedCache * 1 - (duration - (float)timeSpan.TotalMilliseconds) *
            (1 - OptionGroupSingleton<FrostyOptions>.Instance.ChillStartSpeed) / duration;
    }
}

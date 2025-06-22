
namespace TownOfUs.Modifiers;

public sealed class Scatter1Modifier : ScatterModifier
{
	public override bool ShowInFreeplay => true;

	public Scatter1Modifier() : base(25f)
	{

	}

	public override void OnActivate()
	{
		base.OnActivate();

		StartTimer();
	}
}

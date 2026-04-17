using Sandbox;

public enum LaundryState
{
	Dirty,
	Washed,
	Ironed,
	Folded,
	Burnt
}

public class Washable : Component
{
	[Property, Sync, Range( 0f, 100f ), Step( 5f )]
	public float Dirtiness { get; set; } = 0f;

	[Property, Sync]
	public LaundryState State { get; set; } = LaundryState.Dirty;

	protected override void OnStart()
	{
		Dirtiness = System.Random.Shared.Float( 0f, 100f );
	}

	public bool RequiresDetergent()
	{
		return Dirtiness > 50f;
	}

	public void Wash( bool useDetergent = false )
	{
		if ( State != LaundryState.Dirty )
			return;

		if ( useDetergent || !RequiresDetergent() )
		{
			Dirtiness = 0f;
		}
		else
		{
			Dirtiness -= 25f;
			if ( Dirtiness < 0f ) Dirtiness = 0f;
		}

		if ( Dirtiness == 0f )
			State = LaundryState.Washed;
	}

	public void Iron()
	{
		if ( State != LaundryState.Washed )
			return;
		State = LaundryState.Ironed;
	}

	public void Fold()
	{
		if ( State != LaundryState.Ironed )
			return;
		State = LaundryState.Folded;
	}

	public void Burn()
	{
		State = LaundryState.Burnt;
	}
}

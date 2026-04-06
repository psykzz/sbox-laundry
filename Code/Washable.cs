using System.Net.Security;
using Sandbox;

public sealed class Washable : Component
{
	[Property, Range( 0f, 100f ), Step( 5f )]
	public float Dirtiness { get; set; } = 0f;

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
		if ( useDetergent && RequiresDetergent() )
		{
			Dirtiness = 0f;
		}
		else if ( !useDetergent && !RequiresDetergent() )
		{
			Dirtiness = 0f;
		}
		else
		{
			Dirtiness -= 25f; // Washing without detergent reduces dirtiness by 25%
			if ( Dirtiness < 0f )
				Dirtiness = 0f;
		}
	}

	protected override void OnUpdate()
	{

	}
}

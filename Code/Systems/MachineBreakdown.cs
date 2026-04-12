using Sandbox;
using System;

[Title( "Machine Breakdown" ), Category( "Machines" )]
public sealed class MachineBreakdown : Component
{
	/// <summary>
	/// Average number of times per minute a running machine may break down.
	/// E.g. 0.1 = roughly once every 10 minutes; 0.5 = roughly once every 2 minutes.
	/// </summary>
	[Property, Range( 0f, 2f ), Title( "Breakdowns Per Minute" )]
	public float BreakdownChancePerMinute { get; set; } = 0.15f;

	public bool IsBrokenDown { get; private set; } = false;

	public event Action<GameObject> OnBreakdown;
	public event Action<GameObject> OnRepaired;

	/// <summary>
	/// Call this once per fixed update while the owning machine is actively running.
	/// </summary>
	public void TryBreakdown()
	{
		if ( IsBrokenDown )
			return;

		float chancePerFrame = (BreakdownChancePerMinute / 60f) * Time.Delta;
		if ( System.Random.Shared.Float() < chancePerFrame )
			TriggerBreakdown();
	}

	public void TriggerBreakdown()
	{
		if ( IsBrokenDown )
			return;

		IsBrokenDown = true;
		Sound.Play( "beep", GameObject.WorldPosition );
		OnBreakdown?.Invoke( GameObject );
	}

	public void Repair()
	{
		if ( !IsBrokenDown )
			return;

		IsBrokenDown = false;
		OnRepaired?.Invoke( GameObject );
	}
}

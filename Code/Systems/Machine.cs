using Sandbox;
using System;

[Title( "Machine" ), Category( "Machines" )]
public abstract class Machine : Component
{
	[Property, Range( 0f, 2f ), Title( "Breakdowns Per Minute" )]
	public float BreakdownChancePerMinute { get; set; } = 0.15f;

	public bool IsBrokenDown { get; private set; } = false;

	public event Action<GameObject> OnBreakdown;
	public event Action<GameObject> OnRepaired;

	[Group( "Debug" ), Button( "Force Breakdown" )]
	private void ForceBreakdown() => TriggerBreakdown();

	[Group( "Debug" ), Button( "Fix Breakdown" )]
	private void FixBreakdown() => Repair();

	public void TryBreakdown()
	{
		if ( IsBrokenDown ) return;
		float chancePerFrame = BreakdownChancePerMinute / 60f * Time.Delta;
		if ( Random.Shared.Float() < chancePerFrame )
			TriggerBreakdown();
	}

	public void TriggerBreakdown()
	{
		if ( IsBrokenDown ) return;
		IsBrokenDown = true;
		Sound.Play( "beep", GameObject.WorldPosition );
		OnBreakdown?.Invoke( GameObject );
	}

	public void Repair()
	{
		if ( !IsBrokenDown ) return;
		IsBrokenDown = false;
		OnRepaired?.Invoke( GameObject );
	}
}

using Sandbox;

/// <summary>
/// Add to a detergent bottle prop alongside Usable and PickupItem.
/// Throw or drop it into a Washer's trigger area to load it as detergent.
/// </summary>
[Title( "Detergent" ), Category( "Laundry" )]
public sealed class Detergent : Component


{
	private Usable _usable;
	protected override void OnStart()
	{
		_usable = Components.Get<Usable>( FindMode.InSelf );
		if ( _usable is not null )
			_usable.HintText = "Pick Up Detergent";
	}


}

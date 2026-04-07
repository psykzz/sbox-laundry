using Sandbox;

public sealed class PickupItem : Component
{
	[Property]
	public bool DisablePhysicsWhileHeld { get; set; } = true;
	private GameObject beingHeldBy;
	private Rigidbody rigidbody;
	private GameObject originalParent;

	#region Component Lifecycle
	protected override void OnStart()
	{
		rigidbody = Components.Get<Rigidbody>( FindMode.InSelf | FindMode.InAncestors );
		if ( rigidbody is null )
			Log.Warning( $"PickupItem '{GameObject.Name}' does not have a Rigidbody component in its hierarchy. Pickup physics will not work." );

		originalParent = GameObject.Parent;

		var usable = Components.Get<Usable>( FindMode.InSelf );
		if ( usable is not null )
			usable.OnInteract += ( interactor ) => PickUp( interactor );
	}

	#endregion

	#region Public API
	public bool PickUp( GameObject parent )
	{
		if ( beingHeldBy is not null )
			return false;

		if ( parent is null )
			return false;

		beingHeldBy = parent;
		GameObject.SetParent( parent );
		SetPhysicsHeldState( true );
		return true;
	}

	public void Drop()
	{
		if ( beingHeldBy is null )
			return;

		beingHeldBy = null;
		var previousVelocity = rigidbody?.Velocity;
		GameObject.SetParent( originalParent );
		SetPhysicsHeldState( false );
		if ( previousVelocity is not null )
		{
			rigidbody.Velocity = previousVelocity ?? Vector3.Zero;
		}
	}

	public void Throw( Vector3 velocity )
	{
		Drop();
		if ( rigidbody is not null )
			rigidbody.Velocity = velocity;
	}

	public bool IsHeld()
	{
		return beingHeldBy is not null;
	}

	public bool IsHeldBy( GameObject obj )
	{
		return beingHeldBy == obj;
	}



	#endregion


	#region Private Methods
	private void SetPhysicsHeldState( bool isHeld )
	{
		if ( !DisablePhysicsWhileHeld )
			return;

		rigidbody?.Enabled = !isHeld;
		rigidbody?.Gravity = !isHeld;
	}
	#endregion
}

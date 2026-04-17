using System.Threading.Tasks;
using Sandbox;

public sealed class PickupItem : Component
{
	[Property]
	public bool DisablePhysicsWhileHeld { get; set; } = true;
	[Property, Group( "Private" )] private GameObject beingHeldBy;
	[Property, Group( "Private" )] private Rigidbody rigidbody;
	[Property, Group( "Private" )] private GameObject originalParent;

	private Usable _usable;

	#region Component Lifecycle
	protected override void OnStart()
	{
		rigidbody = Components.Get<Rigidbody>( FindMode.InSelf | FindMode.InAncestors );
		if ( rigidbody is null )
			Log.Warning( $"PickupItem '{GameObject.Name}' does not have a Rigidbody component in its hierarchy. Pickup physics will not work." );

		originalParent = GameObject.Parent;

		_usable = Components.Get<Usable>( FindMode.InSelf );
		if ( _usable is not null )
			_usable.OnInteract += ( interactor ) => PickUp( interactor );
	}

	protected override void OnEnabled()
	{
		_ = ResolveRigidBody();
	}

	public async Task ResolveRigidBody()
	{
		while ( rigidbody is null || !rigidbody.IsValid() )
		{
			rigidbody = GameObject.GetComponent<Rigidbody>( true );
			if ( rigidbody is not null && rigidbody.IsValid() )
				break;

			Log.Warning( $"PickupItem '{GameObject.Name}' is waiting for a valid Rigidbody component to be added to its GameObject." );
			await GameTask.Delay( 1 );
		}

		Log.Warning( $"PickupItem '{GameObject.Name}' found a valid Rigidbody component." );
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
		GameObject.SetParent( originalParent );
		SetPhysicsHeldState( false );
	}

	public void Throw( Vector3 direction, float ejectForce = 10f )
	{
		Drop();
		rigidbody.ApplyImpulse( direction.Normal * ejectForce );

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

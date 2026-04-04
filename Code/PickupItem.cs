using Sandbox;

public sealed class PickupItem : Component
{
	[Property]
	public bool DisablePhysicsWhileHeld { get; set; } = true;
	private GameObject beingHeldBy;
	private Rigidbody rigidbody;
	private HighlightOutline highlightOutline;
	private GameObject originalParent;
	private DebounceTimer _debouncer = new();

	#region Component Lifecycle
	protected override void OnStart()
	{
		rigidbody = GameObject.Components.Get<Rigidbody>( FindMode.InSelf | FindMode.InAncestors );
		if ( rigidbody is null )
			Log.Warning( $"PickupItem '{GameObject.Name}' does not have a Rigidbody component in its hierarchy. Pickup physics will not work." );

		highlightOutline = GameObject.Components.Get<HighlightOutline>( FindMode.InSelf | FindMode.InAncestors );
		if ( highlightOutline is null )
			Log.Warning( $"PickupItem '{GameObject.Name}' does not have a HighlightOutline component in its hierarchy. Highlighting will not work." );

		originalParent = GameObject.Parent;
	}

	protected override void OnUpdate()
	{
		_debouncer.Tick();
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
		Highlight( 0.5f );
		return true;
	}

	public void Drop()
	{
		beingHeldBy = null;
		var previousVelocity = rigidbody?.Velocity;
		GameObject.SetParent( originalParent );
		SetPhysicsHeldState( false );
		if ( previousVelocity is not null )
		{
			rigidbody.Velocity = previousVelocity ?? Vector3.Zero;
		}
	}

	public bool IsHeld()
	{
		return beingHeldBy is not null;
	}

	public bool IsHeldBy( GameObject obj )
	{
		return beingHeldBy == obj;
	}

	public void Highlight( float delay = 5f )
	{
		if ( highlightOutline is null )
			return;
		highlightOutline.Enabled = true;
		_debouncer.Run( "highlight", delay, () => highlightOutline.Enabled = false );
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

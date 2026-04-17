
[Title( "Player Interaction Controller" )]
[Category( "Player" )]
[Icon( "touch_app" )]
public sealed class PlayerInteractionController : Component
{
	[Property, Group( "Punch" ), InputAction]
	public string PunchAction { get; set; } = "attack2";

	[Property, Group( "Punch" ), Range( 1f, 100f ), Step( 1f )]
	public float PunchAmount { get; set; } = 5f;

	private PickupItem _heldItem;
	private string _heldAction;
	private CameraComponent _camera;

	protected override void OnStart()
	{
		_camera = GetComponentInParent<CameraComponent>( true );
		if ( _camera is null )
			Log.Error( "PlayerInteractionController requires a CameraComponent in its parent hierarchy." );

		_camera.Enabled = !IsProxy;
	}

	protected override void OnUpdate()
	{
		if ( IsProxy ) return;

		if ( _heldItem?.GameObject is null )
		{
			_heldItem = null;
			_heldAction = null;
		}

		TryDrop();
		TryPunch();

		if ( _heldItem is null )
		{
			var usable = RaycastForUsable();
			if ( usable is not null && usable.CanInteractWith( GameObject ) )
			{
				usable.GameObject.GetComponent<InteractionHint>()?.Highlight( 0.1f );
				if ( Input.Pressed( usable.Action ) )
					TryInteract( usable );
			}
		}
	}

	private void TryDrop()
	{
		if ( _heldItem is null || _heldAction is null )
			return;

		if ( !Input.Released( _heldAction ) )
			return;

		if ( _heldItem.IsHeld() )
			_heldItem.Drop();
		_heldItem = null;
		_heldAction = null;
	}

	private void TryPunch()
	{
		if ( !Input.Pressed( PunchAction ) || _heldItem is null || _camera is null )
			return;

		_heldItem.Throw( _camera.WorldRotation.Forward * PunchAmount * 100f );
		_heldItem = null;
		_heldAction = null;
	}

	private void TryInteract( Usable usable )
	{
		usable.Interact( GameObject );

		// If the interaction caused a pickup on this object, track it for drop/throw
		var pickupItem = usable.Components.Get<PickupItem>( FindMode.InSelf | FindMode.InAncestors );
		if ( pickupItem is not null && pickupItem.IsHeldBy( GameObject ) )
		{
			_heldItem = pickupItem;
			_heldAction = usable.Action;
		}
	}

	private Usable RaycastForUsable()
	{
		if ( _camera is null )
			return null;

		var ray = new Ray( _camera.WorldPosition, _camera.WorldRotation.Forward );
		var trace = Scene.Trace.Ray( ray, 100f )
			.IgnoreGameObjectHierarchy( _camera.GameObject.Parent )
			.Run();

		if ( !trace.Hit || trace.GameObject is null )
			return null;

		return trace.GameObject.GetComponent<Usable>();
	}

	protected override void DrawGizmos()
	{
		if ( _camera is null )
			return;

		var worldStart = _camera.WorldPosition;
		var worldEnd = worldStart + _camera.WorldRotation.Forward * 250f;

		var localStart = Transform.World.PointToLocal( worldStart );
		var localEnd = Transform.World.PointToLocal( worldEnd );

		Gizmo.Draw.Color = Color.Yellow.WithAlpha( 0.8f );
		Gizmo.Draw.Line( localStart, localEnd );
		Gizmo.Draw.LineSphere( localEnd, 2f );
	}
}

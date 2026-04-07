

public sealed class PlayerPickupController : Component
{


	[Group( "Pickup" ), Property, Range( 5f, 100f ), Step( 1f )]
	public float PickupRadius { get; set; } = 24f;

	[Group( "Pickup" ), Property, InputAction]
	public string PickupAction { get; set; } = "attack1";


	[Property, Group( "Punch" ), InputAction]
	public string PunchAction { get; set; } = "attack2";
	[Property, Group( "Punch" ), Range( 1f, 100f ), Step( 1f )]
	public float PunchAmount { get; set; } = 5f;


	private PickupItem heldItem;
	private CameraComponent _camera;

	protected override void OnStart()
	{
		_camera = GetComponentInParent<CameraComponent>();
		if ( _camera is null )
			Log.Error( "PlayerPickupController requires a CameraComponent in its parent hierarchy." );
	}

	protected override void OnUpdate()
	{
		if ( heldItem?.GameObject is null )
			heldItem = null;

		if ( heldItem is null )
			HighlightBestPickupCandidate();

		TryHoldItem();
		TryPunch();
	}

	private void TryHoldItem()
	{
		if ( Input.Pressed( PickupAction ) && heldItem is null )
		{
			TryPickUp();
		}
		else if ( Input.Released( PickupAction ) && heldItem is not null )
		{
			heldItem.Drop();
			heldItem = null;
		}
	}

	private void TryPunch()
	{
		if ( !Input.Pressed( PunchAction ) || heldItem is null || _camera is null )
			return;

		heldItem.Throw( _camera.WorldRotation.Forward * PunchAmount * 100f );
		heldItem = null;
	}

	protected override void DrawGizmos()
	{
		if ( _camera is null )
			return;

		var worldStart = _camera.WorldPosition;
		var worldEnd = worldStart + _camera.WorldRotation.Forward * 250f;

		// Convert to local space so the gizmo lines up with the object's transform
		var localStart = Transform.World.PointToLocal( worldStart );
		var localEnd = Transform.World.PointToLocal( worldEnd );

		Gizmo.Draw.Color = Color.Yellow.WithAlpha( 0.8f );
		Gizmo.Draw.Line( localStart, localEnd );
		Gizmo.Draw.LineSphere( localEnd, 2f );
	}

	private void HighlightBestPickupCandidate()
	{
		var candidate = FindBestPickupCandidate();
		candidate?.GameObject.GetComponent<InteractionHint>()?.Highlight( 0.1f );
	}

	private void TryPickUp()
	{
		var pickupItem = FindBestPickupCandidate();
		if ( pickupItem is null )
			return;

		if ( !pickupItem.PickUp( GameObject ) )
			return;

		heldItem = pickupItem;
	}

	private PickupItem RaycastForPickup()
	{
		if ( _camera is null )
			return null;

		var ray = new Ray( _camera.WorldPosition, _camera.WorldRotation.Forward );
		var trace = Scene.Trace.Ray( ray, 100f )
			.IgnoreGameObjectHierarchy( _camera.GameObject.Parent )
			.Run();

		if ( !trace.Hit || trace.GameObject is null )
			return null;

		return trace.GameObject.GetComponent<PickupItem>();
	}

	private PickupItem NearestToSphere()
	{
		var pickupCenter = _camera.WorldPosition;
		var pickupRadiusSquared = PickupRadius * PickupRadius;
		var forward = _camera.WorldRotation.Forward;

		PickupItem bestItem = null;
		var bestDistanceSquared = float.MaxValue;
		var bestAlignment = float.MinValue;

		foreach ( var pickupItem in Scene.GetAllComponents<PickupItem>() )
		{
			if ( pickupItem?.GameObject is null )
			{
				Log.Warning( "Found a PickupItem component with no GameObject reference. Skipping." );
				continue;
			}

			if ( IsOwnedByPlayer( pickupItem ) )
			{
				Log.Warning( "Found a PickupItem component owned by the player. Skipping." );
				continue;
			}

			var offset = pickupItem.WorldPosition - pickupCenter;
			var distanceSquared = offset.LengthSquared;
			if ( distanceSquared > pickupRadiusSquared )
			{
				continue;
			}

			var alignment = distanceSquared > 0f
				? Vector3.Dot( forward, offset.Normal )
				: 1f;

			if ( bestItem is not null
				&& distanceSquared > bestDistanceSquared )
			{ Log.Warning( "Found a PickupItem component distanceSquared > bestDistanceSquared. Skipping." ); continue; }

			if ( bestItem is not null
				&& distanceSquared == bestDistanceSquared
				&& alignment <= bestAlignment )

			{ Log.Warning( "Found a PickupItem component distanceSquared == bestDistanceSquared && alignment <= bestAlignment. Skipping." ); continue; }

			bestItem?.GetComponent<HighlightOutline>()?.Enabled = false;
			bestItem = pickupItem;
			bestItem.GetComponent<HighlightOutline>()?.Enabled = true;
			bestDistanceSquared = distanceSquared;
			bestAlignment = alignment;

		}

		return bestItem;
	}

	private PickupItem FindBestPickupCandidate()
	{
		return RaycastForPickup();
	}

	private bool IsOwnedByPlayer( PickupItem candidate )
	{
		return candidate?.IsHeldBy( GameObject ) ?? false;
	}
}

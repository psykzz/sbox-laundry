

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

	protected override void OnUpdate()
	{

		UpdateHeldItemTransform();

		if ( heldItem is null )
			HighlightBestPickupCandidate();

		TryHoldItem();
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

	private CameraComponent GetCamera()
	{
		var camera = GetComponentInParent<CameraComponent>();
		if ( camera is null )
		{
			Log.Error( "PlayerPickupController requires a CameraComponent in its parent hierarchy." );
			return null;
		}
		return camera;
	}

	protected override void DrawGizmos()
	{

		var camera = GetCamera();
		if ( camera is null )
			return;

		var worldStart = camera.WorldPosition;
		var worldEnd = worldStart + camera.WorldRotation.Forward * 250f;

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
		// AttachHeldItem();
	}

	private PickupItem SlowClosestPickup()
	{

		var items = Scene.GetAllComponents<PickupItem>();
		var item = items.Where( item => item.GameObject is not null )
					.Where( item => !IsOwnedByPlayer( item ) )
					.Select( item => new
					{
						Item = item,
						DistanceSquared = (item.WorldPosition - GetPickupCenterWorld()).LengthSquared
					} )
					// .Where( x => x.DistanceSquared <= PickupRadius * PickupRadius )
					.OrderBy( x => x.DistanceSquared )
					.Select( x => x.Item );
		return item.FirstOrDefault();

	}

	private PickupItem RaycastForPickup()
	{
		var camera = GetCamera();
		if ( camera is null )
			return null;

		var ray = new Ray( camera.WorldPosition, camera.WorldRotation.Forward );
		var trace = Scene.Trace.Ray( ray, 100f )
			.IgnoreGameObjectHierarchy( camera.GameObject.Parent )
			.Run();

		// debugging
		// DebugOverlay.Trace( trace, duration: 1f );

		if ( !trace.Hit || trace.GameObject is null )
			return null;

		return trace.GameObject.GetComponent<PickupItem>();
	}

	private PickupItem NearestToSphere()
	{
		var pickupCenter = GetPickupCenterWorld();
		var pickupRadiusSquared = PickupRadius * PickupRadius;
		var forward = WorldRotation.Forward;

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


	private void UpdateHeldItemTransform()
	{
		if ( heldItem?.GameObject is null )
		{
			heldItem = null;
			return;
		}
	}

	private void AttachHeldItem()
	{
		var spring = GetComponentInParent<SpringJoint>( includeDisabled: true );
		if ( spring is not null )
		{
			spring.Enabled = true;
			spring.AnchorBody = heldItem.GameObject;
			return;
		}
	}

	private Vector3 GetPickupCenterWorld()
	{
		var camera = GetCamera();
		if ( camera is null )
			return Vector3.Zero;

		return camera.WorldPosition;
	}


	private bool IsOwnedByPlayer( PickupItem candidate )
	{
		return candidate?.IsHeldBy( GameObject ) ?? false;
	}
}

using System.Threading.Tasks;
using Sandbox;

[Title( "Folding Station" ), Category( "Laundry" )]
public sealed class FoldingStation : Component, Component.ICollisionListener
{
	private bool _isFolding = false;
	public bool IsFolding => _isFolding;

	[Property, Group( "Setup" )]
	public Collider FoldingArea { get; private set; }

	[Property, Group( "Internals" )]
	public List<GameObject> StoredClothing { get; private set; } = new();

	[Property, Group( "Setup" )]
	public float FoldingDuration { get; set; } = 2f;

	[Property, Group( "Setup" )]
	public int MaxClothingItems { get; set; } = 1;

	[Property, Group( "Eject" )]
	public float EjectForce { get; set; } = 400f;

	[Property, Group( "Eject" )]
	public float EjectDoorOffset { get; set; } = 20f;

	[Property, Group( "Eject" )]
	public float DisableAreaDuration { get; set; } = 2f;

	private TimeUntil _reenableArea;
	private bool _areaDisabled;

	private Vector3 _ejectPosition => FoldingArea.LocalBounds.Center + FoldingArea.WorldRotation.Forward * EjectDoorOffset;
	private Vector3 _ejectDirection => (FoldingArea.WorldRotation.Forward + Vector3.Up * 0.3f).Normal;

	protected override void OnFixedUpdate()
	{
		if ( _areaDisabled && _reenableArea )
		{
			FoldingArea.Enabled = true;
			_areaDisabled = false;
		}
	}

	public bool AddClothing( GameObject clothing )
	{
		if ( !clothing.IsValid() )
			return false;

		if ( StoredClothing.Count >= MaxClothingItems )
			return false;

		var washable = clothing.GetComponent<Washable>();
		if ( washable is null || washable.State != LaundryState.Ironed )
			return false;

		clothing.GetComponent<PickupItem>()?.Drop();
		clothing.GetComponent<PickupItem>()?.PickUp( GameObject );
		StoredClothing.Add( clothing );
		clothing.WorldPosition = FoldingArea.WorldPosition + Vector3.Up * -10f;
		clothing.Enabled = false;
		return true;
	}

	public bool StartFold()
	{
		if ( _isFolding || StoredClothing.Count == 0 )
		{
			Sound.Play( "beep", GameObject.WorldPosition );
			return false;
		}

		_ = FoldAsync();
		return true;
	}

	private async Task FoldAsync()
	{
		_isFolding = true;
		await Task.DelaySeconds( FoldingDuration );
		_isFolding = false;
		_ = EjectClothing();
	}

	private async Task EjectClothing()
	{
		if ( FoldingArea is not null )
		{
			FoldingArea.Enabled = false;
			_areaDisabled = true;
			_reenableArea = DisableAreaDuration;
		}

		foreach ( var clothing in StoredClothing.ToList() )
		{
			if ( !clothing.IsValid() )
				continue;

			var washable = clothing.GetComponent<Washable>();
			if ( washable is not null && washable.State == LaundryState.Ironed )
				washable.Fold();

			clothing.WorldPosition = Transform.World.PointToWorld( _ejectPosition );
			clothing.Enabled = true;
			await clothing.GetComponent<PickupItem>()?.ResolveRigidBody();
			clothing.GetComponent<PickupItem>()?.Throw( _ejectDirection, EjectForce );
		}

		StoredClothing.Clear();
	}

	public void OnCollisionStart( Collision collision )
	{
		if ( collision.Self.Collider == FoldingArea )
		{
			var washable = collision.Other.GameObject.GetComponent<Washable>();
			if ( washable is not null && washable.State == LaundryState.Ironed )
				AddClothing( collision.Other.GameObject );
		}
	}

	public void OnCollisionStop( Collision collision ) { }

	protected override void DrawGizmos()
	{
		if ( FoldingArea is null ) return;
		var localStart = _ejectPosition;
		var localEnd = _ejectPosition + (_ejectDirection * 20f);
		Gizmo.Draw.Arrow( localStart, localEnd );
	}
}

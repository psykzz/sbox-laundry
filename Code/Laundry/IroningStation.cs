using System.Threading.Tasks;
using Sandbox;

[Title( "Ironing Station" ), Category( "Laundry" )]
public sealed class IroningStation : Component, Component.ICollisionListener
{
	private TimeUntil _finishIron;
	public bool IsIroning => !_finishIron;

	[Property, Group( "Setup" )]
	public Collider IroningArea { get; private set; }

	[Property, Group( "Internals" )]
	public List<GameObject> StoredClothing { get; private set; } = new();

	[Property, Group( "Setup" )]
	public float IroningDuration { get; set; } = 8f;

	/// <summary>How long after ironing finishes before the item burns if not collected.</summary>
	[Property, Group( "Setup" )]
	public float BurntTimeout { get; set; } = 20f;

	[Property, Group( "Setup" )]
	public int MaxClothingItems { get; set; } = 1;

	[Property, Group( "Eject" )]
	public float EjectForce { get; set; } = 800f;

	[Property, Group( "Eject" )]
	public float EjectStaggerDelay { get; set; } = 0.2f;

	[Property, Group( "Eject" )]
	public float DisableAreaDuration { get; set; } = 2f;

	[Property, Group( "Eject" )]
	public float EjectDoorOffset { get; set; } = 20f;

	private bool _wasRunning = false;
	private TimeUntil _reenableArea;
	private bool _areaDisabled;
	private TimeSince _completedAt;
	private bool _completedAndWaiting = false;

	private Vector3 _ejectPosition => IroningArea.LocalBounds.Center + IroningArea.WorldRotation.Forward * EjectDoorOffset;
	private Vector3 _ejectDirection => (IroningArea.WorldRotation.Forward + Vector3.Up * 0.5f).Normal;

	protected override void OnFixedUpdate()
	{
		if ( IsIroning )
		{
			var breakdown = Components.Get<MachineBreakdown>( FindMode.InSelf );
			if ( breakdown is null || !breakdown.IsBrokenDown )
			{
				breakdown?.TryBreakdown();
			}
		}

		if ( _wasRunning && !IsIroning )
		{
			_completedAndWaiting = true;
			_completedAt = 0;
		}

		if ( _completedAndWaiting && _completedAt >= BurntTimeout )
		{
			BurnClothing();
			_completedAndWaiting = false;
		}

		if ( _areaDisabled && _reenableArea )
		{
			IroningArea.Enabled = true;
			_areaDisabled = false;
		}

		_wasRunning = IsIroning;
	}

	public bool AddClothing( GameObject clothing )
	{
		if ( !clothing.IsValid() )
			return false;

		if ( StoredClothing.Count >= MaxClothingItems )
			return false;

		var washable = clothing.GetComponent<Washable>();
		if ( washable is null || washable.State != LaundryState.Washed )
			return false;

		clothing.GetComponent<PickupItem>()?.Drop();
		clothing.GetComponent<PickupItem>()?.PickUp( GameObject );
		StoredClothing.Add( clothing );
		clothing.WorldPosition = IroningArea.WorldPosition + Vector3.Up * -10f;
		clothing.Enabled = false;
		return true;
	}

	public bool StartIron()
	{
		var breakdown = Components.Get<MachineBreakdown>( FindMode.InSelf );
		if ( breakdown?.IsBrokenDown == true )
		{
			Sound.Play( "beep", GameObject.WorldPosition );
			return false;
		}

		if ( StoredClothing.Count == 0 )
		{
			Sound.Play( "beep", GameObject.WorldPosition );
			return false;
		}

		_finishIron = IroningDuration;
		_completedAndWaiting = false;
		return true;
	}

	private void BurnClothing()
	{
		foreach ( var clothing in StoredClothing )
		{
			if ( clothing.IsValid() )
				clothing.GetComponent<Washable>()?.Burn();
		}
		_ = EjectClothing();
	}

	private async Task EjectClothing()
	{
		if ( IroningArea is not null )
		{
			IroningArea.Enabled = false;
			_areaDisabled = true;
			_reenableArea = DisableAreaDuration;
		}

		foreach ( var clothing in StoredClothing.ToList() )
		{
			if ( !clothing.IsValid() )
				continue;

			var washable = clothing.GetComponent<Washable>();
			if ( washable is not null && washable.State == LaundryState.Washed )
				washable.Iron();

			clothing.WorldPosition = Transform.World.PointToWorld( _ejectPosition );
			clothing.Enabled = true;
			await clothing.GetComponent<PickupItem>()?.ResolveRigidBody();
			clothing.GetComponent<PickupItem>()?.Throw( _ejectDirection, EjectForce );
			await Task.DelaySeconds( EjectStaggerDelay );
		}

		StoredClothing.Clear();
		_completedAndWaiting = false;
	}

	public void OnCollisionStart( Collision collision )
	{
		if ( collision.Self.Collider == IroningArea )
		{
			var washable = collision.Other.GameObject.GetComponent<Washable>();
			if ( washable is not null && washable.State == LaundryState.Washed )
				AddClothing( collision.Other.GameObject );
		}
	}

	public void OnCollisionStop( Collision collision ) { }

	protected override void DrawGizmos()
	{
		if ( IroningArea is null ) return;
		var localStart = _ejectPosition;
		var localEnd = _ejectPosition + (_ejectDirection * 20f);
		Gizmo.Draw.Arrow( localStart, localEnd );
	}
}

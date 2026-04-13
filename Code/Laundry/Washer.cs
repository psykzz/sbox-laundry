using System;
using System.Threading.Tasks;

public sealed class Washer : Machine, Component.ICollisionListener
{


	[Property, Group( "Setup" )]
	public GameObject StartWashButton { get; private set; }

	private TimeUntil _finishWash;
	public bool IsWashing => _finishWash.Fraction < 1f;

	[Property, Group( "Setup" )]
	public Collider WashingArea { get; private set; }

	[Property, Group( "Internals" )]
	public List<GameObject> StoredClothing { get; private set; }


	public GameObject CurrentDetergent { get; private set; }


	[Property, Group( "Visuals" )]
	public float ShakeAmount { get; set; } = 1f;

	[Property, Group( "Setup" )]
	public float WashingDuration { get; set; } = 10f;

	[Property, Group( "Visuals" )]
	public SoundPointComponent WashingSound { get; private set; }


	[Property, Group( "Setup" )]
	public int MaxClothingItems { get; set; } = 5;

	[Property, Group( "Eject" ), Title( "Eject Force" )]
	public float EjectForce { get; set; } = 2500f;

	[Property, Group( "Eject" ), Title( "Eject Stagger Delay" )]
	public float EjectStaggerDelay { get; set; } = 0.3f;

	[Property, Group( "Eject" ), Title( "Disable Area Duration" )]
	public float DisableAreaDuration { get; set; } = 3f;

	[Property, Group( "Eject" ), Title( "Eject Door Offset" )]
	public float EjectDoorOffset { get; set; } = 30f;

	private bool _wasRunning;
	private TimeUntil _reenableArea;
	private bool _areaDisabled;

	private Vector3 _ejectPositon => WashingArea.LocalBounds.Center + WashingArea.WorldRotation.Forward * EjectDoorOffset;
	private Vector3 _ejectDirection => (WashingArea.WorldRotation.Forward + Vector3.Up * 0.6f).Normal;


	protected override void OnFixedUpdate()
	{
		if ( IsWashing )
		{
			if ( !IsBrokenDown )
			{
				TryBreakdown();
			}
		}

		if ( _wasRunning && !IsWashing )
		{
			_ = EjectClothing();
		}

		if ( _areaDisabled && _reenableArea )
		{
			WashingArea.Enabled = true;
			_areaDisabled = false;
		}

		if ( IsWashing )
		{
			var rotation = Math.Ceiling( Time.Now * 10 ) % 2 == 0 ? ShakeAmount : -ShakeAmount;
			Shake( rotation );
		}
		else
		{
			WashingSound?.StopSound();
			Shake();
		}

		_wasRunning = IsWashing;
	}

	public bool AddClothing( GameObject clothing )
	{
		if ( clothing.IsValid() == false )
		{
			Log.Warning( $"Attempted to add invalid clothing object to washer: {clothing}" );
			return false;
		}
		if ( StoredClothing.Count >= MaxClothingItems )
		{
			Error();
			return false;
		}

		clothing.GetComponent<PickupItem>()?.Drop();
		clothing.GetComponent<PickupItem>()?.PickUp( GameObject );
		StoredClothing.Add( clothing );
		clothing.Enabled = false;
		return true;
	}

	public bool AddDetergent( GameObject detergent )
	{
		if ( CurrentDetergent is not null )
		{
			Error();
			return false;
		}

		detergent.GetComponent<PickupItem>()?.Drop();
		detergent.GetComponent<PickupItem>()?.PickUp( GameObject );
		detergent.Enabled = false;
		CurrentDetergent = detergent;
		return true;
	}
	public bool StartWash()
	{
		if ( StoredClothing.Count == 0 )
		{
			Error( true );
			return false;
		}

		if ( IsBrokenDown )
		{
			Error( true );
			return false;
		}

		WashingSound?.StartSound();
		_finishWash = WashingDuration;
		return true;
	}

	public void Error( bool beep = false )
	{
		if ( beep )
			Sound.Play( "beep", GameObject.WorldPosition );
	}

	public void OnCollisionStart( Collision collision )
	{
		if ( collision.Self.Collider != WashingArea )
			return;

		var other = collision.Other.GameObject;

		if ( other.GetComponent<Washable>() is not null )
			AddClothing( other );
		else if ( other.GetComponent<Detergent>() is not null )
			AddDetergent( other );
	}


	private static bool IsWhiteColor( Color c ) => c.r > 0.9f && c.g > 0.9f && c.b > 0.9f;

	private async Task EjectClothing()
	{
		BleedColours();
		UpdateDirtiness();
		DisableWashingArea();
		await Eject();
	}

	private void BleedColours()
	{
		// Color bleed: colored items tint white items washed in the same load
		var washables = StoredClothing
			.Where( c => c.IsValid() )
			.Select( c => c.GetComponent<WashableShirt>() )
			.Where( w => w is not null )
			.ToList();

		bool hasColoredItem = washables.Any( w => !IsWhiteColor( w.ClothingColor ) );
		if ( !hasColoredItem )
			return;

		var dominantColor = washables.First( w => !IsWhiteColor( w.ClothingColor ) ).ClothingColor;
		foreach ( var w in washables.Where( w => IsWhiteColor( w.ClothingColor ) ) )
		{
			var bleedColor = Color.Lerp( Color.White, dominantColor, 0.4f );
			w.ClothingColor = bleedColor;
			w.GameObject.GetComponent<WashableShirt>()?.ClothingColor = bleedColor;
		}
	}

	private void UpdateDirtiness()
	{
		// TODO: Wash() is not being called correctly
		bool hasDetergent = CurrentDetergent is not null;
		foreach ( var clothing in StoredClothing.Where( c => c.IsValid() ) )
			clothing.GetComponent<WashableShirt>()?.Wash( hasDetergent );
	}

	private void DisableWashingArea()
	{
		CurrentDetergent = null;

		if ( WashingArea is not null )
		{
			WashingArea.Enabled = false;
			_areaDisabled = true;
			_reenableArea = DisableAreaDuration;
		}
	}

	private async Task Eject()
	{
		foreach ( var clothing in StoredClothing.ToList() )
		{
			if ( !clothing.IsValid() )
				continue;

			clothing.WorldPosition = Transform.World.PointToWorld( _ejectPositon );
			clothing.Enabled = true;
			await clothing.GetComponent<PickupItem>()?.ResolveRigidBody();
			clothing.GetComponent<PickupItem>()?.Throw( _ejectDirection, EjectForce );
			await Task.DelaySeconds( EjectStaggerDelay );
		}

		StoredClothing.Clear();
	}

	public void Shake( float rotation = 0f )
	{
		var washer = GameObject;
		var angle = washer.WorldRotation.Angles();
		angle.roll = rotation;
		washer.WorldRotation = angle.ToRotation();
	}

	protected override void DrawGizmos()
	{
		// Draw a forward arrow to show the eject direction in the editor
		var localStart = _ejectPositon;
		var localEnd = _ejectPositon + (_ejectDirection * 30f);

		Gizmo.Draw.Arrow( localStart, localEnd );
	}

}

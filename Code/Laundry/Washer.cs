using System;
using Sandbox;
using Sandbox.UI;

public sealed class Washer : Component, Component.ICollisionListener
{


	[Property]
	public GameObject StartWashButton { get; private set; }

	private TimeUntil _finishWash;
	public bool IsRunning => _finishWash.Fraction < 1f;

	[Property]
	public Collider WashingArea { get; private set; }

	[Property]
	public List<GameObject> Clothing { get; private set; }


	public GameObject CurrentDetergent { get; private set; }


	[Property]
	public float ShakeAmount { get; set; } = 1f;

	[Property]
	public float WashingDuration { get; set; } = 10f;

	[Property]
	public SoundPointComponent WashingSound { get; private set; }


	[Property]
	public int MaxClothingItems { get; set; } = 5;


	protected override void OnFixedUpdate()
	{
		if ( !_finishWash )
		{
			var rotation = Math.Ceiling( Time.Now * 10 ) % 2 == 0 ? ShakeAmount : -ShakeAmount;
			Shake( rotation );
		}
		else
		{
			WashingSound?.StopSound();
			Shake();
		}
	}

	public bool AddClothing( GameObject clothing )
	{
		if ( Clothing.Count >= MaxClothingItems )
		{
			Error();
			return false;
		}

		clothing.GetComponent<PickupItem>()?.Drop();

		Clothing.Add( clothing );
		clothing.WorldPosition = WashingArea.WorldPosition + Vector3.Up * -10f;
		clothing.GetComponent<Prop>()?.Enabled = false;
		return true;
	}

	public bool AddDetergent( GameObject detergent )
	{
		if ( CurrentDetergent is not null )
		{
			Error();
			return false;
		}

		CurrentDetergent = detergent;
		return true;
	}
	public bool StartWash()
	{
		// todo: FIX detergent
		// if ( Clothing.Count == 0 || CurrentDetergent is null )
		// {
		// 	Error( true );
		// 	return false;
		// }

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
		if ( collision.Self.Collider == WashingArea && collision.Other.GameObject.GetComponent<Washable>() is not null )
		{
			AddClothing( collision.Other.GameObject );
			Log.Info( "Entered washing area" );
		}
	}


	public void Shake( float rotation = 0f )
	{
		var washer = GameObject;
		var angle = washer.WorldRotation.Angles();
		angle.roll = rotation;
		washer.WorldRotation = angle.ToRotation();
	}

}

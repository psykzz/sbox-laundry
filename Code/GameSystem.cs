using System;
using Sandbox;

public sealed class GameSystem : Component
{

	public static GameSystem Instance { get; private set; }

	[Property, Sync]
	public int Money { get; private set; } = 0;

	[Property]
	public List<Color> TshirtColours { get; set; }

	protected override void OnStart()
	{
		Instance = this;
	}

	protected override void OnDestroy()
	{
		if ( Instance == this )
			Instance = null;
	}

	public Sandbox.GameObject SpawnShirt( GameObject prefab, Vector3 position, Rotation rotation )
	{
		var spawned = prefab.Clone( position, rotation );
		spawned.NetworkSpawn();
		var color = System.Random.Shared.FromList( TshirtColours, default );
		var washable = spawned.GetComponent<WashableShirt>();
		if ( washable is not null )
			washable.ClothingColor = color;
		return spawned;
	}

	public bool PurchaseGib()
	{
		if ( Money < 1 )
			return false;

		Money -= 1;
		SpawnGib();
		return true;
	}

	private void SpawnGib()
	{
		Log.Warning( "SpawnGib is not yet implemented." );
	}

	public bool SellGib( GameObject gib )
	{
		if ( gib is null )
			return false;

		// Implementation for selling a gib
		Money += 1;
		return true;
	}


}

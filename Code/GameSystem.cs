using System;
using Sandbox;

public sealed class GameSystem : Component
{

	public static GameSystem Instance { get; private set; }

	[Property]
	public int Money { get; private set; } = 0;

	[Property]
	public List<Color> TshirtColours { get; set; }

	private List<GameObject> _tshirts = new();


	protected override void OnStart()
	{
		Instance = this;
	}

	public Sandbox.GameObject SpawnShirt( GameObject prefab, Vector3 position, Rotation rotation )
	{
		var spawned = prefab.Clone( position, rotation );
		spawned.NetworkSpawn();
		spawned.GetComponent<Prop>()?.Tint = System.Random.Shared.FromList( TshirtColours, default );
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
		throw new NotImplementedException();
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

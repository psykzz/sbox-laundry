using System;
public sealed class Spawner : Component
{
	[Property]
	public GameObject[] ItemsToSpawn { get; set; }

	[Property, Range( 0.1f, 60f ), Step( 0.25f )]
	public float SpawnInterval { get; set; } = 5f;

	[Property]
	private TimeUntil nextSpawn;

	[Property]
	public BoxCollider SpawnArea { get; private set; }

	protected override void OnStart()
	{
		if ( ItemsToSpawn is null || ItemsToSpawn.Length == 0 )
		{
			Log.Warning( $"Spawner '{GameObject.Name}' has no items assigned to spawn." );
		}

		if ( SpawnArea is null )
		{
			Log.Warning( $"Spawner '{GameObject.Name}' does not have a BoxCollider assigned for SpawnArea. Spawning will use the Spawner's position as the spawn point." );
		}

		nextSpawn = Time.Now;
	}

	protected override void OnFixedUpdate()
	{
		if ( nextSpawn )
		{
			var spawnPosition = GetRandomSpawnPoint();
			var itemToSpawn = Random.Shared.FromArray( ItemsToSpawn, default );
			_ = GameSystem.Instance.SpawnShirt( itemToSpawn, spawnPosition, Rotation.Random );

			nextSpawn = SpawnInterval;
		}
	}

	protected override void DrawGizmos()
	{
		if ( SpawnArea is not null )
		{
			Gizmo.Draw.LineBBox( SpawnArea.LocalBounds );
		}
	}

	public Vector3 GetRandomSpawnPoint()
	{
		if ( SpawnArea is not null )
		{

			var point = SpawnArea.Transform.World.PointToWorld( Random.Shared.VectorInCube( SpawnArea.LocalBounds ) );
			point.z = SpawnArea.WorldPosition.z - 50f; // Ensure the spawn point is at the same Z level as the spawner
			return point;
		}
		else
		{
			return GameObject.WorldPosition;
		}
	}
}

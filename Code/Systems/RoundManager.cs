using Sandbox;

public enum RoundState
{
	Waiting,
	Active,
	Ended
}

[Title( "Round Manager" ), Category( "Systems" )]
public sealed class RoundManager : Component
{
	public static RoundManager Instance { get; private set; }

	[Property]
	public float RoundDuration { get; set; } = 180f;

	[Property, Sync]
	public int Score { get; private set; } = 0;

	[Property, Sync]
	public int Round { get; private set; } = 0;

	[Property, Sync]
	public RoundState State { get; private set; } = RoundState.Waiting;


	private TimeUntil _roundEnd;
	public float TimeRemaining => _roundEnd.Relative;

	protected override void OnStart()
	{
		Instance = this;
		StartRound();
	}

	protected override void OnDestroy()
	{
		if ( Instance == this )
			Instance = null;
	}

	protected override void OnFixedUpdate()
	{
		if ( State != RoundState.Active )
			return;

		if ( _roundEnd )
		{
			EndRound();
		}
	}

	public void StartRound()
	{
		Round++;
		Score = 0;
		_roundEnd = RoundDuration;
		State = RoundState.Active;
		Log.Info( $"Round {Round} started. Duration: {RoundDuration}s" );
	}

	public void EndRound()
	{
		State = RoundState.Ended;
		Log.Info( $"Round {Round} ended. Final score: {Score}" );
	}

	public void AddScore( int amount = 1 )
	{
		if ( State != RoundState.Active )
			return;
		Score += amount;
	}
}

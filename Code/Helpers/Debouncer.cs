using System;

// Notice this does NOT inherit from Component
public class DebounceTimer
{
	private class DebounceData
	{
		public Action Action { get; set; }
		public float Delay { get; set; }
		public TimeSince Timer { get; set; }
	}

	private Dictionary<string, DebounceData> _debounces = new();

	public void Run( string key, float delayInSeconds, Action action )
	{
		if ( _debounces.TryGetValue( key, out var data ) )
		{
			data.Timer = 0;
			data.Delay = delayInSeconds;
			data.Action = action;
		}
		else
		{
			_debounces[key] = new DebounceData { Action = action, Delay = delayInSeconds, Timer = 0 };
		}
	}

	// The owning component MUST call this manually
	public void Tick()
	{
		if ( _debounces.Count == 0 ) return;

		List<string> keysToRemove = null;

		foreach ( var kvp in _debounces )
		{
			if ( kvp.Value.Timer >= kvp.Value.Delay )
			{
				kvp.Value.Action?.Invoke();
				keysToRemove ??= new List<string>();
				keysToRemove.Add( kvp.Key );
			}
		}

		if ( keysToRemove != null )
		{
			foreach ( var key in keysToRemove ) _debounces.Remove( key );
		}
	}
}

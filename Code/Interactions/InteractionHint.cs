using Sandbox;

public sealed class InteractionHint : Component
{

	private HighlightOutline highlightOutline;
	private DebounceTimer _debouncer = new();
	protected override void OnStart()
	{
		highlightOutline = GameObject.Components.Get<HighlightOutline>( FindMode.InSelf | FindMode.InAncestors );
		if ( highlightOutline is null )
		{
			highlightOutline = GameObject.AddComponent<HighlightOutline>();
			highlightOutline.Enabled = false;
		}
	}

	protected override void OnUpdate()
	{
		_debouncer.Tick();
	}

	public void Highlight( float delay = 5f )
	{
		if ( highlightOutline is null )
		{
			return;
		}
		highlightOutline.Enabled = true;
		_debouncer.Run( "highlight", delay, () => highlightOutline.Enabled = false );
	}
}

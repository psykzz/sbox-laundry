using Sandbox;

public sealed class InteractionHint : Component
{

	private HighlightOutline highlightOutline;
	private WorldPanel _worldPanel;
	private InteractionHUD _hud;
	private DebounceTimer _debouncer = new();

	protected override void OnStart()
	{
		highlightOutline = GameObject.Components.Get<HighlightOutline>( FindMode.InSelf | FindMode.InAncestors );
		if ( highlightOutline is null )
		{
			highlightOutline = GameObject.AddComponent<HighlightOutline>();
			highlightOutline.Enabled = false;
		}

		_worldPanel = GameObject.Components.Get<WorldPanel>( FindMode.InSelf | FindMode.InAncestors );
		if ( _worldPanel is null )
		{
			_worldPanel = GameObject.AddComponent<WorldPanel>();
			_worldPanel.PanelSize = new Vector2( 500, 250 );
			_worldPanel.RenderScale = 0.25f;
			_worldPanel.Enabled = false;
		}

		_hud = GameObject.Components.Get<InteractionHUD>();
		if ( _hud is null )
		{
			_hud = GameObject.AddComponent<InteractionHUD>();
		}

		_hud.Enabled = false;
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

		// Pull the latest hint text from the Usable on this object each frame highlight is called
		if ( _hud is not null )
		{
			var usable = GameObject.Components.Get<Usable>();

			_hud.ActionKey = usable?.Action;
			_hud.HintText = usable?.HintText ?? _hud.HintText;
			_hud.Enabled = usable?.ShowInteractionHint ?? false;
		}

		highlightOutline.Enabled = true;
		_worldPanel.Enabled = true;

		_debouncer.Run( "highlight", delay, () =>
		{
			highlightOutline.Enabled = false;
			_worldPanel.Enabled = false;
			if ( _hud is not null ) _hud.Enabled = false;
		} );
	}
}

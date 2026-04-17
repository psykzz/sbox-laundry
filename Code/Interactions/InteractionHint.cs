using Sandbox;

public sealed class InteractionHint : Component
{

	private HighlightOutline highlightOutline;
	private WorldPanel _worldPanel;
	private InteractionHUD _hud;
	private DebounceTimer _debouncer = new();
	private Usable _usable;

	protected override void OnStart()
	{
		highlightOutline = GameObject.Components.Get<HighlightOutline>( FindMode.InSelf | FindMode.InAncestors );
		if ( highlightOutline is null )
		{
			highlightOutline = GameObject.AddComponent<HighlightOutline>();
			highlightOutline.Enabled = false;
		}

		_usable = GameObject.Components.Get<Usable>();
		if ( !_usable?.ShowInteractionHint ?? false )
			return;

		_worldPanel = GameObject.Components.Get<WorldPanel>( FindMode.InSelf | FindMode.InAncestors );
		if ( _worldPanel is null )
		{
			_worldPanel = GameObject.AddComponent<WorldPanel>();
			_worldPanel.PanelSize = new Vector2( 500, 250 );
			_worldPanel.RenderScale = 0.25f;
			_worldPanel.Enabled = false;
		}

		_hud = GameObject.Components.Get<InteractionHUD>( FindMode.InSelf | FindMode.InAncestors );
		if ( _hud is null )
		{
			_hud = GameObject.AddComponent<InteractionHUD>();
			_hud.Enabled = false;
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

		// Pull the latest hint text from the Usable on this object each frame highlight is called
		if ( (_usable?.ShowInteractionHint ?? false) && _hud is not null )
		{
			_worldPanel.Enabled = true;
			_hud.ActionKey = _usable?.Action;
			_hud.HintText = _usable?.HintText ?? _hud.HintText;
			_hud.Enabled = true;

		}

		highlightOutline.Enabled = true;


		_debouncer.Run( "highlight", delay, () =>
		{
			highlightOutline.Enabled = false;
			if ( _usable?.ShowInteractionHint ?? false )
			{
				_worldPanel.Enabled = false;
				if ( _hud is not null ) _hud.Enabled = false;
			}
		} );
	}
}

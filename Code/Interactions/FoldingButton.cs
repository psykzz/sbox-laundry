using Sandbox;

[Title( "Folding Button" ), Category( "Laundry" )]
public sealed class FoldingButton : Component
{
	[Property] public FoldingStation FoldingStation { get; set; }

	private Usable _usable;

	protected override void OnStart()
	{
		_usable = Components.Get<Usable>( FindMode.InSelf );
		if ( _usable is not null )
		{
			_usable.OnInteract += OnButtonPress;
			_usable.CanInteract += CanInteract;
		}
	}

	protected override void OnUpdate()
	{
		if ( _usable is null || !FoldingStation.IsValid() )
			return;

		if ( FoldingStation.IsFolding )
			_usable.HintText = "Folding...";
		else if ( FoldingStation.StoredClothing.Count > 0 )
			_usable.HintText = "Fold Clothing";
		else
			_usable.HintText = "Add ironed clothing";
	}

	private bool CanInteract( GameObject _interactor )
	{
		if ( !FoldingStation.IsValid() )
			return false;

		return !FoldingStation.IsFolding && FoldingStation.StoredClothing.Count > 0;
	}

	private void OnButtonPress( GameObject interactor )
	{
		if ( !FoldingStation.IsValid() )
			return;

		FoldingStation.StartFold();
	}
}

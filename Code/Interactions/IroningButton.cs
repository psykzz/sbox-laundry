using Sandbox;

[Title( "Ironing Button" ), Category( "Laundry" )]
public sealed class IroningButton : Component
{
	[Property] public IroningStation IroningStation { get; set; }

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
		if ( _usable is null || !IroningStation.IsValid() )
			return;

		var breakdown = IroningStation.Components.Get<MachineBreakdown>( FindMode.InSelf );
		if ( breakdown?.IsBrokenDown == true )
			_usable.HintText = "Repair Iron";
		else if ( IroningStation.IsIroning )
			_usable.HintText = "Ironing...";
		else if ( IroningStation.StoredClothing.Count > 0 )
			_usable.HintText = "Start Ironing";
		else
			_usable.HintText = "Add washed clothing";
	}

	public bool CanInteract( GameObject _interactor )
	{
		if ( !IroningStation.IsValid() )
			return false;

		var breakdown = IroningStation.Components.Get<MachineBreakdown>( FindMode.InSelf );
		if ( breakdown?.IsBrokenDown == true )
			return true;

		return !IroningStation.IsIroning && IroningStation.StoredClothing.Count > 0;
	}

	public void OnButtonPress( GameObject interactor )
	{
		if ( !IroningStation.IsValid() )
			return;

		var breakdown = IroningStation.Components.Get<MachineBreakdown>( FindMode.InSelf );
		if ( breakdown?.IsBrokenDown == true )
		{
			breakdown.Repair();
			return;
		}

		IroningStation.StartIron();
	}
}

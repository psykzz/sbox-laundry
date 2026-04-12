using Sandbox;

public sealed class WasherButton : Component
{
	[Property] public Washer WashingMachine { get; set; }

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
		if ( _usable is null || !WashingMachine.IsValid() )
			return;

		var breakdown = WashingMachine.Components.Get<MachineBreakdown>( FindMode.InSelf );
		if ( breakdown?.IsBrokenDown == true )
			_usable.HintText = "Repair Machine";
		else if ( WashingMachine.IsWashing )
			_usable.HintText = $"Washing... ({WashingMachine.StoredClothing.Count}/{WashingMachine.MaxClothingItems})";
		else
			_usable.HintText = $"Start Wash ({WashingMachine.StoredClothing.Count}/{WashingMachine.MaxClothingItems})";
	}

	public bool CanInteract( GameObject _interactor )
	{
		if ( !WashingMachine.IsValid() )
			return false;

		var breakdown = WashingMachine.Components.Get<MachineBreakdown>( FindMode.InSelf );
		if ( breakdown?.IsBrokenDown == true )
			return true;

		return !WashingMachine.IsWashing && WashingMachine.StoredClothing.Count > 0;
	}

	public void OnButtonPress( GameObject interactor )
	{
		if ( !WashingMachine.IsValid() )
			return;

		var breakdown = WashingMachine.Components.Get<MachineBreakdown>( FindMode.InSelf );
		if ( breakdown?.IsBrokenDown == true )
		{
			breakdown.Repair();
			return;
		}

		WashingMachine.StartWash();
	}
}

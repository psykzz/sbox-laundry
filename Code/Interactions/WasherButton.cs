using Sandbox;
using Sandbox.Audio;

public sealed class WasherButton : Component
{
	[Property] public Washer WashingMachine { get; set; }

	protected override void OnStart()
	{
		var usable = Components.Get<Usable>( FindMode.InSelf );
		if ( usable is not null )
		{
			usable.OnInteract += OnButtonPress;
			usable.CanInteract += CanInteract;
		}
	}

	public bool CanInteract( GameObject _interactor )
	{
		Log.Warning( $"CanInteract check for washer button. WashingMachine valid: {WashingMachine.IsValid()}, IsRunning: {WashingMachine.IsRunning}, Clothing count: {WashingMachine.StoredClothing.Count()}" );
		return WashingMachine.IsValid()
			&& !WashingMachine.IsRunning
			&& WashingMachine.StoredClothing.Count() > 0;
	}

	public void OnButtonPress( GameObject interactor )
	{
		if ( !WashingMachine.IsValid() )
			return;
		WashingMachine.StartWash();
	}
}

using Sandbox;

public sealed class SellTrigger : Component, Component.ITriggerListener
{
	private BoxCollider collider;

	protected override void OnStart()
	{
		collider = GameObject.Components.Get<BoxCollider>( FindMode.InSelf );
		if ( collider is null )
		{
			collider = GameObject.AddComponent<BoxCollider>();
			collider.IsTrigger = true;
			Log.Warning( $"SellTrigger '{GameObject.Name}' did not have a BoxCollider component. One has been added and configured as a trigger, but you may want to set up the collider yourself for better control over its size and position." );
		}
	}

	public void OnTriggerEnter( Collider other )
	{
		Log.Warning( "Sell Trigger" );

		var pickupItem = other.GameObject.Components.Get<PickupItem>( FindMode.InSelf | FindMode.InAncestors );
		if ( pickupItem is null )
			return;

		pickupItem.Drop();
		GameSystem.Instance.SellGib( pickupItem.GameObject );
		pickupItem.GameObject.Destroy();
	}
}

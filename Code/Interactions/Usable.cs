using System;

[Title( "Usable" )]
[Category( "Interactions" )]
[Icon( "back_hand" )]
public sealed class Usable : Component
{
	[Property, InputAction]
	public string Action { get; set; } = "use";

	[Property]
	public string HintText { get; set; } = "Use";

	public event Action<GameObject> OnInteract;
	public event Func<GameObject, bool> CanInteract;

	public void Interact( GameObject interactor )
	{
		OnInteract?.Invoke( interactor );
	}

	public bool CanInteractCheck( GameObject interactor )
	{
		if ( OnInteract is null )
			return false;
		return CanInteract?.Invoke( interactor ) ?? false;
	}


}


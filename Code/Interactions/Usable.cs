using System;

[Title( "Usable" )]
[Category( "Interactions" )]
[Icon( "back_hand" )]
public sealed class Usable : Component
{
	[Property, InputAction]
	public string Action;

	[Property]
	public bool ShowInteractionHint { get; set; } = false;

	[Property]
	public string HintText { get; set; } = "Use";

	public event Action<GameObject> OnInteract;

	/// <summary>
	/// Optional gate. If not set, the object is always interactable.
	/// </summary>
	public Func<GameObject, bool> CanInteract { get; set; }

	public bool CanInteractWith( GameObject interactor )
	{
		return CanInteract?.Invoke( interactor ) ?? true;
	}

	public void Interact( GameObject interactor )
	{
		OnInteract?.Invoke( interactor );
	}


}


using Sandbox;

public sealed class WashableShirt : Washable
{

	[Property]
	public Color ClothingColor { get; set; } = Color.White;


	protected override void OnFixedUpdate()
	{
		// Update prop tint when clothing color changes
		GameObject.GetComponent<Prop>()?.Tint = ClothingColor;
	}
}

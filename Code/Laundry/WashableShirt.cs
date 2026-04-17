using Sandbox;

public sealed class WashableShirt : Washable
{
	[Property, Sync, Change( nameof( OnColorChanged ) )]
	public Color ClothingColor { get; set; } = Color.White;

	protected override void OnStart()
	{
		base.OnStart();
		ApplyColor();
	}

	private void OnColorChanged( Color oldColor, Color newColor )
	{
		ApplyColor();
	}

	private void ApplyColor()
	{
		var prop = GameObject.GetComponent<Prop>();
		if ( prop is not null )
			prop.Tint = ClothingColor;
	}
}

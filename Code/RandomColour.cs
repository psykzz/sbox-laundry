using Sandbox;



public sealed class RandomColour : Component
{


	[Property]
	public List<Color> Colours { get; set; }

	protected override void OnStart()
	{
		var renderer = GameObject.GetComponent<ModelRenderer>();
		if ( renderer is not null )
		{
			renderer.Tint = System.Random.Shared.FromList( Colours, default );
		}
		else
		{
			Log.Warning( $"RandomColour component on '{GameObject.Name}' could not find a ModelRenderer to set a random material group on." );
		}
	}
	protected override void OnUpdate()
	{

	}
}

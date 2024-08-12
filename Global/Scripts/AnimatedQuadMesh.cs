using Godot;
using System;

[Tool]
[GlobalClass]
public partial class AnimatedQuadMesh : MeshInstance3D
{
	[Export]
	public Texture ShaderTexture
	{
		get => _ShaderTexture;
		set
		{
			if(sm == null)
			{
				sm = (ShaderMaterial)this.Mesh.SurfaceGetMaterial(0);
				
			}
			
			sm.SetShaderParameter(ShaderParameter, value);
			_ShaderTexture = value;
			//if(value == null)
			//	GD.Print("Texture value sent to AnimatedQuadMesh is null!");
		}
	}
	private Texture _ShaderTexture;
	
	[Export]
	public string ShaderParameter = "texture_albedo";
	
	ShaderMaterial sm;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		sm = (ShaderMaterial)this.Mesh.SurfaceGetMaterial(0);
	}

	
}

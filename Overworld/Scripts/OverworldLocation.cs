using Godot;
using System;

[Tool]
[GlobalClass]
public partial class OverworldLocation : RigidBody3D
{
	[Export]
	public string LocationScene;
	
	[Export]
	public Texture _Texture
	{
		get => MyTexture;
		set
		{
			//setting doesn't work while constructing node, but it still works in _Ready()
			try
			{
				SetAQMTextureTo(value);
			}
			catch
			{
				
			}
			MyTexture = value;
		}
	}
	private Texture MyTexture;
	
	private AnimatedQuadMesh aqm;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		SetAQMTextureTo(_Texture);
		
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if(!Engine.IsEditorHint())
		{
			
		}
	}
	
	private void SetAQMTextureTo(Texture t)
	{
		if(aqm == null)
				aqm = GetNode<AnimatedQuadMesh>("AnimatedQuadMesh");
			
		aqm.ShaderTexture = t;
	}
}

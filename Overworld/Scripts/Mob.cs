using Godot;
using System;

[GlobalClass]
public partial class Mob : RigidBody3D
{
	[Export]
	public string AnimationName;
	
	[Export]
	public EncounterData Data;
	
	[Export]
	public MobBehavior Behavior;
	
	public static readonly string BaseCombatScene = "res://Combat/Node Templates/BaseCombat.tscn";
	
	[Export]
	public string CustomCombatScene;
	
	
	
	public bool HasCollided = false;
	
	[ExportGroup("")]
	[Export]//DO NOT USE Exported so that the animationplayer can see it
	public bool FinishedPopUp = false;
	
	private static AnimationLibrary UniversalLib = ResourceLoader.Load<AnimationLibrary>("res://Overworld/MobAnimations/MobUniversalAnim.tres");
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		BodyEntered += OnCollision;
		
		AnimationPlayer player = GetNode<AnimationPlayer>("Rotation/AnimatedQuadMesh/AnimationPlayer");
		
		string filename = "res://Overworld/MobAnimations/" + AnimationName + "/" + AnimationName + ".tres";
		AnimationLibrary newLib = ResourceLoader.Load<AnimationLibrary>(filename);
		
		
		
		if(!player.HasAnimationLibrary("MobUniversalAnim"))
			player.AddAnimationLibrary("MobUniversalAnim", UniversalLib);
		
		if(!player.HasAnimationLibrary(AnimationName))
			player.AddAnimationLibrary(AnimationName, newLib);
		
		player.Play("MobUniversalAnim/PopUp");
		
		Texture popUpTexture = ResourceLoader.Load<Texture>("res://Overworld/MobAnimations/" + AnimationName + "/" + AnimationName + "PopUp.png");
		GetNode<AnimatedQuadMesh>("Rotation/AnimatedQuadMesh").ShaderTexture = popUpTexture;
		SceneTreeTimer t = GetTree().CreateTimer(1.01f);
		t.Timeout += delegate(){player.Play(AnimationName + "/Idle");};
		//Callable.From(;}).CallDeferred();
		
		TileMapDisplay3D display = (TileMapDisplay3D)GetTree().GetFirstNodeInGroup("OverworldTileMapDisplay");
		
		Behavior.Pathfinder = OverworldAStar.GetOverworldAStar(Behavior.MobilityFlags, display);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if(FinishedPopUp)
		{
			MoveAndCollide(Behavior.CalcCurrentMovement(this, delta));
		}
	}
	
	public virtual void OnCollision(Node other)
	{
		GD.Print("Mob collision called!");
		//ignore the collision if the other one responded first
		if(HasCollided)return;
		
		if(other is Mob)
		{
			Mob m = (Mob)other;
			
			if((m.Behavior.Allegiances & Behavior.Allegiances) == 0)
			{
				if(other is OverworldPlayer)
				{
					HasCollided = true;
					return;
				}
				
				m.HasCollided = true;
				m.QueueFree();
				this.QueueFree();
			}
			
		}
	}
}

using Godot;
using System;

public partial class LocationExit : Area3D
{
	public static readonly string DefaultExitScene = "res://Overworld/Overworld.tscn";
	
	[Export]
	public string ExitScene;
	
	
	string StoryVariableOnExit;//increments this variable in the story on collision
	
	[Export]
	public Vector3[] DropLocations;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		this.BodyShapeEntered += OnCollision;
		
		//TODO: should I implement a SceneFrom or something in THJGlobals
		//to distinguish between exits going to different places?
		//as long as the sets of exits drop off in very different places in 
		//their target scenes then they'll probably ping as the closest
		//reentry location anyway
		if(THJGlobals.DropLocation != Vector3.Zero)
		{
			CollisionShape3D closestShape = null;
			float leastDistance = 1000.0f;
			for(int i = 0; i < GetChildren().Count; i++)
			{
				float testDistance = DropLocations[i].DistanceTo(THJGlobals.DropLocation);
				
				if(testDistance < leastDistance)
				{
					closestShape = (CollisionShape3D)ShapeOwnerGetOwner(ShapeFindOwner(i));
					leastDistance = testDistance;
				}
				
			}
			
			if(closestShape == null)GD.Print("No shape in location exits closer than 1000 meters!");
			
			Node3D player = (Node3D)(THJGlobals.MainGame.GetTree().GetFirstNodeInGroup("Player"));
			//player.Position = closestShape.GlobalPosition;
		}
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
	
	
	private void OnCollision(Rid body_rid, Node3D body, long body_shape_index, long local_shape_index)
	//private void OnCollision(Node3D body)
	{
		GD.Print("Exit collision fired!");
		if(!(StoryVariableOnExit == null || StoryVariableOnExit.Equals("")))
			THJGlobals.Story.IncrementVariable(StoryVariableOnExit);
		
		THJGlobals.DropLocation = DropLocations[local_shape_index];
		THJGlobals.MainGame.SetSceneAsync(ExitScene == null ? DefaultExitScene : ExitScene);
		
		//Node newScene = DefaultExitScene.Instantiate();
		//Node distantParent = THJGlobals.MainScene.GetChild(0);
		//distantParent.QueueFree();
		//THJGlobals.MainScene.AddChild(newScene);
	}
}

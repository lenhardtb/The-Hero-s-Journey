using Godot;
using System;

[GlobalClass]
public partial class CameraMagnet : Node3D
{
	[Export]
	public bool ActivatesWithProximity = true;
	[Export]
	public float ProximityRange = 0.0f;
	[Export]
	public Positioning MagnetTarget = Positioning.OnMagnet;
	
	[Export]
	public bool Active = false;
	
	public enum Positioning
	{
		Between, OnMagnet
	}
	
	private static int MoveFrames = Engine.MaxFps / 2;
	private static float lerpWeight = 1.0f / (float)MoveFrames;
	//private int currFrame = 0;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		this.AddToGroup("CameraMagnet");
		
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	
	public override void _Process(double delta)
	{
		Camera3D camera = GetViewport().GetCamera3D();
		Vector3 targetPos = CalcTargetPosition(camera);
		Vector3 inactivePos = ((Node3D)camera.GetParent()).GlobalPosition;
		Vector3 currPos = camera.GlobalPosition;
		float totalDist = inactivePos.DistanceTo(targetPos);
		float currDist = currPos.DistanceTo(targetPos);
		
		
		if(ActivatesWithProximity)
		{
			Active = totalDist < ProximityRange;
		}
		
		//check if another magnet is responsible for moving it
		//TODO: make this more efficient by caching the magnets
		Godot.Collections.Array<Node> magnets = GetTree().GetNodesInGroup("CameraMagnet");
		
		foreach(Node n in magnets)
		{
			if(n == this)continue;
			CameraMagnet cm = (CameraMagnet)n;
			
			//somebody else is pulling it, leave it alone
			if(cm.Active && !Active)return;
			
			//if both are active or both are inactive, we are competing
			//figure out which is closer!
			if(cm.Active == Active)
			{
				float otherDist = inactivePos.DistanceTo(cm.CalcTargetPosition(camera));
				
				//other competing magnet is closer
				if(otherDist < currDist) 
				{
					//GD.Print("Closer matching magnet found!");
					return;
				}
			}
		}
		
		//if we made it to this point, this magnet is responsible
		if(Active)
		{
			camera.TopLevel = true;
			if(currDist > 0.01f)
			{
				camera.GlobalPosition = currPos.Lerp(targetPos, lerpWeight);
				
				Vector3 newPos = camera.GlobalPosition;
				newPos.Z = inactivePos.Z + 1.01f;
				camera.GlobalPosition = newPos;
				
				
			}
			else if(currDist > 0.0f) camera.GlobalPosition = targetPos;
		}
		else
		{
			camera.TopLevel = false;
			if(currPos.DistanceTo(inactivePos) > 0.01f)
			{
				camera.GlobalPosition = currPos.Lerp(inactivePos, lerpWeight);
				
				Vector3 newPos = camera.GlobalPosition;
				newPos.Z = inactivePos.Z + 1.01f;
				camera.GlobalPosition = newPos;
			}
			else if((totalDist - currDist) > 0.0f)
			{
				camera.GlobalPosition = inactivePos;
				
			}
		}
		
	}
	
	private Vector3 CalcTargetPosition(Camera3D camera)
	{
		switch(MagnetTarget)
		{
			case Positioning.Between:
				Vector3 inactivePos = ((Node3D)camera.GetParent()).GlobalPosition;
				return this.GlobalPosition.Lerp(inactivePos, 0.5f);
			default:
				return this.GlobalPosition;
		}
	}
}

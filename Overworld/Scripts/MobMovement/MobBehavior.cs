using Godot;
using System;
using System.Collections.Generic;

[GlobalClass]
public abstract partial class MobBehavior : Resource
{
	[Export(PropertyHint.Flags, "Hero,King,Brigands,Slimes")]
	public int Allegiances = 0;
	
	public static PlayerBehavior Player;
	public static LinkedList<MobBehavior> Mobs = new LinkedList<MobBehavior>();
	
	public AStar2D Pathfinder;
	
	//if match is true, FiltersMobs returns behaviors who alignment contains any of the flags
	//if match is false, FilterMobs returns Behaviors NOT containing any of the flags
	public static LinkedList<Mob> FilterMobs(int allegiances, bool match = true)
	{
		Godot.Collections.Array<Node> Mobs = THJGlobals.MainGame.GetTree().GetNodesInGroup("Mob");
		
		LinkedList<Mob> returner = new LinkedList<Mob>();
		
		//LinkedListNode<MobBehavior> currentNode = Mobs.First;
		for(int i = 0; i < Mobs.Count; i++)
		{
			Mob m = (Mob)Mobs[i];
			//XOR operation - match vs do any flags match
			if(match == ((m.Behavior.Allegiances & allegiances) != 0))
			{
				returner.AddLast(m);
			}
			//currentNode = currentNode.Next;
		}
		
		return returner;
	}
	
	//used in PhysicsProcess(). Should return a velocity, to be used in methods like MoveAndCollide()
	public abstract Vector3 CalcCurrentMovement(Mob m, double delta);
	
}

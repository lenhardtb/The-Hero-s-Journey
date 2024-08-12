using Godot;
using System;
using System.Collections.Generic;
/*
RoamBehavior mobs move in the following ways:
-Move towards enemies and fight them, according to threat.
-When too far out of range or no enemies, move back to a random point around its center.
-Stay in that random spot for a small amount of time, then pick a new point.
*/
[GlobalClass]
public partial class RoamBehavior : MobBehavior
{
	//will pick random points in a range of this point
	[Export]
	public Vector3 Center;
	[Export]
	public float RoamRange;
	
	private Vector3 target;
	private ulong waitTime;
	
	private const float RoamSpeed = 2.4F;
	private const float ChaseSpeed = 3.2F;
	private const float ChaseDistance = 3.0F;
	private const ulong minWait = 1000ul;
	private const int waitRange = 500;
	
	private static Random r;
	
	public RoamBehavior() : base()
	{
		//Mobs.AddLast(this);
		if(r == null)
			r = new Random(THJGlobals.RandomSeed);
		waitTime = 0ul;
		Callable.From(delegate(){target = Center;}).CallDeferred();
		//GD.Print("Center is: " + Center);
	}
	
	public override Vector3 CalcCurrentMovement(Mob m, double delta)
	{
		Vector3 position = m.Transform.Origin;
		
		//GD.Print("Roam: Position: " + position + " Target: " + target + " Move: " + position.MoveToward(target, RoamSpeed * (float)delta));
		LinkedList<Mob> enemyMobs = FilterMobs(Allegiances, false);
		float closestDistance = ChaseDistance;
		Mob closestEnemy = null;
		
		LinkedListNode<Mob> currentNode = enemyMobs.First;
		while(currentNode != null)
		{
			float distance = currentNode.Value.Transform.Origin.DistanceTo(position);
			if(distance < ChaseDistance && distance < closestDistance && currentNode.Value.Behavior != this)
			{
				closestDistance = distance;
				closestEnemy = currentNode.Value;
			}
			currentNode = currentNode.Next;
		}
		
		if(closestEnemy != null)
		{
			return position.DirectionTo(closestEnemy.Transform.Origin) * ChaseSpeed * (float)delta;
		}
		
		//chilling at point
		if(position.DistanceTo(target) < 0.1f)
		{
			if(Time.GetTicksMsec() < waitTime)
			{
				//GD.Print("Roam: Waiting at point!");
				return new Vector3();
			}
			else if(waitTime == 0ul)//at target, just got there
			{
				//GD.Print("Roam: Arrived at point!");
				waitTime = Time.GetTicksMsec() + (ulong)r.Next(waitRange) + minWait;
				return new Vector3();
			}
			else//pick new chill point, move there
			{
				//GD.Print("Roam: Picked new point!");
				Vector3 newTarget = CalcRandomPoint();
				//TODO: make sure new target is sufficiently far away
				target = newTarget;
				waitTime = 0ul;
			}
		}
		//not at point yet
		else// if(position != target)
		{
			//GD.Print("Roam: Moving to point!");
			
			return position.DirectionTo(target) * RoamSpeed * (float)delta;
			
		}
		
		/*TileData roadTd = BackgroundTileMap.GetCellTileDataGlobalPos(this.Transform.Origin, 1);
			if(roadTd != null)
			{
				if((bool)roadTd.GetCustomData("IsRoad"))
				{
					MoveAndCollide(direction * 1.3f);
				}
			}
		*/
		
		return position.DirectionTo(target) * RoamSpeed * (float)delta;
	}
	
	private static Vector3 ZAxis = new Vector3(0.0f,0.0f,1.0f);
	private Vector3 CalcRandomPoint()
	{
		double radians = r.NextDouble() * Math.PI;
		double distance = r.NextDouble() * RoamRange;
		
		Vector3 returner = new Vector3(0.0f,1.0f,0.0f);
		returner = returner.Rotated(ZAxis, (float)radians);
		returner = returner * (float)distance;
		returner = returner + Center;
		
		GD.Print("Target point calculated! " + returner);
		
		return returner;
	}
}

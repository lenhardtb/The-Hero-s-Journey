using Godot;
using System;

//allows the player to interact more nicely with mobs
[GlobalClass]
public partial class PlayerBehavior : MobBehavior
{
	[Export]
	public float MovementSpeed;
	
	
	
	public PlayerBehavior() : base()
	{
		MobBehavior.Player = this;
	}
	
	public override Vector3 CalcCurrentMovement(Mob m, double delta)
	{
		Vector3 direction = new Vector3(0, 0, 0);
		direction.X = Input.GetActionStrength("ui_right") - Input.GetActionStrength("ui_left");
		direction.Y = Input.GetActionStrength("ui_up") - Input.GetActionStrength("ui_down");
		
		direction = direction.Normalized() * MovementSpeed * (float)delta;
		
		if(Pathfinder != null)
		{
			Vector3 testDirection = m.Position + direction / (float)delta;
			long tileId = Pathfinder.GetClosestPoint(new Vector2(testDirection.X, testDirection.Y));
			Vector2 closestValidTile = Pathfinder.GetPointPosition(tileId);
			
		}
		
		/*
		TileData roadTd = BackgroundTileMap.GetCellTileDataGlobalPos(m.Transform.Origin, 1);
		if(roadTd != null)
		{
			if((bool)roadTd.GetCustomData("IsRoad"))
			{
				MoveAndCollide(direction * 1.3f);
			}
		}
		*/
		
		return direction;
	}
	
	
}

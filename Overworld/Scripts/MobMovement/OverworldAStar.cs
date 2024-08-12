using Godot;
using System;
using System.Collections.Generic;

//do I need to make this a globalclass?
public partial class OverworldAStar : AStar2D
{
	//there are only so many distinct pathfinding settings out there
	private static TileMapDisplay3D currentTiles;
	
	//the int are some mobility flags
	private static Dictionary<int, OverworldAStar> cachedPathfinders = 
		new Dictionary<int, OverworldAStar>();
	
	
	private TileMapLayer tiles;
	private TileMapDisplay3D mapDisplay;
	
	
	private OverworldAStar(int k, TileMapDisplay3D display)
	{
		mapDisplay = display;
		tiles = display.tm[0];
		
		//TODO: generate all the points and stuff
		//some values outside the loop for efficiency
		
		foreach(Vector2I cell in tiles.GetUsedCells())
		{
			float weight = getTileWeight(tiles, cell, k);
			if(weight != -1.0f)
			{
				AddPoint(GetAvailablePointId(), cell, weight);
			}
		}
		
		foreach(long index in GetPointIds())
		{
			Vector2 starCoord = GetPointPosition(index);
			Vector2I tileCoord = new Vector2I((int)starCoord.X, (int)starCoord.Y);
			foreach(Vector2I neighbor in tiles.GetSurroundingCells(tileCoord))
			{
				
				long neighborId = GetClosestPoint(neighbor);
				if(GetPointPosition(neighborId).DistanceTo(neighbor) < 0.01f)
					ConnectPoints(index, neighborId);
			}
		}
	}
	
	//returns -1 if invalid
	private float getTileWeight(TileMapLayer t, Vector2I coord, int k, int layer = 0)
	{
		float weight = 1.0f;
		bool isValid = true;
		
		TileData td = t.GetCellTileData(coord);
		
		if(td == null)return -1.0f;
		
		if((bool)td.GetCustomData("IsWater"))
		{
			//can swim
			if((k & 2) != 0)
				isValid = false;
		}
		else 
		{
			if((k & 1) != 0)//can walk
				isValid = false;
		}
		
		if((bool)td.GetCustomData("IsRoad") && ((k & 4) != 0))//prefers roads
			weight *= 1.3f;
		
		if((bool)td.GetCustomData("IsRiver"))
			if((k & 2) != 0)//can swim
				weight *= 1.3f;
			else
				weight *= 0.8f;
		
		
		if(!isValid)
			return -1.0f;
		else
			return weight;
	}
	
	
	public Vector3 GetPointGlobalPosition(int id)
	{
		Vector2 floatPos = this.GetPointPosition(id);
		return mapDisplay.GetGlobalPosCoord(new Vector2I((int)floatPos.X, (int)floatPos.Y));
	}
	
	public static OverworldAStar GetOverworldAStar(int mobilityFlags, TileMapDisplay3D display)
	{
		if(currentTiles != display)
		{
			cachedPathfinders.Clear();
			currentTiles = display;
		}
		
		if(cachedPathfinders.ContainsKey(mobilityFlags))
			return cachedPathfinders[mobilityFlags];
		else
		{
			OverworldAStar newValue = new OverworldAStar(mobilityFlags, display);
			cachedPathfinders.Add(mobilityFlags, newValue);
			return newValue;
		}
		
	}
	//due to how mobs move, they might try to target the same point many times across frames
	//caching a single point id will increase efficiency in the typical case
	//TODO: am I able to override the relevant methods to do caching?
	//private int cachedNextTarget;
	//private int cachedNextValue;
	
	
	
	
	
}

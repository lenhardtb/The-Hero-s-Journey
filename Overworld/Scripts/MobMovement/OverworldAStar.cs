using Godot;
using System;
using System.Collections.Generic;

//do I need to make this a globalclass?
public partial class OverworldAStar : AStar2D
{
	//there are only so many distinct pathfinding settings out there
	private static TileMapDisplay3D currentTiles;
	
	private static Dictionary<AlgorithmKey, OverworldAStar> cachedPathfinders = 
		new Dictionary<AlgorithmKey, OverworldAStar>();
	
	
	private TileMap tiles;
	private TileMapDisplay3D mapDisplay;
	
	
	private OverworldAStar(AlgorithmKey k, TileMapDisplay3D display)
	{
		mapDisplay = display;
		tiles = display.tm;
		
		//TODO: generate all the points and stuff
		//some values outside the loop for efficiency
		int numLayers = tiles.GetLayersCount();
		
		foreach(Vector2I cell in tiles.GetUsedCells(0))
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
	private float getTileWeight(TileMap t, Vector2I coord, AlgorithmKey k)
	{
		float weight = 1.0f;
		bool isValid = true;
		for(int i = 0; i < t.GetLayersCount(); i++)
		{
			TileData td = t.GetCellTileData(i, coord);
			
			if(td == null)continue;
			
			if((bool)td.GetCustomData("IsWater"))
			{
				if(!k.CanSwim)
					isValid = false;
			}
			else 
			{
				if(!k.CanWalk)
					isValid = false;
			}
			
			if((bool)td.GetCustomData("IsRoad") && k.PrefersRoads)
				weight *= 1.3f;
			
			if((bool)td.GetCustomData("IsRiver"))
				if(k.CanSwim)
					weight *= 1.3f;
				else
					weight *= 0.8f;
		}
		
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
	
	public static OverworldAStar GetOverworldAStar(bool prefersRoads, bool canSwim, bool canWalk, TileMapDisplay3D display)
	{
		if(currentTiles != display)
		{
			cachedPathfinders.Clear();
			currentTiles = display;
		}
		AlgorithmKey k = new AlgorithmKey(prefersRoads, canSwim, canWalk);
		
		if(cachedPathfinders.ContainsKey(k))
			return cachedPathfinders[k];
		else
		{
			OverworldAStar newValue = new OverworldAStar(k, display);
			cachedPathfinders.Add(k, newValue);
			return newValue;
		}
		
	}
	//due to how mobs move, they might try to target the same point many times across frames
	//caching a single point id will increase efficiency in the typical case
	//TODO: am I able to override the relevant methods to do caching?
	//private int cachedNextTarget;
	//private int cachedNextValue;
	
	
	private struct AlgorithmKey
	{
		
		public bool PrefersRoads, CanSwim, CanWalk;
		
		public AlgorithmKey(bool prefersRoads, bool canSwim, bool canWalk)
		{
			PrefersRoads = prefersRoads;
			CanSwim = canSwim;
			CanWalk = canWalk;
		}
	}
	
	
}

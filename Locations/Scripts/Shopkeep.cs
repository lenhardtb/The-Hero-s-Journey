using Godot;
using System;
using GodotInk;
using Godot.Collections;
//using Godot.Collections.Generic;
using Array = Godot.Collections.Array;

public partial class Shopkeep : StaticBody3D
{
	[Export]
	public Json StoreData;
	
	[Export]
	public string SaveDataName;
	
	[Export]
	public InkStory Story;
	
	private static Json ShopData = GD.Load<Json>("res://Global/Databases/shopkeepDatabase.json");
	/*
	NOTES ON SAVE DATA
	one piece in the dictionary for 
	[name]_lastDay -> last day the inventory was updated (presumably last visit by player)
	[name]_inventory -> itself a dictionary, with names and amounts
	[name]_ratios -> itself a dictionary, with names and amount/weight/cost mod/time mod
	[name]_currentBuilt
	[name]_currentBuiltTime
	*/
	
	private bool initialized = false;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		base._Ready();
		
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if(!initialized)
		{
			Dictionary<string, float[]> baseRatios = generateInventory(StoreData);
			
			Dictionary<string, Variant> data = THJGlobals.SaveData;
			if(!data.ContainsKey(SaveDataName))
			{
				data[SaveDataName] = "Original Created";
				data[SaveDataName + "_lastDay"] = THJGlobals.InGameTime;
				data[SaveDataName + "_ratios"] = new Dictionary<string, float[]>(baseRatios);
				
				Dictionary<string, int> newInv = new Dictionary<string, int>();
				foreach(var(rkey, rvalue) in baseRatios)
				{
					newInv.Add(rkey, (int)((float[])rvalue)[1]);
				}
				data[SaveDataName + "_inventory"] = newInv;
			}
			else
			{
				AdvanceDays();
			}
			initialized = true;
		}
	}
	
	private void InteractableHandler()
	{
		GD.Print("Shopkeep handler called!");
		ShopUI sui = (ShopUI)(GetTree().GetFirstNodeInGroup("ShopUI"));
		
		TweeningUIHolder holder = (TweeningUIHolder)(GetTree().GetFirstNodeInGroup("TweeningUIHolder"));
		
		Node player = GetTree().GetFirstNodeInGroup("Player");
		((LocationPlayer)player).SetMode(THJGlobals.PlayerMode.Interacting);
		
		sui.OnInteraction(Story, SaveDataName, GetNode<CameraMagnet>("CameraMagnet"));
		holder.TweenIn((TweeningUI)(sui.GetParent()), true);
		GD.Print("Shopkeep node called TweenIn!");
	}
	
	private Dictionary<string, float[]> generateInventory(Json j)
	{
		Dictionary<string, float[]> returner = new Dictionary<string, float[]>();
		
		Dictionary<string, Variant> nodeData = 
			new Dictionary<string, Variant>(
				(Dictionary)j.Data);
		
		Array arr = (Array)nodeData[SaveDataName];
		
		//name, amount, weight, cost modifier, time modifier
		foreach(Variant v in arr)
		{
			Array a = (Array)v;
			
			float[] itemData = new float[a.Count - 1];
			for(int i = 1; i < a.Count; i++)
			{
				itemData[i-1] = (float)a[i];
			}
			returner.Add((string)a[0], itemData);
		}
		
		return returner;
	}
	
	private void AdvanceDays()
	{
		Dictionary<string, float[]> ratios = (Dictionary<string, float[]>)THJGlobals.SaveData[SaveDataName + "_ratios"];
		Dictionary<string, int> inventory = (Dictionary<string, int>)THJGlobals.SaveData[SaveDataName + "_inventory"];
		//TODO: figure out how to weight for items with a max of 1 - they shouldn't always be the highest weighted!!
		double timeLeft = THJGlobals.InGameTime - (double)THJGlobals.SaveData[SaveDataName + "_lastDay"];
		
		//this could hypothetically be written as a for loop, but the 
		//calculation for "i--" is complicated and has to be done at the end
		while(timeLeft > 0.0)
		{
			float highestWeight = 0.0f;
			string highestKey = "";
		
			foreach(var(rkey, rvalue) in ratios)
			{
				float[] rvalues = (float[])rvalue;//casting here to reduce repeated casts
				float baseAm = rvalues[1];
				int currAm = inventory[rkey];
				float weight = (baseAm-currAm)/currAm * rvalues[2];
				if(weight > highestWeight)
				{
					highestWeight = weight;
					highestKey = rkey;
				}
			}
			
			//TODO: grab item time from database
			//TODO: multiply by store weight
			//if less than time left: subtract from time, add 1 to inventory
			//if not: set as currently producing item, add to store save data
			
			timeLeft -= 1.0;
		}
		
		THJGlobals.SaveData[SaveDataName + "_lastDay"] = THJGlobals.InGameTime;
	}
}

using Godot;
using System;
using System.Collections.Generic;

public class CombatTrigger : Dictionary<string, CombatAction>
{
	/*
	The key string should be a signal code for an event that is to be triggered. 
	The value string should be an action on the stack that will be placed in response.
	
	The Action in question's target should be custom tailored for this purpose
	*/
	
	[Export]
	public string Description;
	
	[Export]
	public Texture Icon;
	
	/*
	public List<CombatAction> this[string triggerEvent]
	{
		get
		{
			List<CombatAction> returner = new List<CombatAction>(1);
		
			foreach(string key in this.Keys)
				if(key.BeginsWith(triggerEvent) || triggerEvent.BeginsWith(key))
				{
					returner.Add(base.this[key]);
				}
			return returner;
		}
		
		set
		{
			if(this.KeyExists(value))
				base.this[triggerEvent] = value;
			else
				Add(triggerEvent, value);
		}
	}
	*/
	
	public static CombatTrigger ParseJson(string s)
	{
		Json j = new Json();
		Error e = j.Parse(s);
		
		Godot.Collections.Dictionary<string, Variant> dic = 
			new Godot.Collections.Dictionary<string, Variant>(
				(Godot.Collections.Dictionary)j.Data);
		
		return CombatTrigger.ParseJson(dic);
	}
	public static CombatTrigger ParseJson(Godot.Collections.Dictionary<string, Variant> dic)
	{
		CombatTrigger returner = new CombatTrigger();
		
		returner.Description = (string)dic["Description"];
		
		if(ResourceLoader.Exists((string)dic["Icon"]))
			returner.Icon = ResourceLoader.Load<Texture>((string)dic["Icon"]);
		else
			GD.Print("Icon not found! " + (string)dic["Icon"]);
		
		string[] ignoreKeys = new string[]{"Description", "Icon"};
		foreach(string key in dic.Keys)
		{
			bool isIgnoredKey = false;
			foreach(string ignore in ignoreKeys)
				if(ignore.Equals(key))
					isIgnoredKey = true;
			
			if(!isIgnoredKey)
				returner.Add((string)dic[key], CombatAction.LookupResource((string)dic[key]));
		}
		
		return returner;
	}
	
	
}

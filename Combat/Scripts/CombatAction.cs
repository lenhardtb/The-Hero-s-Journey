using Godot;
using System;
using System.Collections.Generic;

public partial class CombatAction : Resource
{
	//values that will have to be determined at runtime
	//a single postion that is being aimed for; many things will affect AoE
	public CombatUnit.Position TargetedPosition;
	//unit that is making this action
	public CombatUnit Source;

	public String Name;

	//if this action targeted the location in the middle-front, what spaces would be targeted overall?
	public CombatUnit.Position RelativePosition;

	//how many percentage points of threat does this attack ignore?
	public int IgnoreThreat;
	
	public float DamageValue = 1.0f;//base damage multiplier. For basic attacks, this value is 1.0

	public void Execute(List<CombatUnit> units)
	{
		//find all hit units - note that threat will likely cause more to be damaged!
		List<CombatUnit> hitUnits = new List<CombatUnit>();
		foreach (CombatUnit u in units)
		{
			//TODO: formula for relative position mattering
			if ((u.UnitPosition & TargetedPosition) != CombatUnit.Position.NONE)//ally_none and enemy_none are equivalent, and this is placeholder anyway
				hitUnits.Add(u);
		}

		//TODO: threat calculation and distribution
		foreach (CombatUnit u in hitUnits)
		{
			u.CurrentHP -= (int)Math.Round((Source.Attack * DamageValue) - u.Defense);
			int dealtDamage = (int)Math.Round((Source.Attack * DamageValue) - u.Defense);
			
			if(dealtDamage < 0)
			{
				GD.Print("Negative Damage Dealt: " + dealtDamage);
				GD.Print("Formula : (" + Source.Attack + " * " + DamageValue + ") - " + u.Defense);
			}
		}
	}



	/*
	 * Generates a basic CombatAction that simply deals damage to the targeted enemy based on the user's attack.
	 */
	public static CombatAction BasicAttack(CombatUnit source = null)
	{
		CombatAction returner = new CombatAction();
		returner.Source = source;
		returner.DamageValue = 1.0f;
		returner.Name = "Attack";
		returner.IgnoreThreat = 0;
		return returner;
	}


	private static bool databaseLoaded = false;
	private static Dictionary<string, CombatAction> databaseActions;
	/*
	 * Looks in the expected resource for THJ to pull it from the database
	 * Generates a CombatAction object from the information. 
	 * Returns a basic attack if not found.
	 */
	public static CombatAction LookupResource(string name, CombatUnit source = null)
	{
		if (!databaseLoaded)
		{
			try
			{
				Json j = GD.Load<Json>("res://Global/Databases/actionDatabase.json");
				Godot.Collections.Dictionary<string, Variant> nodeData = 
				new Godot.Collections.Dictionary<string, Variant>(
					(Godot.Collections.Dictionary)j.Data);
				
				databaseActions = new Dictionary<string, CombatAction>();
				
				foreach(string key in nodeData.Keys)
				{
					Variant data = nodeData[key];
					CombatAction tempAction;
					
					try
					{
						tempAction = CombatAction.ParseJson(data.ToString());
						databaseActions.Add(key, tempAction);
						//GD.Print("Added action to database: " + key + ": " + tempAction.ToString());
					}
					catch
					{
						GD.Print("Error while parsing action with key \"" + key + "\"");
					}
				}
				
				databaseLoaded = true;
			}
			catch(Exception ex)
			{
				GD.Print(ex.Message);
				GD.Print(ex.StackTrace);
			}
		}//end block for loading database for the first time
		
		if(name.Equals("Basic Attack"))return BasicAttack(source);
		
		CombatAction desc;
		bool nameFound = databaseActions.TryGetValue(name, out desc);
		
		if(nameFound)
		{
			CombatAction returner = (CombatAction)desc.MemberwiseClone();
			returner.Source = source;
			return returner;
		}
		else//action was not found in database
		{
			GD.Print("Could not find action \"" + name + "\" in database!");
			return null;
		}
	}

	public static CombatAction ParseJson(string s)
	{
		CombatAction returner = new CombatAction();
		Json j = new Json();
		Error e = j.Parse(s);
		
		Godot.Collections.Dictionary<string, Variant> dic = 
			new Godot.Collections.Dictionary<string, Variant>(
				(Godot.Collections.Dictionary)j.Data);
		
		returner.Name = (string)dic["Name"];
		returner.DamageValue = (float)dic["DamageValue"];
		returner.RelativePosition = (CombatUnit.Position)((int)dic["Position"]);
		returner.IgnoreThreat = (int)dic["IgnoreThreat"];
		
		return returner;
	}
}

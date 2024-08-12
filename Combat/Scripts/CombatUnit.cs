using Godot;
using System;
using Godot.Collections;

public partial class CombatUnit : Resource
{
	//TODO: use level system to flatten database and automate scaling
	
	//number of milliseconds the GUI will take to shift to an updated hp value. Should probably be one round, but this way I can mess with it.
	private const int HP_DISPLAY_TARGET = CombatEngine.ROUND_TIME;
	
	
	//position comparison constants
	//These will be used in bitwise operations to create/check composite positions
	//1-4 is the front row, starting at the top
	//ally always refers to the player. CombatManager and other classes will use SwitchSide for actions that enemies take.
	[Flags]
	public enum Position
	{
		NONE        = 0b_0000_0000_0000_0000,
		ALLY_ALL    = 0b_0000_0000_1111_1111,
		ALLY_ONE    = 0b_0000_0000_0000_0001,
		ALLY_TWO    = 0b_0000_0000_0000_0010,
		ALLY_THREE  = 0b_0000_0000_0000_0100,
		ALLY_FOUR   = 0b_0000_0000_0000_1000,
		ALLY_FIVE   = 0b_0000_0000_0001_0000,
		ALLY_SIX    = 0b_0000_0000_0010_0000,
		ALLY_SEVEN  = 0b_0000_0000_0100_0000,
		ALLY_EIGHT  = 0b_0000_0000_1000_0000,
		ENEMY_ALL   = 0b_1111_1111_0000_0000,
		ENEMY_ONE   = 0b_0000_0001_0000_0000,
		ENEMY_TWO   = 0b_0000_0010_0000_0000,
		ENEMY_THREE = 0b_0000_0100_0000_0000,
		ENEMY_FOUR  = 0b_0000_1000_0000_0000,
		ENEMY_FIVE  = 0b_0001_0000_0000_0000,
		ENEMY_SIX   = 0b_0010_0000_0000_0000,
		ENEMY_SEVEN = 0b_0100_0000_0000_0000,
		ENEMY_EIGHT = 0b_1000_0000_0000_0000
	}
	
	
	/*
	 * Primarily used to match with UI elements.
	 * Note: this function does not distinguish the unit side! ALLY_ONE and ENEMY_ONE will both return 1
	 */
	public static int[] PosToIndex(Position position)
	{
		System.Collections.Generic.List<int> hits = new System.Collections.Generic.List<int>();

		if((position & Position.ENEMY_ALL) != Position.NONE) position = SwitchSide(position);

		int val = 1;
		for(int i = 0; i < 8; i++)
		{
			if ((position & (Position)val) != Position.NONE)
			{
				hits.Add(i);
			}
			
			val = val * 2;
		}

		return hits.ToArray();
	}

	public static Position SwitchSide(Position position)
	{
		
		//TODO: make efficient version
		//more understandable version:
		int allySide = (int)(position & Position.ALLY_ALL);
		int enemySide = (int)(position & Position.ENEMY_ALL);
		int returner = allySide << 8;
		returner = returner | (enemySide >> 8);
		return (Position)returner;
		
	}
	public string Filename;
	public string Name;

	public Position UnitPosition { get; set; }

	public bool IsAlly { get { return (UnitPosition & Position.ALLY_ALL) != Position.NONE; } }
	public bool IsEnemy { get { return (UnitPosition & Position.ENEMY_ALL) != Position.NONE; } }

	public CombatTrigger Prologue;
	public CombatTrigger RisingAction;
	public CombatTrigger Climax;
	public CombatTrigger FallingAction;
	public CombatTrigger Epilogue;

	
	//default position that has been set by the user; the computer will pick a spot 
	public Position TargetPosition { get; set; }
	
	
	//TODO: setting stats
	private int maxHP = 100;
	public int MaxHP 
	{ 
		get 
		{ 
			return maxHP; 
		}
		set
		{
			int prevMax = maxHP;
			maxHP = value;
			CurrentHP = CurrentHP + (value - prevMax);
		} 
	}

	/*the GUI will show HP slowly draining when units take damage or heal; 
	CombatUnit will handle the calculations to determine what the HP should display as 
	at any given moment while separately maintaining its actual hp.

	when the actual hp changes, the display should start draining from whatever it was displaying as at that time
	*/
	private double fromDisplayTime;
	private double toDisplayTime;
	private int fromDisplayHP;
	private int currentDisplayHP;
	private int currentHP;
	public int CurrentHP { 
		get { return currentHP; } 
		set 
		{
			//display starts scrolling from wherever it was last displayed when hp was changed.
			int displayedHP = CurrentDisplayHP;
			currentHP = Math.Min(maxHP, Math.Max(value, 0));//don't allow setting value below 0 or above max
			fromDisplayTime = Time.GetUnixTimeFromSystem();
			toDisplayTime = fromDisplayTime + HP_DISPLAY_TARGET;
			fromDisplayHP = displayedHP;
			
		} 
	}
	
	public int CurrentDisplayHP 
	{ 
		get 
		{
			if (Time.GetUnixTimeFromSystem() < toDisplayTime)
			{
				return CalcCurrentDisplayHP();
			}//common, much faster calculations
			else if (Time.GetUnixTimeFromSystem() == toDisplayTime)
			{
				return fromDisplayHP;
			}
			else
				return currentHP;
		} 
	}

	private int CalcCurrentDisplayHP()
	{
		//x / hp to elapse to = time elapsed / time to elapse, solve for x
		return (int)(((Time.GetUnixTimeFromSystem() - fromDisplayTime) * (currentHP - fromDisplayHP) / HP_DISPLAY_TARGET) + fromDisplayHP);
	}
	
	public int Attack;
	public int Defense;
	
	//actual objects
	public CombatAction BasicAttack;
	public CombatAction SpecialAttack;
	
	
	public CombatUnit(CombatUnitDescription desc) : this()
	{
		Filename = desc.Filename;
		Name = desc.Name;
		Attack = desc.Attack;
		Defense = desc.Defense;
		MaxHP = desc.MaxHP;

		BasicAttack = CombatAction.LookupResource(desc.BasicAttackName, this);
		SpecialAttack = CombatAction.LookupResource(desc.SpecialAttackName, this);

	}

	public CombatUnit()
	{
		//TODO: set values so that HP bar fills at start of combat. Maybe do that in CombatManager?
		currentHP = maxHP;
		TargetPosition = Position.NONE;
		toDisplayTime = Time.GetUnixTimeFromSystem();
		fromDisplayTime = Time.GetUnixTimeFromSystem();
		
		
		//apply most strategy upgrades at this time
		//maybe make them a class with a MonoBehaviour-like system for applying to things, which will most of the time do nothing?
		//maybe use a system of delegates?
	}


	//TODO: use database cache
	private static bool databaseLoaded = false;
	private static Dictionary<string, CombatUnitDescription> databaseUnits;
	/*
	 * Looks in the expected resource for THJ to pull it from the database
	 * Generates a CombatAction object from the information. 
	 * Returns null if not found.
	 */
	public static CombatUnit LookupResource(string name, Position position = Position.NONE)
	{
		if (!databaseLoaded)
		{
			try
			{
				Json j = GD.Load<Json>("res://Global/Databases/unitDatabase.json");
				Godot.Collections.Dictionary<string, Variant> nodeData = 
				new Godot.Collections.Dictionary<string, Variant>(
					(Godot.Collections.Dictionary)j.Data);
				
				databaseUnits = new Dictionary<string, CombatUnitDescription>();
				
				foreach(string key in nodeData.Keys)
				{
					Variant data = nodeData[key];
					CombatUnitDescription tempDesc;
					
					try
					{
						tempDesc = CombatUnitDescription.ParseJson(data.ToString());
						tempDesc.Filename = key;
						databaseUnits.Add(key, tempDesc);
						//GD.Print("Added unit to database: " + key + ": " + tempDesc.ToString());
					}
					catch
					{
						GD.Print("Error while parsing unit with key \"" + key + "\"");
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
		
		CombatUnitDescription desc;
		bool nameFound = databaseUnits.TryGetValue(name, out desc);
		
		if(nameFound)
		{
			CombatUnit returner = new CombatUnit(desc);
			returner.UnitPosition = position;
			return returner;
		}
		else//unit was not found in database
		{
			GD.Print("Could not find unit \"" + name + "\" in database!");
			return null;
		}
	}

	[Serializable]
	private partial class CombatUnitDatabaseWrapper : Resource
	{
		public CombatUnitDescription[] Units;
	}

	[Serializable]
	public partial class CombatUnitDescription : Resource
	{
		public string Filename;
		public string Name;
		public int Attack;
		public int Defense;
		public int MaxHP;
		public string BasicAttackName;
		public string SpecialAttackName;
		
		
		public static CombatUnitDescription ParseJson(string s)
		{
			CombatUnitDescription returner = new CombatUnitDescription();
			Json j = new Json();
			Error e = j.Parse(s);
			
			Godot.Collections.Dictionary<string, Variant> dic = 
				new Godot.Collections.Dictionary<string, Variant>(
					(Godot.Collections.Dictionary)j.Data);
			
			returner.Name = (string)dic["Name"];
			returner.Attack = (int)dic["Attack"];
			returner.Defense = (int)dic["Defense"];
			returner.MaxHP = (int)dic["Defense"];
			returner.BasicAttackName = (string)dic["BasicAttackName"];
			returner.SpecialAttackName = (string)dic["SpecialAttackName"];
			return returner;
		}
	}
}

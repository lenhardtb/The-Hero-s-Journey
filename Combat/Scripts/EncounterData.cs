using System.Collections;
using System.Collections.Generic;
using Godot;

[GlobalClass]
public partial class EncounterData : Resource
{
	[Export]
	public string[] AllyNames;
	public List<CombatUnit> Allies
	{
		get
		{
			List<CombatUnit> returner = new List<CombatUnit>(AllyNames.Length);
			foreach(string s in AllyNames)
				returner.Add(CombatUnit.LookupResource(s, CombatUnit.Position.ALLY_ONE));
				
			//TODO: generate player units
			return returner;
		}
	}
	[Export]
	public string[] EnemyNames;
	public List<CombatUnit> Enemies
	{
		get
		{
			List<CombatUnit> returner = new List<CombatUnit>(EnemyNames.Length);
			foreach(string s in EnemyNames)
				returner.Add(CombatUnit.LookupResource(s, CombatUnit.Position.ENEMY_ONE));
				
			return returner;
		}
	}
	
	//combat should increment these variables upon win
	[Export]
	public string[] DefeatTags;
	
	//[Export]
	public Texture BackgroundImage
	{
		get => _BackgroundImage;
		set => _BackgroundImage = value;
	}
	private Texture _BackgroundImage;
	[Export]
	public string BackgroundPath
	{
		get => _BackgroundImage?.ResourcePath;
		set => _BackgroundImage = ResourceLoader.Load<Texture>(value);
	}
	
	
	//[Export]
	public PackedScene ReturnScene
	{
		get => ResourceLoader.Load<PackedScene>(_ReturnScene);
		set => _ReturnScene = value.ResourcePath;
	}
	private string _ReturnScene;
	[Export]
	public string ReturnPath
	{
		get => _ReturnScene;
		set => _ReturnScene = value;
	}
}

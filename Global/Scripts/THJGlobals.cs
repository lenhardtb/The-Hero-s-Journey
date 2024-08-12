using Godot;
using System;
using GodotInk;

[GlobalClass]
public partial class THJGlobals : Node
{
	public static InkStory Story;
	
	public static EncounterData CurrentEncounter;
	
	public static double InGameTime;
	
	//should be used when passing between scenes
	public static Vector3 DropLocation;
	
	//allows objects to set the current scene wherever they are and allows MainGame
	//to put things like effects in between
	public static MainGame MainGame;
	
	public static int RandomSeed;
	
	[Export]
	public MainGame StartMainScene
	{
		get => MainGame;
		set => MainGame = value;
	}
	
	public static Godot.Collections.Dictionary<string, Variant> SaveData;
	
	
	public override void _Ready()
	{
		GD.Print("THJGlobals Ready called!");
		
		
	}
	
	public enum PlayerMode
	{
		Moving,     //currently moving around screen
		Interacting,//currently interacting with something
		JustInteracted//gives a buffer so you don't immediately re-interact upon closing message
	}
}

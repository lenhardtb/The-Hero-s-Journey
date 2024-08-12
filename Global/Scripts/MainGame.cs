using Godot;
using System;
using GodotInk;
using Godot.Collections;
using Discord;

[GlobalClass]
public partial class MainGame : Node3D
{
	public bool IsLoading;//currently showing a loading screen
	private string currentlyLoadingScene;
	
	//this node is the one that the "current scene" should be a child of
	//the true MainGame will always include things like a covering loading screen
	//that will likely be parents of this node
	//smoothly changing scenes should be done with ChangeSceneAsync
	//but this is exposed for convenience
	[Export]
	public Node View;
	
	[Export]
	public PackedScene FirstScene;
	
	public Node3D CurrentScene;
	
	public event ViewClosedEventHandler ViewClosed;
	public delegate void ViewClosedEventHandler();
	
	public event ViewOpenedEventHandler ViewOpened;
	public delegate void ViewOpenedEventHandler();
	
	[Export]
	public bool UsingTestData = false;
	
	Discord.Discord discordInstance;
	ActivityManager activityMan;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		GD.Print("Game started!");
		
		//TODO: start on different scene/with different data if in debug version
		THJGlobals.SaveData = new Godot.Collections.Dictionary<string, Variant>();
		THJGlobals.InGameTime = 0.0;
		THJGlobals.RandomSeed = 937403825;
		THJGlobals.Story = ResourceLoader.Load<InkStory>("res://Global/Resources/GlobalsBlankInk.ink");
		THJGlobals.Story.ContinueMaximally();
		
		
		
		
		if(UsingTestData)
		{
			InitTestData();
		}
		
		//setting this up after test data so that money variable exists before creating a listener for it
		//this will be tricky when I implement save files...
		Callable c = Callable.From((string inkvar, int newValue) => MoneyChanged(newValue));
		THJGlobals.Story.ObserveVariable("money", c);
		
		discordInstance = new Discord.Discord(1148422660922023976, (UInt64)Discord.CreateFlags.Default);
		activityMan = discordInstance.GetActivityManager();
		
		this.CallDeferred("AddFirstScene");
	}

	public void AddFirstScene()
	{
		Node3D firstSceneNode = (Node3D)FirstScene.Instantiate();
			View.AddChild(firstSceneNode);
			CurrentScene = firstSceneNode;//(Node3D)View.GetChildren()[0];
	}
	
	private void InitTestData()
	{
		Godot.Collections.Dictionary<string, Variant> data = THJGlobals.SaveData;
		
		Dictionary<string, int> testInventory = new Dictionary<string, int>();
		testInventory.Add("Bread", 2);
		testInventory.Add("Bedroll", 2);
		testInventory.Add("Emerald", 1);
		
		data.Add("inventory", testInventory);
		
		THJGlobals.Story.StoreVariable("money", 200);
	}
	
	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		try
		{
			discordInstance.RunCallbacks();
			
			//I get consistent crashes without capping the activity send rate. 
			//I suspect that all the responses return at once and crash the game.
			if(Engine.GetProcessFrames() % 100 == 0)
			{
				Discord.Activity activity = new Discord.Activity()
				{
					State = "Currently in alpha!",
					Details = "A game where you are not the hero of legend."
				};
				
				activityMan.UpdateActivity(activity, (res) =>
				{
					//if(res != Discord.Result.Ok)GD.Print(res.ToString());
				});
			}
		}
		catch(Exception ex)
		{
			GD.Print(ex.Message + "\n" + ex.StackTrace);
		}
		//GD.PrintS(".");
		if(IsLoading)
		{
			Godot.Collections.Array progress = new Godot.Collections.Array();
			ResourceLoader.ThreadLoadStatus status = ResourceLoader.LoadThreadedGetStatus(currentlyLoadingScene, progress);
			if(status == ResourceLoader.ThreadLoadStatus.Loaded)
			{
				GD.Print("Finished loading scene!");
				//TODO: change this to ackowledge InkChangeLoader stuff
				//may need to change the functionality of InkChangeLoader to be in another thread?
				//Maybe have to instantiate in another thread?
				CurrentScene = (Node3D)((PackedScene)ResourceLoader.LoadThreadedGet(currentlyLoadingScene)).Instantiate();
				CurrentScene.Ready += OpenView;
				View.AddChild(CurrentScene);
				IsLoading = false;
				
			}
			else
			{
				//load bar based on progress
				GD.Print("Progress: " + progress[0]);
				
			}
		}
	}
	
	
	//inputs a resource to use
	public void SetSceneAsync(string path)
	{
		CloseView();
		if(CurrentScene != null)
		CurrentScene.TreeExiting += TreeExitingTest;
		CurrentScene?.QueueFree();
		CurrentScene = null;
		Error e = ResourceLoader.LoadThreadedRequest(path);
		if(e != Error.Ok)
			GD.Print("Some kind of error with requesting the new scene!");
		currentlyLoadingScene = path;
		IsLoading = true;
		if(e == Error.Ok)GD.Print("SetSceneAsync finished!");
	}
	
	private void TreeExitingTest()
	{
		
	}
	
	public void SetSceneAsync(PackedScene newScene)
	{
		
	}
	
	public void CloseView()
	{
		ViewClosed?.Invoke();
	}
	
	public void OpenView()
	{
		ViewOpened?.Invoke();
	}
	
	//running it here means I don't have to worry about things like removing the listener in deconstructors.
	//the component this is meant for should appear in most scenes - all locations and the overworld
	public void MoneyChanged(Variant newValue)
	{
		Node mc = GetTree().GetFirstNodeInGroup("MoneyCounter");
		
		//doing the tests separately so that I don't get exceptions
		if(mc != null)if(mc is MoneyCounter)
		{
			((MoneyCounter)mc).Amount = (int)newValue;
		}
	}
	
}

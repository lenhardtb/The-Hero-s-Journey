using Godot;
using System;
using System.Collections.Generic;

public partial class OverworldPlayer : Mob
{
	
	[Export]
	//Interactables must be at least this close to be interacted with
	public float InteractDistance;
	
	[Export]
	public TileMapDisplay3D BackgroundTileMap;
	//will inherit having a MobBehavior from Mob; just make sure that it is a PlayerBehavior
	
	//public Texture2D crosshairCursor;
	PhysicsBody3D rb;

	AnimationPlayer animator;
	
	Node collidedObject;
	
	THJGlobals.PlayerMode currentMode = THJGlobals.PlayerMode.Moving;

	CollisionShape3D myCollider;

	Vector3 facingDirection;
	bool isMoving;

	//currently unused; building this in for later
	String blockExitMessage;

	[Export]
	Camera3D MyCamera;
	
	TileData td;
	


	// Start is called before the first frame update
	public override void _Ready()
	{
		//this should setup the animation libraries and collision listener
		base._Ready();
		
		rb = (PhysicsBody3D)this;

		myCollider = GetNode<CollisionShape3D>("CollisionShape3D");

		animator = GetNode<AnimationPlayer>("Rotation/AnimatedQuadMesh/AnimationPlayer");
		
		currentMode = THJGlobals.PlayerMode.Moving;
		isMoving = false;
		/*
		facingDirection = Vector2.down;
		rcFilter = new ContactFilter2D().NoFilter();
		rcResults = new RaycastHit2D[20];
		*/
		
		//create basic mouse, with default settings if there's a problem
		//Vector2 cursorOffset = new Vector2(crosshairCursor.width / 2, crosshairCursor.height / 2);
		//Cursor.SetCursor(crosshairCursor, cursorOffset, CursorMode.Auto);

		this.Position = THJGlobals.DropLocation;
	}

	//Update is called once per frame
	public override void _Process(double delta)
	{
		
	}

	

	String testMessage = "";
	
	// PhysicsProcess is called once per physics update
	public override void _PhysicsProcess(double delta)
	{
		TileData newTd = BackgroundTileMap.GetCellTileDataGlobalPos(this.Transform.Origin);
		if(newTd != td && newTd != null)
		{
			td = newTd;
			GD.Print("Tile name: " + td.GetCustomData("Name"));
		}
		
		
		Vector3 direction = Behavior.CalcCurrentMovement(this, delta);
		bool testIsMoving = direction.X != 0 || direction.Y != 0;
		
		//oh we gotta rename these
		if (testIsMoving && (currentMode == THJGlobals.PlayerMode.Moving || currentMode == THJGlobals.PlayerMode.JustInteracted))
		{
			if(FinishedPopUp)
				MoveAndCollide(direction);
			currentMode = THJGlobals.PlayerMode.Moving;
			facingDirection = direction;
			
			//TODO: switch animation based on direction of movement
		}
		else
		{
			
			
		}
		
		
		//animator.SetBool("IsMoving", testIsMoving);
		
		isMoving = testIsMoving;

		
		//set nearby interactables to have a full outline if selected
		GetTree().CallGroup("Interactable", "SetInRange", InteractDistance);
		
		
	}


	public void SetMode(THJGlobals.PlayerMode newMode)
	{
		currentMode = newMode;
	}
	
	public override void OnCollision(Node other)
	{
		GD.Print("Player Collision called!");
		if(other is Mob)
		{
			Mob m = (Mob)other;
			
			if((m.Behavior.Allegiances & Behavior.Allegiances) == 0)
			{
				HasCollided = true;
				m.HasCollided = true;
				
				//LinkedList<Mob> extraAllies = MobBehavior.FilterMobs(Behavior.Allegiances, true);
				//LinkedList<Mob> extraEnemies = MobBehavior.FilterMobs(m.Behavior.Allegiances, true);
				
				//Data.Allies.AddRange
				
				string newScene = CustomCombatScene == null ? BaseCombatScene : CustomCombatScene;
				
				THJGlobals.CurrentEncounter = Data;
				THJGlobals.DropLocation = this.Position;
				THJGlobals.MainGame.SetSceneAsync(newScene);
			
				//THJGlobals.MainScene.GetChild(0).QueueFree();
				//THJGlobals.MainScene.AddChild(newScene.Instantiate());
			}
		}
		else if(other is OverworldLocation)
		{
			OverworldLocation ol = (OverworldLocation)other;
			//TODO: dynamically assign entrance location
			THJGlobals.DropLocation = this.Position;
			THJGlobals.MainGame.SetSceneAsync(ol.LocationScene);
			//THJGlobals.DropLocation = new Vector3();
			//THJGlobals.MainScene.GetChild(0).QueueFree();
			//THJGlobals.MainScene.AddChild(ol.LocationScene.Instantiate());
		}
	}
}

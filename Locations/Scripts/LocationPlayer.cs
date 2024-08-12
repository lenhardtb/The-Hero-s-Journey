using Godot;
using System;
using System.Collections.Generic;

public partial class LocationPlayer : CharacterBody3D
{
	[Export]
	//Interactables must be at least this close to be interacted with
	public float InteractDistance;
	[Export]
	public float MovementSpeed = 50F;
	
	
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
	
	static Vector3[] directionCompare = 
		new Vector3[]{new Vector3(0.0f,-1.0f,0.0f), new Vector3(1.0f,0.0f,0.0f), 
					  new Vector3(-1.0f,0.0f,0.0f), new Vector3(0.0f,1.0f,0.0f)};
	static string[]  directionName    = 
		new  string[]{"Down", "Left", 
					  "Right", "Up"};
	string animDirection = "Down";
	
	

	// Start is called before the first frame update
	public override void _Ready()
	{
		rb = (PhysicsBody3D)this;

		myCollider = GetNode<CollisionShape3D>("CollisionShape3D");

		animator = GetNode<AnimationPlayer>("AnimatedQuadMesh/AnimationPlayer");
		
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

	}

	//Update is called once per frame
	public override void _Process(double delta)
	{
		
	}

	

	// PhysicsProcess is called once per physics update
	public override void _PhysicsProcess(double delta)
	{
		Vector3 direction = new Vector3(0, 0, 0);
		direction.X = Input.GetActionStrength("ui_right") - Input.GetActionStrength("ui_left");
		direction.Y = Input.GetActionStrength("ui_up") - Input.GetActionStrength("ui_down");
		bool testIsMoving = direction.X != 0 || direction.Y != 0;
		
		
		direction = direction.Normalized() * MovementSpeed * (float)delta;
		
		if(testIsMoving)
		{
			int closestIndex = 0;
			float closestDistance = 5.0f;
			for(int i = 0; i < directionCompare.Length; i++)
			{
				float testDist = direction.DistanceTo(directionCompare[i]);
				if(testDist < closestDistance)
				{
					closestIndex = i;
					closestDistance = testDist;
				}
			}
			animDirection = directionName[closestIndex];
			animator.Play("LocationPlayer/Walk" + animDirection);
		}
		else//not moving
			animator.Play("LocationPlayer/Idle" + animDirection);
		//oh we gotta rename these
		if (testIsMoving && (currentMode == THJGlobals.PlayerMode.Moving || currentMode == THJGlobals.PlayerMode.JustInteracted))
		{
			//Position = Position + direction;
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
}

using Godot;
using System;

[GlobalClass]
public partial class InteractableComponent : Area3D
{
	[Export]
	public AnimatedQuadMesh OutlinedMesh;
	
	[Export]
	public NodePath InteractionTarget = "..";
	
	[Export]
	public string InteractionTargetMethod;

	//player cannot interact if farther than this value
	[Export]
	public float Distance = 2.0f;
	//public static Material highlightedMaterial = GD.Load<Material>("res://Global/InteractableOutline.tres");
	//public static Material highlightedMaterialOutOfRange = GD.Load<Material>("res://Global/InteractableOutlineOutOfRange.tres");
	
	public Color outlineColor = new Color("e8c538");
	
	//public static ImageTexture defaultImage = ImageTexture.CreateFromImage(GD.Load<CompressedTexture2D>("res://icon32.png").GetImage());
	
	private Node3D player;
	private ShaderMaterial shader;
	//private Material originalMaterial;
	
	//called after OnInteraction happens
	public event Action OnInteraction;

	//close enough to interact with - may be competing with other interactables
	public bool InRange
	{
		get;
		private set;
	}

	//mouse is currently over this; will still have a faded outline if too far
	public bool IsHovered
	{
		get;
		private set;
	}
	
	//this component should fire if selected
	public bool IsHighlighted
	{
		get;
		private set;
	}
	
	private Node3D _Player;
	private Node3D Player
	{
		get
		{
			if(_Player == null)
				_Player = (Node3D)GetTree().GetFirstNodeInGroup("Player");
			
			return _Player;
		}
		set
		{
			_Player = value;
		}
	}
	
	//private SpriteRenderer spriteRenderer;
	// Start is called before the first frame update
	public override void _Ready()
	{
		AddToGroup("InteractableComponent");
		
		shader = (ShaderMaterial)OutlinedMesh.Mesh.SurfaceGetMaterial(0);
		
		MouseEntered += OnMouseEnter;
		MouseExited += OnMouseExit;
	}

	public override void _Process(double delta)
	{
		SetInRange();
		
		if(InRange)
		{
			Godot.Collections.Array<Node> interactables = GetTree().GetNodesInGroup("InteractableComponent");
			bool isClosest = true;
			
			foreach(Node n in interactables)
			{
				if(n == this)continue;
				InteractableComponent ic = (InteractableComponent)n;
				
				//if both are in range, we are competing
				//figure out which is closer!
				if(ic.InRange)
				{
					float otherDist = ic.GlobalPosition.DistanceTo(Player.GlobalPosition);
					float myDist = GlobalPosition.DistanceTo(Player.GlobalPosition);
					//other competing interactable is closer
					if(otherDist < myDist) 
					{
						isClosest = false;
					}
				}
			}
			//in range and no closer interactables baby!!
			IsHighlighted = isClosest;
		}
		else IsHighlighted = false;//not in range
		
		if(IsHovered || InRange)
			shader.SetShaderParameter("outlineWidth", 1);
		else
			shader.SetShaderParameter("outlineWidth", 0);
		
		Color c = (Color)shader.GetShaderParameter("outlineColor");
		
		if(IsHighlighted)
			c.A = 1.0f;
		else
		{
				c.A = 0.5f;
		}
		
		shader.SetShaderParameter("outlineColor", c);
		
		if(IsHighlighted && Input.IsActionJustReleased("ui_select"))
		{
			DoInteract();
		}
	}

	public void OnMouseEnter()
	{
		GD.Print("mouseenter fired!");
		IsHovered = true;
	}

	public void OnMouseExit()
	{
		IsHovered = false;
	}

	public virtual void DoInteract()
	{
		GD.Print("DoInteract called!");
		GetNode(InteractionTarget).Call(InteractionTargetMethod);
		OnInteraction?.Invoke();
	}

	public void SetInRange()
	{
		Node3D player = Player;
		if(player == null)return;
		
		InRange = chopZ(this.GlobalPosition).DistanceTo(chopZ(player.GlobalPosition)) < Distance;
		
	}
	
	
	private Vector2 chopZ(Vector3 original) {return new Vector2(original.X, original.Y);}
	private Vector3 addZ(Vector2 original, float z = 0.0f) {return new Vector3(original.X, original.Y, z);}
	
}

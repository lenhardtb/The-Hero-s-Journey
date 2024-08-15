using Godot;
using System;
using Godot.Collections;

[Tool]
[GlobalClass]
public partial class QuestPointNode : Control
{
	public QuestPoint QuestPoint;
	
	TextEdit NameTextEdit;
	TextEdit DescriptionTextEdit;
	GridContainer ChangesSummary;
	Button SplitButton;
	
	
	
	public event Action NewPointButtonPressed;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		this.GuiInput += MouseClickedListener;
		EditorInterface.Singleton.GetInspector().PropertyEdited += UpdateValues;
		SplitButton = GetNode<Button>("VBoxContainer2/SplitButton");
		NameTextEdit = GetNode<TextEdit>("VBoxContainer/VBoxContainer/Name");
		DescriptionTextEdit = GetNode<TextEdit>("VBoxContainer/VBoxContainer/Description");
		ChangesSummary = GetNode<GridContainer>("VBoxContainer/ScrollContainer/ChangesSummary");
		
		if(this.QuestPoint == null)
			this.QuestPoint = new QuestPoint();
		
		//have to do this before listeners so it doesn't chang the values while setting them
		NameTextEdit.Text = this.QuestPoint.PointName;
		DescriptionTextEdit.Text = this.QuestPoint.Description;
		
		if(!Engine.IsEditorHint())
		{
			//add relevant listeners
			SplitButton.Pressed += delegate(){NewPointButtonPressed.Invoke();};
			NameTextEdit.TextSet += TextEditChanged;
			DescriptionTextEdit.TextSet += TextEditChanged;
			
			UpdateChangeSummary();
		}
		
		
		
		
		//DON'T add the next buttons here
		//the containing questnode should add them - 
		//this node doesn't have references to the other nodes anyway
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		
	}
	
	public void MouseClickedListener(InputEvent e)
	{
		if(e is InputEventMouseButton)
			if(QuestPoint != null)
				EditorInterface.Singleton.InspectObject(QuestPoint);
	}
	
	//fires when the inspector
	public void UpdateValues(string property)
	{
		Godot.GodotObject editedObject = EditorInterface.Singleton.GetInspector().GetEditedObject();
		if(!(editedObject is QuestPoint))return;
		else if(editedObject == this.QuestPoint)
		{
			UpdateChangeSummary();
		}
	}
	
	private void TextEditChanged()
	{
		this.QuestPoint.PointName = NameTextEdit.Text;
		this.QuestPoint.Description = DescriptionTextEdit.Text;
	}
	
	private void UpdateChangeSummary()
	{
		//free all of its current children except the first two headers
		for(int i = 2; i < ChangesSummary.GetChildren().Count; i++)
			ChangesSummary.GetChild(i).QueueFree();
		
		Godot.Collections.Array<InkChange> changes = this.QuestPoint.MyChanges;
		for(int i = 0; i < changes.Count; i++)
		{
			int numConditions = changes[i].Conditions.Count;
			int numEffects = changes[i].Effects.Count;
			
			ChangesSummary.AddChild(GenerateLabel("(" + i + "/" + changes.Count + ")"));
			ChangesSummary.AddChild(GenerateFillerPanel());
			
			for(int o = 0; o < Math.Max(numConditions, numEffects); o++)
			{
				if(o < numConditions)
				{
					ChangesSummary.AddChild(GenerateLabel(changes[i].Conditions[o].ToString()));
				}
				else
				{
					if(o == 0)
						ChangesSummary.AddChild(GenerateLabel("[No Conditions]"));
					else
						ChangesSummary.AddChild(GenerateFillerPanel());
				}
				
				if(o < numEffects)
				{
					ChangesSummary.AddChild(GenerateLabel(changes[i].Effects[o].ToString()));
				}
				else
				{
					if(o == 0)
						ChangesSummary.AddChild(GenerateLabel("[No Effects]"));
					else
						ChangesSummary.AddChild(GenerateFillerPanel());
				}
			}
			
		}
		
		
	}
	
	private Panel GenerateFillerPanel()
	{
		Panel returner = new Panel();
		//returner.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
		return returner;
	}
	
	private Label GenerateLabel(string text)
	{
		Label returner = new Label();
		returner.Text = text;
		return returner;
	}
	
	public override void _DropData(Vector2 position, Variant data)
	{
		try
		{
			Node dataNode = (Node)data;
		
			NodePath p = EditorInterface.Singleton.GetEditedSceneRoot().GetPathTo(dataNode);
			
			InkChange ic = new InkChange();
			
			BasicInkCondition bic = new BasicInkCondition();
			BasicInkEffect bie = new BasicInkEffect();
			
			bie.ObjectToSet = p;
			
			ic.Conditions.Add(bic);
			ic.Effects.Add(bie);
			
			//Array<InkChange> changes = this.QuestPoint.MyChanges;
			//Array<InkChange> newChanges = new Array<InkChange>(changes.Count + 1);
			//for(int i = 0; i < changes.Count; i++)
				//newChanges[i] = changes[i];
			//newChanges[changes.Count] = ic;
			//this.QuestPoint.MyChanges = newChanges;
			this.QuestPoint.MyChanges.Insert(this.QuestPoint.MyChanges.Count, ic);
		}
		catch(Exception){}
	}
	
	public override bool _CanDropData(Vector2 position, Variant data)
	{
		GD.Print("Trying to drop data of type " + data.GetType().ToString());
		try
		{
			Node nodeTest = (Node)data;
			return true;
		}
		catch(Exception){}
			
		return false;
	} 
	
	public override Variant _GetDragData(Vector2 position)
	{
		return new Quest();
	}
}

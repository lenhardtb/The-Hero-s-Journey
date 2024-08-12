using Godot;
using System;
using Godot.Collections;

[Tool]
[GlobalClass]
public partial class QuestPointNode : Control
{
	public QuestPoint QuestPoint;
	
	Label NameLabel;
	Label DescriptionLabel;
	
	Button SplitButton;
	
	public event Action NewPointButtonPressed;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		this.GuiInput += MouseClickedListener;
		EditorInterface.Singleton.GetInspector().PropertyEdited += UpdateValues;
		SplitButton = GetNode<Button>("VBoxContainer2/SplitButton");
		
		if(SplitButton != null)
			SplitButton.Pressed += delegate(){NewPointButtonPressed.Invoke();};
		else 
		{
			GD.Print("SplitButton was null! Stack trace is:");
			GD.Print(System.Environment.StackTrace);
		}
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
	
	public void UpdateValues(string property)
	{
		if(EditorInterface.Singleton.GetInspector().GetEditedObject() != QuestPoint)return;
		else if(property == "PointName")
		{
			
		}
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
			
			ic.Condition = bic;
			ic.Effect = bie;
			
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

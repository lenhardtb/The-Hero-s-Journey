using Godot;
using System;
using System.Threading;

[Tool]
[GlobalClass]
public partial class QuestNode : Control
{
	private static PackedScene QuestPointNodePacked = ResourceLoader.Load<PackedScene>("res://addons/inkchangeplugin/manager_nodes/QuestPointNode.tscn");
	
	private Quest _Quest;
	public Quest Quest
	{
		get
		{
			return _Quest;
		}
		set
		{
			_Quest = value;
			
			HBoxContainer qpnContainer;
			TextEdit te;
			
			try
			{
				//fetch the nodes to operate on
				qpnContainer = GetNode<HBoxContainer>("ScrollContainer/HBoxContainer");
				te = GetNode<TextEdit>("HFlowContainer/QuestName");
				
				//clear their content
				Godot.Collections.Array<Node> containerChildren = qpnContainer.GetChildren();
				foreach(Node qpnChild in containerChildren)
					qpnChild.QueueFree();
					
				te.Text = "[No Quest Assigned]";
			}
			catch (Exception)
			{
				//this happens a lot when instantiating a packed node
				return;
			}
			
			//if it's null, we just cleared the stuff and we're done!
			if(value == null)
			{
				return;
			}
			
			te.Text = value.QuestName;
			
			if(value.StartPoint == null)
			{
				_MaxSteps = 0;
				_MaxSplits = 0;
				return;
			}
			
			QuestPoint[] currQuestPoint = new QuestPoint[]{value.StartPoint};
			while(currQuestPoint.Length > 0)
			{
				if(currQuestPoint.Length > _MaxSplits)
					_MaxSplits = currQuestPoint.Length;
				
				GridContainer vContainer = new GridContainer();
				
				//vContainer.Columns = 1;//it's 1 by default
				
				for(int i = 0; i < currQuestPoint.Length; i++)
				{
					QuestPointNode qpn = QuestPointNodePacked.Instantiate<QuestPointNode>();
				
					qpn.QuestPoint = currQuestPoint[i];
					
					Button qpnSplitButton = qpn.GetNode<Button>("VBoxContainer2/SplitButton");
					qpnSplitButton.Pressed += delegate()
					{
						QuestPointSplitButtonPressed(qpn);
					};
					
					Node qpnNextParent = qpnSplitButton.GetParent();
					
					int nextIndex = 0;
					foreach(Node child in qpnNextParent.GetChildren())
						if(child.Name.ToString().StartsWith("NextButton"))
						{
							
							((Button)child).Pressed += delegate()
							{
								QuestPointNextButtonPressed(qpn, nextIndex);
							};
							nextIndex++;
						}
					
					vContainer.AddChild(qpn);
				}
				
				qpnContainer.AddChild(vContainer);
				
				QuestPoint[] buildNextQuestPoint = new QuestPoint[]{};
				
				for(int i = 0; i < currQuestPoint.Length; i++)
				{
					QuestPoint[] temp = new QuestPoint[buildNextQuestPoint.Length + currQuestPoint[i].NextPoint.Count];
					
					for(int o = 0; o < temp.Length; o++)
					{
						temp[i] = o < buildNextQuestPoint.Length ? buildNextQuestPoint[i] : currQuestPoint[i].NextPoint[o];
					}
					
					buildNextQuestPoint = temp;
				}
					
				currQuestPoint = buildNextQuestPoint;
			}//while currquestPoint.Length > 0
		}//set Quest
	}
	
	private int _MaxSplits = 0;
	private int _MaxSteps = 0;
	TextEdit QuestName;
	HBoxContainer QuestPointsContainer;
	HFlowContainer SeeAlsoResultsContainer;
	Container CheckBoxesContainer;
	
	
	System.Collections.Generic.Queue<Godot.GodotObject> SeeAlsoResults = new System.Collections.Generic.Queue<Godot.GodotObject>();
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		QuestName = GetNode<TextEdit>("HFlowContainer/QuestName");
		QuestName.TextSet += TextCheck;
		SeeAlsoResultsContainer = GetNode<HFlowContainer>("SeeAlsoPanel/ScrollContainer/SeeAlsoResultsContainer");
		
		CheckBoxesContainer = GetNode<Container>("SeeAlsoPanel/HFlowContainer");
		foreach(Node child in CheckBoxesContainer.GetChildren())
			if(child is Button)
				((Button)child).Pressed += SeeAlsoFilterPressed;
		
		QuestPointsContainer = GetNode<HBoxContainer>("ScrollContainer/HBoxContainer");
		
	}
	
	public override void _Process(double delta)
	{
		
	}
	
	private void TextCheck()
	{
		if(QuestName.Text.Contains("\n"))
		{
			QuestName.Text = QuestName.Text.Replace("\n", "");
			this.Quest.QuestName = QuestName.Text;
		}
	}
	
	private void SeeAlsoFilterPressed()
	{
		bool seeNodes = CheckBoxesContainer.GetNode<Button>("NodesCheckButton").ButtonPressed;
		bool seeVariables = CheckBoxesContainer.GetNode<Button>("VariablesCheckButton").ButtonPressed;
		bool seeQuests = CheckBoxesContainer.GetNode<Button>("QuestsCheckButton").ButtonPressed;
		
		foreach(Node child in SeeAlsoResultsContainer.GetChildren())
		{
			//condition
			//bool calcVisible = true;
			//TODO: structure for these buttons' data
			//if(child.data is Node && !seeNodes)calcVisible = false;
			//child.Visible = calcVaisible;
		}
	}
	
	private void QuestPointSplitButtonPressed(QuestPointNode qpn)
	{
		QuestPoint newQP = new QuestPoint();
		qpn.QuestPoint.NextPoint.Add(newQP);
		
		QuestPointNode newQPN = QuestPointNodePacked.Instantiate<QuestPointNode>();
		newQPN.QuestPoint = newQP;
		
		Container buttonsContainer = newQPN.GetNode<Container>("VBoxContainer2");
		int newNextIndex = buttonsContainer.GetChildren().Count - 2;
		Button newNextButton = new Button();
		newNextButton.Text = "=>";
		newNextButton.Pressed += delegate(){QuestPointNextButtonPressed(newQPN, newNextIndex);};
		
		buttonsContainer.AddChild(newNextButton);
		buttonsContainer.MoveChild(newNextButton, qpn.QuestPoint.NextPoint.Count);
		
		int stepIndex = GetStepIndex(qpn);
		int splitIndex = GetSplitIndex(qpn);
		
		//insert the node
		//steps: 
		//-find the location
		//-go to each row and add either a blank space or the new pointnode
		//adding a row is always necessary but trivial - simply add an element at the end
		Godot.Collections.Array<Node> gridChildren = QuestPointsContainer.GetChildren();
		
		for(int i = 0; i < gridChildren.Count; i++)
		{
			GridContainer child = (GridContainer)gridChildren[i];
			
			Control newNode;
			if(i != splitIndex) 
				newNode = GetFillerPanel();
			else 
				newNode = newQPN;
			
			newNode.SizeFlagsVertical = Control.SizeFlags.Fill | Control.SizeFlags.Expand;
			
			//child.RemoveChild(splitIndex);
			child.AddChild(newNode);
			child.MoveChild(newNode, splitIndex);
			
		}
		_MaxSplits++;
		
	}
	
	private void QuestPointNextButtonPressed(QuestPointNode qpn, int nextIndex)
	{
		
		QuestPoint newQP = new QuestPoint();
		
		//insert questpoint for underlying questpoints
		Godot.Collections.Array<QuestPoint> nextPoint = qpn.QuestPoint.NextPoint;
		nextPoint[nextIndex].PreviousPoint = newQP;//make next look back to new
		newQP.NextPoint.Add(nextPoint[nextIndex]);//make new point forward to next
		nextPoint[nextIndex] = newQP;//make current point forward to new in place of previous one
		
		int stepIndex = GetStepIndex(qpn);
		int splitIndex = GetSplitIndex(qpn);
		
		QuestPointNode newQPN = QuestPointNodePacked.Instantiate<QuestPointNode>();
		newQPN.QuestPoint = newQP;
		
		//insert the node
		//steps: 
		//-find the location
		//-repeatedly shift any nodes in the way right
		//-add a new column if necessary
		Godot.Collections.Array<Node> gridChildren = QuestPointsContainer.GetChildren();
		
		//space set aside for nodes that need to be moved to the next step;
		Godot.Collections.Array<Node> tempNodes = new Godot.Collections.Array<Node>();
		tempNodes.Add(newQPN);
		
		for(int i = stepIndex; i <= _MaxSteps; i++)
		{
			GridContainer child = (GridContainer)gridChildren[i];
			
			if(stepIndex == _MaxSteps)
			{
				GridContainer newColumn = new GridContainer();
				for(int o = 0; o < _MaxSplits; o++)
					newColumn.AddChild(GetFillerPanel());
				
				QuestPointsContainer.AddChild(newColumn);
				child = newColumn;
				_MaxSteps++;
			}
			
			
			int numSwaps = tempNodes.Count;
			for(int o = splitIndex; o < splitIndex + numSwaps; o++)
			{
				Node possibleQuestPoint = child.GetChild(o);
				
				tempNodes.Add(possibleQuestPoint);
				child.RemoveChild(possibleQuestPoint);
				
				child.AddChild(tempNodes[0]);
				child.MoveChild(tempNodes[0], o);
				
				tempNodes.RemoveAt(0);
				
				if(possibleQuestPoint is QuestPointNode)
				{
					QuestPointNode possibleSplitPoint = (QuestPointNode)possibleQuestPoint;
					
					int numAdditional = possibleSplitPoint.QuestPoint.NextPoint.Count;
					if(numAdditional > 1)
					{
						numSwaps += numAdditional;
						for(int u = 0; u < numAdditional; u++)
							tempNodes.Add(GetFillerPanel());
						
					}
				}
			}
			
		}
		
		InsertQuestPoint(newQPN, stepIndex + 1, splitIndex + nextIndex);
	}
	
	private int GetStepIndex(QuestPointNode qpn)
	{
		int stepIndex = 0;
		QuestPoint currentQP = qpn.QuestPoint; 
		while(currentQP != null)
		{
			currentQP = currentQP.PreviousPoint;
			stepIndex++;
		}
		return stepIndex;
	}
	
	private int GetSplitIndex(QuestPointNode qpn)
	{
		//calculate how many splits down for the new node. always append at bottom
		//(this means the first should be at top)
		int splitIndex = 0;
		
		QuestPoint following = qpn.QuestPoint.PreviousPoint;
		QuestPoint previous = qpn.QuestPoint;
		while(following != null)
		{
			for(int i = 0; i < following.NextPoint.Count; i++)
			{
				if(following.NextPoint[i] == previous)
				{
					splitIndex += i;
					//break;//I think this only breaks the innermost and is safe?
				}
			}
			previous = following;
			following = following.PreviousPoint;
		}
		return 0;
	}
	
	private Panel GetFillerPanel()
	{
		Panel returner = new Panel();
		returner.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
		return returner;
	}
	
	private void InsertQuestPoint(QuestPointNode newQPN, int stepIndex, int splitIndex)
	{
		
	}
	
	public override void _DropData(Vector2 position, Variant data)
	{
		
	}
	
	public override bool _CanDropData(Vector2 position, Variant data)
	{
		try
		{
			QuestPoint qpTest = (QuestPoint)data;
			return true;
		}
		catch(Exception){}
		
		return false;
	} 
	
	public override Variant _GetDragData(Vector2 position)
	{
		return this.Quest;
	}
}

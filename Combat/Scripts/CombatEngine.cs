using Godot;
using System;
using System.Collections.Generic;

public partial class CombatEngine : Node3D
{
	//length one round takes to animate in Mseconds
	public const int ROUND_TIME = 1000;
	//length one combat action takes to trigger
	public const int ACTION_TIME = 200;
	private const int LARGE_QUEUE = 10;//how many rounds is considered a large queue that may play quickly.

	string ReturnPath;
	List<string> FinishedTags;
	
	public event Action CombatFinished;
	
	Round currentRound;
	Queue<Round> pastRounds;
	Queue<Round> queuedRounds;
	List<CombatUnit> combatants;

	public bool RoundIdle { get; protected set; }
	public bool RoundInProgress { get; protected set; }
	public bool RoundsQueued { get { return queuedRounds.Count > 0; } }
	public bool LargeQueue { get { return queuedRounds.Count > LARGE_QUEUE; } }
	public bool RoundPaused { get; protected set; }

	private double prevRoundTime;
	private double prevActionTime;
	private double combatFinishedTime;
	private int nextQueueRound;//number of rounds that have been passed and queued
	
	/*
	 * Random jotted down ideas:
	 * make healing apply after damage, so you see it happen. 
	 * do prediction on healing; there's a special status from momentarily hitting 0
	 * death triggers when displayed hp hits 0, not necessarily the moment the killing round happens
	 * 
	 */
	
	//hardcoded list of valid positions. this is such an ugly implementation,
	//but I guess we do know the finite spaces ahead of time...
	private Dictionary<CombatUnit.Position, Vector3> posPositions = 
	new Dictionary<CombatUnit.Position, Vector3>()
		{
			{CombatUnit.Position.ALLY_ONE,   new Vector3(0.0f,0.0f,0.0f)},
			{CombatUnit.Position.ALLY_TWO,   new Vector3(0.0f,0.0f,0.0f)},
			{CombatUnit.Position.ALLY_THREE, new Vector3(0.0f,0.0f,0.0f)},
			{CombatUnit.Position.ALLY_FOUR,  new Vector3(0.0f,0.0f,0.0f)},
			{CombatUnit.Position.ALLY_FIVE,  new Vector3(0.0f,0.0f,0.0f)},
			{CombatUnit.Position.ALLY_SIX,   new Vector3(0.0f,0.0f,0.0f)},
			{CombatUnit.Position.ALLY_SEVEN, new Vector3(0.0f,0.0f,0.0f)},
			//{CombatUnit.Position.ALLY_EIGHT, new Vector3(0.0f,0.0f,0.0f)},
			
			{CombatUnit.Position.ENEMY_ONE,   new Vector3(0.0f,0.0f,0.0f)},
			{CombatUnit.Position.ENEMY_TWO,   new Vector3(0.0f,0.0f,0.0f)},
			{CombatUnit.Position.ENEMY_THREE, new Vector3(0.0f,0.0f,0.0f)},
			{CombatUnit.Position.ENEMY_FOUR,  new Vector3(0.0f,0.0f,0.0f)},
			{CombatUnit.Position.ENEMY_FIVE,  new Vector3(0.0f,0.0f,0.0f)},
			{CombatUnit.Position.ENEMY_SIX,   new Vector3(0.0f,0.0f,0.0f)},
			{CombatUnit.Position.ENEMY_SEVEN, new Vector3(0.0f,0.0f,0.0f)},
			//{CombatUnit.Position.ENEMY_EIGHT, new Vector3(0.0f,0.0f,0.0f)}
		};
	
	//the number of UI elements in associated lists
	private const int NumUIElements = 4;
	//arrays of UI elements are assigned to each space
	//0 = hp bar
	//1 = unused (previously unit's node)
	//2 = space ring
	//3 = unused (previously unit's animator)
	System.Collections.Generic.Dictionary<CombatUnit.Position, Node[]> posUI;
	
	
	//the packed BasicCombatant instances associated with each combatant
	System.Collections.Generic.Dictionary<CombatUnit, Node> unitNodes;

	private static PackedScene PackedBasicCombatant = ResourceLoader.Load<PackedScene>("res://Combat/Node Templates/BasicCombatant.tscn");
	
	
	// Start is called before the first frame update
	public override void _Ready()
	{
		nextQueueRound = 0;
		pastRounds = new Queue<Round>();
		queuedRounds = new Queue<Round>();
		combatants = new List<CombatUnit>();
		RoundIdle = true;
		RoundInProgress = false;
		RoundPaused = false;
		FinishedTags = new List<string>();
		
		//TODO: should LoadEncounter also do this Dictionary unit setup?
		//in fact if I take that approach, do I even need to return anything?
		combatants.AddRange(LoadEncounter());
		
		prevRoundTime = Time.GetTicksMsec();
		prevActionTime = prevRoundTime;
		
		Button b = GetNode<Button>("CanvasLayer/Panel/GoButton");
		b.Pressed += OnGoButtonClick;

		//initialize dictionaries
		posUI = new System.Collections.Generic.Dictionary<CombatUnit.Position, Node[]>(posPositions.Count);
		unitNodes = new System.Collections.Generic.Dictionary<CombatUnit, Node>(combatants.Count);
		
		
		//prepare the UI elements, etc.
		foreach(CombatUnit.Position currentPos in posPositions.Keys)
		{
			Node[] elements = new Node[NumUIElements];
			
			string side = ((currentPos & CombatUnit.Position.ALLY_ALL) != CombatUnit.Position.NONE) ? "Left" : "Right";
			
			//this is only a useful way to find index because I know 
			//that the hardcoded values consist of exactly one space
			int positionValue = CombatUnit.PosToIndex(currentPos)[0];
			
			Node3D space = (Node3D)GetNode("Spaces/" + side + "Side/PaperFolding" + positionValue + "/AQM" + positionValue);
			posPositions[currentPos] = space.GlobalPosition;
			
			elements[0] = GetNode("CanvasLayer/Panel/" + side + "Side/GridContainer/ProgressBar" + positionValue);
			//elements[1] = unpacked;
			elements[2] = space;
			//elements[5] = elements[1].GetNode();
			
			posUI.Add(currentPos, elements);
		}
		
		foreach(CombatUnit u in combatants)
		{
			Node[] elements = GetUI(u.UnitPosition)[0];
			
			Node unpacked = PackedBasicCombatant.Instantiate();
			unpacked.Name = u.Name;
			Node unpackedParent = GetNode("Battlefield/" + (u.IsAlly ? "Left" : "Right") + "Side");
			unpackedParent.AddChild(unpacked);
			
			CombatantAnimationPlayer cap = 
				unpacked.GetNode<CombatantAnimationPlayer>("AnimatedQuadMesh/AnimationPlayer");
			
			cap.Unit = u;//setting the unit will generate the animations from its name
			
			//find the health bars and spaces in the UI for the relevant units; make all others invisible
		
			//AnimationTree tree = cap.GetNode<AnimationTree>("AnimationTree");
			
			//tree.Set("parameters/conditions/idle", true);
			//tree.Set("parameters/conditions/attack", true);
			
			//AnimationNodeStateMachinePlayback stateMachine = (AnimationNodeStateMachinePlayback)tree.Get("parameters/playback");
			//stateMachine.Travel("Start");
			
			//int[] indeces = CombatUnit.PosToIndex(u.UnitPosition);
			
			//if(indeces.Length > 0 && (u.UnitPosition != CombatUnit.Position.NONE))
			//	((Node3D)unpacked).GlobalPosition = posPositions[(CombatUnit.Position)indeces[0]];
			
			foreach(CombatUnit.Position key in posPositions.Keys)
			{
				if((u.UnitPosition & key) != CombatUnit.Position.NONE)
				{
					Vector3 offset = Vector3.Up;
					AnimatedQuadMesh aqm = unpacked.GetNode<AnimatedQuadMesh>("AnimatedQuadMesh");
					offset = offset * aqm.Mesh.GetAabb().Size.Y * 0.25f;
					
					((Node3D)unpacked).GlobalPosition = posPositions[key] + offset;
					
					GD.Print("Sending unit " + u.Name + " to " + posPositions[key]);
					break;//break out of innermost loop
				}
			}
			
			cap.Play(u.Filename + "/Idle");
			
			unitNodes.Add(u, unpacked);
		}
		
		
		//TODO: make space sprites change based on whether they are occupied

	}
	
	private List<Node[]> GetUI(CombatUnit.Position position)
	{
		
		List<Node[]> returner = new List<Node[]>();
		
		if(position == CombatUnit.Position.NONE)
			return returner;
		
		foreach(KeyValuePair<CombatUnit.Position, Node[]> pair in posUI)
		{
			if((position & pair.Key) != CombatUnit.Position.NONE)
			{
				returner.Add(pair.Value);
			}
		}
		
		return returner;
	}
	
	
	
	private List<CombatUnit> LoadEncounter()
	{
		EncounterData data = THJGlobals.CurrentEncounter;
		
		List<CombatUnit> returner = new List<CombatUnit>();
		
		if(Engine.GetProcessFrames() < 10 || data == null)
		{
			if(Engine.GetProcessFrames() < 10)
				GD.Print("Combat opened directly from editor, loading test units.");
			else if(data == null)
				GD.Print("Combat data is null, loading test units.");
			
			CombatUnit testLookup = CombatUnit.LookupResource("SlimeWorker", CombatUnit.Position.ENEMY_ONE);
			
			returner.Add(testLookup);
			
			//TODO: generate party
			CombatUnit hardcodedAlly = new CombatUnit();
			hardcodedAlly.Filename = "OctoRanger";
			hardcodedAlly.Name = "Octo Ranger";
			hardcodedAlly.MaxHP = 100;
			hardcodedAlly.Attack = 50;
			hardcodedAlly.Defense = 20;
			hardcodedAlly.BasicAttack = CombatAction.BasicAttack(hardcodedAlly);
			hardcodedAlly.UnitPosition = CombatUnit.Position.ALLY_ONE;
			returner.Add(hardcodedAlly);
			
			ReturnPath = "res://Overworld/Overworld.tscn";
		}
		else
		{
			//use combat data to fill units
			
			returner.AddRange(data.Enemies);
			returner.AddRange(data.Allies);
			
			
			ReturnPath = data.ReturnPath;
			GD.Print("Will later return to scene + " + data.ReturnPath);
			
			//TODO: generate party
			CombatUnit hardcodedAlly = new CombatUnit();
			hardcodedAlly.Filename = "OctoRanger";
			hardcodedAlly.Name = "Octo Ranger";
			hardcodedAlly.MaxHP = 100;
			hardcodedAlly.Attack = 50;
			hardcodedAlly.Defense = 20;
			hardcodedAlly.BasicAttack = CombatAction.BasicAttack(hardcodedAlly);
			hardcodedAlly.UnitPosition = CombatUnit.Position.ALLY_ONE;
			returner.Add(hardcodedAlly);
			
			FinishedTags.AddRange(data.DefeatTags);
			
			//done extracting info, remove
			THJGlobals.CurrentEncounter = null;
		}
		return returner;
	}

	// Update is called once per frame
	public override void _Process(double delta)
	{
		//GD.Print("Process - Ticks: " + Time.GetTicksMsec() + " Next Round time: " + prevRoundTime + ROUND_TIME);
		if(RoundPaused)
		{
			//somebody won and animations have probably finished
			if(Time.GetTicksMsec() > (combatFinishedTime + ROUND_TIME))
			{
				//TODO:trigger appearance of victory/loss window
				
				//switch out scene TODO: trigger this when victory window is closed instead of immediately
				if(true)//ResourceLoader.Exists(ReturnScene))//it is now assumed that the scene exists
				{
					//PackedScene packedNewScene = ResourceLoader.Load<PackedScene>(ReturnScene);
					THJGlobals.MainGame.SetSceneAsync(ReturnPath);
					
				}
				
			}
		}
		//not paused, no rounds on stack - add a round
		else if ((Time.GetTicksMsec() >= (prevRoundTime + ROUND_TIME)) && queuedRounds.Count > 0)
		{
			GD.Print("Round about to be taken off of stack!");
			currentRound = queuedRounds.Dequeue();

			prevRoundTime = Time.GetTicksMsec();

			bool roundLeft = queuedRounds.Count > 0;
			RoundIdle = !roundLeft;
			RoundInProgress = roundLeft;
			SetAllAnimators("Attack", true);
		}
		else if ((Time.GetTicksMsec() >= (prevActionTime + ACTION_TIME)) && currentRound != null)
		{
			if(currentRound.Count > 0)
			{
				//GD.Print("Action about to be taken off stack!");
				CombatAction a = currentRound.Pop();
				
				a.Execute(combatants);
				
				prevActionTime = Time.GetTicksMsec();
			}
			else//end of round. Check for deaths, swap out
			{
				//separate combatants to determine if any side is defeated
				List<CombatUnit> allyUnits = new List<CombatUnit>();
				List<CombatUnit> enemyUnits = new List<CombatUnit>();

				foreach (CombatUnit u in combatants)
				{
					if(u.IsAlly)
					{
						allyUnits.Add(u);
					}
					else if (u.IsEnemy)
					{
						enemyUnits.Add(u);
					}
				}
				
				//TODO: replace dead units with bodies?
				
				//set states of units and animators
				foreach(CombatUnit cu in combatants)
				{
					//just skip this loop if it isn't onscreen and has no UI to change
					if(cu.UnitPosition == CombatUnit.Position.NONE)continue;
					
					Node[] uiElements = GetUI(cu.UnitPosition)[0];
					Node unitNode = unitNodes[cu];
					if(cu.CurrentHP <= 0)
					{
						GD.Print("Unit has 0 health!");
						//TODO: trigger death animation
						CombatantAnimationPlayer cap = unitNode.GetNode<CombatantAnimationPlayer>("AnimatedQuadMesh/AnimationPlayer");
						cap.Play(cap.Unit.Filename + "/Death");
					}
					else
					{
						//idk what goes here
					}
				}
				
				//check if either side won
				bool didAlliesWin = true;

				foreach(CombatUnit u in enemyUnits)
				{
					if (u.CurrentHP > 0)
					{
						didAlliesWin = false;
						break;
					}
				}

				bool didEnemiesWin = true;

				foreach (CombatUnit u in allyUnits)
				{
					if (u.CurrentHP > 0)
					{
						didEnemiesWin = false;
						break;
					}
				}

				//TODO: break this out
				if(didAlliesWin || didEnemiesWin)//somebody won
				{
					RoundPaused = true;
					combatFinishedTime = Time.GetTicksMsec();
					GodotInk.InkStory story = THJGlobals.Story;
					foreach(string s in FinishedTags)
					{
						story.IncrementVariable(s);
					}
					CombatFinished?.Invoke();
				}
				else//nobody won, move the round forward
				{
					pastRounds.Enqueue(currentRound);
					currentRound = null;
				}
			}
		}
		else if(Time.GetTicksMsec() >= (prevRoundTime + ROUND_TIME))
		{
			RoundIdle = true;
			RoundInProgress = false;
			SetAllAnimators("Idle", true);
			
			//GD.Print("Animators automatically set to Idle!");
		}
		
		
		//set hp displays to display hp calculated by units

		foreach(CombatUnit u in combatants)
		{
			if(u.UnitPosition == CombatUnit.Position.NONE)continue;
			
			List<Node[]> UIList = GetUI(u.UnitPosition);
			if(UIList.Count == 0)GD.Print("Unit " + u.Name + " has no elements at position " + u.UnitPosition);
			Node[] elements = GetUI(u.UnitPosition)[0];
			
			ProgressBar b = (ProgressBar)elements[0];
			
			b.Value = u.CurrentHP * 100.0 / u.MaxHP ;
		}
	}
	
	public void OnGoButtonClick()
	{
		//lock in targets
		foreach (CombatUnit u in combatants)
		{
			CombatAction a = u.BasicAttack;
			if(a.Source != u) a.Source = u;
			
			//TODO: user-set targets
			a.TargetedPosition = CombatUnit.Position.NONE;
			//currently everything just smacks everything but itself
			foreach(CombatUnit u2 in combatants)
			{
				if (u2 != u)
					a.TargetedPosition = a.TargetedPosition | u2.UnitPosition;
			}
			
			//newRound.Actions.Add(a);
		}
		
		Round newRound = new Round(combatants, nextQueueRound);
		nextQueueRound++;
		
		//TODO: is this necessary?
		if (RoundIdle)
		{
			prevRoundTime = Time.GetTicksMsec();
			RoundIdle = false;
			RoundInProgress = true;
		}
		queuedRounds.Enqueue(newRound);
	}

	private List<Node3D> GetSpaces(CombatUnit.Position p)
	{
		//TODO: generate space rings from a position
		return new List<Node3D>();
	}
	
	//convenience function, we will often be setting all units to attack or become idle
	private void SetAllAnimators(string parameter, bool value = true)
	{
		//I expect that later I'll implement a state machine with flags. for now the parameter is there
		if(!value)return;
		
		Node element;//outside the loop for efficiency
		foreach(System.Collections.Generic.KeyValuePair<CombatUnit, Node> kvp in unitNodes)
		{
			element = kvp.Value;
			//TODO: make this more efficient
			CombatantAnimationPlayer cap = element.GetNode<CombatantAnimationPlayer>("AnimatedQuadMesh/AnimationPlayer");
			cap.Play(cap.Unit.Filename + "/" + parameter);
		}
	}
}

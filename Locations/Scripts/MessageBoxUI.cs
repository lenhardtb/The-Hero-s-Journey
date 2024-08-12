using Godot;
using System;
using GodotInk;

public partial class MessageBoxUI : Panel
{
	Label messageText;
	Button nextButton;
	Label choiceMessageText;
	Panel choicePanel;
	Button[] choiceButtons;
	TabContainer tabContainer;

	private bool sameFrame;//I hate this workaround. prevents button from pressing on the same frame as the UI opens
	MessageState currentMode = MessageState.None;

	LocationPlayer playerScript;

	InkStory story;
	
	public const int NUM_BUTTONS = 3;

	//hardcoded string flags
	private const string startChoice = "START CHOICE";
	private const string endChoice = "END CHOICE";
	private const string end = "END";

	//regular expression strings
	//private const string rVariable = "\".*\"";

	
	enum MessageState
	{
		None, //not claiming control - should be invisible
		Message, //sending a basic message that will go to the next line
		Choice, //presenting a choice
		Obtain //giving an item to the player
		//add more as necessary idk
	}
	
	[Signal]
	public delegate void OnFinishedEventHandler();
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		messageText = GetNode<Label>("TabContainer/MessagePanel/MessageLabel");
		nextButton  = GetNode<Button>("NextButton");
		choicePanel = GetNode<Panel>("TabContainer/ChoicePanel");
		tabContainer = GetNode<TabContainer>("TabContainer");
		choiceMessageText = choicePanel.GetNode<Label>("MessageLabel");
		
		choiceButtons = new Button[NUM_BUTTONS];
		
		for (int i = 0; i < choiceButtons.Length; i++)
		{
			choiceButtons[i] = choicePanel.GetNode<Button>("GridContainer/ChoiceButton" + (i + 1));
			int tempValue = i;//the number has to be created new for each listener and not just use i
			choiceButtons[i].Pressed += delegate () { OnNext(tempValue); };
		}

		nextButton.Pressed += OnNext;

		currentMode = MessageState.None;

		Godot.Collections.Array<Node> players = GetTree().GetNodesInGroup("Player");
		foreach(Node n in players)
			if(n is LocationPlayer)
				playerScript = (LocationPlayer)n;
		//LeanTween.moveY(gameObject, offscreenPosition, 0.0f);
		//TweenOutFast();
		sameFrame = true;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (currentMode == MessageState.Message && Input.IsActionJustPressed("ui_select"))
		{
			//don't trigger on the same space press as the one that turned this on
			if (sameFrame) sameFrame = false;
			else OnNext();
		}
	}
	
	public void OnInteraction(String filepath)
	{
		GodotInk.InkStory story = GD.Load<InkStory>(filepath);
		GD.Print("Story created in OnInteraction!");
		
		OnInteraction(story);
	}
	
	public void OnInteraction(InkStory story)
	{
		this.story = THJGlobals.Story;
		
		this.story.ChangeStoryKeepVariables(story.GetRawStory());
		//this.story.LoadState(THJGlobals.Story.SaveState());

		this.story.ChoosePathString("start");
		this.story.Continue();//go to start then do the first redirect every file in this game has
		
		UpdateAppearance();
		
		sameFrame = true;
	}
	
	public void OnNext()
	{
		OnNext(-1);
	}

	//this will use choice if there is actually a choice to be made, otherwise, the value will be ignored
	public void OnNext(int choice)
	{
		if (currentMode == MessageState.None)
		{
			return;
		}
		if(!(story.CanContinue || story.CurrentChoices.Count > 0))
		{
			OnClose();
		}
		else if(choice != -1)
		{
			story.ChooseChoiceIndex(choice);
			//skip prompting text - this line unneccesary by putting brackets around choice text
			//story.Continue();
			story.Continue();//skip answer text
			UpdateAppearance();
		}
		else
		{
			if(story.CanContinue)
				story.Continue();
			UpdateAppearance();
		}
	}

	/*
	 * call each time the display needs updated
	 * set up display of choices, clear text, etc.
	 * does NOT advance the story
	 * 
	 * this should be able to be called repeatedly without making any changes to game state
	 */
	void UpdateAppearance()
	{
		if(story.CurrentChoices.Count > 0)
		{
			
			tabContainer.CurrentTab = 1;
			nextButton.Disabled = true;
			choiceMessageText.Text = story.CurrentText;

			//TODO: determine top 3 choices

			for (int i = 0; i < NUM_BUTTONS; ++i)
			{

				if (i < story.CurrentChoices.Count)
				{
					choiceButtons[i].Visible = true;
					InkChoice choice = story.CurrentChoices[i];
					choiceButtons[i].Text = choice.Text;
				}
				else
				{
					choiceButtons[i].Visible = false;
				}
			}
			currentMode = MessageState.Choice;
		}
		else
		{
			tabContainer.CurrentTab = 0;
			nextButton.Disabled = false;
			messageText.Text = story.CurrentText;
			
			currentMode = MessageState.Message;
		}
	}

	void OnClose()
	{
		messageText.Text = "";
		GetTree().CallGroup("TweeningUIHolder", "TweenOut");

		currentMode = MessageState.None;
		
		//story.ChangeStoryKeepVariables(story.GetRawStory());
		//GD.Print(story.SaveState());
		//THJGlobals.Story.LoadState(story.SaveState());
		playerScript.SetMode(THJGlobals.PlayerMode.JustInteracted);
		
		EmitSignal(SignalName.OnFinished);
		
		return;
	}
}

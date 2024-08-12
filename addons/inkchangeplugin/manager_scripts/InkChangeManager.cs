using Godot;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

[Tool]
[GlobalClass]
public partial class InkChangeManager : Control
{
	public Node CurrentScene;
	
	private Label StatusLabel1;
	private Label StatusLabel2;
	private Panel QuestsView;
	
	private TextEdit SearchBar;
	private Label SearchFeedback;
	Thread searchingThread;
	private VBoxContainer searchQuestResults;
	
	Queue<Quest> searchResults;
	
	private static PackedScene QuestNodePacked = ResourceLoader.Load<PackedScene>("res://addons/inkchangeplugin/manager_nodes/QuestNode.tscn");
	
	private const string questFolderDir = "res://Global/Databases/Quests/";
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		GetNode<Button>("TopMenu/HBoxContainer/CurrentSceneButton").Pressed += CurrentSceneButtonPressed;
		GetNode<Button>("TopMenu/HBoxContainer/NewQuestButton").Pressed += NewQuestButtonPressed;
		SearchBar = GetNode<TextEdit>("MainEditor/QuestsView/HFlowContainer/SearchBar");
		SearchBar.TextSet += SearchBarSet;
		SearchFeedback = GetNode<Label>("MainEditor/QuestsView/HFlowContainer/SearchFeedback");
		
		StatusLabel1 = GetNode<Label>("BottomMenu/HBoxContainer/StatusLabel1");
		StatusLabel2 = GetNode<Label>("BottomMenu/HBoxContainer/StatusLabel2");
		
		QuestsView = GetNode<Panel>("MainEditor/QuestsView");
		searchQuestResults = GetNode<VBoxContainer>("MainEditor/QuestsView/ScrollContainer/VBoxContainer");
		
		searchResults = new Queue<Quest>();
		
		CallDeferred("CurrentSceneButtonPressed");
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if(EditorInterface.Singleton.GetEditedSceneRoot() == this)
		{
			return;
		}
		
		if(searchingThread != null)
		if(searchingThread.IsAlive)
		{
			SearchFeedback.Text = "Searching...";
			if(Monitor.TryEnter(searchResults))
			{
				while(searchResults.Count > 0)
				{
					
					Quest q = searchResults.Dequeue();
					try
					{
						QuestNode qn = QuestNodePacked.Instantiate<QuestNode>();
						
						searchQuestResults.AddChild(qn);
						HSeparator smallSpace = new HSeparator();
						smallSpace.AddThemeConstantOverride("separation", 5);
						searchQuestResults.AddChild(smallSpace);
						
						try
						{
							qn.Quest = q;//this will populate its content;
						}
						catch(Exception ex)
						{
							GD.Print(ex.Message);
							GD.Print(ex.StackTrace);
						}
						
					}
					catch(Exception){GD.Print("Exception thrown!");}
				}
				//TODO: sort search results
				
				Monitor.Exit(searchResults);
			}
		}
		else
		{
			SearchFeedback.Text = "Done searching.";
		}
	}
	
	public override void _EnterTree()
	{
		
	}
	
	public override void _ExitTree()
	{
		
	}
	
	public void SearchBarSet()
	{
		if(EditorInterface.Singleton.GetEditedSceneRoot() == this)return;
		if(SearchBar.Text == "")//default to just searching up scene quests
			CurrentSceneButtonPressed();
		else
		{
			if(SearchBar.Text.EndsWith("\n"))
			{
				GD.Print("Enter hit on search bar!");
				SearchBar.Text = SearchBar.Text.Replace("\n", "");
			}
			StatusLabel1.Text = "Results for term: " + SearchBar.Text;
			
			DispatchSearch(null);
		
		}
	}
	
	public void CurrentSceneButtonPressed()
	{
		Node root = EditorInterface.Singleton.GetEditedSceneRoot();
		if(root == this)return;
		
		if(root == null)
		{
			StatusLabel1.Text = "Editor scene root is null";
			return;
		}
		
		StatusLabel1.Text = root.SceneFilePath;
		//yield(GetTree(), "idle_frame");
		DispatchSearch(null);
	}
	
	public void DispatchSearch(IComparer<Quest> comparer)
	{
		searchingThread = new Thread(this.ThreadedSearch);
		searchingThread.IsBackground = true;
		searchingThread.Priority = ThreadPriority.BelowNormal;
		searchingThread.Start(new Tuple<Queue<Quest>, IComparer<Quest>>(searchResults, comparer));
	}
	
	public void NewQuestButtonPressed()
	{
		GD.Print("New Quest Button Pressed!");
		//questFolderDir <- name of variable with folder to save to
		FileDialog fd = (FileDialog)GD.Load<PackedScene>("res://addons/inkchangeplugin/manager_nodes/QuestNameDialog.tscn").Instantiate();
		
		fd.FileSelected += NewQuestDialogClosed;
		
		EditorInterface.Singleton.GetEditorMainScreen().AddChild(fd);
		
		fd.Show();
		
		
		
		//fd.GrabFocus();
	}
	
	public void NewQuestDialogClosed(string path)
	{
		try
		{
			Quest q = new Quest();
			ResourceSaver.Save(q, path);
			
			QuestNode qn = new QuestNode();
			qn.Quest = q;
			QuestsView.AddChild(qn);
		}
		catch(Exception ex)
		{
			GD.Print(ex.Message + "\n" + ex.StackTrace);
		}
	}
	
	public async void ThreadedSearch(Object o)
	{
		//questfolderdir
		Tuple<Queue<Quest>, IComparer<Quest>> resultsAndComparer = (Tuple<Queue<Quest>, IComparer<Quest>>)o;
		Queue<Quest> results = resultsAndComparer.Item1;
		IComparer<Quest> comparer = resultsAndComparer.Item2;
		
		
		try
		{
			DirAccess questFolder = DirAccess.Open(questFolderDir);
			
			questFolder.ListDirBegin();
			string currFilePath = questFolder.GetNext();
			
			while(currFilePath != "")
			{
				if(questFolder.CurrentIsDir())
				{
					currFilePath = questFolder.GetNext();
					continue;
				}
				
				try
				{
					Quest q = ResourceLoader.Load<Quest>(questFolderDir + "/" + currFilePath);
					
					
					Monitor.Enter(results);
					results.Enqueue(q);
					Monitor.Exit(results);
				}
				catch(Exception ex2)
				{
					GD.Print("exception importing individual quest: " + currFilePath);
					GD.Print(ex2.Message + "\n" + ex2.StackTrace);
				}
				//BasicInkChange ic = new BasicInkChange();
				
				//ic.ValueToSet = (Variant)i;
				
				//q.Changes = new InkChange[]{ic};
				
				
				
				
				currFilePath = questFolder.GetNext();
			}
		}
		catch(Exception ex)
		{
			GD.Print("Exception handled!");
			GD.Print(ex.Message + "\n" + ex.StackTrace);
		}
		
		//make sure this thread is still running so that main thread accepts the last batch
		bool queueEmptied = false;
		while(!queueEmptied)
		{
			Monitor.Enter(results);
			queueEmptied = results.Count == 0;
			Monitor.Exit(results);
		}
	}
}

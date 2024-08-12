using Godot;
using System;
using GodotInk;
using Godot.Collections;

public partial class ShopUI : Panel
{
	Button GoButton;
	ItemList PlayerInventory;
	ItemList ShopInventory;
	ScrollBar DealBar;
	VBoxContainer ReputationEvents;
	Button CloseButton;
	CameraMagnet cm;
	string SaveDataName;
	
	
	
	private static Godot.Collections.Dictionary<string, Variant> itemDatabase;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		GoButton = GetNode<Button>("DealShell/GoButton");
		GoButton.Pressed += OnDealButtonPressed;
		
		DealBar = GetNode<ScrollBar>("DealShell/DealScrollBar");
		DealBar.ValueChanged += OnDealBarValueChanged;
		
		ReputationEvents = GetNode<VBoxContainer>("OverviewShell/ReputationEventsShell/ScrollContainer/ReputationEvents");
		
		PlayerInventory = GetNode<ItemList>("PlayerInventoryShell/ScrollContainer/PlayerInventory");
		ShopInventory = GetNode<ItemList>("ShopInventoryShell/ScrollContainer/ShopInventory");
		PlayerInventory.MultiSelected += (long idx, bool selected) => {OnItemListSelected();};
		ShopInventory.MultiSelected += (long idx, bool selected) => {OnItemListSelected();};
		
		CloseButton = GetNode<Button>("CloseButton");
		CloseButton.Pressed += OnCloseButtonPressed;
		
		if(itemDatabase == null)
		{
			Json j = GD.Load<Json>("res://Global/Databases/itemDatabase.json");
			itemDatabase = new Godot.Collections.Dictionary<string, Variant>(
				(Godot.Collections.Dictionary)j.Data);
		}
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		
	}
	
	public void OnInteraction(InkStory s, string SaveDataName, CameraMagnet cm)
	{
		this.cm = cm;
		this.SaveDataName = SaveDataName;
		
		cm.Active = true;
		
		Dictionary<string, int> inventory = (Dictionary<string, int>)THJGlobals.SaveData[SaveDataName + "_inventory"];
		Dictionary<string, float[]> ratios = (Dictionary<string, float[]>)THJGlobals.SaveData[SaveDataName + "_ratios"];
		
		
		//TODO: initiate PlayerInventory from save data
		Dictionary<string, int> playerInventory = (Dictionary<string, int>)THJGlobals.SaveData["inventory"];
		FillItems(PlayerInventory, playerInventory, new Dictionary<string, float[]>());
		
		
		FillItems(ShopInventory, inventory, ratios);
	}
	
	private void FillItems(ItemList list, Dictionary<string, int> inventory, Dictionary<string, float[]> ratios)
	{
		list.Clear();
		foreach(System.Collections.Generic.KeyValuePair<string, int> pair in inventory)
		{
			int amt = inventory[pair.Key];
			Dictionary<string, Variant> itemData = (Dictionary<string, Variant>)itemDatabase[pair.Key];
			itemData = itemData.Duplicate(true);
			float multiplier = ratios.ContainsKey(pair.Key) ? ratios[pair.Key][3] : 1.0f;
			itemData["cost"] = (int)((int)itemData["cost"] * multiplier);
			itemData["data_name"] = pair.Key;//add this in so it can be fetched later
			
			for(int i = 0; i < amt; i++)
			{
				int idx = list.AddItem((string)itemData["name"]);
				
				list.SetItemMetadata(idx, itemData);
			}
			
		}
	}
	
	public void OnDealButtonPressed()
	{
		int currAmt = THJGlobals.Story.FetchVariable<int>("money");
		int newAmt = currAmt + CalcValue();
		THJGlobals.Story.StoreVariable("money", newAmt);
		
		int[] playerSelected = PlayerInventory.GetSelectedItems();
		int[] shopSelected = ShopInventory.GetSelectedItems();
		
		Dictionary<string, int> shopInventoryData = (Dictionary<string, int>)THJGlobals.SaveData[SaveDataName + "_inventory"];
		
		//TODO: initiate PlayerInventory from save data
		Dictionary<string, int> playerInventoryData = (Dictionary<string, int>)THJGlobals.SaveData["inventory"];
		
		System.Collections.Generic.LinkedList<string> movingItems = new System.Collections.Generic.LinkedList<string>();
		foreach(int index in playerSelected)
		{
			Dictionary<string, Variant> itemData = (Dictionary<string, Variant>)PlayerInventory.GetItemMetadata(index);
			string itemDataName = (string)itemData["data_name"];
			ShopInventory.AddItem(PlayerInventory.GetItemText(index), null, false);
			
			//reflect shift in save data
			playerInventoryData[itemDataName] = playerInventoryData[itemDataName] - 1;
			if(shopInventoryData.ContainsKey(itemDataName))
			{
				shopInventoryData[itemDataName] = shopInventoryData[itemDataName] - 1;
			}
			else shopInventoryData[itemDataName] = 1;
			
			//record any other metadata I need to pass over
		}
		
		foreach(int index in shopSelected)
		{
			Dictionary<string, Variant> itemData = (Dictionary<string, Variant>)ShopInventory.GetItemMetadata(index);
			string itemDataName = (string)itemData["data_name"];
			PlayerInventory.AddItem(ShopInventory.GetItemText(index), null, false);
			
			//reflect shift in save data
			shopInventoryData[itemDataName] = shopInventoryData[itemDataName] - 1;
			if(playerInventoryData.ContainsKey(itemDataName))
			{
				playerInventoryData[itemDataName] = playerInventoryData[itemDataName] + 1;
			}
			else playerInventoryData[itemDataName] = 1;
			//record any other metadata I need to pass over
		}
		if(playerSelected.Length > 0)
		{
			for(int i = playerSelected.Length - 1; i >= 0; i--)
			{
				PlayerInventory.RemoveItem(playerSelected[i]);
			}
			for(int i = ShopInventory.ItemCount-1; i > ShopInventory.ItemCount - playerSelected.Length - 1; i--)
			{
				ShopInventory.SetItemDisabled(i, true);
			}
		}
		if(shopSelected.Length > 0)
		{
			for(int i = shopSelected.Length - 1; i >= 0; i--)
			{
				ShopInventory.RemoveItem(shopSelected[i]);
			}
			for(int i = PlayerInventory.ItemCount-1; i > PlayerInventory.ItemCount - shopSelected.Length - 1; i--)
			{
				PlayerInventory.SetItemDisabled(i, true);
			}
		}
		
	}
	
	public void OnCloseButtonPressed()
	{
		TweeningUIHolder tuih = (TweeningUIHolder)GetTree().GetFirstNodeInGroup("TweeningUIHolder");
		
		tuih.TweenOut();
		
		Node n = GetTree().GetFirstNodeInGroup("Player");
		((LocationPlayer)n).SetMode(THJGlobals.PlayerMode.Moving);
		cm.Active = false;
		
		//force the button to release focus so that it doesn't 
		//register space presses from offscreen
		CloseButton.Hide();
		CloseButton.Show();
		
	}
	
	public void OnItemListSelected()
	{
		int dealAmt = CalcValue();
		
		SetupGoButtonText(dealAmt);
	}
	
	public void OnDealBarValueChanged(double newValue)
	{
		int dealAmt = CalcValue();
		
		SetupGoButtonText(dealAmt);
	}
	
	private int CalcValue()
	{
		//ultimately return an int, but use double for more precise multiplication
		double returner = 0.0;
		//calculate total value of items selected in player inventory
		int[] selected = PlayerInventory.GetSelectedItems();
		foreach(int idx in selected)
		{
			Dictionary<string, Variant> itemData = (Dictionary<string, Variant>)PlayerInventory.GetItemMetadata(idx);
			int baseCost = (int)itemData["cost"];
			returner += baseCost;
		}
		//subtract total value of itmes selected in shop inventory
		selected = ShopInventory.GetSelectedItems();
		foreach(int idx in selected)
		{
			Dictionary<string, Variant> itemData = (Dictionary<string, Variant>)ShopInventory.GetItemMetadata(idx);
			int baseCost = (int)itemData["cost"];
			returner -= baseCost;
		}
		//multiply by current rep
		//TODO: implement entire rep system lol
		
		//multiply by current deal value
		//e.g. 89% comes as -11 in bar
		//keep in mind: "bad deal" means higher value for buying things, lower for selling
		//so we want to increase monetary value if "is selling" and "is taking a good deal"
		//but also increase if "is buying" and "is taking a bad deal"
		if(returner > 0.0 == DealBar.Value > 0.0)
		{
			returner = returner * (DealBar.MaxValue + Math.Abs(DealBar.Value)) / DealBar.MaxValue;
		}
		else
		{
			returner = returner * (DealBar.MaxValue - Math.Abs( DealBar.Value)) / DealBar.MaxValue;
		}
		
		return (int)returner;
	}
	
	private void SetupGoButtonText(int dealAmt)
	{
		int currMoney = CurrentMoney();
		if(currMoney + dealAmt < 0)
		{
			GoButton.Disabled = true;
			GoButton.RemoveThemeColorOverride("font_color");
			GoButton.AddThemeColorOverride("font_color", Godot.Colors.Crimson);
		}
		else
		{
			GoButton.Disabled = false;
			
			//change button text color depending on whether you are making money
			GoButton.RemoveThemeColorOverride("font_color");
			if(dealAmt > 0)
			{
				GoButton.AddThemeColorOverride("font_color", Godot.Colors.LimeGreen);
				GoButton.Text = "+" + dealAmt;
			}
			else
			{
				GoButton.AddThemeColorOverride("font_color", Godot.Colors.White);
				GoButton.Text = dealAmt.ToString();
			}
			
		}
	}
	
	//convenience function
	private int CurrentMoney()
	{
		return THJGlobals.Story.FetchVariable<int>("money");
	}
}


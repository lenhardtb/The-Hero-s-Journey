using Godot;
using System;
using GodotInk;

public partial class BasicNPC : StaticBody3D
{
	[Export]
	public InkStory Story;
	
	// _Ready is called before the first frame update
	public override void _Ready()
	{
		
	}

	public void InteractableHandler()
	{
		GD.Print("BasicNPC interact handler called!");
		//TODO: individualize this? I could use getfirstnodeingroup() for the holder
		// and cast to just call the method instead of hoping TweenIn exists
		MessageBoxUI mb = (MessageBoxUI)(GetTree().GetFirstNodeInGroup("MessageBoxUI"));
		
		//TODO: use better method:
		//iterate backwards along parents until you find its tweening parent and then its tweening holder
		//this change would be less reliant on specific node configurations
		TweeningUIHolder holder = (TweeningUIHolder)(GetTree().GetFirstNodeInGroup("TweeningUIHolder"));
		
		holder.TweenIn((TweeningUI)(mb.GetParent()), true);
		
		Node player = GetTree().GetFirstNodeInGroup("Player");
		((LocationPlayer)player).SetMode(THJGlobals.PlayerMode.Interacting);
		
		mb.OnInteraction(Story);
	}
	
}


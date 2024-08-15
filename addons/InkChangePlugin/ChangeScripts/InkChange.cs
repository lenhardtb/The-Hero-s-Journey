using Godot;
using System;
using GodotInk;
using Godot.Collections;

[GlobalClass]
public partial class InkChange : Resource
{
	//when InkChangeLoader checks this on open, should it stop listening?
	[Export]
	public bool OneAndDone = false;
	
	[Export]
	public Array<InkCondition> Conditions = new Array<InkCondition>();
	
	[Export]
	public Array<InkEffect> Effects = new Array<InkEffect>();
	
	[Export]
	public string ScenePath;
	
	public string[] GetVariables()
	{
		System.Collections.Generic.HashSet<string> vars = 
			new System.Collections.Generic.HashSet<string>(Conditions.Count);
		
		foreach(InkCondition c in Conditions)
		{
			vars.Add(c.Variable);
		}
		
		string[] returner = new string[vars.Count];
		
		vars.CopyTo(returner);
		
		return returner;
	}
	
	public bool AllConditionsMet(InkStory story)
	{
		bool returner = true;
		
		foreach(InkCondition c in Conditions)
		{
			if(!c.HasChangeHappened(story.FetchVariable(c.Variable)))
				returner = false;
		}
		
		return returner;
	}
	
	public void DoChanges(InkStory story, Node basePathNode, bool doQuickly = false)
	{
		foreach(InkEffect e in Effects)
		{
			//e.DoChange(story.FetchVariable(e.Variable), basePathNode, doQuickly);
			e.DoChange(default(int), basePathNode, doQuickly);
			
		}
	}
	
	public void UnDoChanges(InkStory story, Node basePathNode)
	{
		foreach(InkEffect e in Effects)
		{
			//e.UnDoChange(story.FetchVariable(e.Variable), basePathNode);
			e.UnDoChange(default(int), basePathNode);
			
		}
	}
}

using Godot;
using System;
using System.Collections.Generic;

[GlobalClass]
public partial class InkChangeLoader : Node
{
	[Export]
	public InkChange[] InkDependentChanges = new InkChange[0];
	
	Dictionary<InkChange, Callable> registered;
	
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		//GD.Print("InkChangeLoader ready called!");
		registered = new Dictionary<InkChange, Callable>(InkDependentChanges.Length);
		
		this.TreeExited += ClearAllListeners;
		
		if(InkDependentChanges != null)
			AddListeners(InkDependentChanges);
	}
	
	public void AddListeners(ICollection<InkChange> InkDependentChanges)
	{
		foreach(InkChange c in InkDependentChanges)
		{
			foreach(InkCondition cond in c.Conditions)
			{
				int currentValue = (int)THJGlobals.Story.FetchVariable(cond.Variable);
				//TODO: does this still work for, say strings?
				if(currentValue == default(int))//likely not initialized
					THJGlobals.Story.StoreVariable(cond.Variable, default(int));//initialize it now
			}
		}
		
		foreach(InkChange ic in InkDependentChanges)
		{
			string[] vars = ic.GetVariables();
			
			bool ranChange = false;
			if(ic.AllConditionsMet(THJGlobals.Story))
			{
				ic.DoChanges(THJGlobals.Story, THJGlobals.MainGame.CurrentScene, true);
				ranChange = true;
			}
			
			//there's gotta be a cleaner way to express this
			if (!ranChange || !ic.OneAndDone)
			{
				Callable call = Callable.From((string var1, Variant var2) => VariableChanged(ic, var1, var2));
				
				registered.Add(ic, call);
				
				foreach(string inkVar in vars)
				{
					
					THJGlobals.Story.ObserveVariable(inkVar, call);
					
					GD.Print("Successfully observed " + inkVar);
				}
			}
		}
	}
	
	private void VariableChanged(InkChange ic, string varName, Variant newValue)
	{
		GD.Print("MyInkObserver detected change in " + varName + " to " + newValue);
		Variant valueV = (Variant)newValue;
		if(ic.AllConditionsMet(THJGlobals.Story))
		{
			ic.DoChanges(THJGlobals.Story, THJGlobals.MainGame.CurrentScene, false);
			
			//should I check whether dochange changed the value here?
			if(ic.OneAndDone)
			{
				Callable c = registered[ic];
				registered.Remove(ic);
				
				foreach(string variable in ic.GetVariables())
					THJGlobals.Story.RemoveVariableObserver(c, variable);
				
			}
		}
	}
	
	public void ClearAllListeners()
	{
		GodotInk.InkStory s = THJGlobals.Story;
		foreach(KeyValuePair<InkChange, Callable> p in registered)
		{
			foreach(string variable in p.Key.GetVariables())
				s.RemoveVariableObserver(p.Value, variable);
		}
	}
}

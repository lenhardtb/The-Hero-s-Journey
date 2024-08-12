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
		foreach(InkChange c in InkDependentChanges)
		{
			int currentValue = (int)THJGlobals.Story.FetchVariable(c.Condition.Variable);
			if(currentValue == default(int))//likely not initialized
				THJGlobals.Story.StoreVariable(c.Condition.Variable, default(int));//initialize it now
		}
		
		foreach(InkChange c in InkDependentChanges)
		{
			string inkVar = c.Condition.Variable;
			
			if(c.Condition.HasChangeHappened(THJGlobals.Story.FetchVariable(inkVar)))
			{
				c.Effect.DoChange(THJGlobals.Story.FetchVariable(inkVar), this, true);
			}
			
			if(!c.Condition.HasChangeFinished(THJGlobals.Story.FetchVariable(inkVar)))
			{
				MyInkObserver newObs = new MyInkObserver(THJGlobals.Story, registered, this, c);
				
				Callable call = Callable.From((string var1, int var2) => RunChange(newObs, var1, var2));
				
				registered.Add(c, call);
				
				THJGlobals.Story.ObserveVariable(inkVar, call);
				
				//GD.Print("Successfully observed " + inkVar);
			}
			
		}
		this.TreeExited += ClearAllListeners;
	}
	
	private void RunChange(MyInkObserver mio, string varName, int newValue)
	{
		mio.RunChange(varName, (Variant)newValue);
	}
	
	private void ClearAllListeners()
	{
		GodotInk.InkStory s = THJGlobals.Story;
		foreach(KeyValuePair<InkChange, Callable> p in registered)
		{
			s.RemoveVariableObserver(p.Value, p.Key.Condition.Variable);
		}
	}
	
	//Ink.Runtime.Story.VariableObserver obs = delegate(string variable, object value){};
	
	public partial class MyInkObserver : GodotObject
	{
	 	Dictionary<InkChange, Callable> registered;
		GodotInk.InkStory s;
		Node pathRoot;
		public InkChange i;
		
		public MyInkObserver(GodotInk.InkStory s, Dictionary<InkChange, Callable> registered, Node pathRoot, InkChange i)
		{
			this.s = s;
			this.registered = registered;
			this.pathRoot = pathRoot;
			this.i = i;
		}
		
		public void RunChange(string variable, object value)
		{
			GD.Print("MyInkObserver detected change in " + variable + " to " + value);
			Variant valueV = (Variant)value;
			if(i.Condition.HasChangeHappened(valueV))
			{
				i.Effect.DoChange(valueV, pathRoot);
				
				//should I check whether dochange changed the value here?
				if(i.Condition.HasChangeFinished(valueV))
				{
					Callable c = registered[i];
					s.RemoveVariableObserver(c, variable);
					registered.Remove(i);
				}
			}
		}
	}
}

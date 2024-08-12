using Godot;
using System;

public partial class VariableInkEffect : InkEffect
{
	[Export]
	public string VariableToSet;
	
	[Export]
	public Variant ValueToSet;
	
	//bool represents: should this simply "undo" to whatever it was before DoChange?
	[Export]
	public bool CalcUndo = true;
	
	[Export]
	public Variant UnDoValue;
	
	public override void DoChange(Variant variable, Node basePathNode, bool doEfficient = false)
	{
		if(CalcUndo)
		{
			UnDoValue = THJGlobals.Story.FetchVariable(VariableToSet);
		}
		
		THJGlobals.Story.StoreVariable(VariableToSet, ValueToSet);
	}
	
	public override void UnDoChange(Variant variable, Node basePathNode)
	{
		THJGlobals.Story.StoreVariable(VariableToSet, UnDoValue);
	}
	
}

using Godot;
using System;

public partial class CompositeInkCondition : InkCondition
{
	public InkCondition[] SubConditions;
	
	//used in InkChangeLoader.Ready() to determine if DoChange() should be called on scene start
	public override bool HasChangeHappened(Variant variable)
	{
		foreach(InkCondition ic in SubConditions)
		{
			if(!ic.HasChangeHappened(variable))return false;
			
			
		}
		return true;
	}
	
	//used in InkChangeLoader.Ready() to determine if change should be added as a listener
	//may also be used to expedite runtime in DoChange()
	public override bool HasChangeFinished(Variant variable)
	{
		foreach(InkCondition ic in SubConditions)
		{
			if(!ic.HasChangeFinished(variable))return false;
			
			
		}
		return true;
	}
}

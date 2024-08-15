using Godot;
using System;

[GlobalClass]
public abstract partial class InkCondition : Resource
{
	[Export]
	public string Variable;
	
	//HasChangeHappened() evaluates based on this
	[Export]
	public EqualityType Equality = EqualityType.Equals;
	
	//for strings, greater/less than does alphabetic comparison
	public enum EqualityType
	{
		GreaterThan, LessThan, Equals, NotEquals
	}
	
	//TODO: support multiple value types
	[Export]
	public Variant ValueToCompare;
	
	//used in InkChangeLoader.Ready() to determine if DoChange() should be called on scene start
	public virtual bool HasChangeHappened(Variant variable) => false;
	//used in InkChangeLoader.Ready() to determine if change should be added as a listener
	//may also be used to expedite runtime in DoChange()
	public virtual bool HasChangeFinished(Variant variable) => false;
	
	
}

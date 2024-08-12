using Godot;
using System;

[GlobalClass]
public abstract partial class InkEffect : Resource
{
	//returns true if the last possible change has been made and the listener should be removed
	//return value should be equivalent to HasChangeFinished() after running DoChange
	//if doEfficient is true, use as a flag to ignore things like over-time animations or to use time-saving measures
	public abstract void DoChange(Variant variable, Node basePathNode, bool doEfficient = false);
	public abstract void UnDoChange(Variant variable, Node basePathNode);
}

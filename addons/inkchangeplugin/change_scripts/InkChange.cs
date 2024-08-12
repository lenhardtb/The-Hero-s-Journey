using Godot;
using System;

[GlobalClass]
public partial class InkChange : Resource
{
	//when InkChangeLoader checks this on open, should it stop listening?
	[Export]
	public bool OneAndDone = false;
	
	[Export]
	public InkCondition Condition;
	
	[Export]
	public InkEffect Effect;
	
	[Export]
	public string ScenePath;
	
}

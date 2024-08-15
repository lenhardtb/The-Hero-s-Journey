using Godot;
using System;
using Godot.Collections;

[Tool]
[GlobalClass]
public partial class QuestPoint : Resource
{
	[Export]
	public Array<InkChange> MyChanges = new Array<InkChange>();
	
	//edited in QuestPoint GUI
	public string PointName = "New QuestPoint";
	
	//edited in QuestPoint GUI
	public string Description = "New QuestPoint Description";
	
	public QuestPoint PreviousPoint;
	//exported just in case I gotta get nitty-gritty with something the GUI can't do
	[Export]
	public Array<QuestPoint> NextPoint = new Array<QuestPoint>();
	
}

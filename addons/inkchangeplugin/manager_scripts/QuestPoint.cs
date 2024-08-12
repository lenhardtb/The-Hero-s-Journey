using Godot;
using System;
using Godot.Collections;

[Tool]
[GlobalClass]
public partial class QuestPoint : Resource
{
	[Export]
	public Array<InkChange> MyChanges;
	
	[Export]
	public string PointName;
	
	[Export]
	public string Description;
	
	public QuestPoint PreviousPoint;
	public Array<QuestPoint> NextPoint = new Array<QuestPoint>();
	
}

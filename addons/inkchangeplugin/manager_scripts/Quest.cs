using Godot;
using System;
using System.Collections.Generic;
using Godot.Collections;

[Tool]
[GlobalClass]
public partial class Quest : Resource
{
	[Export]
	public QuestPoint StartPoint;
	
	[Export]
	public string QuestName = "ERR: Quest name missing";
	
	[Export]
	public bool InvisibleToPlayer = false;
	
	public int Contains(string inkVar)
	{
		return RecursiveContains(StartPoint, inkVar);
	}
	
	private int RecursiveContains(QuestPoint qp, string inkVar)
	{
		int sumContains = 0;
		
		if(qp.NextPoint.Count > 0)
		{
			foreach(QuestPoint nqp in qp.NextPoint)
			{
				sumContains += RecursiveContains(nqp, inkVar);
			}
		}
		
		sumContains += SingleContains(qp, inkVar);
		
		return sumContains;
	}
	
	private int SingleContains(QuestPoint qp, string inkVar)
	{
		
		return 0;
	}
}

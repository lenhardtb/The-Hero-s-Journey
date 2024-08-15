using Godot;
using System;

[GlobalClass]
public partial class BasicInkCondition : InkCondition
{
	//after the change runs, should we expect to no longer need to listen to the variable?
	[Export]
	public bool Repeats = false;
	
	
	public override bool HasChangeHappened(Variant variable)
	{
		try
		{
			int varI = (int)variable;
			int valI = (int)ValueToCompare;
			
			return MeetsConditions(varI, valI, Equality);
		} catch{}
		
		/*try
		{
			string varS = (string)variable;
			string valS = (string)ValueToCompare;
			
			return MeetsConditions(varS, valS, Equality);
		} catch{}
		
		try
		{
			bool varB = (bool)variable;
			bool valB = (bool)ValueToCompare;
			
			return MeetsConditions(varB, valB, Equality);
		} catch{}*/
		
		//doesn't appear to be an implemented type, or types don't match
		GD.Print("Hit default condition on BasicInkChanger!");
		GD.Print("Values were " + variable + " and " + ValueToCompare);
		return false;
	}
	
	public override bool HasChangeFinished(Variant variable)
	{
		return HasChangeHappened(variable) && Repeats;
	}
	
	
	
	private static bool MeetsConditions(string val1, string val2, EqualityType equality)
	{
		switch(equality)
		{
			case EqualityType.GreaterThan: 
				return String.Compare(val1, val2) > 0;
			case EqualityType.LessThan:
				return String.Compare(val1, val2) < 0;
			case EqualityType.Equals: 
				return val1.Equals(val2);
			case EqualityType.NotEquals:
				return !val1.Equals(val2);
		}
		return false;
	}
	
	private static bool MeetsConditions(int val1, int val2, EqualityType equality)
	{
		switch(equality)
		{
			case EqualityType.GreaterThan: 
				return val1 > val2;
			case EqualityType.LessThan:
				return val1 < val2;
			case EqualityType.Equals: 
				return val1 == val2;
			case EqualityType.NotEquals:
				return val1 != val2;
		}
		return false;
	}
	
	private static bool MeetsConditions(bool val1, bool val2, EqualityType equality)
	{
		switch(equality)
		{
			case EqualityType.GreaterThan: 
				return val1 && !val2;
			case EqualityType.LessThan:
				return !val1 && val2;
			case EqualityType.Equals: 
				return val1 == val2;
			case EqualityType.NotEquals:
				return val1 != val2;
		}
		return false;
	}
	
}

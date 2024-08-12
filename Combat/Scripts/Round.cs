using Godot;
using System;
using System.Collections.Generic;

public partial class Round : Stack<CombatAction>
{
	//which round it is. shouldn't be needed but maybe xp calculations... idk
	public int RoundNumber { get; }
	
	//any units that were defeated in this round
	public List<CombatUnit> Defeated;

	
	public Round(ICollection<CombatUnit> units, int roundNumber) : base(units.Count)
	{
		foreach(CombatUnit u in units)
			Push(u.BasicAttack);

		RoundNumber = roundNumber;
	}
}

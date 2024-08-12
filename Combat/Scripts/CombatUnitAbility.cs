using Godot;
using System;

public class CombatUnitAbility
{
	/*
	Provides alternate CombatTrigger versions for use by users.
	Abilities can be placed into slots by the user and in combat will be added as their slot placement
	*/
	public string Description;
	public Texture Icon;
	
	public CombatTrigger Prologue;
	public CombatTrigger RisingAction;
	public CombatTrigger Climax;
	public CombatTrigger FallingAction;
	public CombatTrigger Epilogue;
	
	
	public static CombatUnitAbility ParseJSon(string json)
	{
		CombatUnitAbility returner = new CombatUnitAbility();
		
		return returner;
	}
}

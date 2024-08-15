using Godot;
using System;

[GlobalClass]
public partial class CombatInkEffect : InkEffect
{
	public static readonly PackedScene  BaseCombatScene = ResourceLoader.Load<PackedScene>("res://Combat/Node Templates/BaseCombat.tscn");
	
	[Export]
	public EncounterData Encounter;
	
	public override void DoChange(Variant variable, Node basePathNode, bool doQuickly = false)
	{
		MessageBoxUI box = GetMessageBox();
		box.OnFinished += OnMessageBoxClosed;
	}
	
	public override void UnDoChange(Variant variable, Node basePathNode)
	{
		MessageBoxUI box = GetMessageBox();
		box.OnFinished -= OnMessageBoxClosed;
	}
	
	private void OnMessageBoxClosed()
	{
		MessageBoxUI box = GetMessageBox();
		box.OnFinished -= OnMessageBoxClosed;
		
		THJGlobals.CurrentEncounter = Encounter;
		THJGlobals.MainGame.SetSceneAsync(BaseCombatScene.ResourcePath);
	}
	
	private static MessageBoxUI GetMessageBox()
	{
		return (MessageBoxUI)THJGlobals.MainGame.GetTree().GetFirstNodeInGroup("MessageBoxUI");
	}
	
}

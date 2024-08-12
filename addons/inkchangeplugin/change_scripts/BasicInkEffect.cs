using Godot;
using System;

[GlobalClass]
public partial class BasicInkEffect : InkEffect
{
	[Export]
	public NodePath ObjectToSet;
	
	[Export]
	public string PropertyToSet;
	
	[Export]
	public Variant ValueToSet;
	
	//for undoing
	private Node replacedNode;
	private Node originalNode;
	private Variant originalValue;
	
	public override void DoChange(Variant variable, Node basePathNode, bool doQuickly = false)
	{
		//string[] splitParam = ParamToSet.Split(':', 2);
		Node n = THJGlobals.MainGame.View.GetChild(0).GetNode(ObjectToSet);
		originalNode = n;
		
		//TODO: deep values e.g. resource parameters?
		if(PropertyToSet != null && PropertyToSet != "")//setting a property
		{
			originalValue = (Variant)n._Get(PropertyToSet);
			n._Set(PropertyToSet, ValueToSet);
		}
		else //replacing that node
		{
			Node newNode = ((PackedScene)ValueToSet).Instantiate();
			replacedNode = newNode;
			n.GetParent().CallDeferred("add_child", newNode);
			if(newNode is Node2D)
			{
				Vector2 formerPos = ((Node2D)n).Position;
				newNode.CallDeferred("_set", "position", formerPos + ((Node2D)newNode).Position);
			}
			else if(newNode is Node3D)
			{
				Vector3 formerPos = ((Node3D)n).Position;
				newNode.CallDeferred("_set", "position", formerPos + ((Node3D)newNode).Position);
			}
			
			n.GetParent().RemoveChild(n);
		}
	}
	
	public override void UnDoChange(Variant variable, Node basePathNode)
	{
		//TODO: deep values e.g. resource parameters?
		if(PropertyToSet != null && PropertyToSet != "")//setting a property
		{
			originalNode._Set(PropertyToSet, ValueToSet);
		}
		else //replacing that node
		{
			Node parent = replacedNode.GetParent();
			parent.CallDeferred("add_child", originalNode);
			if(replacedNode is Node2D)
			{
				Vector2 formerPos = ((Node2D)replacedNode).Position;
				originalNode.CallDeferred("_set", "position", formerPos + ((Node2D)originalNode).Position);
			}
			else if(replacedNode is Node3D)
			{
				Vector3 formerPos = ((Node3D)replacedNode).Position;
				originalNode.CallDeferred("_set", "position", formerPos + ((Node3D)originalNode).Position);
			}
			
			replacedNode.QueueFree();
		}
	}
}

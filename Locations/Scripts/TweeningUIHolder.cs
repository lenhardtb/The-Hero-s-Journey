using Godot;
using System;
using System.Collections.Generic;
using System.Reflection;
/*
Maintains a set of its children, only one of which can be displayed, similarly to a TabContainer.
Instead of tabs, each child must have an input string registered, and it will slide in from offscreen
when its input is called.
*/
public partial class TweeningUIHolder : CanvasLayer
{
	private const float TweenTime = 1.0f;
	
	[Export]
	public int CurrentUI = -1;
	
	private Vector2[] initialPositions;
	private bool isLocked;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		isLocked = false;
		TweeningUI[] tus = GetTweeningChildren();
		initialPositions = new Vector2[tus.Length];
		for(int i = 0; i < tus.Length; i++)
		{
			if(i == CurrentUI) continue;
			
			TweeningUI tu = tus[i];
			
			initialPositions[i] = tu.Position;
			
			tu.Position = GetOffscreenPosition(i);
		}
	}


	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		TweeningUI[] tus = GetTweeningChildren();
		for(int i = 0; i < tus.Length; i++)
		{
			if(tus[i].IsInputMap)
			if(Input.IsActionJustReleased(tus[i].InputTrigger))
			{
				
				if(i == CurrentUI)
					TweenOut();
				else
					TweenIn(i);
				break;
			}
		}
	}
	
	public bool TweenIn(String trigger, bool lockIn = false)
	{
		
		TweeningUI[] tus = GetTweeningChildren();
		for(int i = 0; i < tus.Length; i++)
		{
			if(trigger == tus[i].InputTrigger)
			{
				;
				return TweenIn(i, false);
			}
		}
		return false;
	}
	
	public bool TweenIn(TweeningUI tu, bool lockIn = false)
	{
		TweeningUI[] tus = GetTweeningChildren();
		for(int i = 0; i < tus.Length; i++)
		{
			if(tu == tus[i])
			{
				return TweenIn(i, isLocked);
			}
		}
		return false;
	}
	
	public bool TweenIn(int index, bool lockIn = false)
	{
		if(isLocked)return false;
		
		if(index == CurrentUI)return true;
		
		TweeningUI[] tus = GetTweeningChildren();
		
		Tween t = CreateTween();
		PropertyTweener pt;
		//tween out previous control if one is out
		if(CurrentUI != -1)
		{
			pt = t.TweenProperty(tus[CurrentUI], "position", initialPositions[CurrentUI], TweenTime);
			pt.SetTrans(Enum.Parse<Tween.TransitionType>(tus[index].TransType));
		}
		//tween in selected unit
		pt = t.TweenProperty(tus[index], "position", initialPositions[index], TweenTime);
		pt.SetTrans(Enum.Parse<Tween.TransitionType>(tus[index].TransType));
		CurrentUI = index;
		isLocked = lockIn;
		
		return true;
	}
	
	public void TweenOut()
	{
		if(CurrentUI == -1)return;
		TweeningUI[] tus = GetTweeningChildren();
		Tween t = CreateTween();
		PropertyTweener pt = t.TweenProperty(tus[CurrentUI], "position", 
			GetOffscreenPosition(tus[CurrentUI].Direction), TweenTime);
		pt.SetTrans(Enum.Parse<Tween.TransitionType>(tus[CurrentUI].TransType));
		CurrentUI = -1;
		isLocked = false;
	}
	
	
	private Vector2 GetOffscreenPosition(int index)
	{
		return GetOffscreenPosition(GetTweeningChildren()[index].Direction);
	}
	
	private Vector2 GetOffscreenPosition(String side)
	{
		Vector2 screenDimensions = GetViewport().GetVisibleRect().Size;
		switch(side)
		{
			case "Left":
				return new Vector2(screenDimensions.Y * -1.2f, 0);
			case "Right":
				return new Vector2(screenDimensions.X * 1.2f, 0);
			case "Top":
				return new Vector2(0, screenDimensions.Y * -1.2f);
			case "Bottom":
				return new Vector2(0, screenDimensions.Y * 1.2f);
			default:
				break;
		}
		return new Vector2(0, 25);
	}
	
	public int GetIndexOfTweeningUI(TweeningUI tu)
	{
		return Array.IndexOf(GetTweeningChildren(), tu);
	}
	
	public TweeningUI[] GetTweeningChildren()
	{
		List<TweeningUI> returner = new List<TweeningUI>();
		
		foreach(Node n in GetChildren())
			if(n is TweeningUI tu)
				returner.Add(tu);
		
		return returner.ToArray();
	}
	
	public int GetTweeningChildCount()
	{
		return GetTweeningChildren().Length;
	}
}




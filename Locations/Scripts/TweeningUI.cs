using Godot;
using System;


/*
This class has been abandoned in favor of another solution. 
I am leaving it here in case this solution becomes viable as I learn more about Godot.
*/
public partial class TweeningUI : Panel
{
	
	[Export(PropertyHint.Enum, "Left,Right,Top,Bottom")]
	public String Direction = "Bottom";
	
	[Export]
	public String InputTrigger = "";
	
	[Export(PropertyHint.Enum, "Linear,Sine,Quint,"+
	"Quart,Quad,Expo,Elastic,Cubic,Circ,"+
	"Bounce,Back,Spring")]
	public String TransType = "Elastic";
	
	//true if the inputtrigger is registered on the input map.
	public bool IsInputMap;
	
	public override void _Ready()
	{
		IsInputMap = false;
		IsInputMap = InputMap.GetActions().Contains(InputTrigger);
//		for(int i = 0; i < maps.Length; i++)
//		{
//			if(maps[i].Equals(InputTrigger))IsInputMap = true;
//		}
	}
}

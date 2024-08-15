#if TOOLS
using Godot;
using System;

[Tool]
public partial class InkChangePlugin : EditorPlugin
{	
	InkChangeManager _Dock;
	
	PackedScene questNodePacked;
	
	SceneChangedEventHandler dockListener;
	public override void _EnterTree()
	{
		_Dock = GD.Load<PackedScene>("res://addons/InkChangePlugin/InkChangeManagerDock.tscn").Instantiate<InkChangeManager>();
		dockListener = (Node n) => _Dock.CurrentScene = n;
		this.SceneChanged += dockListener;
		AddControlToBottomPanel(_Dock, "InkChangeManager");
	}

	public override void _ExitTree()
	{
		RemoveControlFromBottomPanel(_Dock);
		// Erase the control from the memory.
		//this.SceneChanged -= dockListener;
		
		_Dock.Free();
	}
	
}
#endif

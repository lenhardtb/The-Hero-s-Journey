using Godot;
using System;

[Tool]
public partial class TileMapDisplay3D : Sprite3D
{
	//TODO: dynamically calculate this const based on tm
	public const int MAP_PIXELS_UNIT = 32;
	[Export]
	public SubViewport sub;
	
	[Export]
	public TileMapLayer[] tm = new TileMapLayer[]{};
	
	[Export]
	public Camera2D cam;
	
	bool isRendering;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		isRendering = true;
		if(sub == null)
		{	
			sub = GetNodeOrNull<SubViewport>("SubViewport");
			if(sub == null) isRendering = false;
		}
		if(cam == null)
		{
			cam = GetNodeOrNull<Camera2D>("SubViewport/Camera2D");
			if(cam == null) isRendering = false;
		}
		if(tm.Length == 0)
		{	
			System.Collections.Generic.List<TileMapLayer> layerHits = new System.Collections.Generic.List<TileMapLayer>(1); 
			
			foreach(Node n in sub.GetChildren())
			{
				//bool isTML = ();
				if(n is TileMapLayer)
					layerHits.Add((TileMapLayer)n);
			}
			
			tm = layerHits.ToArray();
			
			if(tm == null) isRendering = false;
		}
		if(isRendering)
		{
			
			
			//the editor has some weird flaw where subviewports are adjusted 
			//to be viewed with their top corner at 0,0 instead of on the camera view
			//so I just move them into view in the editor and move them back 
			//when actually running the game
			Vector2 calcZero = Vector2.Zero;
			
			if(Engine.IsEditorHint())
			{
				//zero-size and position rectangle
				Rect2I layerRect = new Rect2I();
				foreach(TileMapLayer n in tm)
				{
					Rect2I usedRect = n.GetUsedRect();
					layerRect = layerRect.Merge(usedRect);
					GD.Print("Rect added! noe size " + layerRect.Size);
				}
				
				//subviewport rect has to stay centered on 0,0
				//so we need to make a rect that is just as big in the other direction
				Rect2I invertRect = new Rect2I();
				invertRect.Position = -layerRect.End;
				invertRect.End = -layerRect.Position;
				invertRect = invertRect.Abs();
				
				Rect2I merged = layerRect.Merge(invertRect);
				sub.Size = layerRect.Merge(invertRect).Size * MAP_PIXELS_UNIT;
				
				//adjust maps to be in view
				calcZero = merged.Size / 2 * MAP_PIXELS_UNIT;
				
			}
			else
			{
				//TODO: set viewport size to main view size
				Vector2 mainViewSize = GetViewport().GetVisibleRect().Size;
				sub.Size = (Vector2I)(mainViewSize);
				calcZero = Vector2.Zero;
			}
			
			foreach(Node n in sub.GetChildren())
			{
				if(n is Node2D)
					((Node2D)n).Position = calcZero;
			}
		}
		GD.Print("Got to end of TileMap ready()!");
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if(!Engine.IsEditorHint())
		{
			
			
			float zPosition = this.GlobalPosition.Z;
			Vector3 cameraPosition = GetViewport().GetCamera3D().GlobalPosition;
			this.GlobalPosition = new Vector3(cameraPosition.X, cameraPosition.Y, zPosition);
			Vector2 subCameraPosition = chopZ(cameraPosition) * MAP_PIXELS_UNIT;
			subCameraPosition = new Vector2(subCameraPosition.X, -subCameraPosition.Y);
			cam.Position = subCameraPosition;
		}
	}
	
	
	public TileData GetCellTileData(Vector2I cellCoord, int layer = 0)
	{
		return tm[layer].GetCellTileData(cellCoord);
	}
	
	public Vector3 GetGlobalPosCoord(Vector2I cellCoord, int layer = 0)
	{
		Vector2 calcPos = tm[layer].MapToLocal(cellCoord) + tm[layer].Position;
		calcPos = calcPos * MAP_PIXELS_UNIT * PixelSize;
		
		//2d uses opposite Y axis from 3D, also use this map's Z 
		return new Vector3(calcPos.X, -calcPos.Y, this.Position.Z);
	}
	
	public Vector2I GetCoordGlobalPos(Vector3 globalPos, int layer = 0)
	{
		Vector2 calcPoint = chopZ(globalPos);
		
		//convert to meters/units/tiles 
		//dividing these consts is approx. 4000 in the usual THJ case
		calcPoint = calcPoint / MAP_PIXELS_UNIT / PixelSize;
		
		//truncate values TODO: is this the best way? should I round?
		//positioning uses opposite up-down polarity from tile indeces and screen position
		Vector2I cellCoord = new Vector2I((int)(calcPoint.X), (int)(-calcPoint.Y));
		
		Vector2 localPos = chopZ(ToLocal(globalPos));
		
		//3D positioning uses opposite up-down polarity from tile indeces and screen position
		localPos = new Vector2(localPos.X, -localPos.Y) / PixelSize;
		localPos = localPos - tm[layer].Position - cam.Position;
		
		
		cellCoord = tm[layer].LocalToMap(localPos);
		
		return cellCoord;
	}
	
	public TileData GetCellTileDataGlobalPos(Vector2 globalPos, int layer = 0) 
	{
		return GetCellTileDataGlobalPos(addZ(globalPos), layer);
	}
	
	public TileData GetCellTileDataGlobalPos(Vector3 globalPos, int layer = 0)
	{
		Vector2I cellCoord = GetCoordGlobalPos(globalPos);
		
		return GetCellTileData(cellCoord, layer);
	}
	
	public TileData GetCellTileDataMousePos(int layer = 0)
	{
		Viewport view = this.GetViewport();
		
		//use current camera position
		Vector2 calcPoint = chopZ(view.GetCamera3D().GlobalPosition + this.GlobalPosition);
		//adjust by mouse position on screen, multiply by PixelSize to convert to meters
		Vector2 mousePoint = (view.GetMousePosition() - view.GetVisibleRect().End/2) * PixelSize;
		//positioning uses opposite up-down polarity from tile indeces and screen position
		mousePoint.Y = -mousePoint.Y;
		
		//mouse position in world is camera global, offset by mouse in screen
		calcPoint = calcPoint + mousePoint;
		
		return GetCellTileDataGlobalPos(calcPoint, layer);
	}
	
	private Vector2 chopZ(Vector3 original) {return new Vector2(original.X, original.Y);}
	private Vector3 addZ(Vector2 original, float z = 0.0f) {return new Vector3(original.X, original.Y, z);}
	 
	
	public override String[] _GetConfigurationWarnings()
	{
		String[] baseWarnings = base._GetConfigurationWarnings();
		
		System.Collections.Generic.List<String> returner;
		if(baseWarnings != null) returner = new System.Collections.Generic.List<String>(baseWarnings);
		else returner = new System.Collections.Generic.List<String>();
		
		if(tm == null)
			returner.Add("No TileMap attached!");
		if(sub == null)
			returner.Add("No Viewport attached!");
		
		return returner.ToArray();
	}
}

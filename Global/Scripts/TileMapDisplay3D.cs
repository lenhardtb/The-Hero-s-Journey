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
	public TileMap tm;
	
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
		if(tm == null)
		{	
			tm = GetNodeOrNull<TileMap>("SubViewport/TileMap");
			if(tm == null) isRendering = false;
		}
		if(isRendering)
		{
			Rect2I tmRect = tm.GetUsedRect();
			sub.Size = tm.GetUsedRect().Size * MAP_PIXELS_UNIT;
			
			//the tilemap does not appear contered in the view when it is not centered around (0,0)
			//we have the correct size to make the sprite, but we need to shift it to actually be inside
			Vector2 uncenteredOffset = new Vector2(tmRect.Size.X, tmRect.Size.Y) / 2.0f + new Vector2(tmRect.Position.X, tmRect.Position.Y);
			
			//this lets the view look normal in the editor
			tm.Position = Engine.IsEditorHint() ? (sub.Size / 2) : new Vector2();
			this.Texture = sub.GetTexture();
			
			tm.Position = tm.Position - uncenteredOffset * MAP_PIXELS_UNIT;
		}
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		
	}
	
	
	public TileData GetCellTileData(Vector2I cellCoord, int layer = 0)
	{
		return tm.GetCellTileData(layer, cellCoord);
	}
	
	public Vector3 GetGlobalPosCoord(Vector2I cellCoord)
	{
		Vector2 calcPos = tm.MapToLocal(cellCoord) + tm.Position;
		calcPos = calcPos * MAP_PIXELS_UNIT * PixelSize;
		
		//2d uses opposite Y axis from 3D, also use this map's Z 
		return new Vector3(calcPos.X, -calcPos.Y, this.Position.Z);
	}
	
	public Vector2I GetCoordGlobalPos(Vector3 globalPos)
	{
		Vector2 calcPoint = new Vector2(globalPos.X, globalPos.Y);
		
		//convert to meters/units/tiles 
		//dividing these consts is approx. 4000 in the usual THJ case
		calcPoint = calcPoint / MAP_PIXELS_UNIT / PixelSize;
		
		//truncate values TODO: is this the best way? should I round?
		//positioning uses opposite up-down polarity from tile indeces and screen position
		Vector2I cellCoord = new Vector2I((int)(calcPoint.X), (int)(-calcPoint.Y));
		
		Vector2 localPos = chopZ(ToLocal(globalPos));
		
		//positioning uses opposite up-down polarity from tile indeces and screen position
		localPos = new Vector2(localPos.X, -localPos.Y) / PixelSize;
		localPos = localPos - tm.Position;
		
		
		cellCoord = tm.LocalToMap(localPos);
		
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

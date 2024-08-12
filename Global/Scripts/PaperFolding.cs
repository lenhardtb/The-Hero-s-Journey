using Godot;
using System;

[Tool]
[GlobalClass]
public partial class PaperFolding : Node3D
{
	[Export]
	/// <summary>
	/// Used in editor to quickly find ideal values using the editor widget. 
	/// </summary>
	public bool SettingNoFold;
	[Export]
	public Transform3D NoFold;
	[Export]
	public bool SettingFullFold;
	[Export]
	public Transform3D FullFold;
	
	[Export]
	public Curve AlteredCurve;
	
	
	[Export(PropertyHint.Range, "0.0,1.0,")]
	public float CurrentFoldAmount
	{
		get => _CurrentFoldAmount;
		set
		{
			if(value > 1.0 || value < 0.0) return;
			_CurrentFoldAmount = value;
			//rotate appropriately
			UpdateRotation();
			//propagate through parent/children
			UpdateFoldAmount(value);
		}
	}
	protected float _CurrentFoldAmount = 0.0f;
	
	
	private bool hasFoldingParent = false;
	private PaperFolding foldingParent;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Transform3D zeroCompare = new Transform3D();
		zeroCompare.Basis.X = Vector3.Zero;
		zeroCompare.Basis.Y = Vector3.Zero;
		zeroCompare.Basis.Z = Vector3.Zero;
		
		if(!NoFold.Basis.GetRotationQuaternion().IsNormalized())
		{
			GD.Print("Unnormalized quaternion found in " + this.Name + " NoFold!");
			if(NoFold == zeroCompare)GD.Print("Quaternion was null!");
			NoFold = Transform3D.Identity;
		}
		if(!FullFold.Basis.GetRotationQuaternion().IsNormalized())
		{
			GD.Print("Unnormalized quaternion found in " + this.Name + " FullFold!");
			FullFold = Transform3D.Identity;
		}
		//this is relevant whether in the editor or not
		//if(NoFold.Equals(default(Basis)))NoFold = Transform3D.Identity;
		//if(FullFold.Equals(default(Basis)))FullFold = Transform3D.Identity;
		
		if(AlteredCurve == null)
		{
			AlteredCurve = new Curve();
			//line from 0 to 1
			AlteredCurve.AddPoint(new Vector2(0.0f,0.0f), 0, 0, Curve.TangentMode.Free, Curve.TangentMode.Free);
			AlteredCurve.AddPoint(new Vector2(1.0f,1.0f), 0, 0, Curve.TangentMode.Linear, Curve.TangentMode.Free);
		}
		else if (AlteredCurve.PointCount < 2)//keep it from crashing without destroying the existing data
		{
			AlteredCurve.AddPoint(new Vector2(0.0f,0.0f), 0, 0, Curve.TangentMode.Free, Curve.TangentMode.Free);
			AlteredCurve.AddPoint(new Vector2(1.0f,1.0f), 0, 0, Curve.TangentMode.Linear, Curve.TangentMode.Free);
		}
		
		UpdateHasFoldingParent();
		
		if(hasFoldingParent)
		{
			UpdateFoldAmount(foldingParent.CurrentFoldAmount, true);
		}
		
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if(Engine.IsEditorHint())
		{
			if(SettingNoFold && (NoFold != Transform))
			{
				NoFold = new Transform3D(Transform.Basis, Transform.Origin);
			}
			if(SettingFullFold && (FullFold != Transform))
			{
				FullFold = new Transform3D(Transform.Basis, Transform.Origin);
			}
		}
	}
	
	public void UpdateRotation()
	{
		//this is ridiculous but I'm fucking sick of these errors
		Transform3D tempNoFold;
		if(!NoFold.Basis.GetRotationQuaternion().IsNormalized())
			tempNoFold = Transform3D.Identity;
		else
			tempNoFold = NoFold;
		Transform3D tempFullFold;
		if(!FullFold.Basis.GetRotationQuaternion().IsNormalized())
			tempFullFold = Transform3D.Identity;
		else
			tempFullFold = FullFold;
		
		Curve tempCurve;
		if(AlteredCurve == null)
		{
			tempCurve = new Curve();
			
			tempCurve.AddPoint(new Vector2(0.0f,0.0f), 0, 0, Curve.TangentMode.Free, Curve.TangentMode.Free);
			tempCurve.AddPoint(new Vector2(1.0f,1.0f), 0, 0, Curve.TangentMode.Linear, Curve.TangentMode.Free);
		}
		else
		tempCurve = AlteredCurve;
		
		this.Transform = tempNoFold.InterpolateWith(tempFullFold, tempCurve.SampleBaked(CurrentFoldAmount));
	}
	
	public void UpdateHasFoldingParent()
	{
		Node parent = GetParent();
		//parent at least exists; if true it might be a foldable
		hasFoldingParent = (parent != null);
		//check if it is actually a foldable
		if(hasFoldingParent) hasFoldingParent = parent is PaperFolding;
		
		if(hasFoldingParent)
			foldingParent = (PaperFolding)parent;
		else
			foldingParent = null;
	}
	
	public void UpdateFoldAmount(float value, bool fromParent = false)
	{
		if(fromParent || !hasFoldingParent)//update, propagate to children
		{
			_CurrentFoldAmount = value;
			UpdateRotation();
			
			Godot.Collections.Array<Node> children = GetChildren();
			foreach(Node c in children)
			{
				if(c is PaperFolding)
				{
					((PaperFolding)c).UpdateFoldAmount(value, true);
				}
			}
		}
		else//propagate up to lowest-level folding parent
		{
			foldingParent.UpdateFoldAmount(value);
			return;
		}
	}
}




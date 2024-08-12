using Godot;
using System;

public partial class MoneyCounter : Control
{
	//I wish I could export this somehow
	//returns amount of time to elapse in seconds
	public float CalcTime(int change)
	{
		return 0.5f * (float)Math.Log(2.0f * (change - 0.45f));
	}
	[Export]
	public Curve TickCurve;
	
	private int PrevAmt;
	public int DisplayAmt
	{
		get;
		private set;
	}
	private int TargetAmt;
	private int TotalFrames;
	private int ElapsedFrames;
	public int Amount
	{
		get => TargetAmt;
		set
		{
			int difference = Math.Abs(value - DisplayAmt);
			PrevAmt = DisplayAmt;
			TargetAmt = value;
			
			TotalFrames = (int)((float)Engine.MaxFps * CalcTime(difference));
			ElapsedFrames = 0;
		}
	}
	
	private Label _MyLabel;
	public Label MyLabel
	{
		get
		{
			if(_MyLabel == null)
			{
				_MyLabel = GetNode<Label>("Label");
			}
			return _MyLabel;
		}
		
		set
		{
			_MyLabel = value;
		}
	}
	
	public string Text
	{
		get => MyLabel.Text;
		set => MyLabel.Text = value;
	}
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		ElapsedFrames = -1;
	}
	
	

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if(ElapsedFrames == -1)
		{
			int amount = THJGlobals.Story.FetchVariable<int>("money");
			PrevAmt = amount;
			DisplayAmt = amount;
			TargetAmt = amount;
			TotalFrames = 1;
			ElapsedFrames = 1;
			
			Text = amount.ToString();
		}
		
		if(ElapsedFrames < TotalFrames)
		{
			float DiffProgress = TickCurve.SampleBaked((float)ElapsedFrames / (float)TotalFrames);
			DisplayAmt = (int)(DiffProgress * (float)(TargetAmt - PrevAmt)) + PrevAmt;
			Text = DisplayAmt.ToString();
			ElapsedFrames++;
		}
	}
	
	
}

using Godot;
using System;

[GlobalClass]
public partial class CombatantAnimationPlayer : AnimationPlayer
{
	public static readonly string[] AnimationNames = new string[]{"Idle", "Attack", "Death"};
	
	[Export]
	public CombatUnit Unit
	{
		get =>_Unit;
		set
		{
			_Unit = value;
			if(value == null)
				GD.Print("Unit is null!");
			else
				this.CreateCombatantAnimations(value);
		}
	}
	
	private CombatUnit _Unit;
	
	
	public override void _Ready()
	{
		base._Ready();
		//this.CurrentAnimationChanged += testAnimationChanged;
	}
	
	public void testAnimationChanged(string animName)
	{
		GD.Print("Animation changed! " + animName);
		
		//Animation a = GetAnimation(animName);
		
		//GD.Print("Num frames: " + a.TrackGetKeyCount(0));
	}
	
	public void Start()
	{
		//this.Play()
		/*
		AnimationTree tree = GetNode<AnimationTree>("AnimationTree");
		AnimationNodeStateMachinePlayback stateMachine = (AnimationNodeStateMachinePlayback)tree.Get("parameters/playback");
		stateMachine.Start("Start");*/
	}
	
	
	// Library Loaded / State Edit Version
	private void CreateCombatantAnimations(CombatUnit c)
	{
		//"res://Combat/Sprites/Satyr/Satyr.tres"
		string filename = "res://Combat/Sprites/" 
			+ c.Filename + "/" + c.Filename + ".tres";
		if(ResourceLoader.Exists(filename))
		{
			AnimationLibrary newLib = ResourceLoader.Load<AnimationLibrary>(filename);
			
			if(HasAnimationLibrary((StringName)c.Filename))
			{
				GD.Print("Animation Library already exists and error prevented: " + c.Filename);
			}
			else
				AddAnimationLibrary(c.Filename, newLib);
		
		}
		
		AnimationTree tree = GetNode<AnimationTree>("AnimationTree");
		tree.TreeRoot = CreateStateMachine(c.Filename);
	}

	private static void PrintNodeNames(AnimationNodeStateMachine ansm, string filename)
	{
		int NumTransitions = ansm.GetTransitionCount();
		System.Collections.Generic.HashSet<string> nodeNames = new System.Collections.Generic.HashSet<string>(NumTransitions);
		
		for(int i = 0; i < NumTransitions; i++)
		{
			nodeNames.Add(ansm.GetTransitionFrom(i));
			nodeNames.Add(ansm.GetTransitionTo(i));
		}
		
		
		
		
		System.Collections.Generic.HashSet<string> animNames = new System.Collections.Generic.HashSet<string>(NumTransitions);
		foreach(string s in AnimationNames)
		{
			AnimationNodeAnimation ana = (AnimationNodeAnimation)ansm.GetNode(s);
			animNames.Add(ana.Animation);
		}
		
		
	}
	
	private AnimationNodeStateMachine CreateStateMachine(string filename)
	{
		AnimationNodeStateMachine ansm = new AnimationNodeStateMachine();
		
		AnimationNodeAnimation node;
		foreach(string animName in AnimationNames)
		{
			node = new AnimationNodeAnimation();
			node.Animation = filename + "/" + animName;
			ansm.AddNode(animName, node);
		}
		
		//start to idle
		AnimationNodeStateMachineTransition ansmt = new AnimationNodeStateMachineTransition();
		ansm.AddTransition("Start", AnimationNames[0], ansmt);
		
		//idle to attack
		ansmt = new AnimationNodeStateMachineTransition();
		ansmt.AdvanceCondition = "attack";
		ansmt.AdvanceMode = AnimationNodeStateMachineTransition.AdvanceModeEnum.Auto;
		ansm.AddTransition(AnimationNames[0], AnimationNames[1], ansmt);
		
		//attack to idle
		ansmt = new AnimationNodeStateMachineTransition();
		ansmt.AdvanceCondition = "idle";
		ansmt.SwitchMode = AnimationNodeStateMachineTransition.SwitchModeEnum.AtEnd;//at end
		ansmt.AdvanceMode = AnimationNodeStateMachineTransition.AdvanceModeEnum.Auto;
		ansm.AddTransition(AnimationNames[1], AnimationNames[0], ansmt);
		
		//idle to death
		ansmt = new AnimationNodeStateMachineTransition();
		ansmt.AdvanceCondition = "death";
		ansmt.AdvanceMode = AnimationNodeStateMachineTransition.AdvanceModeEnum.Auto;
		ansm.AddTransition(AnimationNames[0], AnimationNames[2], ansmt);
		
		//attack to death
		ansmt = new AnimationNodeStateMachineTransition();
		ansmt.AdvanceCondition = "death";
		ansmt.SwitchMode = AnimationNodeStateMachineTransition.SwitchModeEnum.AtEnd;//at end
		ansmt.AdvanceMode = AnimationNodeStateMachineTransition.AdvanceModeEnum.Auto;
		ansm.AddTransition(AnimationNames[1], AnimationNames[2], ansmt);
		
		//death to end
		ansmt = new AnimationNodeStateMachineTransition();
		ansm.AddTransition(AnimationNames[2], "End", ansmt);
		
		return ansm;
	}
	
	/* Edit Version
	private void CreateCombatantAnimations(CombatUnit c)
	{
		GD.Print("CreateCombatAnimations called for unit " + c.Name);
		foreach(StringName sn in AnimationNames)
			EditAnimation(GetAnimation("Combat/" + sn), sn, c.Filename);
		
		//Play(c.Filename + "/" + startAnim);//AnimationNames[1]);
	}
	
	private void EditAnimation(Animation a, string animationName, string imageFilename)
	{
		int trackIndex = 0;
		int originalKeyCount = a.TrackGetKeyCount(trackIndex);
		//a.TrackSetPath(trackIndex, "../:ShaderTexture");
		//a.ValueTrackSetUpdateMode(trackIndex, Animation.UpdateMode.Discrete);
		//a.LoopMode = Animation.LoopModeEnum.Linear;
		//a.TrackSetEnabled(trackIndex, true);
		//add each image found as a keyframe
		Texture2D t;//memory reserved outside loop for efficiency
		int frameIndex = 0;
		float framerate = 0.1f;
		bool frameFound = true;
		//TODO: account for extra keys in original animation
		while(frameFound)
		{
			//"res://Combat/Sprites/Satyr/SatyrIdle1.png"
			
			string filepath = "res://Combat/Sprites/" 
					+ imageFilename + "/" + imageFilename + animationName + (frameIndex + 1) + ".png";
			
			
			frameFound = ResourceLoader.Exists(filepath);
			
			if(frameFound)
			{
				t = ResourceLoader.Load<Texture2D>(filepath);
				GD.Print("Successfully found " + filepath);
			
				if(frameIndex >= originalKeyCount)
					a.TrackInsertKey(trackIndex, frameIndex * framerate, t);
				else
					a.TrackSetKeyValue(trackIndex, frameIndex, t);
				
				//GD.Print("Sucessfully created animation for " +
				//"res://Combat/Sprites/" 
					//+ imageFilename + "/" + imageFilename + animationName + (frameIndex + 1) + ".png");
				frameIndex++;
			}
			else
			{
				//GD.Print("Failed to find a combat animation frame at " + filepath);
			}
		}
		a.Length = frameIndex * framerate;
		//GD.Print(imageFilename + "/" + animationName + " has " + frameIndex + " frames, " + a.TrackGetKeyCount(trackIndex) + " according to itself.");
		//return a;
	}
	*/


	/* Wholesale Create Version
	private void CreateCombatantAnimations(CombatUnit c)
	{
		AnimationLibrary newLib = new AnimationLibrary();
		
		foreach(string animName in AnimationNames)
		{
			newLib.AddAnimation(animName, CreateAnimation(animName, c.Filename));
		}
		
		if(!HasAnimationLibrary((StringName)c.Filename));
			AddAnimationLibrary(c.Filename, newLib);
		
		
		AnimationTree tree = GetNode<AnimationTree>("AnimationTree");
		AnimationNodeStateMachine ansm = new AnimationNodeStateMachine();
		tree.TreeRoot = ansm;
		
		string startAnim = CreateStates(ansm, c.Filename);
		
		Play(c.Filename + "/" + startAnim);//AnimationNames[1]);
	}
	
	private Animation CreateAnimation(string animationName, string imageFilename)
	{
		Animation a = new Animation();
		int trackIndex = a.AddTrack(Animation.TrackType.Value);
		
		
		a.TrackSetPath(trackIndex, "../:ShaderTexture");
		a.ValueTrackSetUpdateMode(trackIndex, Animation.UpdateMode.Discrete);
		a.LoopMode = Animation.LoopModeEnum.Linear;
		a.TrackSetEnabled(trackIndex, true);
		//add each image found as a keyframe
		Texture2D t;//memory reserved outside loop for efficiency
		int frameIndex = 0;
		float framerate = 0.1f;
		bool frameFound = true;
		while(frameFound)
		{
			//"res://Combat/Sprites/Satyr/SatyrIdle1.png"
			
			string filepath = "res://Combat/Sprites/" 
					+ imageFilename + "/" + imageFilename + animationName + (frameIndex + 1) + ".png";
			
			frameFound = ResourceLoader.Exists(filepath);
			
			if(frameFound)
			{
				t = ResourceLoader.Load<Texture2D>(filepath);
			
				a.TrackInsertKey(trackIndex, frameIndex * framerate, t);
				
				GD.Print("Key added to track " + trackIndex + " at time " + frameIndex * framerate + " using image " + t.ResourcePath);
				//GD.Print("Sucessfully created animation for " +
				//"res://Combat/Sprites/" 
					//+ imageFilename + "/" + imageFilename + animationName + (frameIndex + 1) + ".png");
				frameIndex++;
			}
			else
			{
				//GD.Print("Failed to find a combat animation frame at " + filepath);
			}
		}
		a.Length = frameIndex * framerate;
		GD.Print("Length is " + frameIndex * framerate + " and has " + a.TrackGetKeyCount(trackIndex) + " frames.");
		//GD.Print(imageFilename + "/" + animationName + " has " + frameIndex + " frames, " + a.TrackGetKeyCount(trackIndex) + " according to itself.");
		return a;
	}
	


	//result is the track that should autoplay
	private string CreateStates(AnimationNodeStateMachine ansm, string filename)
	{
		AnimationNodeAnimation node;
		foreach(string animName in AnimationNames)
		{
			node = new AnimationNodeAnimation();
			node.Animation = animName;
			ansm.AddNode(animName, node);
		}
		
		//start to idle
		AnimationNodeStateMachineTransition ansmt = new AnimationNodeStateMachineTransition();
		ansm.AddTransition("Start", AnimationNames[0], ansmt);
		
		//idle to attack
		ansmt = new AnimationNodeStateMachineTransition();
		ansmt.AdvanceCondition = "attack";
		ansm.AddTransition(AnimationNames[0], AnimationNames[1], ansmt);
		
		//attack to idle
		ansmt = new AnimationNodeStateMachineTransition();
		ansmt.AdvanceCondition = "idle";
		ansmt.SwitchMode = AnimationNodeStateMachineTransition.SwitchModeEnum.AtEnd;//at end
		ansm.AddTransition(AnimationNames[1], AnimationNames[0], ansmt);
		
		//idle to death
		ansmt = new AnimationNodeStateMachineTransition();
		ansmt.AdvanceCondition = "death";
		ansm.AddTransition(AnimationNames[0], AnimationNames[2], ansmt);
		
		//attack to death
		ansmt = new AnimationNodeStateMachineTransition();
		ansmt.AdvanceCondition = "death";
		ansmt.SwitchMode = AnimationNodeStateMachineTransition.SwitchModeEnum.AtEnd;//at end
		ansm.AddTransition(AnimationNames[1], AnimationNames[2], ansmt);
		
		//death to end
		ansmt = new AnimationNodeStateMachineTransition();
		ansm.AddTransition(AnimationNames[2], "End", ansmt);
		
		return AnimationNames[0];
	}
	*/
}

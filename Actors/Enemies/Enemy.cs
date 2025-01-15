using Godot;
using System;

public partial class Enemy : Actor
{

	[Export] protected int scoreValue = 100;

	[Export] protected PackedScene deathEffect;
	[Export] protected double deathEffectDuration = 0.5;
	[Export] protected AudioStream deathSound;
	[Export] protected float deathSoundVolume = -12;

	public override void _OnDeath(Node source)
    {

		if (source is Sonic) {

			((Sonic)source).currentScore += scoreValue;

		}

		AudioStreamPlayer sound = new();

		GetParent().AddChild(sound);
		sound.Stream = deathSound;
		sound.VolumeDb = deathSoundVolume;
		sound.Play();

		Timer kill = new Timer();

		sound.AddChild(kill);

		kill.WaitTime = 1;

		kill.Timeout += sound.QueueFree;
		kill.Start();

		Node3D deathEff = (Node3D)deathEffect.Instantiate();

		GetParent().AddChild(deathEff);
		deathEff.GlobalPosition = GlobalPosition;

		Timer kill2 = new Timer();

		deathEff.AddChild(kill2);

		kill2.WaitTime = deathEffectDuration;

		kill2.Timeout += deathEff.QueueFree;
		kill2.Start();


        base._OnDeath(source);
    
    }

}

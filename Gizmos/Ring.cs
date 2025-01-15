using Godot;
using System;
using System.Collections.ObjectModel;

public partial class Ring : Area3D
{

	[Export] private AudioStream ringSound = null;

	[Export] private float soundVolume = 0.0f;
	
	[Export] private PackedScene collectEffect;

	[Export] private double collectEffectDuration = 0.5;

	public void OnCollect(Node3D b) {

		((Sonic)b).currentRings += 1;
		((Sonic)b).currentScore += 10;

		AudioStreamPlayer sound = new();

		GetParent().AddChild(sound);
		sound.Stream = ringSound;
		sound.VolumeDb = soundVolume;
		sound.Play();

		Timer kill = new Timer();

		sound.AddChild(kill);

		kill.WaitTime = 1;

		kill.Timeout += sound.QueueFree;
		kill.Start();

		GpuParticles3D effect = collectEffect.Instantiate<GpuParticles3D>();

		effect.Emitting = true;

		GetParent().AddChild(effect);

		effect.GlobalPosition = GlobalPosition;

		Timer kill2 = new Timer();

		effect.AddChild(kill2);

		kill2.WaitTime = collectEffectDuration;

		kill2.Timeout += effect.QueueFree;
		kill2.Start();

		QueueFree();

	}

}

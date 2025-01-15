using Godot;
using System;

public partial class Projectile : Area3D
{

	[Export] private float damage = 1f;
	[Export] private float lifetime = 5f;
	public Vector3 Velocity = Vector3.Zero;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{

		BodyEntered += OnBodyEntered;

		Timer t = new Timer();
		AddChild(t);
		t.WaitTime = lifetime;
		t.Timeout += QueueFree;
		t.Start();

	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

    public override void _PhysicsProcess(double delta)
    {
   
        base._PhysicsProcess(delta);

		GlobalPosition += Velocity * (float)delta;

    }
	
	public void OnBodyEntered(Node3D body)
	{ 

		if (body is Actor) {

			((Actor)body)._OnHit(damage, GlobalPosition.DirectionTo(body.GlobalPosition), this);

		}

		QueueFree();

	}

}

using Godot;
using System;
using System.Collections.Generic;

public partial class Trail3d : MeshInstance3D
{

	private Godot.Collections.Array<Godot.Collections.Dictionary<string, Variant>> points;

	[Export] private float distanceBetweenSegments = 0.2f;
	[Export] private float startRadius = 0.5f;
	[Export] private float endRadius = 0f;
	[Export] private int pointsAround = 4;
	[Export] private float pointLifetime = 1f;

	[Export] private Color startColor = Colors.White;
	[Export] private Color endColor = Colors.Transparent;

	private Vector3 previousPosition;

	private float squaredDistanceBetweenSegments;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{

		squaredDistanceBetweenSegments = distanceBetweenSegments * distanceBetweenSegments;
		previousPosition = GlobalPosition;

	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{

		if (previousPosition.DistanceSquaredTo(GlobalPosition) >= squaredDistanceBetweenSegments) {

			AppendPoint();

		}

		Godot.Collections.Array<int> removeQueue = new();

		foreach (var p in points) {

			p["time"] = (float)p["time"] - (float)delta;
			if ((float)p["time"] < 0) {

				removeQueue.Add(points.IndexOf(p));

			}

		}

		for (int i = 0; i < removeQueue.Count; i++) {

			RemovePoint(i);

		}

		_Draw(delta);

	}

	private void _Draw(double delta) {



	}

	private void AppendPoint() {

		Godot.Collections.Dictionary<string, Variant> dict = new()
        {
            { "position", GlobalPosition },
            { "up", GlobalBasis.Y },
            { "axis", GlobalBasis.Z },
            { "life", pointLifetime }
        };

		points.Add(dict);

	}

	private void RemovePoint(int id) {

		points.RemoveAt(id);

	}

}

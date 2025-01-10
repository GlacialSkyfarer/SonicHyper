using Godot;
using System;
using System.Collections.Generic;

[GlobalClass]
public partial class Trail3D : MeshInstance3D
{

	private Godot.Collections.Array<Godot.Collections.Dictionary<string, Variant>> points = new();

	[Export] public bool Enabled = true;

	[Export] private float distanceBetweenSegments = 0.2f;
	[Export] private float startRadius = 0.5f;
	[Export] private float endRadius = 0f;
	[Export] private int pointsAround = 4;
	[Export] private float pointLifetime = 1f;

	[Export] private Color startColor = Colors.White;
	[Export] private Color endColor = Colors.Transparent;

	[Export(PropertyHint.ExpEasing)] float sizeCurve = 1f;

	private Vector3 previousPosition;

	private float squaredDistanceBetweenSegments;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{

		squaredDistanceBetweenSegments = distanceBetweenSegments * distanceBetweenSegments;
		previousPosition = GlobalPosition;

		Mesh = new ImmediateMesh();

	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{

		if (previousPosition.DistanceSquaredTo(GlobalPosition) >= squaredDistanceBetweenSegments && Enabled) {

			AppendPoint();
			previousPosition = GlobalPosition;

		}

		Godot.Collections.Array<int> removeQueue = new();

		foreach (var p in points) {

			p["life"] = (float)p["life"] - (float)delta;
			if ((float)p["life"] < 0) {

				removeQueue.Add(points.IndexOf(p));

			}

		}

		for (int i = 0; i < removeQueue.Count; i++) {

			RemovePoint(i);

		}

		_Draw(delta);

	}

	private void _Draw(double delta) {

		ImmediateMesh mesh = (ImmediateMesh)Mesh;

		mesh.ClearSurfaces();

		if (points.Count < 2 || pointsAround < 2) return;

		mesh.SurfaceBegin(Mesh.PrimitiveType.Triangles);

		for (int i = 0; i < points.Count; i++) {

			Godot.Collections.Array<Vector3> verts = new();
			Godot.Collections.Array<Vector3> verts2 = new();

			Vector3 position = (Vector3)points[i]["position"];
			Vector3 up = (Vector3)points[i]["up"];
			Vector3 axis = (Vector3)points[i]["axis"];
			axis = axis.Normalized();
			float life = (float)points[i]["life"];

			Vector3 position2;
			Vector3 up2;
			Vector3 axis2;
			float life2;

			if (i != points.Count - 1) {

				
				position2 = (Vector3)points[i+1]["position"];
				up2 = (Vector3)points[i+1]["up"];
				axis2 = (Vector3)points[i+1]["axis"];
				axis2 = axis2.Normalized();
				life2 = (float)points[i+1]["life"];


			} else if (Enabled) {

				position2 = GlobalPosition;
				up2 = GlobalBasis.Y;
				axis2 = GlobalBasis.Z.Normalized();
				life2 = (float)points[i]["life"];

			} else {

				mesh.SurfaceEnd();

				return;

			}
			
			float cornerAngle = Mathf.Tau / pointsAround;

			for (int j = 0; j < pointsAround; j++) {

				Vector3 direction = up.Rotated(axis, cornerAngle * j);
				Vector3 point = position + direction * Mathf.Lerp(endRadius, startRadius, Mathf.Pow(life / pointLifetime, sizeCurve));

				verts.Add(ToLocal(point));

				Vector3 direction2 = up2.Rotated(axis2, cornerAngle * j);
				Vector3 point2 = position2 + direction2 * Mathf.Lerp(endRadius, startRadius, Mathf.Pow(life2 / pointLifetime, sizeCurve));

				verts2.Add(ToLocal(point2));

			}

			for (int k = 0; k < verts.Count; k++) {

				Color color = endColor.Lerp(startColor, life / pointLifetime);
				Color color2 = endColor.Lerp(startColor, life2 / pointLifetime);

				int kw = Mathf.PosMod(k + 1, verts.Count);

				//Left Side
				mesh.SurfaceSetColor(color);
				mesh.SurfaceAddVertex(verts[kw]);
				mesh.SurfaceAddVertex(verts[k]);
				mesh.SurfaceSetColor(color2);
				mesh.SurfaceAddVertex(verts2[k]);
				//Right Side
				mesh.SurfaceAddVertex(verts2[k]);
				mesh.SurfaceAddVertex(verts2[kw]);
				mesh.SurfaceSetColor(color);
				mesh.SurfaceAddVertex(verts[kw]);

			}

		}

		mesh.SurfaceEnd();

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

using Godot;
using System;

public partial class RingTrail : Node3D
{

	string ringPath = "res://Scenes/Gizmos/giz_ring.tscn";

	[Export] public Godot.Collections.Dictionary func_godot_properties = new();
	GameManager gameManager;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{

		gameManager = GetNode<GameManager>("/root/GameManager");

		if (func_godot_properties.ContainsKey("targetname") && (string)func_godot_properties["targetname"] != "") {

			gameManager.SetTargetName((string)func_godot_properties["targetname"], this);

			if ((bool)func_godot_properties["isfirst"]) {

				CallDeferred("StartChain");

			}

		} else {

			PackedScene ringScene = GD.Load<PackedScene>(ringPath);
			Ring r = ringScene.Instantiate<Ring>();
			AddChild(r);
			r.Position = Vector3.Zero;

		}

	}

	private void StartChain() {

		var array = new Godot.Collections.Array<Vector3>(){

			Vector3.Zero

		};

		gameManager.CallTarget((string)func_godot_properties["target"], "Trigger", array, this);

	}

	public void Trigger(Godot.Collections.Array<Vector3> currentNodes, RingTrail origin) {

		if (!(bool)func_godot_properties["isfirst"]) {

			currentNodes.Add(origin.ToLocal(GlobalPosition));
			gameManager.CallTarget((string)func_godot_properties["target"], "Trigger", currentNodes, origin);

		} else {

			Curve3D curve = new Curve3D();
			foreach (Vector3 v in currentNodes) {

				curve.AddPoint(v);

			}

			int rings = (int)func_godot_properties["rings"];
			bool skipFirst = (bool)func_godot_properties["skipfirst"];

			PackedScene ringScene = GD.Load<PackedScene>(ringPath);

			for (int i = 0; i < rings; i++) {

				Vector3 ringPos;
				if (skipFirst) {

					ringPos = curve.SampleBaked(curve.GetBakedLength() * (((float)i + 1) / rings), true);

				} else {

					ringPos = curve.SampleBaked(curve.GetBakedLength() * ((float)i / (rings - 1)), true);

				}
				Ring ring = ringScene.Instantiate<Ring>();
				AddChild(ring);
				ring.Position = ringPos;

			}
		}

	}
}

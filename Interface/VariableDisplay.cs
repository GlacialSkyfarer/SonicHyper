using Godot;
using System;

public partial class VariableDisplay : Label
{

	[Export] NodePath targetPath;
	Node target;
	[Export] string targetVariable = "";
	[Export] string prefix = "";
	[Export] string suffix = "";


	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{

		target = GetNode(targetPath);

	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{

		Text = prefix + target.Get(targetVariable) + suffix;

		if (targetVariable == "currentRings" && (int)target.Get(targetVariable) == 0) {

			Modulate = Colors.Red;

		} else {

			Modulate = Colors.White;

		}

	}
}

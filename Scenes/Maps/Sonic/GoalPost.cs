using Godot;
using System;

public partial class GoalPost : Area3D
{

	[Export] private NodePath labelPath;
	[Export] private NodePath uiPath;
	private ColorRect ui;
	private Label label;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{

		label = GetNode<Label>(labelPath);
		ui = GetNode<ColorRect>(uiPath);
		ui.Visible = false;

		BodyEntered += OnBodyEntered;

	}

	private void OnBodyEntered(Node3D body) {

		if (body is Sonic) {

			Sonic s = (Sonic)body;

			int score = s.currentScore;

			body.QueueFree();

			label.Text = "You Scored: " + score + "!";

			ui.Visible = true;

		}


	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}

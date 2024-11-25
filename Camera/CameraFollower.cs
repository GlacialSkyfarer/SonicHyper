using Godot;
using System;

public partial class CameraFollower : Node3D
{

	[Export] public Godot.Collections.Array<float> zoomSettings = new Godot.Collections.Array<float>() { 4f, 6f, 8f, 2f };

	float cameraDistance = 4f;

	int zoomIndex = 0;

	[Export] public float horSensitivity = 1f;
	[Export] public float verSensitivity = 1f;

	[Export] public float traction = 1f;

	[Export] public NodePath springArmPath;
	public SpringArm3D springArm;

	[Export] public NodePath targetPath;
	public Node3D target;

	float verRotationValue = 0f;
	float horRotationValue = 0f;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{

		base._Ready();
		target = GetNode<Node3D>(targetPath);
		springArm = GetNode<SpringArm3D>(springArmPath);

	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{

		base._Process(delta);

		GlobalPosition = GlobalPosition.Lerp(target.GlobalPosition, 10 * (float)delta);

		if (Input.IsActionJustPressed("toggle_zoom")) {

			zoomIndex = (zoomIndex + 1) % zoomSettings.Count;

		}

		cameraDistance = Mathf.Lerp(cameraDistance, zoomSettings[zoomIndex], 2 * (float)delta);

		springArm.SpringLength = cameraDistance;

		Vector2 rightStick = Input.GetVector("camera_left", "camera_right", "camera_up", "camera_down");

		verRotationValue = Mathf.Lerp(verRotationValue, rightStick.Y, (float)delta * traction);
		horRotationValue = Mathf.Lerp(horRotationValue, rightStick.X, (float)delta * traction);

		GlobalRotation = new Vector3(GlobalRotation.X - ((float)delta * verSensitivity * verRotationValue), GlobalRotation.Y - ((float)delta * horSensitivity * (horRotationValue + Input.GetAxis("move_left", "move_right") * 0.2f)), GlobalRotation.Z);
		RotationDegrees = new Vector3(Mathf.Clamp(RotationDegrees.X, -80, -5), RotationDegrees.Y, RotationDegrees.Z);

	}
}

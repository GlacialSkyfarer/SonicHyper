using Godot;
using System;

public enum SonicPlayerState {

	Idle,
	Running,
	SpinJumping,
	SpinFalling

}

public enum SonicRunState {
	
	Stopped,
	Jogging,
	Running,
	Sprinting,
	Exceeding

}

public partial class Sonic : CharacterBody3D
{

	#region References

	[ExportCategory("References")]

	[Export] private NodePath cameraPath;
	private Camera3D camera;

	#endregion

	#region Variables

	SonicPlayerState currentPlayerState = SonicPlayerState.Idle;
	SonicRunState currentRunState = SonicRunState.Stopped;

	[ExportCategory("Movement")]
	[ExportGroup("Running")] 

	[ExportSubgroup("Speed")]
	[Export] private float startingSpeed = 5f;
	[Export] private float joggingSpeed = 10f;
	[Export] private float runningSpeed = 15f;
	[Export] private float sprintingSpeed = 20f;
	[Export] private float acceleration = 3f;
	[Export] private float deceleration = 6f;
	[ExportSubgroup("Handling")]
	[Export] private float turnSpeed = 10f;
	[Export] private float turnFriction = 2f;
	private float currentSpeed = 0f;

	[ExportGroup("Jumping")]

	[Export] Godot.Collections.Array<SonicPlayerState> jumpableStates = new Godot.Collections.Array<SonicPlayerState> { SonicPlayerState.Idle, SonicPlayerState.Running };
	[Export] private float maxJumpHeight = 4.0f;
	[Export] private float fallGravityMultiplier = 1.5f;

	float jumpBuffer = 0f;
	[Export] float maxJumpBuffer = 0.1f;
	float coyoteTime = 0f;
	[Export] float maxCoyoteTime = 0.1f;

	#endregion

	public override void _Ready() {
		
		base._Ready();

		camera = GetNode<Camera3D>(cameraPath);

	}

    public override void _Process(double delta)
    {

        base._Process(delta);

		jumpBuffer = Mathf.Max(jumpBuffer - (float)delta, 0f);
		coyoteTime = Mathf.Max(coyoteTime - (float)delta, 0f);

		if (IsOnFloor()) {

			coyoteTime = maxCoyoteTime;

		}

		if (Input.IsActionJustPressed("jump")) {

			jumpBuffer = maxJumpBuffer;

		}

    }

    public override void _PhysicsProcess(double delta)
	{
		Vector3 velocity = Velocity;

		velocity += GetGravity() * (float)delta * GetGravityMultiplier() * 0.5f;

		velocity = _StateProcess(velocity, delta);

		Velocity = velocity;
		MoveAndSlide();

		Velocity += GetGravity() * (float)delta * GetGravityMultiplier() * 0.5f;

	}

	private Vector3 _StateProcess(Vector3 velocity, double delta) {

		Vector2 inputVector = Input.GetVector("move_left", "move_right", "move_back", "move_forward");
		Vector3 cameraTransformedInputVector = camera.GlobalTransform.Basis.X * inputVector.X - camera.GlobalTransform.Basis.Z * inputVector.Y;
		Vector3 flatInputVector = new Vector3(cameraTransformedInputVector.X, 0, cameraTransformedInputVector.Z).Normalized();

		if (jumpableStates.Contains(currentPlayerState) && coyoteTime > 0f && jumpBuffer > 0f) {

			currentPlayerState = SonicPlayerState.SpinJumping;
			
			velocity += Vector3.Up * Mathf2.GetJumpForce(GetGravity().Y, maxJumpHeight);

		}

		if (flatInputVector != Vector3.Zero) {

			float angleTo = -flatInputVector.SignedAngleTo(-GlobalBasis.Z, GlobalBasis.Y);

			float turnAmount =	Mathf.Min(turnSpeed * (float)delta, Mathf.Abs(angleTo));

			RotateY(Mathf.Sign(angleTo) * turnAmount);

			currentSpeed -= turnAmount * turnFriction;

		}

		switch (currentPlayerState) {
			
			case SonicPlayerState.Idle:

				if (inputVector != Vector2.Zero) {
					currentPlayerState = SonicPlayerState.Running;
				}

				currentSpeed = Mathf.MoveToward(currentSpeed, 0f, deceleration * (float)delta);

				velocity = -GlobalBasis.Z * currentSpeed;

			break;

			case SonicPlayerState.Running:

				if (inputVector == Vector2.Zero) {

					currentPlayerState = SonicPlayerState.Idle;
					currentRunState = SonicRunState.Stopped;

				} else if (currentSpeed <= joggingSpeed) {
					
					currentRunState = SonicRunState.Jogging;

				} else if (currentSpeed <= runningSpeed) {

					currentRunState = SonicRunState.Running;

				} else if (currentSpeed <= sprintingSpeed) {

					currentRunState = SonicRunState.Sprinting;

				} else {

					currentRunState = SonicRunState.Exceeding;

				}

				currentSpeed = Mathf.MoveToward(currentSpeed, runningSpeed, acceleration * (float)delta);

				velocity = -GlobalBasis.Z * currentSpeed;

			break;

			case SonicPlayerState.SpinJumping:

				if (IsOnFloor()) {

					currentPlayerState = SonicPlayerState.Idle;

				}

				if (velocity.Y < 0f || !Input.IsActionPressed("jump")) {

					currentPlayerState = SonicPlayerState.SpinFalling;

				}

				velocity = velocity.MoveToward(flatInputVector * runningSpeed, acceleration * (float)delta);

			break;

			case SonicPlayerState.SpinFalling:

				if (IsOnFloor()) {

					currentPlayerState = SonicPlayerState.Idle;

				}

				velocity = velocity.MoveToward(flatInputVector * runningSpeed, acceleration * (float)delta);

			break;

		}

		return velocity;

	}

	private float GetGravityMultiplier() {

		switch (currentPlayerState) {

			case SonicPlayerState.SpinFalling:
				return fallGravityMultiplier;
			default:
				return 1f;

		}

	}

}

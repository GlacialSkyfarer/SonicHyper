using Godot;
using System;
using System.Data;

public enum SonicPlayerState {

	Standing,
	SpinJumping,
	SpinFalling,
	ChargingDash,
	Rolling

}

public partial class Sonic : CharacterBody3D
{

	#region References

	[ExportCategory("References")]	

	[Export] private NodePath cameraPath;
	private Camera3D camera;
	[Export] private NodePath animatorPath;
	private AnimationPlayer animator;
	[Export] private NodePath standingColliderPath;
	private CollisionShape3D standingCollider;
	[Export] private NodePath ballColliderPath;
	private CollisionShape3D ballCollider;
	[Export] private NodePath standingModelPath;
	private Node3D standingModel;
	[Export] private NodePath ballModelPath;
	private Node3D ballModel;
	[Export] private NodePath floorCastPath;
	private RayCast3D floorCast;

	#endregion

	#region Variables

	private SonicPlayerState currentPlayerState = SonicPlayerState.Standing;

	[ExportCategory("Movement")]
	[ExportGroup("Running")] 

	[ExportSubgroup("Speed")]
	[Export] private float startingSpeed = 5f;
	[Export] private float joggingSpeed = 10f;
	[Export] private float runningSpeed = 15f;
	[Export] private float sprintingSpeed = 20f;
	[Export] private float maximumSpeed = 50f;
	[Export] private float acceleration = 3f;
	[Export] private float deceleration = 6f;
	[Export] private float airAcceleration = 3f;
	[Export] private float airDeceleration = 6f;
	[Export] private float slopeAccel = 5f;
	[Export] private float slopeDecel = 3f;
	[Export] private float slopeSlideAngle = 22.5f;
	[ExportSubgroup("Handling")]
	Vector3 floorNormal = Vector3.Up;
	[Export] private float turnSpeed = 10f;
	[Export] private float turnFriction = 2f;
	private Vector3 currentDirection = Vector3.Forward;
	private float currentSpeed = 0f;

	[ExportGroup("Jumping")]

	[Export] private Godot.Collections.Array<SonicPlayerState> jumpableStates = new Godot.Collections.Array<SonicPlayerState> { SonicPlayerState.Standing };
	[Export] private Godot.Collections.Array<SonicPlayerState> spinDashableStates = new Godot.Collections.Array<SonicPlayerState> {SonicPlayerState.Standing };
	[Export] private float maxJumpHeight = 4.0f;
	[Export] private float fallGravityMultiplier = 1.5f;

	private float currentJumpBuffer = 0f;
	[Export] float maxJumpBuffer = 0.1f;
	private float currentCoyoteTime = 0f;
	[Export] private float maxCoyoteTime = 0.1f;

	private bool jumpLock = false;

	[ExportGroup("Spindash")]

	[Export] private float minimumDashSpeed = 10f;
	[Export] private float maximumDashSpeed = 35f;
	[Export] private float dashChargeTime = 0.5f;
	[Export] private float rollDeceleration = 3f;
	private float currentDashChargeTime = 0f;


	[ExportCategory("Animation")]
	[Export] private Godot.Collections.Dictionary<string,string> animations;

	#endregion

	public override void _Ready() {
		
		base._Ready();

		camera = GetNode<Camera3D>(cameraPath);
		animator = GetNode<AnimationPlayer>(animatorPath);
		standingCollider = GetNode<CollisionShape3D>(standingColliderPath);
		ballCollider = GetNode<CollisionShape3D>(ballColliderPath);
		standingModel = GetNode<Node3D>(standingModelPath);
		ballModel = GetNode<Node3D>(ballModelPath);
		floorCast = GetNode<RayCast3D>(floorCastPath);

	}

    public override void _Process(double delta)
    {

        base._Process(delta);

		currentJumpBuffer = Mathf.Max(currentJumpBuffer - (float)delta, 0f);
		currentCoyoteTime = Mathf.Max(currentCoyoteTime - (float)delta, 0f);
		currentDashChargeTime = Mathf.Min(currentDashChargeTime + (float)delta, dashChargeTime);

		if (IsOnFloor()) {

			currentCoyoteTime = maxCoyoteTime;

		}

		if (Input.IsActionJustPressed("jump")) {

			currentJumpBuffer = maxJumpBuffer;

		}

		_AnimationProcess(delta);

    }

    public override void _PhysicsProcess(double delta)
	{

		Vector3 velocity = Velocity;

		if (IsOnFloor()) {

			velocity = VelocityToLocal(velocity);

		}

		Vector3 gravity = GetGravity();

		velocity += gravity * (float)delta * GetGravityMultiplier() * 0.5f;

		RotateOnSlope();
	
		UpDirection = GlobalBasis.Y;

		velocity = _StateProcess(velocity, gravity, delta);

		if (IsOnFloor()) {
			
			Velocity = VelocityToGlobal(velocity);

		} else {

			Velocity = velocity;

		}

		MoveAndSlide();

		velocity = Velocity;

		if (IsOnFloor()) {

			velocity = VelocityToLocal(velocity);

		}

		velocity += gravity * (float)delta * GetGravityMultiplier() * 0.5f;

		if (IsOnFloor()) {
			
			Velocity = VelocityToGlobal(velocity);

		} else {

			Velocity = velocity;

		}

		currentSpeed = Mathf.Clamp(currentSpeed, 0, Velocity.Length());

	}

	private Vector3 _StateProcess(Vector3 velocity, Vector3 gravity, double delta) {
		
		Vector2 inputVector = Input.GetVector("move_left", "move_right", "move_back", "move_forward");
		Vector3 cameraTransformedInputVector = camera.GlobalBasis.X * inputVector.X - camera.GlobalBasis.Z * inputVector.Y;
		cameraTransformedInputVector = new Vector3(cameraTransformedInputVector.X, 0, cameraTransformedInputVector.Z).Normalized();

		if (cameraTransformedInputVector != Vector3.Zero) {

			float angleTo = -cameraTransformedInputVector.SignedAngleTo(currentDirection, Vector3.Up);

			float turnAmount =	Mathf.Min(turnSpeed * (float)delta, Mathf.Abs(angleTo));

			currentDirection = currentDirection.Rotated(Vector3.Up, Mathf.Sign(angleTo) * turnAmount);

			currentSpeed -= turnAmount * turnFriction;

		}

		standingModel.LookAt(standingModel.GlobalPosition - (currentDirection.X * GlobalBasis.X) - (currentDirection.Z * GlobalBasis.Z), GlobalBasis.Y);
		ballModel.LookAt(ballModel.GlobalPosition - (currentDirection.X * GlobalBasis.X) - (currentDirection.Z * GlobalBasis.Z), GlobalBasis.Y);

		currentSpeed = Mathf.Clamp(currentSpeed, 0, maximumSpeed);

		if (jumpableStates.Contains(currentPlayerState) && currentCoyoteTime > 0f && currentJumpBuffer > 0f) {

			currentPlayerState = SonicPlayerState.SpinJumping;

			currentJumpBuffer = 0f;
			currentCoyoteTime = 0f;

			jumpLock = true;

			velocity += Vector3.Up * Mathf2.GetJumpForce(gravity.Y, maxJumpHeight);

		}

		if (spinDashableStates.Contains(currentPlayerState) && Input.IsActionJustPressed("action")) {

			currentPlayerState = SonicPlayerState.ChargingDash;
			currentDashChargeTime = 0f;

		}

		float accelMult = (float)delta;
		
		switch (currentPlayerState) {

			case SonicPlayerState.Standing:

				if (!IsOnFloor()) {

					velocity = VelocityToLocal(velocity);
					
					currentPlayerState = SonicPlayerState.SpinFalling;

				}

				if (currentSpeed == 0 && inputVector != Vector2.Zero) {
					
					currentSpeed = startingSpeed;

				}

				currentSpeed = Mathf.MoveToward(currentSpeed, runningSpeed * inputVector.Length(), (currentSpeed > runningSpeed ? deceleration : acceleration) * accelMult);

				velocity = GetRunningVelocity(velocity, currentDirection);

			break;

			case SonicPlayerState.SpinJumping:

				if (IsOnFloor() && !jumpLock) {

					currentPlayerState = SonicPlayerState.Standing;

				}

				jumpLock = false;

				if ((velocity * GlobalBasis.Y).Y < 0f  || !Input.IsActionPressed("jump")) {

					currentPlayerState = SonicPlayerState.SpinFalling;

				}

				if (inputVector != Vector2.Zero) {

					currentSpeed = Mathf.MoveToward(currentSpeed, joggingSpeed * inputVector.Length(), (currentSpeed > joggingSpeed ? airDeceleration : airAcceleration) * accelMult);

				} else {

					currentSpeed = Mathf.MoveToward(currentSpeed, 0, airDeceleration * accelMult);

				}

				velocity = GetRunningVelocity(velocity, currentDirection);

			break;

			case SonicPlayerState.SpinFalling:

				if (IsOnFloor()) {

					currentPlayerState = SonicPlayerState.Standing;

				}

				if (inputVector != Vector2.Zero) {

					currentSpeed = Mathf.MoveToward(currentSpeed, joggingSpeed * inputVector.Length(), (currentSpeed > joggingSpeed ? airDeceleration : airAcceleration) * accelMult);

				} else {

					currentSpeed = Mathf.MoveToward(currentSpeed, 0, airDeceleration * accelMult);

				}

				velocity = GetRunningVelocity(velocity, currentDirection);

			break;

			case SonicPlayerState.ChargingDash:

				currentSpeed = Mathf.MoveToward(currentSpeed, 0f, deceleration * (float)delta);

				if (Input.IsActionJustReleased("action")) {

					currentSpeed += Mathf.Remap(currentDashChargeTime, 0, dashChargeTime, minimumDashSpeed, maximumDashSpeed);
					currentDashChargeTime = 0f;
					currentPlayerState = SonicPlayerState.Rolling;

				}

				velocity = GetRunningVelocity(velocity, currentDirection);

			break;

			case SonicPlayerState.Rolling:

				if (!IsOnFloor()) {
					
					velocity = VelocityToLocal(velocity);
					
					currentPlayerState = SonicPlayerState.SpinFalling;

				}

				if (Input.IsActionJustPressed("action") || currentSpeed < startingSpeed) {

					currentPlayerState = SonicPlayerState.Standing;

				}

				currentSpeed = Mathf.MoveToward(currentSpeed, 0f, rollDeceleration * (float)delta);

				velocity = GetRunningVelocity(velocity, currentDirection);

			break;

		}

		return velocity;

	}

	private void _AnimationProcess(double delta) {

		switch (currentPlayerState) {

			case SonicPlayerState.Standing: 

				standingCollider.Disabled = false;
				ballCollider.Disabled = true;
				standingModel.Visible = true;
				ballModel.Visible = false;

				if (currentSpeed == 0) {

					PlayAnimation("Idle");

					animator.SpeedScale = 1;

				} else if (currentSpeed <= joggingSpeed) {

					PlayAnimation("Jog");

					animator.SpeedScale = Mathf.Remap(currentSpeed, 0, joggingSpeed, 0.5f, 2);

				} else if (currentSpeed <= runningSpeed) {

					PlayAnimation("Run");

					animator.SpeedScale = Mathf.Remap(currentSpeed, joggingSpeed, runningSpeed, 1, 2);

				} else if (currentSpeed <= sprintingSpeed) {

					PlayAnimation("Sprint");

					animator.SpeedScale = Mathf.Remap(currentSpeed, runningSpeed, sprintingSpeed, 1, 2);

				} else {

					PlayAnimation("Jet");

					animator.SpeedScale = Mathf.Remap(currentSpeed, sprintingSpeed, 50, 1, 3);

				}

			break;
			case SonicPlayerState.ChargingDash:
			case SonicPlayerState.Rolling:
			case SonicPlayerState.SpinJumping:
			case SonicPlayerState.SpinFalling:

				ballCollider.Disabled = false;
				standingCollider.Disabled = true;
				standingModel.Visible = false;
				ballModel.Visible = true;

			break;

		}

	}

	private void PlayAnimation(string key) {

		if (animations.ContainsKey(key)) {

			animator.Play(animations[key]);

		} else {

			GD.PrintErr("No such animation: '" + key + "'.");

		}

	}

	private float GetGravityMultiplier() {

		switch (currentPlayerState) {

			case SonicPlayerState.SpinFalling:
				return fallGravityMultiplier;
			default:
				return 1f;

		}

	}

	private void RotateOnSlope() {

		Vector3 colNorm = floorCast.GetCollisionNormal();

		if (IsOnFloor() && GlobalBasis.Y != colNorm) {

			GD.Print(colNorm);

			Quaternion targetRotation = new Quaternion(GlobalBasis.Y, colNorm);

			Quaternion *= targetRotation;
		
		} else {

			GlobalBasis = Basis.Identity;

		}

	}
	
	private Vector3 GetRunningVelocity(Vector3 velocity, Vector3 movementDirection) {

		return movementDirection * currentSpeed + Vector3.Up * velocity.Y;

	}

	private Vector3 VelocityToLocal(Vector3 velocity) {

		Quaternion q = new Quaternion(GlobalBasis.Y, Vector3.Up).Normalized();

		return q * velocity;

	}

	private Vector3 VelocityToGlobal(Vector3 velocity) {

		Quaternion q = new Quaternion(Vector3.Up, GlobalBasis.Y).Normalized();

		return q * velocity;

	}

}

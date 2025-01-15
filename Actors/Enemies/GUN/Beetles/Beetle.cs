using Godot;
using Godot.NativeInterop;
using System;
using System.Drawing;
using System.IO;
using System.Linq;

public enum BeetleWeaponType {

    None,
    Gun,
    Bomb

}

public partial class Beetle : Enemy
{

    [Export] private BeetleWeaponType weapon = BeetleWeaponType.None;
    [Export] private PackedScene bullet;
    [Export] private float bulletSpeed = 1f;
    [Export] private PackedScene bomb;
    [Export] private bool followsPath = false;
    [Export] private bool loop = false;
    [Export] private NodePath path3DPath;
    Godot.Collections.Array<Vector3> path = new();
    [Export] private float pointThreshold = 0.5f;
    int currentPoint = 1;
    [Export] private float movementSpeed = 1f;
    [Export] private bool alreadySpawned = true;
    [Export] private float weaponCooldown = 1f;
    float currentWeaponCooldown = 0f;

    bool movingBack = false;
    bool moving = true;

    float squaredPointThreshold;

    [Export] private NodePath lookBoxPath;
    private Area3D lookBox;

    public override void _Ready()
    {
        base._Ready();
        if (followsPath){

            Path3D pat = GetNode<Path3D>(path3DPath);
            Curve3D cur = pat.Curve;
            for (int i = 0; i < cur.PointCount; i++) {

                path.Add(pat.ToGlobal(cur.GetPointPosition(i)));

            }

            GlobalPosition = path[0];

        } 
        lookBox = GetNode<Area3D>(lookBoxPath);
        squaredPointThreshold = pointThreshold * pointThreshold;
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);

        currentWeaponCooldown = Mathf.Max(currentWeaponCooldown - (float)delta, 0);

        Node3D target = GetClosestTarget();

        if (followsPath) {

            Vector3 targetPoint = path[currentPoint];

            Velocity = Velocity.MoveToward(GlobalPosition.DirectionTo(targetPoint) * movementSpeed, (float)delta * movementSpeed * 4);

            if (GlobalPosition.DistanceSquaredTo(targetPoint) <= squaredPointThreshold) {

                if (loop) {

                    currentPoint = Mathf.PosMod(currentPoint + 1, path.Count);

                } else {

                    if (movingBack) {

                        currentPoint = Mathf.Clamp(currentPoint - 1, 0, path.Count - 1);
                        if (currentPoint == 0) movingBack = false;

                    } else {

                        currentPoint = Mathf.Clamp(currentPoint + 1, 0, path.Count - 1);
                        if (currentPoint == path.Count - 1) movingBack = true;

                    }

                }

            }

        }

        if (weapon != BeetleWeaponType.Gun && followsPath) {

            Vector3 direction = GlobalPosition.DirectionTo(path[currentPoint]);
                
            float angleTo = GlobalBasis.Z.SignedAngleTo(direction, Vector3.Up);

            float angle = Mathf.Min(Mathf.Sign(angleTo) * (float)delta * 5, angleTo);
            
            RotateY(angle);

        } else {

            if (target != null) {

                Vector3 direction = GlobalPosition.DirectionTo(new Vector3(target.GlobalPosition.X, GlobalPosition.Y, target.GlobalPosition.Z));
                
                float angleTo = GlobalBasis.Z.SignedAngleTo(direction, Vector3.Up);

                float angle = Mathf.Min(Mathf.Sign(angleTo) * (float)delta * 5, angleTo);
                
                RotateY(angle);

            }

        }

        switch (weapon) {

            case BeetleWeaponType.None:
            break;
            case BeetleWeaponType.Gun:

                if (currentWeaponCooldown <= 0 && target != null) {

                    Projectile p = bullet.Instantiate<Projectile>();
                    GetParent().AddChild(p);
                    p.Velocity = GlobalPosition.DirectionTo(target.GlobalPosition) * bulletSpeed;
                    p.GlobalPosition = GlobalPosition;
                    currentWeaponCooldown = weaponCooldown;

                } else if (target == null) {

                    currentWeaponCooldown = weaponCooldown / 2;

                }

            break;
            case BeetleWeaponType.Bomb:
                throw new NotImplementedException();

        }
        
        MoveAndSlide();

    }

    private Node3D GetClosestTarget() {

        if (lookBox.HasOverlappingBodies()) {

            float distance = 999999;
            Node3D target = null;
            foreach (Node3D n in lookBox.GetOverlappingBodies()) {

                if (GlobalPosition.DistanceSquaredTo(n.GlobalPosition) < distance) {

                    target = n;
                    distance = GlobalPosition.DistanceSquaredTo(n.GlobalPosition);

                }

            }

            return target;

        }

        return null;

    }

    public void OnHurtboxEntered(Node3D body) {

        if (body is Actor) {

            ((Actor)body)._OnHit(1, GlobalPosition.DirectionTo(body.GlobalPosition), this);

        }

    }

}

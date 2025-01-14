using Godot;
using System;
using System.IO;

public enum BeetleWeaponType {

    None,
    Gun,
    Bomb

}

public partial class Beetle : Enemy
{

    [Export] private BeetleWeaponType weapon = BeetleWeaponType.None;
    [Export] private bool followsPath = false;
    [Export] private NodePath path3DPath;
    private Path3D path3D;
    [Export] private bool alreadySpawned = true;
    [Export] private float weaponCooldown = 1f;
    float currentWeaponCooldown = 0f;

    [Export] private NodePath lookBoxPath;
    private Area3D lookBox;

    public override void _Ready()
    {
        base._Ready();
        if (followsPath) path3D = GetNode<Path3D>(path3DPath);
        lookBox = GetNode<Area3D>(lookBoxPath);
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);

        Node3D target = GetClosestTarget();

        if (target != null) {

            Vector3 direction = GlobalPosition.DirectionTo(new Vector3(target.GlobalPosition.X, GlobalPosition.Y, target.GlobalPosition.Z));
            
            Quaternion quat = new(GlobalBasis.Z, direction);
            
            Quaternion = Quaternion.Slerp(Quaternion * quat, 0.2f);

        }

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

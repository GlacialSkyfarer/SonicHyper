using Godot;
using System;

public partial class Actor : CharacterBody3D
{

    #region variables

    public float hp;
    [Export]
    protected float maxHP;

    #endregion

    public override void _Ready()
    {
        base._Ready();
    }

    public virtual void _OnHit(float damage, Vector3 direction, Node source) {

        hp -= damage;
        if (hp <= 0) {
            _OnDeath(source);
        }

    }

    public virtual void _OnDeath(Node source) {
        QueueFree();
    }

}

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

    public virtual void OnHit(float damage, Vector3 direction) {

        hp -= damage;
        if (hp <= 0) {
            OnDeath();
        }

    }

    protected virtual void OnDeath() {
        QueueFree();
    }

}

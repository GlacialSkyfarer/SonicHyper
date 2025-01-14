using Godot;
using System;

public partial class Enemy : Actor
{

	[Export] protected int scoreValue = 100;

	public override void _OnDeath(Node source)
    {

		if (source is Sonic) {

			((Sonic)source).currentScore += scoreValue;

		}

        base._OnDeath(source);
    
    }

}

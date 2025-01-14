using Godot;
using System;
using System.Collections.ObjectModel;

public partial class Ring : Area3D
{
	
	public void OnCollect(Node3D b) {

		((Sonic)b).currentRings += 1;
		((Sonic)b).currentScore += 10;
		QueueFree();

	}

}

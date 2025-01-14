using Godot;
using System;

public partial class VariableDisplay : Label
{

	[Export] NodePath targetPath;
	Node target;
	[Export] string targetVariable = "";
	[Export] string prefix = "";
	[Export] string suffix = "";


	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{

		target = GetNode(targetPath);

	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{

		if (targetVariable == "time_left") {
		
			Text = prefix + TimeString((float)target.Get("wait_time") - (float)target.Get(targetVariable)) + suffix;

			if ((float)target.Get("time_left") < 1) {
				
				Modulate = Colors.Red;

			} else {

				Modulate = Colors.White;

			}

		} else {

			Text = prefix + target.Get(targetVariable) + suffix;

		}

		if (targetVariable == "currentScore") {
		
			float value = (float)target.Get(targetVariable);
			string result = "ERROR";

			switch (value) {

				case < 10:
					result = "0000" + target.Get(targetVariable);
				break;
				case < 100:
					result = "000" + target.Get(targetVariable);
				break;
				case < 1000:
					result = "00" + target.Get(targetVariable);
				break;
				case < 10000:
					result = "0" + target.Get(targetVariable);
				break;
				case < 100000:
					result = (string)target.Get(targetVariable);
				break;
				default:
					result = "R A D I C A L !";
				break;

			}

			Text = prefix + result + suffix;

		}

		if (targetVariable == "currentRings" && (int)target.Get(targetVariable) == 0) {

			Modulate = Colors.Red;

		} else {

			Modulate = Colors.White;

		}

	}

	string TimeString(float time) {

		int minutes = (int)time / 60;
		time -= minutes * 60;
		int seconds = (int)time;
		int miliseconds = (int)((time - (int)time) * 100);
		return (minutes < 10 ? "0" : "") + minutes + ":" + (seconds < 10 ? "0" : "") + seconds + ":" + (miliseconds < 10 ? "0" : "") + miliseconds;

	}

}

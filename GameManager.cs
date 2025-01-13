using Godot;
using System;
using System.Collections.Generic;

public partial class GameManager : Node
{

	public void CallTarget(string target, string method = "Trigger", params Variant[] args) {
		
		Godot.Collections.Array<Node> targets = GetTree().GetNodesInGroup(target);

		foreach (Node t in targets) {

			if (t.HasMethod(method)) {
				t.Call(method, args);
			}

		}

	}
	
	public Godot.Collections.Array<string> GetTargets(Godot.Collections.Dictionary<string, Variant> func_godot_properties) {
		
		Godot.Collections.Array<string> targets = new Godot.Collections.Array<string>();

		foreach (KeyValuePair<string, Variant> kvp in func_godot_properties) {
			
			if (kvp.Key is string) {
				
				if (((string)kvp.Key).StartsWith("target") && !((string)kvp.Key).Equals("targetname")) {

					targets.Add((string)kvp.Value);

				}

			}

		}
		
		return targets;
		
	}

	public void SetTargetName(string targetname, Node node) {
		
		node.AddToGroup(targetname);

	}

	public async void HitStop(double time, float timeScale = 0) {

		Engine.TimeScale = timeScale;
		await ToSignal(GetTree().CreateTimer(time, ignoreTimeScale: true), "timeout");
		Engine.TimeScale = 1;

	}
	
}

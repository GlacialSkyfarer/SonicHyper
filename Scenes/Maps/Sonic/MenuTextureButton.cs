using Godot;
using System;

public partial class MenuTextureButton : TextureButton
{

	[Export] private ButtonType type = ButtonType.Quit;
	[Export] private PackedScene scene;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{

		ButtonUp += OnButtonPressed;

	}

	private void OnButtonPressed() {

		switch (type) {

			case ButtonType.Quit:
				GetTree().Quit();
				break;
			case ButtonType.loadScene:
				GetTree().ChangeSceneToPacked(scene);
				break;

		}
 
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}

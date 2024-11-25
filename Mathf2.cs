using Godot;

public class Mathf2 {

    public static float GetJumpForce(float gravity, float jumpHeight) {

        return Mathf.Sqrt(2 * Mathf.Abs(gravity) * jumpHeight);

    }

    public static Vector3 MaxLength(Vector3 a, Vector3 b) {

		return a.Length() > b.Length() ? a : b;

	}

}
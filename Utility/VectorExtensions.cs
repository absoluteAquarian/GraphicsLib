using Microsoft.Xna.Framework;

namespace GraphicsLib.Utility;

public static class VectorExtensions {
	public static Vector3 AsVector3(this Vector2 vector) => new(vector, 0f);
}

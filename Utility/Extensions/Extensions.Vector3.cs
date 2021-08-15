using Microsoft.Xna.Framework;

namespace GraphicsLib.Utility.Extensions{
	public static partial class Extensions{
		public static Vector2 XY(this Vector3 vector)
			=> new Vector2(vector.X, vector.Y);
	}
}

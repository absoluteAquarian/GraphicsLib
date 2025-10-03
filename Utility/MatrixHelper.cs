using Microsoft.Xna.Framework;

namespace GraphicsLib.Utility;

public static class MatrixHelper {
	/// <inheritdoc cref="Matrix.CreateTranslation(Vector3)"/>
	public static Matrix CreateTranslation(Vector2 position) => Matrix.CreateTranslation(position.AsVector3());
}

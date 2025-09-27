using Microsoft.Xna.Framework;

namespace GraphicsLib.Utility;

/// <summary>
/// A vector that is guaranteed to be normalized (length of 1).
/// </summary>
public readonly struct NormalizedVector3 {
	/// <summary>
	/// The underlying vector.
	/// </summary>
	public Vector3 Vector { get; }

	/// <summary>
	/// Creates a normalized vector from <see cref="Vector3.Up"/>
	/// </summary>
	public NormalizedVector3() {
		Vector = Vector3.Up;
	}

	/// <summary>
	/// Creates a normalized vector from the given vector
	/// </summary>
	/// <param name="vector">The vector to normalize</param>
	public NormalizedVector3(Vector3 vector) {
		// Force invalid vectors to point in hardcoded directions
		if (float.IsNaN(vector.X) || float.IsNaN(vector.Y) || float.IsNaN(vector.Z) || vector == Vector3.Zero)
			Vector = Vector3.Up;
		else if (float.IsPositiveInfinity(vector.X))
			Vector = Vector3.Right;
		else if (float.IsNegativeInfinity(vector.X))
			Vector = Vector3.Left;
		else if (float.IsPositiveInfinity(vector.Y))
			Vector = Vector3.Up;
		else if (float.IsNegativeInfinity(vector.Y))
			Vector = Vector3.Down;
		else if (float.IsPositiveInfinity(vector.Z))
			Vector = Vector3.Forward;
		else if (float.IsNegativeInfinity(vector.Z))
			Vector = Vector3.Backward;
		else
			Vector = Vector3.Normalize(vector);
	}

	/// <summary>
	/// Implicitly converts a <see cref="NormalizedVector3"/> to a <see cref="Vector3"/>.
	/// </summary>
	/// <param name="normalized">The normalized vector to convert.</param>
	public static implicit operator Vector3(NormalizedVector3 normalized) => normalized.Vector;

	/// <summary>
	/// Implicitly converts a <see cref="Vector3"/> to a <see cref="NormalizedVector3"/>.
	/// </summary>
	/// <param name="vector">The vector to convert.</param>
	public static implicit operator NormalizedVector3(Vector3 vector) => new(vector);
}

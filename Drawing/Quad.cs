using Microsoft.Xna.Framework.Graphics;
using System;

namespace GraphicsLib.Drawing;

/// <inheritdoc cref="Quad{TVertex}"/>
public static class Quad {
	/// <summary>
	/// Creates a new quad compound primitive from the given vertices.
	/// </summary>
	/// <typeparam name="TVertex">The vertex type.</typeparam>
	/// <param name="a">The first vertex of the quad.</param>
	/// <param name="b">The second vertex of the quad.</param>
	/// <param name="c">The third vertex of the quad.</param>
	/// <param name="d">The fourth vertex of the quad.</param>
	/// <returns>The quad primitive.</returns>
	public static Quad<TVertex> Create<TVertex>(TVertex a, TVertex b, TVertex c, TVertex d)
		where TVertex : struct, IVertexType
	{
		return new Quad<TVertex>(a, b, c, d);
	}
}

/// <summary>
/// A compound primitive representing a quad and consisting of two triangles.<br/>
/// <b>NOTE:</b> The order of the vertices is clockwise.
/// </summary>
/// <param name="A">The first vertex of the quad.</param>
/// <param name="B">The second vertex of the quad.</param>
/// <param name="C">The third vertex of the quad.</param>
/// <param name="D">The fourth vertex of the quad.</param>
public record struct Quad<TVertex>(TVertex A, TVertex B, TVertex C, TVertex D) : IPrimitive<Quad<TVertex>, TVertex>
	where TVertex : struct, IVertexType
{
	static int IPrimitive<Quad<TVertex>, TVertex>.IndexCount => 6;

	static int IPrimitive<Quad<TVertex>, TVertex>.VertexCount => 4;

	static Quad<TVertex> IPrimitive<Quad<TVertex>, TVertex>.Create(ReadOnlySpan<TVertex> vertices) {
		return new Quad<TVertex>(vertices[0], vertices[1], vertices[2], vertices[3]);
	}

	static void IPrimitive<Quad<TVertex>, TVertex>.ExtractVertices(in Quad<TVertex> self, Span<TVertex> destination) {
		destination[0] = self.A;
		destination[1] = self.B;
		destination[2] = self.C;
		destination[3] = self.D;
	}

	static void IPrimitive<Quad<TVertex>, TVertex>.MapIndices(in Quad<TVertex> self, short baseIndex, Span<short> destination) {
		destination[0] = baseIndex;
		destination[1] = (short)(baseIndex + 1);
		destination[2] = (short)(baseIndex + 3);
		destination[3] = (short)(baseIndex + 2);
		destination[4] = (short)(baseIndex + 3);
		destination[5] = (short)(baseIndex + 1);
	}
}

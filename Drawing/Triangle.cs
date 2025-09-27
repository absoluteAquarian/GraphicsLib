using Microsoft.Xna.Framework.Graphics;
using System;

namespace GraphicsLib.Drawing;

/// <inheritdoc cref="Triangle{TVertex}"/>
public static class Triangle {
	/// <summary>
	/// Creates a new triangle primitive from the given vertices.
	/// </summary>
	/// <typeparam name="TVertex">The vertex type.</typeparam>
	/// <param name="a">The first vertex of the triangle.</param>
	/// <param name="b">The second vertex of the triangle.</param>
	/// <param name="c">The third vertex of the triangle.</param>
	/// <returns>The triangle primitive.</returns>
	public static Triangle<TVertex> Create<TVertex>(TVertex a, TVertex b, TVertex c)
		where TVertex : struct, IVertexType
	{
		return new Triangle<TVertex>(a, b, c);
	}
}

/// <summary>
/// A primitive representing a triangle.
/// </summary>
/// <param name="A">The first vertex of the triangle.</param>
/// <param name="B">The second vertex of the triangle.</param>
/// <param name="C">The third vertex of the triangle.</param>
public record struct Triangle<TVertex>(TVertex A, TVertex B, TVertex C) : IPrimitive<Triangle<TVertex>, TVertex>
	where TVertex : struct, IVertexType
{
	static int IPrimitive<Triangle<TVertex>, TVertex>.IndexCount => 3;

	static int IPrimitive<Triangle<TVertex>, TVertex>.VertexCount => 3;

	static Triangle<TVertex> IPrimitive<Triangle<TVertex>, TVertex>.Create(ReadOnlySpan<TVertex> vertices) {
		return new Triangle<TVertex>(vertices[0], vertices[1], vertices[2]);
	}

	static void IPrimitive<Triangle<TVertex>, TVertex>.ExtractVertices(in Triangle<TVertex> self, Span<TVertex> destination) {
		destination[0] = self.A;
		destination[1] = self.B;
		destination[2] = self.C;
	}

	static void IPrimitive<Triangle<TVertex>, TVertex>.MapIndices(short baseIndex, Span<short> destination) {
		destination[0] = baseIndex;
		destination[1] = (short)(baseIndex + 1);
		destination[2] = (short)(baseIndex + 2);
	}
}

/// <summary>
/// A reference to a <see cref="Triangle{TVertex}"/> primitive in a <see cref="PrimitiveBuilder{TVertex}"/>
/// </summary>
/// <typeparam name="TVertex">The vertex type.</typeparam>
/// <param name="ref">The raw primitive reference.</param>
public readonly ref struct TriangleRef<TVertex>(PrimitiveRef<Triangle<TVertex>, TVertex> @ref)
	where TVertex : struct, IVertexType
{
	private readonly ref TVertex _a = ref @ref[0];
	private readonly ref TVertex _b = ref @ref[1];
	private readonly ref TVertex _c = ref @ref[2];

	/// <inheritdoc cref="Triangle{TVertex}.A"/>
	public ref TVertex A => ref _a;
	
	/// <inheritdoc cref="Triangle{TVertex}.B"/>
	public ref TVertex B => ref _b;
	
	/// <inheritdoc cref="Triangle{TVertex}.C"/>
	public ref TVertex C => ref _c;
}

using Microsoft.Xna.Framework.Graphics;
using System;

namespace GraphicsLib.Drawing;

/// <inheritdoc cref="Point{TVertex}"/>
public static class Point {
	/// <summary>
	/// Creates a new point primitive from the given vertex.
	/// </summary>
	/// <typeparam name="TVertex">The vertex type.</typeparam>
	/// <param name="vertex">The vertex representing the point.</param>
	/// <returns>The point primitive.</returns>
	public static Point<TVertex> Create<TVertex>(TVertex vertex)
		where TVertex : struct, IVertexType
	{
		return new Point<TVertex>(vertex);
	}
}

/// <summary>
/// A primitive representing a single point.
/// </summary>
/// <param name="Vertex">The vertex representing the point.</param>
public record struct Point<TVertex>(TVertex Vertex) : IPrimitive<Point<TVertex>, TVertex>
	where TVertex : struct, IVertexType
{
	static int IPrimitive<Point<TVertex>, TVertex>.IndexCount => 1;

	static int IPrimitive<Point<TVertex>, TVertex>.VertexCount => 1;

	static Point<TVertex> IPrimitive<Point<TVertex>, TVertex>.Create(ReadOnlySpan<TVertex> vertices) {
		return new Point<TVertex>(vertices[0]);
	}

	static void IPrimitive<Point<TVertex>, TVertex>.ExtractVertices(in Point<TVertex> self, Span<TVertex> destination) {
		destination[0] = self.Vertex;
	}

	static void IPrimitive<Point<TVertex>, TVertex>.MapIndices(short baseIndex, Span<short> destination) {
		destination[0] = baseIndex;
	}
}

/// <summary>
/// A reference to a <see cref="Point{TVertex}"/> primitive in a <see cref="PrimitiveBuilder{TVertex}"/>
/// </summary>
/// <typeparam name="TVertex">The vertex type.</typeparam>
/// <param name="ref">The raw primitive reference.</param>
public readonly ref struct PointRef<TVertex>(PrimitiveRef<Point<TVertex>, TVertex> @ref)
	where TVertex : struct, IVertexType
{
	private readonly ref TVertex _vertex = ref @ref[0];

	/// <inheritdoc cref="Point{TVertex}.Vertex"/>
	public ref TVertex Vertex => ref _vertex;
}

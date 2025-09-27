using Microsoft.Xna.Framework.Graphics;
using System;

namespace GraphicsLib.Drawing;

/// <inheritdoc cref="Line{TVertex}"/>
public static class Line {
	/// <summary>
	/// Creates a new line primitive from the given start and end vertices.
	/// </summary>
	/// <typeparam name="TVertex">The vertex type.</typeparam>
	/// <param name="start">The starting vertex of the line.</param>
	/// <param name="end">The ending vertex of the line.</param>
	/// <returns>The line primitive.</returns>
	public static Line<TVertex> Create<TVertex>(TVertex start, TVertex end)
		where TVertex : struct, IVertexType
	{
		return new Line<TVertex>(start, end);
	}
}

/// <summary>
/// A primitive representing a line segment.
/// </summary>
/// <param name="Start">The starting vertex of the line.</param>
/// <param name="End">The ending vertex of the line.</param>
public record struct Line<TVertex>(TVertex Start, TVertex End) : IPrimitive<Line<TVertex>, TVertex>
	where TVertex : struct, IVertexType
{
	static int IPrimitive<Line<TVertex>, TVertex>.IndexCount => 2;

	static int IPrimitive<Line<TVertex>, TVertex>.VertexCount => 2;

	static Line<TVertex> IPrimitive<Line<TVertex>, TVertex>.Create(ReadOnlySpan<TVertex> vertices) {
		return new Line<TVertex>(vertices[0], vertices[1]);
	}

	static void IPrimitive<Line<TVertex>, TVertex>.ExtractVertices(in Line<TVertex> self, Span<TVertex> destination) {
		destination[0] = self.Start;
		destination[1] = self.End;
	}

	static void IPrimitive<Line<TVertex>, TVertex>.MapIndices(in Line<TVertex> self, short baseIndex, Span<short> destination) {
		destination[0] = baseIndex;
		destination[1] = (short)(baseIndex + 1);
	}
}

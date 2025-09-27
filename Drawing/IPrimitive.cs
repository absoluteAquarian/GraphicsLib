using Microsoft.Xna.Framework.Graphics;
using System;

namespace GraphicsLib.Drawing;

/// <summary>
/// An interface representing a simple object or shape to draw to the screen.
/// </summary>
public interface IPrimitive<TSelf, TVertex>
	where TSelf : struct, IPrimitive<TSelf, TVertex>
	where TVertex : struct, IVertexType
{
	/// <summary>
	/// The number of indices required to represent this primitive when using indexed drawing.
	/// </summary>
	static abstract int IndexCount { get; }

	/// <summary>
	/// The number of vertices that make up a single instance of this primitive.
	/// </summary>
	static abstract int VertexCount { get; }

	/// <summary>
	/// Creates a new instance of this primitive from the given vertices.
	/// </summary>
	/// <param name="source">A span containing the vertices to use to create the primitive.  Must be at least <see cref="VertexCount"/> in length.</param>
	/// <returns>The created primitive instance.</returns>
	static abstract TSelf Create(ReadOnlySpan<TVertex> source);

	/// <summary>
	/// Extracts the vertices that make up this primitive into <paramref name="destination"/>.
	/// </summary>
	/// <param name="self">The primitive instance</param>
	/// <param name="destination">The destination span to write the vertices into.  Must be at least <see cref="VertexCount"/> in length.</param>
	static abstract void ExtractVertices(in TSelf self, Span<TVertex> destination);

	/// <summary>
	/// Maps the indices required to represent this primitive into <paramref name="destination"/>.
	/// </summary>
	/// <param name="baseIndex">The base index to offset the mapped indices by.</param>
	/// <param name="destination">The destination span to write the indices into.  Must be at least <see cref="IndexCount"/> in length.</param>
	static abstract void MapIndices(short baseIndex, Span<short> destination);
}

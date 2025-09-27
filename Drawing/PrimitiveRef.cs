using Microsoft.Xna.Framework.Graphics;
using System;

namespace GraphicsLib.Drawing;

/// <summary>
/// Represents a reference to a primitive's vertices in the vertex buffer for a <see cref="PrimitiveBuilder{TVertex}"/>
/// </summary>
/// <typeparam name="TPrimitive">The primitive type.</typeparam>
/// <typeparam name="TVertex">The vertex type.</typeparam>
public readonly ref struct PrimitiveRef<TPrimitive, TVertex>
	where TPrimitive : struct, IPrimitive<TPrimitive, TVertex>
	where TVertex : struct, IVertexType
{
	private readonly Span<TVertex> _vertices;
	private readonly Span<short> _indices;

	/// <summary>
	/// Gets a reference to the vertex at the specified index in this primitive.
	/// </summary>
	/// <param name="index">The index of the vertex in the primitive.</param>
	public ref TVertex this[int index] => ref _vertices[_indices[index]];

	/// <summary>
	/// Creates a reference to a primitive's vertices in the vertex buffer for a <see cref="PrimitiveBuilder{TVertex}"/>
	/// </summary>
	/// <param name="builder">The primitive builder containing the vertex buffer.</param>
	/// <param name="baseVertex">The base vertex index for this primitive in the vertex buffer.</param>
	public PrimitiveRef(PrimitiveBuilder<TVertex> builder, short baseVertex) {
		builder.GetData(out var vertices, out _, out _, out _);
		_vertices = vertices;
		_indices = new short[TPrimitive.IndexCount];
		TPrimitive.MapIndices(baseVertex, _indices);
	}

	// Used by PrimitiveBuilder<TVertex>.VertexReader
	internal PrimitiveRef(Span<TVertex> vertices, short baseVertex) {
		_vertices = vertices;
		_indices = new short[TPrimitive.IndexCount];
		TPrimitive.MapIndices(baseVertex, _indices);
	}
}

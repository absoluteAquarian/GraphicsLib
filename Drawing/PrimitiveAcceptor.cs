using Microsoft.Xna.Framework.Graphics;
using System;

namespace GraphicsLib.Drawing;

/// <summary>
/// The base class for objects that can translate primitives into vertex and index data.
/// </summary>
/// <typeparam name="TVertex">The vertex type.</typeparam>
public abstract class PrimitiveAcceptor<TVertex>
	where TVertex : struct, IVertexType
{
	/// <summary>
	/// Determines if this acceptor can accept the specified primitive object.
	/// </summary>
	/// <typeparam name="T">The primitive type.</typeparam>
	/// <param name="index">The index of the primitive were it to be added next.</param>
	/// <returns><see langword="true"/> if this acceptor can accept the specified primitive type; otherwise, <see langword="false"/>.</returns>"
	public abstract bool Accepts<T>(int index) where T : struct, IPrimitive<T, TVertex>;

	/// <summary>
	/// Copies the state of this acceptor to another acceptor of the same type.<br/>
	/// This is used when cloning, and you are encouraged to copy any vertex and index data to the new acceptor.
	/// </summary>
	/// <param name="clone">The acceptor to copy state to.</param>
	public abstract void CopyTo(PrimitiveAcceptor<TVertex> clone);

	/// <summary>
	/// Extracts the data needed to render the built primitives.
	/// </summary>
	/// <param name="vertices">The array of vertices.</param>
	/// <param name="indices">The array of indices.</param>
	/// <param name="primitiveCount">The number of primitives built.</param>
	public abstract void GetData(out TVertex[] vertices, out short[] indices, out int primitiveCount);

	/// <summary>
	/// Gets the base vertex index for the specified primitive index.
	/// </summary>
	/// <param name="index">The zero-based index of the primitive.</param>
	public abstract int GetVertexBase(int index);

	/// <summary>
	/// Creates a new instance of the same type as this acceptor.<br/>
	/// This is used when cloning the acceptor.
	/// </summary>
	/// <returns>The new acceptor instance.</returns>
	public abstract PrimitiveAcceptor<TVertex> NewInstance();

	/// <summary>
	/// Adds the specified primitive to the vertex buffer for this acceptor.
	/// </summary>
	/// <typeparam name="T">The primitive type.</typeparam>
	/// <param name="primitive">The primitive instance.</param>
	public abstract void Push<T>(in T primitive) where T : struct, IPrimitive<T, TVertex>;

	/// <summary>
	/// Creates a new acceptor that is a copy of this instance.<br/>
	/// The cloned acceptor is expected to also have copies of any vertex and index data.
	/// </summary>
	/// <returns>The cloned acceptor.</returns>
	public PrimitiveAcceptor<TVertex> Clone() {
		var clone = NewInstance();
		CopyTo(clone);
		return clone;
	}

	/// <summary>
	/// Reads back a previously pushed primitive from the vertex buffer.
	/// </summary>
	/// <typeparam name="T">The primitive type.</typeparam>
	/// <param name="index">The index of the primitive to read back.</param>
	/// <returns>The primitive instance at the specified index.</returns>
	public T Read<T>(int index)
		where T : struct, IPrimitive<T, TVertex>
	{
		if (!Accepts<T>(index))
			throw new InvalidOperationException($"This acceptor does not accept primitives of type {FriendlyName<T>.Value} at index {index}");

		GetData(out var vertices, out _, out _);

		if (vertices is not { Length: > 0 })
			throw new InvalidOperationException("The acceptor is still waiting for all primitives to be pushed");

		int baseVertex = GetVertexBase(index);

		if (baseVertex < 0 || baseVertex + T.VertexCount > vertices.Length)
			throw new ArgumentOutOfRangeException(nameof(index), "The specified index is out of range");

		return T.Create(vertices.AsSpan(baseVertex, T.VertexCount));
	}

	/// <summary>
	/// Directly writes a primitive to the vertex buffer.
	/// </summary>
	/// <typeparam name="T">The primitive type.</typeparam>
	/// <param name="index">The index of the primitive to overwrite.</param>
	/// <param name="primitive">The primitive instance to write.</param>
	public void Write<T>(int index, in T primitive)
		where T : struct, IPrimitive<T, TVertex>
	{
		if (!Accepts<T>(index))
			throw new InvalidOperationException($"This acceptor does not accept primitives of type {FriendlyName<T>.Value} at index {index}");

		GetData(out var vertices, out _, out _);

		if (vertices is not { Length: > 0 })
			throw new InvalidOperationException("The acceptor is still waiting for all primitives to be pushed");

		int baseVertex = GetVertexBase(index);

		if (baseVertex < 0 || baseVertex + T.VertexCount > vertices.Length)
			throw new ArgumentOutOfRangeException(nameof(index), "The specified index is out of range");

		T.ExtractVertices(
			in primitive,
			vertices.AsSpan(baseVertex, T.VertexCount)
		);
	}
}

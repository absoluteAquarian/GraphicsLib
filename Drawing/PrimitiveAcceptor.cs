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
	/// The raw vertex data.  If the acceptor is still waiting for all primitives to be pushed, this should be empty.
	/// </summary>
	public abstract ReadOnlySpan<TVertex> Vertices { get; }

	/// <summary>
	/// <see cref="Vertices"/> that can be directly written to.<br/>
	/// Used by <see cref="Write"/>
	/// </summary>
	protected abstract Span<TVertex> WritableVertices { get; }

	internal Span<TVertex> WritableVerticesInternal => WritableVertices;

	/// <summary>
	/// The raw index data.  If the acceptor is still waiting for all primitives to be pushed, this should be empty.
	/// </summary>
	public abstract ReadOnlySpan<short> Indices { get; }

	/// <summary>
	/// Represents the maximum number of <see cref="IPrimitive{TSelf, TVertex}"/> instances that can be accepted by this acceptor.
	/// </summary>
	public int MaxPrimitives { get; protected set; }

	internal bool hasChanges;
	/// <summary>
	/// Indicates whether the vertex or index data has changed since the last time it was uploaded to the GPU.<br/>
	/// This property can be used by acceptors that read it to determine if data needs to be re-evaluated in <see cref="PrepareDataToUpload"/>
	/// </summary>
	public bool HasChanges {
		get => hasChanges;
		set => hasChanges |= value;
	}

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
	/// Gets the base vertex index for the specified primitive index.
	/// </summary>
	/// <param name="index">The zero-based index of the primitive.</param>
	public abstract int GetVertexBase(int index);

	/// <summary>
	/// Gets the index of the primitive that contains the specified vertex index.
	/// </summary>
	/// <param name="vertexIndex">The vertex index.</param>
	/// <returns>The primitive index.</returns>
	public abstract int GetPrimitiveIndex(short vertexIndex);

	/// <summary>
	/// Creates a new instance of the same type as this acceptor.<br/>
	/// This is used when cloning the acceptor.
	/// </summary>
	/// <returns>The new acceptor instance.</returns>
	public abstract PrimitiveAcceptor<TVertex> NewInstance();

	/// <summary>
	/// Prepares the vertex and index data to be uploaded to the GPU.
	/// </summary>
	/// <param name="vertices">The vertex data.</param>
	/// <param name="indices">The index data.</param>
	/// <param name="gpuPrimitiveCount">The number of primitives to render.</param>
	public virtual void PrepareDataToUpload(out TVertex[] vertices, out short[] indices, out int gpuPrimitiveCount) {
		vertices = Vertices.ToArray();
		indices = Indices.ToArray();
		gpuPrimitiveCount = MaxPrimitives;
	}

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
		clone.HasChanges = HasChanges;
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

		if (Vertices is not { Length: > 0 } vertices)
			throw new InvalidOperationException("The acceptor is still waiting for all primitives to be pushed");

		int baseVertex = GetVertexBase(index);

		if (baseVertex < 0 || baseVertex + T.VertexCount > vertices.Length)
			throw new ArgumentOutOfRangeException(nameof(index), "The specified index is out of range");

		return T.Create(vertices.Slice(baseVertex, T.VertexCount));
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

		if (WritableVertices is not { Length: > 0 } vertices)
			throw new InvalidOperationException("The acceptor is still waiting for all primitives to be pushed");

		int baseVertex = GetVertexBase(index);

		if (baseVertex < 0 || baseVertex + T.VertexCount > vertices.Length)
			throw new ArgumentOutOfRangeException(nameof(index), "The specified index is out of range");

		T.ExtractVertices(
			in primitive,
			vertices.Slice(baseVertex, T.VertexCount)
		);

		HasChanges = true;
	}
}

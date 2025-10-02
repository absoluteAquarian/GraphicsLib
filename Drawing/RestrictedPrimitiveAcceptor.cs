using GraphicsLib.Utility;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace GraphicsLib.Drawing;

/// <summary>
/// A <see cref="PrimitiveAcceptor{TVertex}"/> designed for (although not limited to) <see cref="PrimitiveType.TriangleList"/> and <see cref="PrimitiveType.LineList"/> primitives.<br/>
/// The first primitive must be of type <typeparamref name="TInitial"/>, while all subsequent primitives must be of type <typeparamref name="TConsequent"/>
/// </summary>
/// <typeparam name="TVertex">The vertex type.</typeparam>
/// <typeparam name="TInitial">The type of the first primitive to be accepted.</typeparam>
/// <typeparam name="TConsequent">The type of all subsequent primitives to be accepted.</typeparam>
public class RestrictedPrimitiveAcceptor<TVertex, TInitial, TConsequent> : PrimitiveAcceptor<TVertex>
	where TInitial : struct, IPrimitive<TInitial, TVertex>
	where TConsequent : struct, IPrimitive<TConsequent, TVertex>
	where TVertex : struct, IVertexType
{
	/// <summary>
	/// Represents the maximum number of <see cref="IPrimitive{TSelf, TVertex}"/> instances that can be accepted by this acceptor.
	/// </summary>
	protected readonly int primitiveCount;
	/// <summary>
	/// The primitive type that this acceptor is designed to build.
	/// </summary>
	protected readonly PrimitiveType mode;
	/// <summary>
	/// Whether to connect the last primitive to the first when using <see cref="PrimitiveType.TriangleStrip"/> or <see cref="PrimitiveType.LineStrip"/>.
	/// </summary>
	protected readonly bool connectLastToFirst;

	/// <summary>
	/// The raw vertex data.
	/// </summary>
	protected TVertex[] vertices;
	/// <summary>
	/// The raw index data.
	/// </summary>
	protected short[] indices;
	/// <summary>
	/// The index of the next vertex to be written to <see cref="vertices"/>
	/// </summary>
	protected int currentVertex;
	/// <summary>
	/// The index of the next vertex index to be written to <see cref="indices"/>
	/// </summary>
	protected int currentIndex;

	/// <inheritdoc/>
	public override ReadOnlySpan<TVertex> Vertices => WritableVertices;

	/// <inheritdoc/>
	protected override Span<TVertex> WritableVertices => currentVertex < vertices.Length ? [] : vertices;

	/// <inheritdoc/>
	public override ReadOnlySpan<short> Indices => currentVertex < vertices.Length ? [] : indices;

	/// <summary>
	/// Initializes a new instance of the <see cref="RestrictedPrimitiveAcceptor{TVertex, TInitial, TConsequent}"/> class.
	/// </summary>
	/// <param name="primitiveCount">The number of primitives to accept. Must be a positive integer.</param>
	/// <param name="mode">The mode that determines the type of primitives to be processed.</param>
	/// <param name="connectLastToFirst">A value indicating whether the last primitive should be connected to the first when using <see cref="PrimitiveType.TriangleStrip"/> or <see cref="PrimitiveType.LineStrip"/>.</param>
	/// <exception cref="ArgumentOutOfRangeException"/>
	public RestrictedPrimitiveAcceptor(int primitiveCount, PrimitiveType mode, bool connectLastToFirst) {
		if (primitiveCount <= 0)
			throw new ArgumentOutOfRangeException(nameof(primitiveCount), "Value must be positive");

		this.primitiveCount = primitiveCount;
		this.mode = mode;
		this.connectLastToFirst = connectLastToFirst && mode is PrimitiveType.TriangleStrip or PrimitiveType.LineStrip;

		currentVertex = 0;
		currentIndex = 0;

		base.MaxPrimitives = primitiveCount;

		if (this.connectLastToFirst && this.primitiveCount > 1) {
			// TriangleStrip and LineStrip both use Point, so one additional primitive is needed
			// The caller doesn't need to actually provide it; it's automatically "added" to the end of the index list
			base.MaxPrimitives++;
		}
	}

	/// <inheritdoc/>
	public override bool Accepts<T>(int index) {
		if (index < 0 || index >= primitiveCount)
			ExceptionHelper.ThrowIndexOutOfRange(index, 0, primitiveCount);

		if (typeof(T) == typeof(TInitial))
			return index == 0;
		else if (typeof(T) == typeof(TConsequent))
			return index > 0;
		else
			return false;
	}

	/// <inheritdoc/>
	public override void CopyTo(PrimitiveAcceptor<TVertex> clone) {
		var restrictedAcceptor = (RestrictedPrimitiveAcceptor<TVertex, TInitial, TConsequent>)clone;
		restrictedAcceptor.vertices = (TVertex[])vertices.Clone();
		restrictedAcceptor.indices = (short[])indices.Clone();
		restrictedAcceptor.currentVertex = currentVertex;
		restrictedAcceptor.currentIndex = currentIndex;
	}

	/// <inheritdoc/>
	public override int GetVertexBase(int index) {
		if (index < 0 || index >= primitiveCount)
			ExceptionHelper.ThrowIndexOutOfRange(index, 0, primitiveCount);

		if (index == 0)
			return 0;
		else
			return TInitial.VertexCount + (index - 1) * TConsequent.VertexCount;
	}

	/// <inheritdoc/>
	public override int GetPrimitiveIndex(short vertexIndex) {
		if (vertexIndex < 0 || vertexIndex >= currentVertex)
			ExceptionHelper.ThrowIndexOutOfRange(vertexIndex, 0, currentVertex);

		if (vertexIndex < TInitial.VertexCount)
			return 0;
		else
			return 1 + (vertexIndex - TInitial.VertexCount) / TConsequent.VertexCount;
	}

	/// <inheritdoc/>
	public override PrimitiveAcceptor<TVertex> NewInstance() {
		return new RestrictedPrimitiveAcceptor<TVertex, TInitial, TConsequent>(primitiveCount, mode, connectLastToFirst);
	}

	/// <inheritdoc/>
	public override void PrepareDataToUpload(out TVertex[] vertices, out short[] indices, out int gpuPrimitiveCount) {
		// Since the arrays are accessible, just hand them off directly instead of copying their data
		vertices = this.vertices;
		indices = this.indices;
		gpuPrimitiveCount = this.primitiveCount;
	}

	/// <inheritdoc/>
	public override void Push<T>(in T primitive) {
		if (typeof(T) == typeof(TInitial)) {
			EnsureInitializedArrays();

			if (currentVertex > 0)
				throw new InvalidOperationException($"Primitives after the first must be of type {FriendlyName<TConsequent>.Value}");
		} else if (typeof(T) == typeof(TConsequent)) {
			EnsureInitializedArrays();

			if (currentVertex < TInitial.VertexCount)
				throw new InvalidOperationException($"The first primitive must be of type {FriendlyName<TInitial>.Value}");
			else if (currentVertex >= vertices.Length)
				throw new InvalidOperationException("Vertex limit has been reached on this builder");
		} else
			throw new NotSupportedException($"Primitive must be of type {FriendlyName<TInitial>.Value} or {FriendlyName<TConsequent>.Value}");

		T.ExtractVertices(
			in primitive,
			vertices.AsSpan(currentVertex, T.VertexCount)
		);

		T.MapIndices(
			(short)currentVertex,
			indices.AsSpan(currentIndex, T.IndexCount)
		);

		currentVertex += T.VertexCount;
		currentIndex += T.IndexCount;

		if (typeof(T) == typeof(TConsequent) && currentVertex >= vertices.Length && connectLastToFirst) {
			// Ensure that the last index is the same as the first
			indices[^1] = 0;
		}
	}

	/// <summary>
	/// Ensures that the <see cref="vertices"/> and <see cref="indices"/> arrays are initialized to their correct sizes.
	/// </summary>
	protected virtual void EnsureInitializedArrays() {
		vertices ??= primitiveCount == 1
			? new TVertex[TInitial.VertexCount]
			: new TVertex[TInitial.VertexCount + (primitiveCount - 1) * TConsequent.VertexCount];

		indices ??= primitiveCount == 1
			? new short[TInitial.IndexCount]
			: mode is PrimitiveType.TriangleStrip or PrimitiveType.LineStrip && connectLastToFirst
				? new short[TInitial.IndexCount + (primitiveCount - 1) * TConsequent.IndexCount + 1]
				: new short[TInitial.IndexCount + (primitiveCount - 1) * TConsequent.IndexCount];
	}
}

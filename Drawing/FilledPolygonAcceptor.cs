using GraphicsLib.Utility;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace GraphicsLib.Drawing;

/// <summary>
/// Represents a specialized <see cref="PrimitiveAcceptor{TVertex}"/> using <see cref="PrimitiveType.TriangleList"/> where each triangle shares the first vertex.<br/>
/// Subsequent triangles are defined using a <see cref="Point"/> primitive since the triangle will share an edge with the previous triangle.
/// </summary>
/// <typeparam name="TVertex">The vertex type.</typeparam>
public class FilledPolygonAcceptor<TVertex> : RestrictedPrimitiveAcceptor<TVertex, Triangle<TVertex>, Point<TVertex>>
	where TVertex : struct, IVertexType
{
	/// <summary>
	/// Initializes a new instance of the <see cref="FilledPolygonAcceptor{TVertex}"/> class.
	/// </summary>
	/// <param name="primitiveCount">The number of primitives to accept. Must be a positive integer.</param>
	public FilledPolygonAcceptor(int primitiveCount) : base(primitiveCount, PrimitiveType.TriangleList, false) { }

	/// <inheritdoc/>
	public override PrimitiveAcceptor<TVertex> NewInstance() {
		// This method is needed for Clone() to return the correct object type

		return new FilledPolygonAcceptor<TVertex>(base.primitiveCount);
	}

	/// <inheritdoc/>
	public override void Push<T>(in T primitive) {
		if (typeof(T) == typeof(Triangle<TVertex>)) {
			EnsureInitializedArrays();

			// Push the vertices for a triangle
			if (base.currentVertex > 0)
				throw new InvalidOperationException($"Primitives after the first must be of type {nameof(Point)}");

			T.ExtractVertices(
				in primitive,
				base.vertices.AsSpan(base.currentVertex, T.VertexCount)
			);

			base.currentVertex += 3;
		} else if (typeof(T) == typeof(Point<TVertex>)) {
			EnsureInitializedArrays();
			
			// Push the vertex for a point
			if (base.currentVertex < 3)
				throw new InvalidOperationException($"The first primitive must be of type {nameof(Triangle)}");
			else if (base.currentVertex >= base.vertices.Length)
				throw new InvalidOperationException("Vertex limit has been reached on this builder");

			T.ExtractVertices(
				in primitive,
				base.vertices.AsSpan(base.currentVertex, T.VertexCount)
			);

			base.currentVertex++;
		} else {
			// Use the original logic
			base.Push(in primitive);
		}
	}

	/// <inheritdoc/>
	protected override void EnsureInitializedArrays() {
		// 1 Triangle + (N - 1) Point
		base.vertices ??= new TVertex[3 + base.primitiveCount - 1];

		// N Triangle
		if (base.indices is null) {
			base.indices = new short[base.primitiveCount * 3];

			// We can deterministically fill the indices array here
			base.indices[0] = 0;
			base.indices[1] = 1;
			base.indices[2] = 2;

			for (int i = 3; i < base.indices.Length; i += 3) {
				base.indices[i] = 0;
				base.indices[i + 1] = (short)(i / 3 + 1);
				base.indices[i + 2] = (short)(i / 3 + 2);
			}

			base.currentIndex = base.indices.Length;
		}
	}
}

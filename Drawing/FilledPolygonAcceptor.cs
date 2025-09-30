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

	public override PrimitiveAcceptor<TVertex> NewInstance() {
		// This method is needed for Clone() to return the correct object type

		return new FilledPolygonAcceptor<TVertex>(base.primitiveCount);
	}

	/// <inheritdoc/>
	public override void Push<T>(in T primitive) {
		if (typeof(T) == typeof(Point<TVertex>)) {
			base.EnsureInitializedArrays();
			
			// Push the vertex for a point
			if (base.currentVertex < 3)
				throw new InvalidOperationException($"The first primitive must be of type {nameof(Triangle)}");
			else if (base.currentVertex >= base.vertices.Length)
				throw new InvalidOperationException("Vertex limit has been reached on this builder");

			base.vertices[base.currentVertex] = Conversion.UnsafeCast<T, Point<TVertex>>(in primitive).Vertex;

			// Push the indices for a triangle instead
			base.indices[currentIndex++] = 0;
			base.indices[currentIndex++] = (short)(base.currentVertex - 1);
			base.indices[currentIndex++] = (short)base.currentVertex;

			base.currentVertex++;
		} else {
			// Use the original logic
			base.Push(primitive);
		}
	}

	/// <inheritdoc/>
	protected override void EnsureInitializedArrays() {
		// 1 Triangle + (N - 1) Point
		base.vertices ??= new TVertex[3 + base.primitiveCount - 1];

		// N Triangle
		base.indices ??= new short[base.primitiveCount * 3];
	}
}

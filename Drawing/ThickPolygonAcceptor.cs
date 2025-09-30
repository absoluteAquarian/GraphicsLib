using GraphicsLib.Utility;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace GraphicsLib.Drawing;

/// <summary>
/// Represents a specialized <see cref="PrimitiveAcceptor{TVertex}"/> using <see cref="PrimitiveType.TriangleStrip"/> where each triangle shares an edge with the previous triangle.<br/>
/// Subsequent triangles are defined using a <see cref="Point{TVertex}"/> primitive since the triangle will share an edge with the previous triangle.
/// <para/>
/// The vertex and index data is expected to be in a specific order, so it's encouraged to use these functions from <see cref="SimpleShapes"/> instead:
/// <list type="bullet">
/// <item></item>
/// </list>
/// </summary>
/// <typeparam name="TVertex"></typeparam>
public class ThickPolygonAcceptor<TVertex> : RestrictedPrimitiveAcceptor<TVertex, Point<TVertex>, Point<TVertex>>
	where TVertex : struct, IVertexType
{
	/// <summary>
	/// Initializes a new instance of the <see cref="ThickPolygonAcceptor{TVertex}"/> class.
	/// </summary>
	/// <param name="primitiveCount">The number of primitives to accept. Must be a positive integer.</param>
	public ThickPolygonAcceptor(int primitiveCount) : base(primitiveCount, PrimitiveType.TriangleStrip, false) { }

	/// <inheritdoc/>
	public override bool Accepts<T>(int index) {
		if (index < 0 || index >= base.primitiveCount)
			ExceptionHelper.ThrowIndexOutOfRange(index, 0, base.primitiveCount);

		return typeof(T) == typeof(Point<TVertex>);
	}

	/// <inheritdoc/>
	public override PrimitiveAcceptor<TVertex> NewInstance() {
		// This method is needed for Clone() to return the correct object type

		return new ThickPolygonAcceptor<TVertex>(base.primitiveCount);
	}

	/// <inheritdoc/>
	public override void Push<T>(in T primitive) {
		if (typeof(T) == typeof(Point<TVertex>)) {
			EnsureInitializedArrays();
			
			// Push the vertex for a point
			if (base.currentVertex >= base.vertices.Length)
				throw new InvalidOperationException("Vertex limit has been reached on this builder");

			base.vertices[base.currentVertex] = Conversion.UnsafeCast<T, Point<TVertex>>(in primitive).Vertex;

			if (base.currentVertex == 0) {
				// The first Point is actually the leading Triangle
				base.indices[base.currentIndex++] = 0;
				base.indices[base.currentIndex++] = (short)(primitiveCount - 1);
				base.indices[base.currentIndex++] = (short)(primitiveCount / 2 - 1);
			} else {
				// Successive points are also visually triangles, but only need the point of the new triangle
				// Consider the first triangle to be ABC.  Point D would render as triangle BCD in the opposite winding order.
				// The simpler algorithm to find the vertex index is recursive.
				// For a square, the index array will end up being:
				//   [ 0, 7, 3, 6, 2, 5, 2, 4, 0, 7 ]

				int previous = base.indices[base.currentIndex - 1];
				int iteration = base.currentIndex;
				int max = base.primitiveCount;

				base.indices[base.currentIndex++] = (short)(previous == 0 ? max - 1 : iteration % 2 == 0 ? previous - max / 2 : previous + max / 2 - 1);
			}

			base.currentVertex++;
		} else {
			// Use the original logic
			base.Push(primitive);
		}
	}

	/// <inheritdoc/>
	protected override void EnsureInitializedArrays() {
		// N Point
		base.vertices ??= new TVertex[base.primitiveCount];

		// 1 Triangle + (N - 1) Point
		base.indices ??= new short[3 + base.primitiveCount - 1];
	}
}

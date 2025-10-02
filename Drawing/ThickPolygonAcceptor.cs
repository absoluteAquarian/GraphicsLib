using GraphicsLib.Utility;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace GraphicsLib.Drawing;

/// <summary>
/// Represents a specialized <see cref="PrimitiveAcceptor{TVertex}"/> using <see cref="PrimitiveType.TriangleList"/> for constructing polygons with thick edges.
/// <para/>
/// The vertex and index data is expected to be in a specific order, so it's encouraged to use these functions from <see cref="SimpleShapes"/> instead:
/// <list type="bullet">
/// <item><see cref="SimpleShapes.ThickPolygon(Span{Vector2}, Color, float)"/></item>
/// <item><see cref="SimpleShapes.ThickPolygon(Span{Vector3}, Color, float)"/></item>
/// <item><see cref="SimpleShapes.ThickPolygon(Span{Vector2}, Color, Color, float)"/></item>
/// <item><see cref="SimpleShapes.ThickPolygon(Span{Vector3}, Color, Color, float)"/></item>
/// <item><see cref="SimpleShapes.ThickPolygon(Span{ValueTuple{Vector2, Color}}, float)"/></item>
/// <item><see cref="SimpleShapes.ThickPolygon(Span{ValueTuple{Vector3, Color}}, float)"/></item>
/// </list>
/// </summary>
public class ThickPolygonAcceptor : RestrictedPrimitiveAcceptor<VertexPositionColor, Point<VertexPositionColor>, Point<VertexPositionColor>> {
	/// <summary>
	/// Initializes a new instance of the <see cref="ThickPolygonAcceptor"/> class.
	/// </summary>
	/// <param name="primitiveCount">The number of primitives to accept. Must be a positive integer.</param>
	public ThickPolygonAcceptor(int primitiveCount) : base(primitiveCount, PrimitiveType.TriangleList, false) { }

	/// <inheritdoc/>
	public override bool Accepts<T>(int index) {
		if (index < 0 || index >= base.primitiveCount)
			ExceptionHelper.ThrowIndexOutOfRange(index, 0, base.primitiveCount);

		return typeof(T) == typeof(Point<VertexPositionColor>);
	}

	/// <inheritdoc/>
	public override PrimitiveAcceptor<VertexPositionColor> NewInstance() {
		// This method is needed for Clone() to return the correct object type

		return new ThickPolygonAcceptor(base.primitiveCount);
	}

	/// <inheritdoc/>
	public override void PrepareDataToUpload(out VertexPositionColor[] vertices, out short[] indices, out int gpuPrimitiveCount) {
		// Because this shape has some unconventional behaviour for the last third of the vertices, that needs to be enforced before the data is uploaded.

		if (HasChanges && base.vertices is not null) {
			int slice = primitiveCount / 3;

			for (int i = slice * 2; i < primitiveCount; i++) {
				// Check the source for SimpleShapes.ThickPolygon() for an explanation for how the middle vertices are processed

				int outer = i % slice;
				int inner = outer + slice;
				int nextOuter = (outer + 1) % slice;
				int nextInner = nextOuter + slice;

				ref VertexPositionColor currentVertex = ref base.vertices[i];
				ref VertexPositionColor outerVertex = ref base.vertices[outer];
				ref VertexPositionColor innerVertex = ref base.vertices[inner];
				ref VertexPositionColor nextOuterVertex = ref base.vertices[nextOuter];
				ref VertexPositionColor nextInnerVertex = ref base.vertices[nextInner];

				Vector3 midpoint = (outerVertex.Position + innerVertex.Position) / 2f;
				Vector3 nextMidpoint = (nextOuterVertex.Position + nextInnerVertex.Position) / 2f;

				currentVertex.Position = (midpoint + nextMidpoint) / 2f;

				Color blend = Color.Lerp(outerVertex.Color, nextInnerVertex.Color, 0.5f);
				Color blend2 = Color.Lerp(innerVertex.Color, nextOuterVertex.Color, 0.5f);

				currentVertex.Color = Color.Lerp(blend, blend2, 0.5f);
			}
		}

		base.PrepareDataToUpload(out vertices, out indices, out gpuPrimitiveCount);

		// By default, PrepareDataToUpload just sets gpuPrimitiveCount to primitiveCount, but that's wrong here
		gpuPrimitiveCount = 4 * base.primitiveCount / 3;
	}

	/// <inheritdoc/>
	public override void Push<T>(in T primitive) {
		if (typeof(T) == typeof(Point<VertexPositionColor>)) {
			EnsureInitializedArrays();
			
			// Push the vertex for a point
			if (base.currentVertex >= base.vertices.Length)
				throw new InvalidOperationException("Vertex limit has been reached on this builder");

			T.ExtractVertices(
				in primitive,
				base.vertices.AsSpan(base.currentVertex, T.VertexCount)
			);

			base.currentVertex++;
		} else {
			// Use the original logic
			base.Push(primitive);
		}
	}

	/// <inheritdoc/>
	protected override void EnsureInitializedArrays() {
		// N Point
		base.vertices ??= new VertexPositionColor[base.primitiveCount];

		// (4 * N) Triangle
		if (base.indices is null) {
			base.indices = new short[4 * base.primitiveCount];

			// We can deterministically fill the indices array here
			int slice = base.primitiveCount / 3;
			int midwayPointOffset = slice * 2;
			int current = 0;

			for (int i = 0; i < slice; i++) {
				// Each "edge" is made up of 4 triangles with a common vertex

				short shared = (short)(i + midwayPointOffset);
				short outer = (short)i;
				short inner = (short)(outer + slice);
				short nextOuter = (short)((outer + 1) % slice);
				short nextInner = (short)(nextOuter + slice);

				// The first triangle
				base.indices[current++] = shared;
				base.indices[current++] = outer;
				base.indices[current++] = nextOuter;
				// The second triangle
				base.indices[current++] = shared;
				base.indices[current++] = nextOuter;
				base.indices[current++] = nextInner;
				// The third triangle
				base.indices[current++] = shared;
				base.indices[current++] = nextInner;
				base.indices[current++] = inner;
				// The fourth triangle
				base.indices[current++] = shared;
				base.indices[current++] = inner;
				base.indices[current++] = outer;
			}

			base.currentIndex = base.indices.Length;
		}
	}
}

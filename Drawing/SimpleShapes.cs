using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace GraphicsLib.Drawing;

/// <summary>
/// Provides predefined <see cref="PrimitiveBuilder{TVertex}"/> instances for simple shapes.
/// </summary>
public static class SimpleShapes {
	#region LineSegment
	/// <summary>
	/// Creates a builder for a single line segment with the specified color for both endpoints.<para/>
	/// Defined primitives:
	/// <list type="bullet">
	/// <item>[0] : Line</item>
	/// </list>
	/// </summary>
	/// <param name="color">The color for both endpoints of the line.</param>
	/// <returns>The builder instance.</returns>
	public static PrimitiveBuilder<VertexPositionColor> LineSegment(Color color) => LineSegment(color, color);

	/// <summary>
	/// Creates a builder for a single line segment with the specified colors for the start and end points.<para/>
	/// Defined primitives:
	/// <list type="bullet">
	/// <item>[0] : Line</item>
	/// </list>
	/// </summary>
	/// <param name="startColor">The color for the starting endpoint of the line.</param>
	/// <param name="endColor">The color for the ending endpoint of the line.</param>
	/// <returns>The builder instance.</returns>
	public static PrimitiveBuilder<VertexPositionColor> LineSegment(Color startColor, Color endColor) => LineSegment(Vector3.Zero, startColor, Vector3.Zero, endColor);

	/// <inheritdoc cref="LineSegment(Vector3, Vector3)"/>
	public static PrimitiveBuilder<VertexPositionColor> LineSegment(Vector2 start, Vector2 end) => LineSegment(new Vector3(start, 0f), new Vector3(end, 0f));

	/// <summary>
	/// Creates a builder for a single line segment with the specified start and end positions, and the color white for both endpoints.<para/>
	/// Defined primitives:
	/// <list type="bullet">
	/// <item>[0] : Line</item>
	/// </list>
	/// </summary>
	/// <param name="start">The position of the starting endpoint of the line, relative to where the primitive will be drawn.</param>
	/// <param name="end">The position of the ending endpoint of the line, relative to where the primitive will be drawn.</param>
	/// <returns>The builder instance.</returns>
	public static PrimitiveBuilder<VertexPositionColor> LineSegment(Vector3 start, Vector3 end) => LineSegment(start, Color.White, end, Color.White);

	/// <inheritdoc cref="LineSegment(Vector3, Vector3, Color)"/>
	public static PrimitiveBuilder<VertexPositionColor> LineSegment(Vector2 start, Vector2 end, Color color) => LineSegment(new Vector3(start, 0f), new Vector3(end, 0f), color);

	/// <summary>
	/// Creates a builder for a single line segment with the specified start and end positions, and the specified color for both endpoints.<para/>
	/// Defined primitives:
	/// <list type="bullet">
	/// <item>[0] : Line</item>
	/// </list>
	/// </summary>
	/// <param name="start">The position of the starting endpoint of the line, relative to where the primitive will be drawn.</param>
	/// <param name="end">The position of the ending endpoint of the line, relative to where the primitive will be drawn.</param>
	/// <param name="color">The color for both endpoints of the line.</param>
	public static PrimitiveBuilder<VertexPositionColor> LineSegment(Vector3 start, Vector3 end, Color color) => LineSegment(start, color, end, color);

	/// <inheritdoc cref="LineSegment(Vector3, Color, Vector3, Color)"/>
	public static PrimitiveBuilder<VertexPositionColor> LineSegment(Vector2 start, Color startColor, Vector2 end, Color endColor) => LineSegment(new Vector3(start, 0f), startColor, new Vector3(end, 0f), endColor);

	/// <summary>
	/// Creates a builder for a single line segment with the specified start and end positions and colors.<para/>
	/// Defined primitives:
	/// <list type="bullet">
	/// <item>[0] : Line</item>
	/// </list>
	/// </summary>
	/// <param name="start">The position of the starting endpoint of the line, relative to where the primitive will be drawn.</param>
	/// <param name="startColor">The color for the starting endpoint of the line.</param>
	/// <param name="end">The position of the ending endpoint of the line, relative to where the primitive will be drawn.</param>
	/// <param name="endColor">The color for the ending endpoint of the line.</param>
	/// <returns>The builder instance.</returns>
	public static PrimitiveBuilder<VertexPositionColor> LineSegment(Vector3 start, Color startColor, Vector3 end, Color endColor) {
		return PrimitiveBuilder.CreateLineList<VertexPositionColor>(count: 1)
			.PushPrimitive(
				Line.Create(
					start: new VertexPositionColor(start, startColor),
					end: new VertexPositionColor(end, endColor)
				)
			);
	}
	#endregion

	#region PolyLine
	/// <inheritdoc cref="PolyLine(Span{Vector3}, Color)"/>
	public static PrimitiveBuilder<VertexPositionColor> PolyLine(Span<Vector2> positions, Color color)
		=> PolyLine(positions, color, static (p, c) => new VertexPositionColor(new Vector3(p, 0f), c));

	/// <summary>
	/// Creates a builder for a polyline with the specified positions and color.<para/>
	/// Defined primitives for N positions:
	/// <list type="bullet">
	/// <item>[0] : Line</item>
	/// <item>[1 .. N-2] : Point</item>
	/// </list>
	/// </summary>
	/// <param name="positions">The positions of the polyline's vertices, relative to where the primitive will be drawn.</param>
	/// <param name="color">The color for all vertices of the polyline.</param>
	/// <returns>The builder instance.</returns>
	public static PrimitiveBuilder<VertexPositionColor> PolyLine(Span<Vector3> positions, Color color)
		=> PolyLine(positions, color, static (p, c) => new VertexPositionColor(p, c));

	/// <inheritdoc cref="PolyLine(Span{ValueTuple{Vector3, Color}})"/>
	public static PrimitiveBuilder<VertexPositionColor> PolyLine(Span<(Vector2, Color)> vertices)
		=> PolyLine(vertices, 0, static (t, _) => new VertexPositionColor(new Vector3(t.Item1, 0f), t.Item2));

	/// <summary>
	/// Creates a builder for a polyline with the specified positions and colors.<para/>
	/// Defined primitives for N vertices:
	/// <list type="bullet">
	/// <item>[0] : Line</item>
	/// <item>[1 .. N-2] : Point</item>
	/// </list>
	/// </summary>
	/// <param name="vertices">The positions and colors of the polyline's vertices, relative to where the primitive will be drawn.</param>
	/// <returns>The builder instance.</returns>
	public static PrimitiveBuilder<VertexPositionColor> PolyLine(Span<(Vector3, Color)> vertices)
		=> PolyLine(vertices, 0, static (t, _) => new VertexPositionColor(t.Item1, t.Item2));

	private static PrimitiveBuilder<VertexPositionColor> PolyLine<TFrom, TExtra>(
		Span<TFrom> vertexData,
		TExtra extraData,
		Func<TFrom, TExtra, VertexPositionColor> transform
	) {
		if (vertexData.Length < 2)
			throw new ArgumentException("A polyline must have at least 2 vertices.", nameof(vertexData));

		Span<Point<VertexPositionColor>> pointPrimitives = stackalloc Point<VertexPositionColor>[vertexData.Length - 2];

		for (int i = 0; i < pointPrimitives.Length; i++)
			pointPrimitives[i] = Point.Create(transform(vertexData[i + 2], extraData));

		return PrimitiveBuilder.CreateLineStrip<VertexPositionColor>(count: 1 + pointPrimitives.Length, connectLastToFirst: false)
			.PushPrimitive(
				Line.Create(
					start: transform(vertexData[0], extraData),
					end: transform(vertexData[1], extraData)
				)
			)
			.PushPrimitives(
				pointPrimitives
			);
	}
	#endregion

	#region HollowPolygon
	/// <inheritdoc cref="HollowPolygon(Span{Vector3}, Color)"/>
	public static PrimitiveBuilder<VertexPositionColor> HollowPolygon(Span<Vector2> positions, Color color)
		=> HollowPolygon(positions, color, static (p, c) => new VertexPositionColor(new Vector3(p, 0f), c));

	/// <summary>
	/// Creates a builder for a hollow polygon with the specified positions and color.<para/>
	/// Defined primitives for N positions:
	/// <list type="bullet">
	/// <item>[0] : Line</item>
	/// <item>[1 .. N-2] : Point</item>
	/// </list>
	/// </summary>
	/// <param name="positions">The positions of the polygon's vertices, relative to where the primitive will be drawn.</param>
	/// <param name="color">The color for all vertices of the polygon.</param>
	/// <returns>The builder instance.</returns>
	public static PrimitiveBuilder<VertexPositionColor> HollowPolygon(Span<Vector3> positions, Color color)
		=> HollowPolygon(positions, color, static (p, c) => new VertexPositionColor(p, c));

	/// <inheritdoc cref="HollowPolygon(Span{ValueTuple{Vector2, Color}})"/>
	public static PrimitiveBuilder<VertexPositionColor> HollowPolygon(Span<(Vector2, Color)> vertices)
		=> HollowPolygon(vertices, 0, static (t, _) => new VertexPositionColor(new Vector3(t.Item1, 0f), t.Item2));

	/// <summary>
	/// Creates a builder for a hollow polygon with the specified positions and colors.<para/>
	/// Defined primitives for N vertices:
	/// <list type="bullet">
	/// <item>[0] : Line</item>
	/// <item>[1 .. N-2] : Point</item>
	/// </list>
	/// </summary>
	/// <param name="vertices">The positions and colors of the polygon's vertices, relative to where the primitive will be drawn.</param>
	/// <returns>The builder instance.</returns>
	public static PrimitiveBuilder<VertexPositionColor> HollowPolygon(Span<(Vector3, Color)> vertices)
		=> HollowPolygon(vertices, 0, static (t, _) => new VertexPositionColor(t.Item1, t.Item2));

	private static PrimitiveBuilder<VertexPositionColor> HollowPolygon<TFrom, TExtra>(
		Span<TFrom> vertexData,
		TExtra extraData,
		Func<TFrom, TExtra, VertexPositionColor> transform
	) {
		if (vertexData.Length < 3)
			throw new ArgumentException("A polygon must have at least 3 vertices.", nameof(vertexData));

		Span<Point<VertexPositionColor>> pointPrimitives = stackalloc Point<VertexPositionColor>[vertexData.Length - 2];

		for (int i = 0; i < pointPrimitives.Length; i++)
			pointPrimitives[i] = Point.Create(transform(vertexData[i + 2], extraData));

		return PrimitiveBuilder.CreateLineStrip<VertexPositionColor>(count: 1 + pointPrimitives.Length, connectLastToFirst: true)
			.PushPrimitive(
				Line.Create(
					start: transform(vertexData[0], extraData),
					end: transform(vertexData[1], extraData)
				)
			)
			.PushPrimitives(
				pointPrimitives
			);
	}
	#endregion

	#region ThickPolygon
	/// <inheritdoc cref="ThickPolygon(Span{Vector3}, Color, float)"/>
	public static PrimitiveBuilder<VertexPositionColor> ThickPolygon(Span<Vector2> positions, Color color, float thickness)
		=> ThickPolygon(positions, color, thickness, static (p) => new Vector3(p, 0f), static (_, c) => c, static (_, c) => c);

	/// <summary>
	/// Creates a builder for a hollow polygon with the specified positions, color, and thickness.<para/>
	/// Defined primitives for N positions:
	/// <list type="bullet">
	/// <item>[0 .. 2N] : Point</item>
	/// </list>
	/// </summary>
	/// <param name="positions">
	/// The positions of the polygon's vertices, relative to where the primitive will be drawn.<br/>
	/// Must be defined in a clockwise order according to world space.<br/>
	/// Each position will create two vertices in the builder's vertex buffer: one for the inside edge and one for the outside edge.<br/>
	/// The outside vertex will be at index <c>i</c> and the inside vertex will be at index <c>i + N</c>.
	/// </param>
	/// <param name="color">The color for all vertices of the polygon.</param>
	/// <param name="thickness">The thickness of the polygon's edges.</param>
	/// <returns>The builder instance.</returns>
	public static PrimitiveBuilder<VertexPositionColor> ThickPolygon(Span<Vector3> positions, Color color, float thickness)
		=> ThickPolygon(positions, color, thickness, static (p) => p, static (_, c) => c, static (_, c) => c);

	/// <inheritdoc cref="ThickPolygon(Span{Vector3}, Color, Color, float)"/>
	public static PrimitiveBuilder<VertexPositionColor> ThickPolygon(Span<Vector2> positions, Color innerColor, Color outerColor, float thickness)
		=> ThickPolygon(positions, (innerColor, outerColor), thickness, static (p) => new Vector3(p, 0f), static (p, c) => c.innerColor, static (p, c) => c.outerColor);

	/// <summary>
	/// Creates a builder for a hollow polygon with the specified positions, colors, and thickness.<br/>
	/// The first vertex defines the direction of the "inside" of the polygon.<br/>
	/// Of the two angles formed by the first vertex's adjacent vertices, the direction to the "inside" is the smaller angle.<para/>
	/// Defined primitives for N positions:
	/// <list type="bullet">
	/// <item>[0 .. 2N] : Point</item>
	/// </list>
	/// </summary>
	/// <param name="positions">
	/// The positions of the polygon's vertices, relative to where the primitive will be drawn.<br/>
	/// Must be defined in a clockwise order according to world space.<br/>
	/// Each position will create two vertices in the builder's vertex buffer: one for the inside edge and one for the outside edge.<br/>
	/// The outside vertex will be at index <c>i</c> and the inside vertex will be at index <c>i + N</c>
	/// </param>
	/// <param name="innerColor">The color for the inner vertices of the polygon.</param>
	/// <param name="outerColor">The color for the outer vertices of the polygon.</param>
	/// <param name="thickness">The thickness of the polygon's edges.</param>
	/// <returns>The builder instance.</returns>
	public static PrimitiveBuilder<VertexPositionColor> ThickPolygon(Span<Vector3> positions, Color innerColor, Color outerColor, float thickness)
		=> ThickPolygon(positions, (innerColor, outerColor), thickness, static (p) => p, static (_, c) => c.innerColor, static (_, c) => c.outerColor);

	/// <inheritdoc cref="ThickPolygon(Span{ValueTuple{Vector3, Color}}, float)"/>
	public static PrimitiveBuilder<VertexPositionColor> ThickPolygon(Span<(Vector2, Color)> vertices, float thickness)
		=> ThickPolygon(vertices, 0, thickness, static (t) => new Vector3(t.Item1, 0f), static (t, _) => t.Item2, static (t, _) => t.Item2);

	/// <summary>
	/// Creates a builder for a hollow polygon with the specified positions, colors, and thickness.<br/>
	/// Defined primitives for N vertices:
	/// <list type="bullet">
	/// <item>[0 .. 2N] : Point</item>
	/// </list>
	/// </summary>
	/// <param name="vertices">
	/// The positions and colors of the polygon's vertices, relative to where the primitive will be drawn.<br/>
	/// Must be defined in a clockwise order according to world space.<br/>
	/// Each position will create two vertices in the builder's vertex buffer: one for the inside edge and one for the outside edge.<br/>
	/// The outside vertex will be at index <c>i</c> and the inside vertex will be at index <c>i + N</c>
	/// </param>
	/// <param name="thickness">The thickness of the polygon's edges.</param>
	/// <returns>The builder instance.</returns>
	public static PrimitiveBuilder<VertexPositionColor> ThickPolygon(Span<(Vector3, Color)> vertices, float thickness)
		=> ThickPolygon(vertices, 0, thickness, static (t) => t.Item1, static (t, _) => t.Item2, static (t, _) => t.Item2);

	private static PrimitiveBuilder<VertexPositionColor> ThickPolygon<TFrom, TExtra>(
		Span<TFrom> vertexData,
		TExtra extraData,
		float thickness,
		Func<TFrom, Vector3> getPosition,
		Func<TFrom, TExtra, Color> getInnerColor,
		Func<TFrom, TExtra, Color> getOuterColor
	) {
		if (vertexData.Length < 3)
			throw new ArgumentException("A polygon must have at least 3 vertices.", nameof(vertexData));

		if (thickness <= 0f)
			throw new ArgumentOutOfRangeException(nameof(thickness), $"Line thickness was negative or zero ({thickness})");

		/*
		
		Some explanation may be needed to understand this builder.
		Essentially, the polygon will be constructed of trapezoids.

		In particular, the corners need to be arranged as follows (for a rectangle):

		A.......................B
		|:'.           ....'':' |    Triangles:
		| : '.  ...''''    .'   |      AHD  (CW)
		|  :  'E---------F'     |      HDG  (CCW)
		|   :  |         |:     |      DGC  (CW)
		|    : |         | :    |      GCF  (CCW)
		|     :|         |  :   |      CFB  (CW)
		|     .H---------G.  :  |      FBE  (CCW)
		|   .'    ....'''  '. : |      BEA  (CW)
		| .:..''''           '.:|      EAH  (CCW)
		D'''''''''''''''''''''''C

		This is easy to implement using a TriangleStrip primitive builder.

		As for the locations of vertices A and C, some simple math can be used for that.

		     T = thickness
		offset = sqrt( (T/2)^2 + (T/2)^2 )
		       = sqrt( (T^2)/4 + (T^2)/4 )
		       = sqrt( (T^2)/2 )
		       = T * sqrt( 1/2 )
		       = T * 0.7071067811865476

		     A = target - Vector(offset)
		     C = target + Vector(offset)

		Where target is the position from the parameter list.

		*/

		const float SQRT_1_OVER_2 = 0.7071067811865476f;

		float offset = thickness * SQRT_1_OVER_2;

		Span<Vector3> vertexPositions = stackalloc Vector3[vertexData.Length];
		for (int i = 0; i < vertexData.Length; i++)
			vertexPositions[i] = getPosition(vertexData[i]);

		Span<Vector3> actualVertexPositions = stackalloc Vector3[vertexData.Length * 2];
		int lastIndex = vertexData.Length - 1;

		Vector3 previousDirection = default;
		for (int current = 0, previous = lastIndex, next = 1; current <= lastIndex; current++, previous++, next++) {
			// By adding vectors representing the two edges, we get a vector pointing diagonally outwards from the corner
			// It's important that the current vertex is considered the relative origin so that the direction is correct
			Vector3 edge1 = vertexPositions[previous] - vertexPositions[current];
			Vector3 edge2 = vertexPositions[next] - vertexPositions[current];
			Vector3 direction = Vector3.Normalize(edge1 + edge2);

			if (current > 0) {
				// Ensure that for concave polygons, the "positive" direction refers to the same side of the polygon for all vertices
				if (Vector3.Dot(direction, previousDirection) < 0f)
					direction = -direction;
			}

			// This direction will be used for the "inside" vertex (e.g. E) and the opposite direction for the "outside" vertex (e.g. A)
			actualVertexPositions[current] = vertexPositions[current] - direction * offset;
			actualVertexPositions[current + vertexData.Length] = vertexPositions[current] + direction * offset;

			// Update the indices before they're incremented
			if (previous == lastIndex)
				previous = -1;
			if (next == lastIndex)
				next = -1;

			// Store the direction for use by the next iteration
			previousDirection = direction;
		}

		Span<Point<VertexPositionColor>> pointPrimitives = stackalloc Point<VertexPositionColor>[actualVertexPositions.Length];
		int halfLength = vertexData.Length;

		for (int i = 0, v = 0; i < pointPrimitives.Length; i++, v++) {
			pointPrimitives[i] = Point.Create(
				new VertexPositionColor(
					actualVertexPositions[i],
					i < halfLength ? getOuterColor(vertexData[v], extraData) : getInnerColor(vertexData[v], extraData)
				)
			);

			if (v == lastIndex)
				v = -1;
		}

		// Now we can actually create the builder
		var builder = new PrimitiveBuilder<VertexPositionColor>(
			new ThickPolygonAcceptor<VertexPositionColor>(primitiveCount: pointPrimitives.Length),
			PrimitiveType.TriangleStrip
		);

		return builder.PushPrimitives(pointPrimitives);
	}
	#endregion

	#region FilledPolygon
	/// <inheritdoc cref="FilledPolygon(Span{Vector3}, Color)"/>
	public static PrimitiveBuilder<VertexPositionColor> FilledPolygon(Span<Vector2> positions, Color color)
		=> FilledPolygon(positions, color, static (p, c) => new VertexPositionColor(new Vector3(p, 0f), c));

	/// <summary>
	/// Creates a filled polygon primitive using the specified positions and color.<para/>
	/// Defined primitives for N positions:
	/// <list type="bullet">
	/// <item>[0] : Triangle</item>
	/// <item>[1 .. N-3] : Point</item>
	/// </list>
	/// </summary>
	/// <param name="positions">The positions of the polygon's vertices, relative to where the primitive will be drawn.</param>
	/// <param name="color">The color for all vertices of the polygon.</param>
	/// <returns>The builder instance.</returns>
	public static PrimitiveBuilder<VertexPositionColor> FilledPolygon(Span<Vector3> positions, Color color)
		=> FilledPolygon(positions, color, static (p, c) => new VertexPositionColor(p, c));

	/// <inheritdoc cref="FilledPolygon(Span{ValueTuple{Vector2, Color}})"/>
	public static PrimitiveBuilder<VertexPositionColor> FilledPolygon(Span<(Vector2, Color)> vertices)
		=> FilledPolygon(vertices, 0, static (t, _) => new VertexPositionColor(new Vector3(t.Item1, 0f), t.Item2));

	/// <summary>
	/// Creates a filled polygon primitive using the specified positions and colors.<para/>
	/// Defined primitives for N vertices:
	/// <list type="bullet">
	/// <item>[0] : Triangle</item>
	/// <item>[1 .. N-3] : Point</item>
	/// </list>
	/// </summary>
	/// <param name="vertices">The positions and colors of the polygon's vertices, relative to where the primitive will be drawn.</param>
	/// <returns>The builder instance.</returns>
	public static PrimitiveBuilder<VertexPositionColor> FilledPolygon(Span<(Vector3, Color)> vertices)
		=> FilledPolygon(vertices, 0, static (t, _) => new VertexPositionColor(t.Item1, t.Item2));

	private static PrimitiveBuilder<VertexPositionColor> FilledPolygon<TFrom, TExtra>(
		Span<TFrom> vertexData,
		TExtra extraData,
		Func<TFrom, TExtra, VertexPositionColor> transform
	) {
		if (vertexData.Length < 3)
			throw new ArgumentException("A polygon must have at least 3 vertices.", nameof(vertexData));

		Span<Point<VertexPositionColor>> pointPrimitives = stackalloc Point<VertexPositionColor>[vertexData.Length - 3];

		for (int i = 0; i < pointPrimitives.Length; i++)
			pointPrimitives[i] = Point.Create(transform(vertexData[i + 3], extraData));

		var builder = new PrimitiveBuilder<VertexPositionColor>(
			new FilledPolygonAcceptor<VertexPositionColor>(primitiveCount: 1 + pointPrimitives.Length),
			PrimitiveType.TriangleList
		);

		return builder
			.PushPrimitive(
				Triangle.Create(
					a: transform(vertexData[0], extraData),
					b: transform(vertexData[1], extraData),
					c: transform(vertexData[2], extraData)
				)
			)
			.PushPrimitives(
				pointPrimitives
			);
	}
	#endregion
}

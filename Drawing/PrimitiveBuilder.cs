using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace GraphicsLib.Drawing;

/// <inheritdoc cref="PrimitiveBuilder{TVertex}"/>
public static class PrimitiveBuilder {
	/// <summary>
	/// Creates a builder for a sequence of isolated triangles.
	/// </summary>
	/// <typeparam name="TVertex">The vertex type.</typeparam>
	/// <param name="count">The number of triangles to be built.</param>
	/// <returns>The builder instance.</returns>
	public static PrimitiveBuilder<TVertex> CreateTriangleList<TVertex>(int count)
		where TVertex : struct, IVertexType
	{
		return new PrimitiveBuilder<TVertex>(
			new RestrictedPrimitiveAcceptor<TVertex, Triangle<TVertex>, Triangle<TVertex>>(
				count,
				PrimitiveType.TriangleList,
				false
			),
			PrimitiveType.TriangleList
		);
	}

	/// <summary>
	/// Creates a builder for a sequence of connected triangles.<br/>
	/// The first primitive must be a triangle, and each subsequent primitive must be a point.<br/>
	/// Each triangle after the first will share an edge with the previous triangle in the strip.
	/// </summary>
	/// <typeparam name="TVertex">The vertex type.</typeparam>
	/// <param name="count">The number of triangles to be built.</param>
	/// <param name="connectLastToFirst">If <see langword="true"/>, the last triangle's last vertex will be the same as the first triagnle's first vertex.</param>
	/// <returns>The builder instance.</returns>
	public static PrimitiveBuilder<TVertex> CreateTriangleStrip<TVertex>(int count, bool connectLastToFirst = false)
		where TVertex : struct, IVertexType
	{
		return new PrimitiveBuilder<TVertex>(
			new RestrictedPrimitiveAcceptor<TVertex, Triangle<TVertex>, Point<TVertex>>(
				count,
				PrimitiveType.TriangleStrip,
				connectLastToFirst
			),
			PrimitiveType.TriangleStrip
		);
	}

	/// <summary>
	/// Creates a builder for a sequence of isolated line segments.
	/// </summary>
	/// <typeparam name="TVertex">The vertex type.</typeparam>
	/// <param name="count">The number of line segments to be built.</param>
	/// <returns>The builder instance.</returns>
	public static PrimitiveBuilder<TVertex> CreateLineList<TVertex>(int count)
		where TVertex : struct, IVertexType
	{
		return new PrimitiveBuilder<TVertex>(
			new RestrictedPrimitiveAcceptor<TVertex, Line<TVertex>, Line<TVertex>>(
				count,
				PrimitiveType.LineList,
				false
			),
			PrimitiveType.LineList
		);
	}

	/// <summary>
	/// Creates a builder for a sequence of connected line segments.<br/>
	/// The first primitive must be a line, and each subsequent primitive must be a point.
	/// </summary>
	/// <typeparam name="TVertex">The vertex type.</typeparam>
	/// <param name="count">The number of line segments to be built.</param>
	/// <param name="connectLastToFirst">If <see langword="true"/>, the last line segment's ending vertex will be the same as the first line segment's starting vertex.</param>
	/// <returns>The builder instance.</returns>
	public static PrimitiveBuilder<TVertex> CreateLineStrip<TVertex>(int count, bool connectLastToFirst = false)
		where TVertex : struct, IVertexType
	{
		return new PrimitiveBuilder<TVertex>(
			new RestrictedPrimitiveAcceptor<TVertex, Line<TVertex>, Point<TVertex>>(
				count,
				PrimitiveType.LineStrip,
				connectLastToFirst
			),
			PrimitiveType.LineStrip
		);
	}

	/// <summary>
	/// Creates a builder for a sequence of isolated points.
	/// </summary>
	/// <typeparam name="TVertex">The vertex type.</typeparam>
	/// <param name="count">The number of points to be built.</param>
	/// <returns>The builder instance.</returns>
	public static PrimitiveBuilder<TVertex> CreatePointList<TVertex>(int count)
		where TVertex : struct, IVertexType
	{
		return new PrimitiveBuilder<TVertex>(
			new RestrictedPrimitiveAcceptor<TVertex, Point<TVertex>, Point<TVertex>>(
				count,
				PrimitiveType.PointListEXT,
				false
			),
			PrimitiveType.PointListEXT
		);
	}

	/// <summary>
	/// Creates a builder for a sequence of isolated quads.<br/>
	/// Each quad is made up of two triangles.
	/// </summary>
	/// <typeparam name="TVertex">The vertex type.</typeparam>
	/// <param name="count">The number of quads to be built.</param>
	/// <returns>The builder instance.</returns>
	public static PrimitiveBuilder<TVertex> CreateQuad<TVertex>(int count)
		where TVertex : struct, IVertexType
	{
		return new PrimitiveBuilder<TVertex>(
			new RestrictedPrimitiveAcceptor<TVertex, Quad<TVertex>, Quad<TVertex>>(
				count * 2,
				PrimitiveType.TriangleList,
				false
			),
			PrimitiveType.TriangleList
		);
	}
}

/// <summary>
/// A builder for creating the vertex and index data for a sequence of primitives.
/// </summary>
/// <typeparam name="TVertex">The vertex type.</typeparam>
public class PrimitiveBuilder<TVertex>
	where TVertex : struct, IVertexType
{
	private readonly PrimitiveAcceptor<TVertex> _acceptor;
	private readonly PrimitiveType _mode;

	/// <summary>
	/// An optional adjustment matrix to apply to all vertices when rendering the built primitives.
	/// </summary>
	public Matrix? Transform { get; set; }

	/// <summary>
	/// An optional custom shader to use when rendering the built primitives.
	/// </summary>
	public Effect Shader { get; set; }

	/// <summary>
	/// If an error occurred during rendering, this will contain the exception that was thrown.
	/// </summary>
	public Exception RenderError { get; internal set; }

	/// <summary>
	/// Creates a new <see cref="PrimitiveBuilder{TVertex}"/> instance.
	/// </summary>
	/// <param name="acceptor">The primitive acceptor that will handle the actual building of the vertex and index data.</param>
	/// <param name="mode">The primitive type to use when rendering the built primitives.</param>
	public PrimitiveBuilder(PrimitiveAcceptor<TVertex> acceptor, PrimitiveType mode) {
		ArgumentNullException.ThrowIfNull(acceptor);

		_acceptor = acceptor;
		_mode = mode;
	}

	/// <summary>
	/// Extracts the data needed to render the built primitives.
	/// </summary>
	/// <param name="vertices">The array of vertices.</param>
	/// <param name="indices">The array of indices.</param>
	/// <param name="primitiveCount">The number of primitives built.</param>
	/// <param name="mode">The primitive type to use when rendering.</param>
	public void GetData(out TVertex[] vertices, out short[] indices, out int primitiveCount, out PrimitiveType mode) {
		_acceptor.GetData(out vertices, out indices, out primitiveCount);
		mode = _mode;
	}

	/// <summary>
	/// Adds the specified primitive to the vertex buffer for this builder.
	/// </summary>
	/// <typeparam name="T">The primitive type.</typeparam>
	/// <param name="primitive">The primitive instance.</param>
	public PrimitiveBuilder<TVertex> PushPrimitive<T>(in T primitive)
		where T : struct, IPrimitive<T, TVertex>
	{
		_acceptor.Push(in primitive);
		return this;
	}

	/// <summary>
	/// Adds the specified primitives to the vertex buffer for this builder.
	/// </summary>
	/// <typeparam name="T">The primitive type.</typeparam>
	/// <param name="primitives">A span containing the primitive instances to add.</param>
	/// <returns>This builder instance.</returns>
	public PrimitiveBuilder<TVertex> PushPrimitives<T>(ReadOnlySpan<T> primitives)
		where T : struct, IPrimitive<T, TVertex>
	{
		foreach (ref readonly T primitive in primitives)
			_acceptor.Push(in primitive);

		return this;
	}

	/// <summary>
	/// Adds the specified primitives to the vertex buffer for this builder.
	/// </summary>
	/// <typeparam name="T">The primitive type.</typeparam>
	/// <param name="primitives">A span containing the primitive instances to add.</param>
	public PrimitiveBuilder<TVertex> PushPrimitives<T>(in Span<T> primitives)
		where T : struct, IPrimitive<T, TVertex>
	{
		foreach (ref readonly T primitive in primitives)
			_acceptor.Push(in primitive);

		return this;
	}

	/// <summary>
	/// Reads back a previously pushed primitive from the vertex buffer.
	/// </summary>
	/// <typeparam name="T">The primitive type.</typeparam>
	/// <param name="index">The index of the primitive to read back.</param>
	/// <returns>The primitive instance at the specified index.</returns>
	public T ReadPrimitive<T>(int index)
		where T : struct, IPrimitive<T, TVertex>
	{
		return _acceptor.Read<T>(index);
	}

	/// <summary>
	/// Reads back a previously pushed <see cref="Point{TVertex}"/> primitive from the vertex buffer.
	/// </summary>
	/// <param name="index">The index of the primitive to read back.</param>
	/// <returns>The primitive instance at the specified index.</returns>
	public Point<TVertex> ReadPoint(int index) => ReadPrimitive<Point<TVertex>>(index);

	/// <summary>
	/// Reads back a previously pushed <see cref="Line{TVertex}"/> primitive from the vertex buffer.
	/// </summary>
	/// <param name="index">The index of the primitive to read back.</param>
	/// <returns>The primitive instance at the specified index.</returns>
	public Line<TVertex> ReadLine(int index) => ReadPrimitive<Line<TVertex>>(index);

	/// <summary>
	/// Reads back a previously pushed <see cref="Triangle{TVertex}"/> primitive from the vertex buffer.
	/// </summary>
	/// <param name="index">The index of the primitive to read back.</param>
	/// <returns>The primitive instance at the specified index.</returns>
	public Triangle<TVertex> ReadTriangle(int index) => ReadPrimitive<Triangle<TVertex>>(index);

	/// <summary>
	/// Reads back a previously pushed <see cref="Quad{TVertex}"/> primitive from the vertex buffer.
	/// </summary>
	/// <param name="index">The index of the primitive to read back.</param>
	/// <returns>The primitive instance at the specified index.</returns>
	public Quad<TVertex> ReadQuad(int index) => ReadPrimitive<Quad<TVertex>>(index);

	/// <summary>
	/// Directly writes a primitive to the vertex buffer.
	/// </summary>
	/// <typeparam name="T">The primitive type.</typeparam>
	/// <param name="index">The index of the primitive to write to.</param>
	/// <param name="primitive">The primitive instance to write.</param>
	public void WritePrimitive<T>(int index, in T primitive)
		where T : struct, IPrimitive<T, TVertex>
	{
		_acceptor.Write(index, primitive);
	}
}


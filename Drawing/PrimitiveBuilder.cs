using GraphicsLib.Utility;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;

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

	/// <summary>
	/// Creates a translation matrix that moves the origin point for a <see cref="PrimitiveBuilder{TVertex}"/> to the specified world coordinate on screen.<br/>
	/// The screen's position is implied to be <see cref="Main.screenPosition"/>
	/// </summary>
	/// <param name="worldCoordinate">The world coordinate to move the origin to.</param>
	/// <returns>The translation matrix.</returns>
	public static Matrix GetOnScreenOrigin(Vector2 worldCoordinate) => GetOnScreenOrigin(worldCoordinate, Main.screenPosition);

	/// <summary>
	/// Creates a translation matrix that moves the origin point for a <see cref="PrimitiveBuilder{TVertex}"/> to the specified world coordinate on screen.
	/// </summary>
	/// <param name="worldCoordinate">The world coordinate to move the origin to.</param>
	/// <param name="screenPosition">The current screen position in the world.</param>
	/// <returns>The translation matrix.</returns>
	public static Matrix GetOnScreenOrigin(Vector2 worldCoordinate, Vector2 screenPosition) => Matrix.CreateTranslation(new Vector3(worldCoordinate - screenPosition, 0f));
}

/// <summary>
/// A builder for creating the vertex and index data for a sequence of primitives.
/// </summary>
/// <typeparam name="TVertex">The vertex type.</typeparam>
public class PrimitiveBuilder<TVertex>
	where TVertex : struct, IVertexType
{
	/// <summary>
	/// The primitive acceptor that will handle the actual building of the vertex and index data.
	/// </summary>
	public readonly PrimitiveAcceptor<TVertex> acceptor;

	/// <summary>
	/// The primitive type to use when rendering the built primitives.
	/// </summary>
	public readonly PrimitiveType mode;

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

		this.acceptor = acceptor;
		this.mode = mode;
	}

	/// <summary>
	/// Creates a new builder that is a copy of this builder, including any vertex and index data that has been built so far.
	/// </summary>
	/// <returns>The cloned builder instance.</returns>
	public PrimitiveBuilder<TVertex> Clone() {
		var clone = NewInstance();
		CopyTo(clone);
		return clone;
	}

	/// <summary>
	/// Copies the state of this builder to another builder of the same type.<br/>
	/// This is used when cloning, and you are encouraged to copy any vertex and index data to the new builder.
	/// </summary>
	/// <param name="clone">The builder to copy state to.</param>
	public virtual void CopyTo(PrimitiveBuilder<TVertex> clone) {
		acceptor.CopyTo(clone.acceptor);
		clone.Transform = Transform;
		clone.Shader = Shader;
		clone.RenderError = RenderError;
	}

	/// <summary>
	/// Creates a new instance of the same type as this builder.<br/>
	/// This is used when cloning the builder.
	/// </summary>
	/// <returns>The new builder instance.</returns>
	public virtual PrimitiveBuilder<TVertex> NewInstance() {
		return new PrimitiveBuilder<TVertex>(acceptor.NewInstance(), mode);
	}

	/// <summary>
	/// Adds the specified primitive to the vertex buffer for this builder.
	/// </summary>
	/// <typeparam name="T">The primitive type.</typeparam>
	/// <param name="primitive">The primitive instance.</param>
	public PrimitiveBuilder<TVertex> PushPrimitive<T>(in T primitive)
		where T : struct, IPrimitive<T, TVertex>
	{
		acceptor.Push(in primitive);
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
			acceptor.Push(in primitive);

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
			acceptor.Push(in primitive);

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
		return acceptor.Read<T>(index);
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
		acceptor.Write(index, primitive);
	}

	/// <summary>
	/// Fills the vertex buffer with the specified primitive, starting at the specified index and continuing to the end of the buffer.
	/// </summary>
	/// <typeparam name="T">The primitive type.</typeparam>
	/// <param name="primitive">The primitive instance to write.</param>
	/// <param name="start">The index of the first primitive to write to.</param>
	public void FillPrimitive<T>(in T primitive, int start)
		where T : struct, IPrimitive<T, TVertex>
	{
		FillPrimitive(in primitive, start, int.MaxValue);
	}

	/// <summary>
	/// Fills the vertex buffer with the specified primitive, starting at the specified index and continuing for the specified count.<br/>
	/// If the count exceeds the number of available primitives, it will stop at the end of the buffer.
	/// </summary>
	/// <typeparam name="T">The primitive type.</typeparam>
	/// <param name="primitive">The primitive instance to write.</param>
	/// <param name="start">The index of the first primitive to write to.</param>
	/// <param name="count">The number of primitives to write.</param>
	/// <exception cref="InvalidOperationException"/>
	public void FillPrimitive<T>(in T primitive, int start, int count)
		where T : struct, IPrimitive<T, TVertex>
	{
		if (acceptor.Vertices is not { Length: > 0 })
			throw new InvalidOperationException("This builder did not have all primitives pushed yet");

		if (start < 0 || start >= acceptor.MaxPrimitives)
			ExceptionHelper.ThrowSequenceStartOutOfRange(start, acceptor.MaxPrimitives);

		if (count < 0)
			ExceptionHelper.ThrowSequenceCountNegative(count);

		int limit = (int)Math.Min((uint)(start + count), (uint)acceptor.MaxPrimitives);
		for (int i = start; i < limit; i++)
			acceptor.Write(i, primitive);
	}

	/// <summary>
	/// Gets an object that can access references to primitives previously pushed to this builder.
	/// </summary>
	public VertexReader GetReader() => new VertexReader(this);

	/// <summary>
	/// An object that can access references to primitives previously pushed to this builder.
	/// </summary>
	public readonly ref struct VertexReader {
		private readonly PrimitiveBuilder<TVertex> _source;
		private readonly int _numVertices;

		internal VertexReader(PrimitiveBuilder<TVertex> builder) {
			_source = builder;
			_numVertices = builder.acceptor.WritableVerticesInternal.Length;
		}

		/// <summary>
		/// Gets a reference to the primitive at the specified index.
		/// </summary>
		/// <typeparam name="TPrimitive">The primitive type.</typeparam>
		/// <param name="index">The index of the primitive to get a reference to.</param>
		/// <returns>A reference to the primitive at the specified index.</returns>
		/// <exception cref="InvalidOperationException"/>
		/// <exception cref="ArgumentOutOfRangeException"/>
		public PrimitiveRef<TPrimitive, TVertex> GetReference<TPrimitive>(int index)
			where TPrimitive : struct, IPrimitive<TPrimitive, TVertex>
		{
			if (!_source.acceptor.Accepts<TPrimitive>(index))
				throw new InvalidOperationException($"The acceptor for this reader's builder does not accept primitives of type {FriendlyName<TPrimitive>.Value} at index {index}");

			if (_numVertices == 0)
				throw new InvalidOperationException("This reader's builder did not have all primitives pushed yet when the reader was created");

			int baseVertex = _source.acceptor.GetVertexBase(index);

			if (baseVertex < 0)
				ExceptionHelper.ThrowSequenceStartNegative(baseVertex);
			else if (baseVertex + TPrimitive.VertexCount > _numVertices)
				ExceptionHelper.ThrowSequenceExceedsLength(baseVertex, TPrimitive.VertexCount, _numVertices);

			return new PrimitiveRef<TPrimitive, TVertex>(_source, (short)baseVertex);
		}

		/// <summary>
		/// Gets a reference to the <see cref="Point{TVertex}"/> primitive at the specified index.
		/// </summary>
		/// <param name="index">The index of the primitive to get a reference to.</param>
		/// <returns>A reference to the primitive at the specified index.</returns>
		/// <exception cref="InvalidOperationException"/>
		/// <exception cref="ArgumentOutOfRangeException"/>
		public PointRef<TVertex> GetPointReference(int index) => new PointRef<TVertex>(GetReference<Point<TVertex>>(index));

		/// <summary>
		/// Gets a reference to the <see cref="Line{TVertex}"/> primitive at the specified index.
		/// </summary>
		/// <param name="index">The index of the primitive to get a reference to.</param>
		/// <returns>A reference to the primitive at the specified index.</returns>
		/// <exception cref="InvalidOperationException"/>
		/// <exception cref="ArgumentOutOfRangeException"/>
		public LineRef<TVertex> GetLineReference(int index) => new LineRef<TVertex>(GetReference<Line<TVertex>>(index));

		/// <summary>
		/// Gets a reference to the <see cref="Triangle{TVertex}"/> primitive at the specified index.
		/// </summary>
		/// <param name="index">The index of the primitive to get a reference to.</param>
		/// <returns>A reference to the primitive at the specified index.</returns>
		/// <exception cref="InvalidOperationException"/>
		/// <exception cref="ArgumentOutOfRangeException"/>
		public TriangleRef<TVertex> GetTriangleReference(int index) => new TriangleRef<TVertex>(GetReference<Triangle<TVertex>>(index));

		/// <summary>
		/// Gets a reference to the <see cref="Quad{TVertex}"/> primitive at the specified index.
		/// </summary>
		/// <param name="index">The index of the primitive to get a reference to.</param>
		/// <returns>A reference to the primitive at the specified index.</returns>
		/// <exception cref="InvalidOperationException"/>
		/// <exception cref="ArgumentOutOfRangeException"/>
		public QuadRef<TVertex> GetQuadReference(int index) => new QuadRef<TVertex>(GetReference<Quad<TVertex>>(index));
	}
}


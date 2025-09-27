using GraphicsLib.Utility;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SerousCommonLib.API;
using System;

namespace GraphicsLib.Drawing;

/// <summary>
/// A helper class containing extension methods for manipulating <see cref="IPrimitive{TSelf, TVertex}"/> values.
/// </summary>
public static class PrimitiveExtensions {
	/// <inheritdoc cref="SetPosition{TVertex}(TVertex, Vector3)"/>
	public static TVertex SetPosition<TVertex>(this TVertex @this, Vector2 position)
		where TVertex : struct, IVertexType
	{
		return SetPosition(@this, new Vector3(position, 0f));
	}

	/// <summary>
	/// Sets the position of the given vertex to the specified position.
	/// </summary>
	/// <typeparam name="TVertex">The vertex type.</typeparam>
	/// <param name="this">The vertex to modify.</param>
	/// <param name="position">The new position to set.</param>
	/// <returns>The modified vertex.</returns>
	/// <exception cref="NotSupportedException"/>
	public static TVertex SetPosition<TVertex>(this TVertex @this, Vector3 position)
		where TVertex : struct, IVertexType
	{
		if (typeof(TVertex) == typeof(VertexPositionColor)) {
			Conversion.UnsafeCastRef<TVertex, VertexPositionColor>(ref @this).Position = position;
			return @this;
		} else if (typeof(TVertex) == typeof(VertexPositionColorTexture)) {
			Conversion.UnsafeCastRef<TVertex, VertexPositionColorTexture>(ref @this).Position = position;
			return @this;
		} else if (typeof(TVertex) == typeof(VertexPositionNormalTexture)) {
			Conversion.UnsafeCastRef<TVertex, VertexPositionNormalTexture>(ref @this).Position = position;
			return @this;
		} else if (typeof(TVertex) == typeof(VertexPositionTexture)) {
			Conversion.UnsafeCastRef<TVertex, VertexPositionTexture>(ref @this).Position = position;
			return @this;
		} else
			throw new NotSupportedException($"The vertex type {typeof(TVertex).FullNameUpgraded()} is not supported by this method");
	}

	/// <summary>
	/// Sets the color of the given vertex to the specified color.
	/// </summary>
	/// <typeparam name="TVertex">The vertex type.</typeparam>
	/// <param name="this">The vertex to modify.</param>
	/// <param name="color">The new color to set.</param>
	/// <returns>The modified vertex.</returns>
	/// <exception cref="NotSupportedException"/>
	public static TVertex SetColor<TVertex>(this TVertex @this, Color color)
		where TVertex : struct, IVertexType
	{
		if (typeof(TVertex) == typeof(VertexPositionColor)) {
			Conversion.UnsafeCastRef<TVertex, VertexPositionColor>(ref @this).Color = color;
			return @this;
		} else if (typeof(TVertex) == typeof(VertexPositionColorTexture)) {
			Conversion.UnsafeCastRef<TVertex, VertexPositionColorTexture>(ref @this).Color = color;
			return @this;
		} else
			throw new NotSupportedException($"The vertex type {typeof(TVertex).FullNameUpgraded()} is not supported by this method");
	}

	/// <summary>
	/// Sets the texture coordinate of the given vertex to the specified texture coordinate.
	/// </summary>
	/// <typeparam name="TVertex">The vertex type.</typeparam>
	/// <param name="this">The vertex to modify.</param>
	/// <param name="texCoord">The new texture coordinate to set.</param>
	/// <returns>The modified vertex.</returns>
	/// <exception cref="NotSupportedException"/>
	public static TVertex SetTextureCoordinate<TVertex>(this TVertex @this, Vector2 texCoord)
		where TVertex : struct, IVertexType
	{
		if (typeof(TVertex) == typeof(VertexPositionColorTexture)) {
			Conversion.UnsafeCastRef<TVertex, VertexPositionColorTexture>(ref @this).TextureCoordinate = texCoord;
			return @this;
		} else if (typeof(TVertex) == typeof(VertexPositionNormalTexture)) {
			Conversion.UnsafeCastRef<TVertex, VertexPositionNormalTexture>(ref @this).TextureCoordinate = texCoord;
			return @this;
		} else if (typeof(TVertex) == typeof(VertexPositionTexture)) {
			Conversion.UnsafeCastRef<TVertex, VertexPositionTexture>(ref @this).TextureCoordinate = texCoord;
			return @this;
		} else
			throw new NotSupportedException($"The vertex type {typeof(TVertex).FullNameUpgraded()} is not supported by this method");
	}
}

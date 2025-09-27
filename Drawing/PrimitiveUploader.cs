using GraphicsLib.Collections;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SerousCommonLib.API;
using SerousCommonLib.API.Rendering;
using System;
using System.Collections.Generic;
using Terraria;

namespace GraphicsLib.Drawing;

/// <summary>
/// An object which takes the vertices from a <see cref="PrimitiveBuilder{TVertex}"/> and uploads them to the GPU for rendering.
/// </summary>
public static class PrimitiveUploader {
	private static List<InstancePool<VertexBuffer>> _vertexBuffers = [];
	private static List<InstancePool<IndexBuffer>> _indexBuffers = [];

	internal static event Action OnUnload;

	internal static void Load() {

	}

	internal static void Unload() {
		OnUnload?.Invoke();
		OnUnload = null;

		foreach (var pool in _vertexBuffers)
			pool.Dispose();

		_vertexBuffers = null;

		foreach (var pool in _indexBuffers)
			pool.Dispose();

		_indexBuffers = null;
	}

	private static SpriteBatchCapture _batchParams;

	/// <summary>
	/// Attempts to end the current sprite batch, capturing its parameters for use in restarting it later via <see cref="RestartPreviousSpriteBatch"/>
	/// </summary>
	public static void EndCurrentSpriteBatch() {
		if (_batchParams is null) {
			_batchParams = new SpriteBatchCapture(Main.spriteBatch);
			Main.spriteBatch.End();
		} else
			throw new InvalidOperationException("The sprite batch has already been captured.  Cannot recapture until it has been restarted.");
	}

	/// <summary>
	/// Attempts to restart the previous sprite batch, given that it was ended via <see cref="EndCurrentSpriteBatch"/>
	/// </summary>
	public static void RestartPreviousSpriteBatch() {
		if (_batchParams is not null) {
			Main.spriteBatch.Begin(_batchParams);
			_batchParams = null;
		} else
			throw new InvalidOperationException("There is no captured sprite batch to restart.  Cannot restart until it has been ended.");
	}

	/// <summary>
	/// Renders the primitives built by the given <see cref="PrimitiveBuilder{TVertex}"/> instance.
	/// </summary>
	/// <typeparam name="TVertex">The vertex type.</typeparam>
	/// <param name="builder">The builder instance.</param>
	/// <returns><see langword="true"/> if the builder was rendered; otherwise, <see langword="false"/>.</returns>
	public static bool Render<TVertex>(PrimitiveBuilder<TVertex> builder)
		where TVertex : struct, IVertexType
	{
		var transforms = new SpriteBatchTransform(Main.spriteBatch);

		bool hadActiveBatch = Main.spriteBatch.IsActive();
		Effect customEffect = null;
		if (hadActiveBatch) {
			customEffect = _batchParams.customEffect;
			EndCurrentSpriteBatch();
		} else if (_batchParams is not null)
			customEffect = _batchParams.customEffect;  // Ensure that the custom shader is still being used

		try {
			builder.RenderError = null;

			builder.GetData(out var vertices, out var indices, out int primitiveCount, out PrimitiveType mode);

			if (vertices is not { Length: > 0 }) {
				// Something went wrong, skip rendering the builder
				return false;
			}

			var device = Main.graphics.GraphicsDevice;

			// Set up the vertex and index buffers
			var vertexBuffer = PrimitiveBuffers<TVertex>.GetOrCreateVertexBuffer(vertices.Length, device);

			device.SetVertexBuffer(null);  // Allow vertex data to be uploaded

			vertexBuffer.SetData(vertices, 0, vertices.Length, SetDataOptions.Discard);

			var indexBuffer = PrimitiveBuffers<TVertex>.GetOrCreateIndexBuffer(indices.Length, device);

			indexBuffer.SetData(indices);

			// Set up the device for rendering the primitive
			transforms.ApplyTransform(builder.Transform ?? Matrix.Identity);

			device.SetVertexBuffer(vertexBuffer);
			device.Indices = indexBuffer;

			if (customEffect is not null) {
				// Draw using each pass
				foreach (var pass in customEffect.CurrentTechnique.Passes) {
					pass.Apply();
					device.DrawIndexedPrimitives(mode, 0, 0, vertices.Length, 0, primitiveCount);
				}
			} else
				device.DrawIndexedPrimitives(mode, 0, 0, vertices.Length, 0, primitiveCount);

			return true;
		} catch (Exception ex) {
			// tModLoader should automatically log the exception as "silently caught"
			builder.RenderError = ex;
			return false;
		} finally {
			if (hadActiveBatch)
				RestartPreviousSpriteBatch();
		}
	}

	internal static int ReserveVertexBuffers() {
		var pool = InstancePool<VertexBuffer>.Reserve(16);
		_vertexBuffers.Add(pool);
		return pool.Index;
	}

	internal static int ReserveIndexBuffers() {
		var pool = InstancePool<IndexBuffer>.Reserve(16);
		_indexBuffers.Add(pool);
		return pool.Index;
	}
}

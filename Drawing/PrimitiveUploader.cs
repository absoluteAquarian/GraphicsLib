using GraphicsLib.Collections;
using GraphicsLib.Utility;
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

	private static Effect vpcShader;
	private static EffectParameter vpcWorldViewProj;
	private static Effect vptShader;
	private static EffectParameter vptWorldViewProj;
	private static Effect vpctShader;
	private static EffectParameter vpctWorldViewProj;

	internal static void Load() {
		ThreadUtils.InvokeOnMainThread(static () => {
			var device = Main.graphics.GraphicsDevice;

			vpcShader = new BasicEffect(device) {
				VertexColorEnabled = true
			};
			vpcWorldViewProj = vpcShader.Parameters["WorldViewProj"];

			vptShader = new BasicEffect(device) {
				TextureEnabled = true
			};
			vptWorldViewProj = vptShader.Parameters["WorldViewProj"];

			vpctShader = new BasicEffect(device) {
				VertexColorEnabled = true,
				TextureEnabled = true
			};
			vpctWorldViewProj = vpctShader.Parameters["WorldViewProj"];
		});
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
		if (hadActiveBatch)
			EndCurrentSpriteBatch();

		try {
			if (typeof(TVertex) != typeof(VertexPositionColor) && typeof(TVertex) != typeof(VertexPositionColorTexture) && typeof(TVertex) != typeof(VertexPositionTexture)) {
				// Not supported until we have proper shaders for the other vertex types
				throw new NotSupportedException($"The vertex type ({typeof(TVertex).FullNameUpgraded()}) is not supported by this method");
			}

			builder.RenderError = null;

			builder.acceptor.PrepareDataToUpload(out var vertices, out var indices, out int primitiveCount);

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
			var viewport = device.Viewport;

			Matrix orthographic = Matrix.CreateOrthographicOffCenter(
				left: 0,
				right: viewport.Width,
				bottom: viewport.Height,
				top: 0,
				zNearPlane: 0,
				zFarPlane: 1
			);

			Matrix projection;
			if (builder.Transform is Matrix adjustment)
				projection = adjustment * transforms.transformMatrix * orthographic;
			else
				projection = transforms.transformMatrix * orthographic;

			device.SetVertexBuffer(vertexBuffer);
			device.Indices = indexBuffer;

			PrimitiveType mode = builder.mode;

			if (builder.Shader is Effect shader) {
				shader.Parameters["WorldViewProj"]?.SetValue(projection);

				// Draw using each pass
				foreach (var pass in shader.CurrentTechnique.Passes) {
					pass.Apply();
					device.DrawIndexedPrimitives(mode, 0, 0, vertices.Length, 0, primitiveCount);
				}
			} else {
				// Ensure that the GraphicsDevice expects the vertex type being used
				if (typeof(TVertex) == typeof(VertexPositionColor)) {
					vpcWorldViewProj.SetValue(projection);
					vpcShader.CurrentTechnique.Passes[0].Apply();
				} else if (typeof(TVertex) == typeof(VertexPositionTexture)) {
					vptWorldViewProj.SetValue(projection);
					vptShader.CurrentTechnique.Passes[0].Apply();
				} else if (typeof(TVertex) == typeof(VertexPositionColorTexture)) {
					vpctWorldViewProj.SetValue(projection);
					vpctShader.CurrentTechnique.Passes[0].Apply();
				}

				device.DrawIndexedPrimitives(mode, 0, 0, vertices.Length, 0, primitiveCount);
			}

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

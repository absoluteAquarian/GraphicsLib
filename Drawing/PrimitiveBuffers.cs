using GraphicsLib.Collections;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace GraphicsLib.Drawing;

internal static class PrimitiveBuffers<TVertex>
	where TVertex : struct, IVertexType
{
	private static int _vertexPoolIndex;
	private static int _indexPoolIndex;
	private static Dictionary<int, PooledInstance<VertexBuffer>> _lengthToVertexBuffer;
	private static Dictionary<int, PooledInstance<IndexBuffer>> _lengthToIndexBuffer;
	private static bool _ready;

	internal static ref VertexBuffer GetOrCreateVertexBuffer(int length, GraphicsDevice device) {
		SetupUnloading();

		return ref GetOrCreateBuffer(
			length,
			device,
			_vertexPoolIndex,
			ref _lengthToVertexBuffer,
			static (d, l) => new VertexBuffer(d, typeof(TVertex), l, BufferUsage.WriteOnly)
		);
	}

	internal static ref IndexBuffer GetOrCreateIndexBuffer(int length, GraphicsDevice device) {
		SetupUnloading();

		return ref GetOrCreateBuffer(
			length,
			device,
			_indexPoolIndex,
			ref _lengthToIndexBuffer,
			static (d, l) => new IndexBuffer(d, IndexElementSize.SixteenBits, l, BufferUsage.WriteOnly)
		);
	}

	private static ref TBuffer GetOrCreateBuffer<TBuffer>(
		int length,
		GraphicsDevice device,
		int poolIndex,
		ref Dictionary<int, PooledInstance<TBuffer>> lengthToBuffer,
		Func<GraphicsDevice, int, TBuffer> createBuffer
	) {
		lengthToBuffer ??= [];

		if (lengthToBuffer.TryGetValue(length, out var instance))
			return ref instance.Data;

		var buffer = createBuffer(device, length);
		instance = InstancePool<TBuffer>.GetPool(poolIndex).CreateNextInstance();
		instance.Data = buffer;
		lengthToBuffer[length] = instance;

		return ref instance.Data;
	}

	private static void SetupUnloading() {
		if (!_ready) {
			_vertexPoolIndex = PrimitiveUploader.ReserveVertexBuffers();
			_indexPoolIndex = PrimitiveUploader.ReserveIndexBuffers();

			PrimitiveUploader.OnUnload += static () => {
				_vertexPoolIndex = -1;
				_indexPoolIndex = -1;
				_lengthToVertexBuffer = null;
				_lengthToIndexBuffer = null;
				_ready = false;
			};

			_ready = true;
		}
	}
}

using GraphicsLib.Utility.Extensions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;

namespace GraphicsLib.Meshes {
	/// <summary>
	/// A vertex mesh bound to a texture
	/// </summary>
	public sealed class Mesh {
		public Texture2D texture;
		public VertexPositionColorTexture[] mesh;
		public Effect shader;

		public readonly int ID;

		private VertexPositionColorTexture[] defaultMesh;

		private bool customIndexBuffer;
		private int[] iBufferArr;

		/// <summary>
		/// Creates a new <seealso cref="Mesh"/> instance
		/// </summary>
		/// <param name="texture">The texture</param>
		/// <param name="positions">The screen positions to draw this mesh's vertices at</param>
		/// <param name="textureCoords">The texture coordinates for this mesh's vertices.  Expected values per vertex range from 0 to 1, inclusive</param>
		/// <param name="color">The color for this mesh's vertices</param>
		/// <param name="shader">The shader to draw this mesh with</param>
		public Mesh(Texture2D texture, Vector2[] positions, Vector2[] textureCoords, Color color, Effect shader = null) {
			if(texture is null || texture.IsDisposed)
				throw new ArgumentException("Invalid texture (null or disposed)");

			this.texture = texture;
			this.shader = shader;

			if(positions is null)
				throw new ArgumentNullException(nameof(positions), "Position array was null");
			if(textureCoords is null)
				throw new ArgumentNullException(nameof(textureCoords), "Texture coordinate array was null");

			if(positions.Length % 3 != 0)
				throw new ArgumentException("Position array must have a length which is a multiple of 3");
			if(textureCoords.Length % 3 != 0)
				throw new ArgumentException("Texture coordinate array must have a length which is a multiple of 3");

			if(positions.Length != textureCoords.Length)
				throw new ArgumentException("Arrays must have equal length");

			mesh = new VertexPositionColorTexture[positions.Length];

			for(int i = 0; i < positions.Length; i++) {
				var tex = textureCoords[i];
				if(tex.X < 0 || tex.X > 1 || tex.Y < 0 || tex.Y > 1)
					throw new ArgumentOutOfRangeException($"textureCoords[{i}] had invalid values (X: {tex.X}, Y: {tex.Y}).  Expected values are between 0 and 1, inclusive.");

				ref var rMesh = ref mesh[i];
				rMesh.Position = new Vector3(positions[i], 0);
				rMesh.TextureCoordinate = tex;
				rMesh.Color = color;
			}

			defaultMesh = new VertexPositionColorTexture[mesh.Length];
			Array.Copy(mesh, defaultMesh, mesh.Length);

			ID = CoreMod.indirectMeshNextID++;
			CoreMod.indirectMeshes.Add(ID, this);
		}

		/// <summary>
		/// Creates a new <seealso cref="Mesh"/> instance
		/// </summary>
		/// <param name="texture">The texture</param>
		/// <param name="positions">The screen positions to draw this mesh's vertices at</param>
		/// <param name="textureCoords">The texture coordinates for this mesh's vertices.  Expected values per vertex range from 0 to 1, inclusive.</param>
		/// <param name="colors">The colors for this mesh's vertices</param>
		/// <param name="shader">The shader to draw this mesh with</param>
		public Mesh(Texture2D texture, Vector2[] positions, Vector2[] textureCoords, Color[] colors, Effect shader = null) {
			if(texture is null || texture.IsDisposed)
				throw new ArgumentException("Invalid texture (null or disposed)");

			this.texture = texture;
			this.shader = shader;

			if(positions is null)
				throw new ArgumentNullException(nameof(positions), "Position array was null");
			if(textureCoords is null)
				throw new ArgumentNullException(nameof(textureCoords), "Texture coordinate array was null");
			if(colors is null)
				throw new ArgumentNullException(nameof(colors), "Colors array was null");

			if(positions.Length % 3 != 0)
				throw new ArgumentException("Position array must have a length which is a multiple of 3");
			if(textureCoords.Length % 3 != 0)
				throw new ArgumentException("Texture coordinate array must have a length which is a multiple of 3");
			if(colors.Length % 3 != 0)
				throw new ArgumentException("Color array must have a length which is a multiple of 3");

			if(positions.Length != textureCoords.Length || positions.Length != colors.Length)
				throw new ArgumentException("Arrays must have equal length");

			mesh = new VertexPositionColorTexture[positions.Length];

			for(int i = 0; i < positions.Length; i++) {
				var tex = textureCoords[i];
				if(tex.X < 0 || tex.X > 1 || tex.Y < 0 || tex.Y > 1)
					throw new ArgumentOutOfRangeException($"textureCoords[{i}] had invalid values (X: {tex.X}, Y: {tex.Y}).  Expected values are between 0 and 1, inclusive.");

				ref var rMesh = ref mesh[i];
				rMesh.Position = new Vector3(positions[i], 0);
				rMesh.TextureCoordinate = tex;
				rMesh.Color = colors[i];
			}

			defaultMesh = new VertexPositionColorTexture[mesh.Length];
			Array.Copy(mesh, defaultMesh, mesh.Length);

			ID = CoreMod.indirectMeshNextID++;
			CoreMod.indirectMeshes.Add(ID, this);
		}

		/// <summary>
		/// Creates a new <seealso cref="Mesh"/> instance
		/// </summary>
		/// <param name="texture">The texture</param>
		/// <param name="vertices">The vertex data.  Expected values for each vertex's texture coordinates range from 0 to 1, inclusive</param>
		/// <param name="shader">The shader to draw this mesh with</param>
		public Mesh(Texture2D texture, VertexPositionColorTexture[] vertices, Effect shader = null) {
			this.texture = texture;
			this.shader = shader;

			mesh = new VertexPositionColorTexture[vertices.Length];

			for(int i = 0; i < vertices.Length; i++) {
				var tex = vertices[i].TextureCoordinate;
				if(tex.X < 0 || tex.X > 1 || tex.Y < 0 || tex.Y > 1)
					throw new ArgumentOutOfRangeException($"textureCoords[{i}] had invalid values (X: {tex.X}, Y: {tex.Y}).  Expected values are between 0 and 1, inclusive.");
			}

			Array.Copy(vertices, mesh, vertices.Length);

			defaultMesh = new VertexPositionColorTexture[mesh.Length];
			Array.Copy(mesh, defaultMesh, mesh.Length);

			ID = CoreMod.indirectMeshNextID++;
			CoreMod.indirectMeshes.Add(ID, this);
		}

		/// <summary>
		/// Creates a new <seealso cref="Mesh"/> instance with a custom index buffer
		/// </summary>
		/// <param name="texture">The texture</param>
		/// <param name="vertices">The vertex data.  Expected values for each vertex's texture coordinates range from 0 to 1, inclusive</param>
		/// <param name="indices">The index data</param>
		/// <param name="shader">The shader to draw this mesh with</param>
		/// <remarks>NOTE: Modifying the <seealso cref="mesh"/> array length will cause the library to force the index buffer back to the "one vertex per index" functionality, so keep that in mind.</remarks>
		public Mesh(Texture2D texture, VertexPositionColorTexture[] vertices, int[] indices, Effect shader = null) : this(texture, vertices, shader) {
			if(indices is null)
				throw new ArgumentNullException(nameof(indices));

			if(indices.Length < 1)
				throw new ArgumentException("Index array was too small", nameof(indices));

			iBuffer = new IndexBuffer(Main.graphics.GraphicsDevice, IndexElementSize.ThirtyTwoBits, indices.Length, BufferUsage.WriteOnly);
			iBuffer.SetData(indices);

			oldMeshLength = mesh.Length;

			customIndexBuffer = true;
			iBufferArr = (int[])indices.Clone();
		}

		/// <summary>
		/// Creates a new <seealso cref="Mesh"/> instance with a custom index buffer
		/// </summary>
		/// <param name="texture">The texture</param>
		/// <param name="positions">The screen positions to draw this mesh's vertices at</param>
		/// <param name="textureCoords">The texture coordinates for this mesh's vertices.  Expected values per vertex range from 0 to 1, inclusive.</param>
		/// <param name="color">The color for this mesh's vertices</param>
		/// <param name="indices">The idnex data</param>
		/// <param name="shader">The shader to draw this mesh with</param>
		/// <remarks>NOTE: Modifying the <seealso cref="mesh"/> array length will cause the library to force the index buffer back to the "one vertex per index" functionality, so keep that in mind.</remarks>
		public Mesh(Texture2D texture, Vector2[] positions, Vector2[] textureCoords, Color color, int[] indices, Effect shader = null) {
			if(texture is null || texture.IsDisposed)
				throw new ArgumentException("Invalid texture (null or disposed)");

			this.texture = texture;
			this.shader = shader;

			if(positions is null)
				throw new ArgumentNullException(nameof(positions), "Position array was null");
			if(textureCoords is null)
				throw new ArgumentNullException(nameof(textureCoords), "Texture coordinate array was null");

			if(positions.Length != textureCoords.Length)
				throw new ArgumentException("Arrays must have equal length");

			mesh = new VertexPositionColorTexture[positions.Length];

			for(int i = 0; i < positions.Length; i++) {
				var tex = textureCoords[i];
				if(tex.X < 0 || tex.X > 1 || tex.Y < 0 || tex.Y > 1)
					throw new ArgumentOutOfRangeException($"textureCoords[{i}] had invalid values (X: {tex.X}, Y: {tex.Y}).  Expected values are between 0 and 1, inclusive.");

				ref var rMesh = ref mesh[i];
				rMesh.Position = new Vector3(positions[i], 0);
				rMesh.TextureCoordinate = tex;
				rMesh.Color = color;
			}

			defaultMesh = new VertexPositionColorTexture[mesh.Length];
			Array.Copy(mesh, defaultMesh, mesh.Length);

			if(indices is null)
				throw new ArgumentNullException(nameof(indices));

			if(indices.Length < 1)
				throw new ArgumentException("Index array was too small", nameof(indices));

			iBuffer = new IndexBuffer(Main.graphics.GraphicsDevice, IndexElementSize.ThirtyTwoBits, indices.Length, BufferUsage.WriteOnly);
			iBuffer.SetData(indices);

			oldMeshLength = mesh.Length;

			customIndexBuffer = true;
			iBufferArr = (int[])indices.Clone();

			ID = CoreMod.indirectMeshNextID++;
			CoreMod.indirectMeshes.Add(ID, this);
		}

		/// <summary>
		/// Creates a new <seealso cref="Mesh"/> instance with a custom index buffer
		/// </summary>
		/// <param name="texture">The texture</param>
		/// <param name="positions">The screen positions to draw this mesh's vertices at</param>
		/// <param name="textureCoords">The texture coordinates for this mesh's vertices.  Expected values per vertex range from 0 to 1, inclusive.</param>
		/// <param name="colors">The colors for this mesh's vertices</param>
		/// <param name="indices">The idnex data</param>
		/// <param name="shader">The shader to draw this mesh with</param>
		/// <remarks>NOTE: Modifying the <seealso cref="mesh"/> array length will cause the library to force the index buffer back to the "one vertex per index" functionality, so keep that in mind.</remarks>
		public Mesh(Texture2D texture, Vector2[] positions, Vector2[] textureCoords, Color[] colors, int[] indices, Effect shader = null) {
			if(texture is null || texture.IsDisposed)
				throw new ArgumentException("Invalid texture (null or disposed)");

			this.texture = texture;
			this.shader = shader;

			if(positions is null)
				throw new ArgumentNullException(nameof(positions), "Position array was null");
			if(textureCoords is null)
				throw new ArgumentNullException(nameof(textureCoords), "Texture coordinate array was null");
			if(colors is null)
				throw new ArgumentNullException(nameof(colors), "Colors array was null");

			if(positions.Length != textureCoords.Length || positions.Length != colors.Length)
				throw new ArgumentException("Arrays must have equal length");

			mesh = new VertexPositionColorTexture[positions.Length];

			for(int i = 0; i < positions.Length; i++) {
				var tex = textureCoords[i];
				if(tex.X < 0 || tex.X > 1 || tex.Y < 0 || tex.Y > 1)
					throw new ArgumentOutOfRangeException($"textureCoords[{i}] had invalid values (X: {tex.X}, Y: {tex.Y}).  Expected values are between 0 and 1, inclusive.");

				ref var rMesh = ref mesh[i];
				rMesh.Position = new Vector3(positions[i], 0);
				rMesh.TextureCoordinate = tex;
				rMesh.Color = colors[i];
			}

			defaultMesh = new VertexPositionColorTexture[mesh.Length];
			Array.Copy(mesh, defaultMesh, mesh.Length);

			if(indices is null)
				throw new ArgumentNullException(nameof(indices));

			if(indices.Length < 1)
				throw new ArgumentException("Index array was too small", nameof(indices));

			iBuffer = new IndexBuffer(Main.graphics.GraphicsDevice, IndexElementSize.ThirtyTwoBits, indices.Length, BufferUsage.WriteOnly);
			iBuffer.SetData(indices);

			oldMeshLength = mesh.Length;

			customIndexBuffer = true;
			iBufferArr = (int[])indices.Clone();

			ID = CoreMod.indirectMeshNextID++;
			CoreMod.indirectMeshes.Add(ID, this);
		}

		/// <summary>
		/// Draws this mesh immediately, bypassing <seealso cref="Main.spriteBatch"/>
		/// </summary>
		public void Draw() {
			if(!customIndexBuffer && mesh.Length % 3 != 0)
				throw new InvalidOperationException("Vertex array must have a length which is a multiple of 3");
			else if(customIndexBuffer && iBufferArr.Length % 3 != 0)
				throw new InvalidOperationException("Index buffer must have a length which is a multiple of 3");

			GraphicsDevice device = Main.graphics.GraphicsDevice;

			device.Textures[0] = texture;

			EnsureBuffersAreInitialized(device);
			EnsureTrianglesAreFrontFaced();

			//Vertices can end up having a "backwards" face if the points are connected in a counter-clockwise fashion... make them be not culled
			device.RasterizerState = RasterizerState.CullNone;
			device.BlendState = BlendState.AlphaBlend;

			if(shader != null) {
				foreach(var pass in shader.CurrentTechnique.Passes) {
					pass.Apply();

					//Make sure that the first texture is this mesh's texture
					if(!object.ReferenceEquals(device.Textures[0], texture))
						device.Textures[0] = texture;

					device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, mesh.Length, 0, iBufferArr.Length / 3);
				}
			} else
				device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, mesh.Length, 0, iBufferArr.Length / 3);
		}

		private VertexBuffer vBuffer;
		private IndexBuffer iBuffer;

		private int oldMeshLength = -1;

		private void EnsureBuffersAreInitialized(GraphicsDevice device) {
			if(vBuffer is null || vBuffer.IsDisposed || oldMeshLength != mesh.Length)
				vBuffer = new VertexBuffer(device, typeof(VertexPositionColorTexture), mesh.Length, BufferUsage.WriteOnly);

			if(iBuffer is null || iBuffer.IsDisposed || oldMeshLength != mesh.Length) {
				iBuffer = new IndexBuffer(device, IndexElementSize.ThirtyTwoBits, mesh.Length, BufferUsage.WriteOnly);
				iBuffer.SetData(iBufferArr = GetDefaultIndexBuffer());

				customIndexBuffer = false;
			}

			//GraphicsDevice must not have a vertex buffer bound to it to set data on ANY VertexBuffer
			device.SetVertexBuffer(null);
			vBuffer.SetData(mesh);
			device.SetVertexBuffer(vBuffer);

			device.Indices = iBuffer;

			oldMeshLength = mesh.Length;
		}

		private void EnsureTrianglesAreFrontFaced() {
			for(int i = 0; i < iBufferArr.Length - 1; i += 3) {
				var vert = mesh[iBufferArr[i]].Position.XY();
				var vert2 = mesh[iBufferArr[i + 1]].Position.XY();
				var vert3 = mesh[iBufferArr[i + 2]].Position.XY();

				var dir2 = vert.DirectionToRotation(vert2);
				var dir3 = vert.DirectionToRotation(vert3);

				//Ensure that the "angle" checked is between the two vectors
				if(dir2 < 0)
					dir2 += MathHelper.TwoPi;
				if(dir3 < 0)
					dir3 += MathHelper.TwoPi;

				if(dir3 > 3 * MathHelper.PiOver2 && dir2 < MathHelper.PiOver2)
					dir2 += MathHelper.TwoPi;
				if(dir2 > 3 * MathHelper.PiOver2 && dir3 < MathHelper.PiOver2)
					dir3 += MathHelper.TwoPi;

				if(dir2 - dir3 > 0)
					throw new InvalidOperationException($"Vertex/index data had a counter-clockwise triangle (Vertices: {iBufferArr[i]}, {iBufferArr[i + 1]}, {iBufferArr[i + 2]})");
			}
		}

		private int[] GetDefaultIndexBuffer() {
			int[] arr = new int[mesh.Length];
			for(int i = 0; i < arr.Length; i++)
				arr[i] = i;
			return arr;
		}

		public void UpdateIndexBuffer(int[] indices) {
			if(iBuffer is null || iBuffer.IsDisposed)
				iBuffer = new IndexBuffer(Main.graphics.GraphicsDevice, IndexElementSize.ThirtyTwoBits, indices.Length, BufferUsage.WriteOnly);

			iBuffer.SetData(indices);
			iBufferArr = (int[])indices.Clone();

			customIndexBuffer = true;
		}

		/// <summary>
		/// Moves the entire <seealso cref="Mesh"/> by a given <paramref name="offset"/>
		/// </summary>
		/// <param name="offset"></param>
		public void ApplyTranslation(Vector2 offset) {
			if(offset.HasNaNs())
				throw new ArgumentException("Offset had a NaN component");

			for(int i = 0; i < mesh.Length; i++) {
				ref var rMesh = ref mesh[i];
				rMesh.Position.X += offset.X;
				rMesh.Position.Y += offset.Y;
			}
		}

		/// <summary>
		/// Rotates the entire <seealso cref="Mesh"/> around a given world postition, <paramref name="rotationOrigin"/>, by the given <paramref name="radians"/>
		/// </summary>
		/// <param name="rotationOrigin">The world location used as the "anchor" for rotating the mesh</param>
		/// <param name="radians">The rotation</param>
		public void ApplyRotation(Vector2 rotationOrigin, float radians) {
			if(rotationOrigin.HasNaNs())
				throw new ArgumentException("Rotation origin had a NaN component");

			for(int i = 0; i < mesh.Length; i++) {
				ref var vertex = ref mesh[i];

				if(vertex.Position.X != rotationOrigin.X || vertex.Position.Y != rotationOrigin.Y) {
					//Only move the vertex if it isn't the rotation origin
					Vector2 rotated = vertex.Position.XY();

					rotated = rotated.RotatedBy(radians, rotationOrigin);

					vertex.Position.X = rotated.X;
					vertex.Position.Y = rotated.Y;
				}
			}
		}

		/// <summary>
		/// Scales the mesh along both axes by the given <paramref name="scale"/>
		/// </summary>
		/// <param name="scale">The scale factor</param>
		/// <param name="scaleCenter">The world position used as the "anchor" for scaling the mesh</param>
		/// <param name="scaleAxesDirection">(Optional) The direction of the X-axis for the scaling to occur on.  Defaults to 0</param>
		public void ApplyScale(float scale, Vector2 scaleCenter, float? scaleAxesDirection = null)
			=> ApplyScale(new Vector2(scale), scaleCenter, scaleAxesDirection);

		/// <summary>
		/// Scales the mesh along both axes by the given <paramref name="scale"/>
		/// </summary>
		/// <param name="scale">The scale factor</param>
		/// <param name="scaleCenter">The world position used as the "anchor" for scaling the mesh</param>
		/// <param name="scaleAxesDirection">(Optional) The direction of the X-axis for the scaling to occur on.  Defaults to 0</param>
		public void ApplyScale(Vector2 scale, Vector2 scaleCenter, float? scaleAxesDirection = null) {
			float dir = scaleAxesDirection ?? 0;

			if(scale.HasNaNs())
				throw new ArgumentException("Scale had a NaN component");

			if(scale.X == 0 || scale.Y == 0)
				throw new ArgumentException("Scale had a zero component");

			for(int i = 0; i < mesh.Length; i++) {
				ref var vertex = ref mesh[i];

				//Don't do anything if the center is the same as the vertex
				Vector2 scaled = vertex.Position.XY();
				if(scaleCenter == scaled)
					continue;

				//Get the offset from the center, then project it onto the axes
				Vector2 diff = scaled - scaleCenter;

				//Apply the scale on the normal X/Y axes, then just rotate the resulting vector
				diff = diff.RotatedBy(-dir);

				diff *= scale;

				diff = diff.RotatedBy(dir);

				vertex.Position.X = scaleCenter.X + diff.X;
				vertex.Position.Y = scaleCenter.Y + diff.Y;
			}
		}

		public void Reset() {
			if(mesh.Length != defaultMesh.Length)
				mesh = new VertexPositionColorTexture[defaultMesh.Length];

			Array.Copy(defaultMesh, mesh, mesh.Length);
		}

		internal void Dispose() {
			vBuffer?.Dispose();
			vBuffer = null;

			iBuffer?.Dispose();
			iBuffer = null;

			mesh = null;
			defaultMesh = null;

			texture = null;
			shader = null;
		}
	}
}

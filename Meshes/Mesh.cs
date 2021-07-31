using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;

namespace GraphicsLib.Meshes{
	/// <summary>
	/// A vertex mesh bound to a texture
	/// </summary>
	public class Mesh{
		public Texture2D texture;
		public VertexPositionColorTexture[] mesh;
		public Effect shader;

		public readonly int ID;

		/// <summary>
		/// Creates a new <seealso cref="Mesh"/> instance
		/// </summary>
		/// <param name="texture">The texture</param>
		/// <param name="positions">The screen positions to draw this mesh's vertices at</param>
		/// <param name="textureCoords">The texture coordinates for this mesh's vertices.  Expected values per vertex range from 0 to 1, inclusive.</param>
		/// <param name="color">The color for this mesh's vertices</param>
		/// <param name="shader">The shader to draw this mesh with</param>
		public Mesh(Texture2D texture, Vector2[] positions, Vector2[] textureCoords, Color color, Effect shader = null){
			this.texture = texture;
			this.shader = shader;

			if(positions.Length % 3 != 0)
				throw new ArgumentException("Position array must have a length which is a multiple of 3");
			if(textureCoords.Length % 3 != 0)
				throw new ArgumentException("Texture coordinate array must have a length which is a multiple of 3");

			if(positions.Length != textureCoords.Length)
				throw new ArgumentException("Arrays must have equal length");

			mesh = new VertexPositionColorTexture[positions.Length];

			unsafe{
				fixed(Vector2* positionPtr = positions){
					fixed(Vector2* texturePtr = textureCoords){
						fixed(VertexPositionColorTexture* meshPtr = mesh){
							Vector2* nfPosition = positionPtr;
							Vector2* nfTexture = texturePtr;
							VertexPositionColorTexture* nfMesh = meshPtr;

							int length = positions.Length;
							for(int i = 0; i < length; i++){
								var tex = *nfTexture;
								if(tex.X < 0 || tex.X > 1 || tex.Y < 0 || tex.Y > 1)
									throw new ArgumentOutOfRangeException($"textureCoords[{i}] had invalid values (X: {tex.X}, Y: {tex.Y}).  Expected values are between 0 and 1, inclusive.");

								nfMesh->Position = new Vector3(*nfPosition, 0);
								nfMesh->TextureCoordinate = tex;
								nfMesh->Color = color;

								nfPosition++;
								nfTexture++;
								nfMesh++;
							}
						}
					}
				}
			}

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
		public Mesh(Texture2D texture, Vector2[] positions, Vector2[] textureCoords, Color[] colors, Effect shader = null){
			this.texture = texture;
			this.shader = shader;

			if(positions.Length % 3 != 0)
				throw new ArgumentException("Position array must have a length which is a multiple of 3");
			if(textureCoords.Length % 3 != 0)
				throw new ArgumentException("Texture coordinate array must have a length which is a multiple of 3");
			if(colors.Length % 3 != 0)
				throw new ArgumentException("Color array must have a length which is a multiple of 3");

			if(positions.Length != textureCoords.Length || positions.Length != colors.Length)
				throw new ArgumentException("Arrays must have equal length");

			mesh = new VertexPositionColorTexture[positions.Length];

			unsafe{
				fixed(Vector2* positionPtr = positions){
					fixed(Vector2* texturePtr = textureCoords){
						fixed(Color* colorPtr = colors){
							fixed(VertexPositionColorTexture* meshPtr = mesh){
								Vector2* nfPosition = positionPtr;
								Vector2* nfTexture = texturePtr;
								Color* nfColor = colorPtr;
								VertexPositionColorTexture* nfMesh = meshPtr;

								int length = positions.Length;
								for(int i = 0; i < length; i++){
									var tex = *nfTexture;
									if(tex.X < 0 || tex.X > 1 || tex.Y < 0 || tex.Y > 1)
										throw new ArgumentOutOfRangeException($"textureCoords[{i}] had invalid values (X: {tex.X}, Y: {tex.Y}).  Expected values are between 0 and 1, inclusive.");

									nfMesh->Position = new Vector3(*nfPosition, 0);
									nfMesh->TextureCoordinate = tex;
									nfMesh->Color = *nfColor;

									nfPosition++;
									nfTexture++;
									nfColor++;
									nfMesh++;
								}
							}
						}
					}
				}
			}

			ID = CoreMod.indirectMeshNextID++;
			CoreMod.indirectMeshes.Add(ID, this);
		}

		/// <summary>
		/// Draws this mesh immediately, bypassing <seealso cref="Main.spriteBatch"/>
		/// </summary>
		public void Draw(){
			if(mesh.Length % 3 != 0)
				throw new InvalidOperationException("Vertex array must have a length which is a multiple of 3");

			GraphicsDevice device = Main.graphics.GraphicsDevice;

			device.Textures[0] = texture;

			//Create the vertex buffer
			VertexBuffer buffer = new VertexBuffer(device, typeof(VertexPositionColorTexture), mesh.Length, BufferUsage.WriteOnly);

			device.SetVertexBuffer(null);
			buffer.SetData(mesh);
			device.SetVertexBuffer(buffer);

			if(shader != null){
				foreach(var pass in shader.CurrentTechnique.Passes){
					pass.Apply();

					//Make sure that the first texture is this mesh's texture
					if(!object.ReferenceEquals(device.Textures[0], texture))
						device.Textures[0] = texture;

					device.DrawPrimitives(PrimitiveType.TriangleList, 0, mesh.Length / 3);
				}
			}else
				device.DrawPrimitives(PrimitiveType.TriangleList, 0, mesh.Length / 3);
		}
	}
}

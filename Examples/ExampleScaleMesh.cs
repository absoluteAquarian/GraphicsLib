using GraphicsLib.Meshes;
using GraphicsLib.Utility.Extensions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ModLoader;

namespace GraphicsLib.Examples{
	public class ExampleScaleMesh : ModProjectile{
		//This class acts an example of scaling meshes
		//Unlike regular texture drawing, scaling has to be done on a defined "axis"
		public override void SetStaticDefaults(){
			DisplayName.SetDefault("Scaling Mesh");
		}

		public Mesh drawMesh;

		public override void SetDefaults(){
			projectile.width = 32;
			projectile.height = 32;
			projectile.tileCollide = false;
			projectile.aiStyle = -1;
			projectile.timeLeft = Max_Timeleft;
		}

		public const int Max_Timeleft = 60 * 20;

		public const int AI_ScaleVertically = 1;
		public const int AI_ScaleHorizontally = 2;
		public const int AI_RotateThenScale = 3;

		private bool spawned;

		private float scaleAxisX;
		private float origScale;

		public override void AI(){
			if(projectile.ai[0] < AI_ScaleVertically || projectile.ai[0] > AI_RotateThenScale){
				projectile.active = false;
				return;
			}

			//This projectile doesn't move, but if you were to use a Mesh on a moving target, you would have to move the mesh accordingly
			//  via "mesh.ApplyTranslation()"
			projectile.velocity = Vector2.Zero;

			if(!spawned){
				spawned = true;

				origScale = projectile.scale;

				//Initialize the mesh and, if the type is AI_RotateThenScale, rotate it 45 degrees clockwise
				if(!Main.dedServ){
					drawMesh = new Mesh(Main.projectileTexture[projectile.type],
						//Positions
						new Vector2[]{
							projectile.TopLeft,
							projectile.TopRight,
							projectile.BottomLeft,
							projectile.BottomRight
						},
						//Texture coordinates
						//IMPORTANT: texture coordinates are NORMALIZED, meaning they range from 0 to 1 for the ENTIRE TEXTURE
						new Vector2[]{
							Vector2.Zero,
							Vector2.UnitX,
							Vector2.UnitY,
							Vector2.One
						},
						Color.White,
						//Index data
						//The values in this array correspond to what positions/texture coordinates/colors to use in the other arrays
						new int[]{
							//Triangle 1
							0, 1, 2,
							//Triangle 2
							1, 3, 2
						},
						null);

					drawMesh.ApplyTranslation(-Main.screenPosition);

					if(projectile.ai[0] == AI_RotateThenScale){
						scaleAxisX = MathHelper.ToRadians(45);
						drawMesh.ApplyRotation(projectile.Center - Main.screenPosition, scaleAxisX);
					}
				}
			}

			projectile.ai[1]++;
		}

		public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor){
			if(projectile.ai[0] < AI_ScaleVertically || projectile.ai[0] > AI_RotateThenScale)
				return false;

			//No SpriteBatch should be active when drawing meshes
			spriteBatch.End();

			//NOTE: To reduce the likelyhood of positions being inaccurate, update meshes in Draw hooks
			//Reset the mesh so scaling is consistent
			drawMesh.Reset();

			//Ensure that the mesh is in the correct location
			drawMesh.ApplyTranslation(projectile.position - Main.screenPosition - drawMesh.mesh[0].Position.XY());

			//Scaling is done using Math.Sin
			//6f * timer = 1 rotation per second
			float newScale = origScale + (float)Math.Sin(MathHelper.ToRadians(6f * projectile.ai[1])) * 0.5f;

			if(projectile.ai[0] == AI_ScaleVertically)
				drawMesh.ApplyScale(new Vector2(1, newScale), projectile.Bottom - Main.screenPosition);
			else if(projectile.ai[0] == AI_ScaleHorizontally)
				drawMesh.ApplyScale(new Vector2(newScale, 1), projectile.Bottom - Main.screenPosition);
			else if(projectile.ai[0] == AI_RotateThenScale){
				drawMesh.ApplyRotation(projectile.Center - Main.screenPosition, scaleAxisX);

				//This AI shows usage of the "scale axis" parameter of ApplyScale
				//Scaling is done "vertically", but on this scale axis
				drawMesh.ApplyScale(new Vector2(1, newScale), projectile.Center - Main.screenPosition, scaleAxisX);
			}

			drawMesh.Draw();

			spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);

			return false;
		}
	}
}

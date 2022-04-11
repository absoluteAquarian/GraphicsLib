using GraphicsLib.Meshes;
using GraphicsLib.Utility.Extensions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;

namespace GraphicsLib.Examples {
	public class ExampleScaleMesh : ModProjectile {
		//This class acts an example of scaling meshes
		//Unlike regular texture drawing, scaling has to be done on a defined "axis"
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Scaling Mesh");
		}

		public Mesh drawMesh;

		public override void SetDefaults() {
			Projectile.width = 32;
			Projectile.height = 32;
			Projectile.tileCollide = false;
			Projectile.aiStyle = -1;
			Projectile.timeLeft = Max_Timeleft;
		}

		public const int Max_Timeleft = 60 * 20;

		public const int AI_ScaleVertically = 1;
		public const int AI_ScaleHorizontally = 2;
		public const int AI_RotateThenScale = 3;

		private bool spawned;

		private float scaleAxisX;
		private float origScale;

		public override void AI() {
			if(Projectile.ai[0] < AI_ScaleVertically || Projectile.ai[0] > AI_RotateThenScale) {
				Projectile.active = false;
				return;
			}

			//This projectile doesn't move, but if you were to use a Mesh on a moving target, you would have to move the mesh accordingly
			//  via "mesh.ApplyTranslation()"
			Projectile.velocity = Vector2.Zero;

			if(!spawned) {
				spawned = true;

				origScale = Projectile.scale;

				//Initialize the mesh and, if the type is AI_RotateThenScale, rotate it 45 degrees clockwise
				if(!Main.dedServ) {
					drawMesh = new Mesh(TextureAssets.Projectile[Projectile.type].Value,
						//Positions
						new Vector2[]{
							Projectile.TopLeft,
							Projectile.TopRight,
							Projectile.BottomLeft,
							Projectile.BottomRight
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
							2, 1, 3
						},
						null);

					drawMesh.ApplyTranslation(-Main.screenPosition);

					if(Projectile.ai[0] == AI_RotateThenScale) {
						scaleAxisX = MathHelper.ToRadians(45);
						drawMesh.ApplyRotation(Projectile.Center - Main.screenPosition, scaleAxisX);
					}
				}
			}

			Projectile.ai[1]++;
		}

		public override bool PreDraw(ref Color lightColor) {
			if(Projectile.ai[0] < AI_ScaleVertically || Projectile.ai[0] > AI_RotateThenScale)
				return false;

			//No SpriteBatch should be active when drawing meshes
			Main.spriteBatch.End();

			//NOTE: To reduce the likelyhood of positions being inaccurate, update meshes in Draw hooks
			//Reset the mesh so scaling is consistent
			drawMesh.Reset();

			//Ensure that the mesh is in the correct location
			drawMesh.ApplyTranslation(Projectile.position - Main.screenPosition - drawMesh.mesh[0].Position.XY());

			//Scaling is done using Math.Sin
			//6f * timer = 1 rotation per second
			float newScale = origScale + (float)Math.Sin(MathHelper.ToRadians(6f * Projectile.ai[1])) * 0.5f;

			if(Projectile.ai[0] == AI_ScaleVertically)
				drawMesh.ApplyScale(new Vector2(1, newScale), Projectile.Bottom - Main.screenPosition);
			else if(Projectile.ai[0] == AI_ScaleHorizontally)
				drawMesh.ApplyScale(new Vector2(newScale, 1), Projectile.Bottom - Main.screenPosition);
			else if(Projectile.ai[0] == AI_RotateThenScale) {
				drawMesh.ApplyRotation(Projectile.Center - Main.screenPosition, scaleAxisX);

				//This AI shows usage of the "scale axis" parameter of ApplyScale
				//Scaling is done "vertically", but on this scale axis
				drawMesh.ApplyScale(new Vector2(1, newScale), Projectile.Center - Main.screenPosition, scaleAxisX);
			}

			drawMesh.Draw();

			//These lines are necessary to prevent errors from ocurring when drawing the next projectile
			Main.spriteBatch.Begin();
			int shader = Main.GetProjectileDesiredShader(Projectile.whoAmI);
			Main.CurrentDrawnEntityShader = shader != 0 ? 0 : 1;
			Main.instance.PrepareDrawnEntityDrawing(Projectile, shader);

			return false;
		}
	}
}

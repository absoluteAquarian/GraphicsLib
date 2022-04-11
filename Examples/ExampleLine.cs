using GraphicsLib.Primitives;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace GraphicsLib.Examples {
	public class ExampleLine : ModProjectile {
		//This is an example of drawing primitive lines using the library
		//The projectile alternates between drawing a line for its velocity or drawing where it's been based on its AI
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Line");

			ProjectileID.Sets.TrailingMode[Projectile.type] = 0;
			ProjectileID.Sets.TrailCacheLength[Projectile.type] = 300;
		}

		public override void SetDefaults() {
			Projectile.width = 16;
			Projectile.height = 16;
			Projectile.tileCollide = false;
			Projectile.aiStyle = -1;
			Projectile.timeLeft = Max_Timeleft;
		}

		private const int Max_Timeleft = 60 * 20;

		public const int AI_ShowVelocity = 1;
		public const int AI_ShowLocations = 2;
		public const int AI_ShowLocation_LerpColor = 3;

		public override void AI() {
			int swapTime = Projectile.ai[0] == AI_ShowVelocity
				? 26
				: (Projectile.ai[0] == AI_ShowLocations || Projectile.ai[0] == AI_ShowLocation_LerpColor
					? 10
					: -1);

			if(swapTime == -1) {
				Projectile.active = false;
				return;
			}

			if(Projectile.timeLeft % swapTime == 0)
				Projectile.velocity = Main.rand.NextVector2Unit() * 7f;
		}

		public override bool PreDraw(ref Color lightColor) {
			if(Projectile.ai[0] < AI_ShowVelocity || Projectile.ai[0] > AI_ShowLocation_LerpColor)
				return true;

			//The SpriteBatch must have ended before submitting packets
			Main.spriteBatch.End();

			//IMPORTANT: All coordinates are in WORLD COORDINATES, not screen coordinates
			//If you have a screen coordinate and want to use it, add "Main.screenPosition" to it

			//Draw a box representing the projectile's hitbox
			Color boxColor = Color.Green * 0.8f;
			PrimitiveDrawing.DrawFilledRectangle(Projectile.position, Projectile.BottomRight, boxColor, boxColor, boxColor, boxColor);

			if(Projectile.ai[0] == AI_ShowVelocity) {
				//Draws a single line from the projectile's center to where it will be a few ticks in the future
				PrimitiveDrawing.DrawLineStrip(new Vector2[]{
						Projectile.Center,
						Projectile.Center + Projectile.velocity * 10
					}, new Color[]{
						Color.Red,
						Color.Yellow
					});
			} else if(Projectile.ai[0] == AI_ShowLocations) {
				//Draws a series of connected lines based on where the projectile has been
				//This example uses a PrimitivePacket directly, but using PrimitiveDrawing.DrawLineStrip() would also work
				if(Max_Timeleft - Projectile.timeLeft >= 1) {
					int toDraw = Math.Min(Max_Timeleft - Projectile.timeLeft, ProjectileID.Sets.TrailCacheLength[Projectile.type]);

					//A LineStrip packet expects two points for the initial AddDraw call to make the first line, then one point after that for the other lines
					PrimitivePacket packet = new(PrimitiveType.LineStrip);

					packet.AddDraw(PrimitiveDrawing.ToPrimitive(Projectile.Center, Color.White),
						PrimitiveDrawing.ToPrimitive(Projectile.oldPos[0] + Projectile.Size / 2f, Color.White));

					for(int i = 1; i < toDraw; i++)
						packet.AddDraw(PrimitiveDrawing.ToPrimitive(Projectile.oldPos[i] + Projectile.Size / 2f, Color.White));

					//Calling PrimitiveDrawing.SubmitPacket() draws the packet immediately, hence why the SpriteBatch had to be ended earlier
					PrimitiveDrawing.SubmitPacket(packet);
				}
			} else if(Projectile.ai[0] == AI_ShowLocation_LerpColor) {
				//Similar to AI_ShowLocations, except the color is lerped
				if(Max_Timeleft - Projectile.timeLeft >= 1) {
					int toDraw = Math.Min(Max_Timeleft - Projectile.timeLeft, ProjectileID.Sets.TrailCacheLength[Projectile.type]);

					List<Vector2> coords = new() {
						Projectile.Center,
						Projectile.oldPos[0] + Projectile.Size / 2f
					};
					for(int i = 1; i < toDraw; i++)
						coords.Add(Projectile.oldPos[i] + Projectile.Size / 2f);

					PrimitiveDrawing.DrawLineStrip(coords.ToArray(), start: Color.Red, end: Color.Green);
				}
			}

			//These lines are necessary to prevent errors from ocurring when drawing the next projectile
			Main.spriteBatch.Begin();
			int shader = Main.GetProjectileDesiredShader(Projectile.whoAmI);
			Main.CurrentDrawnEntityShader = shader != 0 ? 0 : 1;
			Main.instance.PrepareDrawnEntityDrawing(Projectile, shader);
			
			return true;
		}
	}
}

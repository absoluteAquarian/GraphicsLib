using GraphicsLib.Primitives;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace GraphicsLib.Examples{
	public class ExampleLine : ModProjectile{
		//This is an example of drawing primitive lines using the library
		//The projectile alternates between drawing a line for its velocity or drawing where it's been based on its AI
		public override void SetStaticDefaults(){
			DisplayName.SetDefault("Line");

			ProjectileID.Sets.TrailingMode[projectile.type] = 0;
			ProjectileID.Sets.TrailCacheLength[projectile.type] = 300;
		}

		public override void SetDefaults(){
			projectile.width = 16;
			projectile.height = 16;
			projectile.tileCollide = false;
			projectile.aiStyle = -1;
			projectile.timeLeft = Max_Timeleft;
		}

		private const int Max_Timeleft = 60 * 20;

		public const int AI_ShowVelocity = 1;
		public const int AI_ShowLocations = 2;
		public const int AI_ShowLocation_LerpColor = 3;

		public override void AI(){
			int swapTime = projectile.ai[0] == AI_ShowVelocity
				? 26
				: (projectile.ai[0] == AI_ShowLocations || projectile.ai[0] == AI_ShowLocation_LerpColor
					? 10
					: -1);

			if(swapTime == -1){
				projectile.active = false;
				return;
			}

			if(projectile.timeLeft % swapTime == 0)
				projectile.velocity = Main.rand.NextVector2Unit() * 7f;
		}

		public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor){
			if(projectile.ai[0] < AI_ShowVelocity || projectile.ai[0] > AI_ShowLocation_LerpColor)
				return true;

			//The SpriteBatch must have ended before submitting packets
			spriteBatch.End();

			//IMPORTANT: All coordinates are in WORLD COORDINATES, not screen coordinates
			//If you have a screen coordinate and want to use it, add "Main.screenPosition" to it

			//Draw a box representing the projectile's hitbox
			Color boxColor = Color.Green * 0.8f;
			PrimitiveDrawing.DrawFilledRectangle(projectile.position, projectile.BottomRight, boxColor, boxColor, boxColor, boxColor);

			if(projectile.ai[0] == AI_ShowVelocity){
				//Draws a single line from the projectile's center to where it will be a few ticks in the future
				PrimitiveDrawing.DrawLineStrip(new Vector2[]{
						projectile.Center,
						projectile.Center + projectile.velocity * 10
					}, new Color[]{
						Color.Red,
						Color.Yellow
					});
			}else if(projectile.ai[0] == AI_ShowLocations){
				//Draws a series of connected lines based on where the projectile has been
				//This example uses a PrimitivePacket directly, but using PrimitiveDrawing.DrawLineStrip() would also work
				if(Max_Timeleft - projectile.timeLeft >= 1){
					int toDraw = Math.Min(Max_Timeleft - projectile.timeLeft, ProjectileID.Sets.TrailCacheLength[projectile.type]);

					//A LineStrip packet expects two points for the initial AddDraw call to make the first line, then one point after that for the other lines
					PrimitivePacket packet = new PrimitivePacket(PrimitiveType.LineStrip);

					packet.AddDraw(PrimitiveDrawing.ToPrimitive(projectile.Center, Color.White),
						PrimitiveDrawing.ToPrimitive(projectile.oldPos[0] + projectile.Size / 2f, Color.White));

					for(int i = 1; i < toDraw; i++)
						packet.AddDraw(PrimitiveDrawing.ToPrimitive(projectile.oldPos[i] + projectile.Size / 2f, Color.White));

					//Calling PrimitiveDrawing.SubmitPacket() draws the packet immediately, hence why the SpriteBatch had to be ended earlier
					PrimitiveDrawing.SubmitPacket(packet);
				}
			}else if(projectile.ai[0] == AI_ShowLocation_LerpColor){
				//Similar to AI_ShowLocations, except the color is lerped
				if(Max_Timeleft - projectile.timeLeft >= 1){
					int toDraw = Math.Min(Max_Timeleft - projectile.timeLeft, ProjectileID.Sets.TrailCacheLength[projectile.type]);

					List<Vector2> coords = new List<Vector2>(){
						projectile.Center,
						projectile.oldPos[0] + projectile.Size / 2f
					};
					for(int i = 1; i < toDraw; i++)
						coords.Add(projectile.oldPos[i] + projectile.Size / 2f);

					PrimitiveDrawing.DrawLineStrip(coords.ToArray(), start: Color.Red, end: Color.Green);
				}
			}

			spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
			return true;
		}
	}
}

using GraphicsLib.Drawing;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace GraphicsLib.Examples {
	public class ExampleLine : ModProjectile {
		// This is an example of drawing primitive lines using the library
		// The projectile alternates between drawing a line for its velocity or drawing where it's been based on its AI

		private static PrimitiveBuilder<VertexPositionColor> hitbox;
		private static PrimitiveBuilder<VertexPositionColor> velocityLine;
		private static PrimitiveBuilder<VertexPositionColor> previousLocations;

		private const int WIDTH = 16;
		private const int HEIGHT = 16;
		private const int POSITION_HISTORY = 300;

		public override void SetStaticDefaults() {
			ProjectileID.Sets.TrailingMode[Projectile.type] = 0;
			ProjectileID.Sets.TrailCacheLength[Projectile.type] = POSITION_HISTORY;

			hitbox = SimpleShapes.HollowPolygon(
				positions: [
					new Vector2(-WIDTH / 2, -HEIGHT / 2),  // Top-left corner
					new Vector2( WIDTH / 2, -HEIGHT / 2),  // Top-right corner
					new Vector2( WIDTH / 2,  HEIGHT / 2),  // Bottom-right corner
					new Vector2(-WIDTH / 2,  HEIGHT / 2)   // Bottom-left corner
				],
				color: Color.Green * 0.8f
			);

			velocityLine = SimpleShapes.LineSegment(
				start: Vector2.Zero,
				startColor: Color.Red,
				end: Vector2.Zero,  // This will be modified later
				endColor: Color.Yellow
			);

			previousLocations = SimpleShapes.PolyLine(
				positions: stackalloc Vector2[POSITION_HISTORY],  // Positions to overwrite later
				color: Color.Transparent  // Color will be overwritten
			);
		}

		public override void SetDefaults() {
			Projectile.width = WIDTH;
			Projectile.height = HEIGHT;
			Projectile.tileCollide = false;
			Projectile.aiStyle = -1;
			Projectile.timeLeft = MAX_TIMELEFT;
		}

		public int Mode {
			get => (int)Projectile.ai[0];
			set => Projectile.ai[0] = value;
		}

		private const int MAX_TIMELEFT = 60 * 20;

		public const int AI_ShowVelocity = 1;
		public const int AI_ShowLocations = 2;
		public const int AI_ShowLocation_LerpColor = 3;

		public override void AI() {
			int velocityTime = -1;
			if (Mode == AI_ShowVelocity) {
				// 26 ticks
				velocityTime = 42;
			} else if (Mode == AI_ShowLocations || Mode == AI_ShowLocation_LerpColor) {
				// 10 ticks
				velocityTime = 18;
			} else {
				// Invalid projectile
				Projectile.active = false;
				return;
			}

			if (Projectile.timeLeft % velocityTime == 0)
				Projectile.velocity = Main.rand.NextVector2Unit() * 6f;
		}

		public override bool PreDraw(ref Color lightColor) {
			lightColor = Color.White;

			if (Mode != AI_ShowVelocity && Mode != AI_ShowLocations && Mode != AI_ShowLocation_LerpColor)
				return true;

			// The SpriteBatch must have ended before using primitives
			PrimitiveUploader.EndCurrentSpriteBatch();

			// This Matrix will move the primitives to the correct locations on-screen
			Matrix transform = Matrix.CreateTranslation(new Vector3(Projectile.Center - Main.screenPosition, 0f));

			// Draw a box representing the projectile's hitbox
			hitbox.Transform = transform;
			PrimitiveUploader.Render(hitbox);

			if (Mode == AI_ShowVelocity) {
				// Draws a single line from the projectile's center to where it will be a few ticks in the future
				velocityLine.Transform = transform;

				// Modify the only Line primitive
				var primitive = velocityLine.ReadLine(0);
				primitive.End = primitive.End.SetPosition(Projectile.velocity * 10);
				velocityLine.WritePrimitive(0, primitive);

				// Render it
				PrimitiveUploader.Render(velocityLine);
			} else if (Mode == AI_ShowLocations) {
				// Draws a series of connected lines based on where the projectile has been
				int elapsedTicks = MAX_TIMELEFT;

				if (elapsedTicks >= 1) {
					previousLocations.Transform = transform;

					int positionCount = Math.Min(elapsedTicks, ProjectileID.Sets.TrailCacheLength[Projectile.type]);
					Vector2 halfSize = Projectile.Size / 2f;

					// Modify the first Line primitive
					var linePrimitive = previousLocations.ReadLine(0);
					linePrimitive.Start = linePrimitive.Start.SetColor(Color.White);
					linePrimitive.End = linePrimitive.End.SetPosition(Projectile.oldPos[0] + halfSize);
					linePrimitive.End = linePrimitive.End.SetColor(Color.White);
					previousLocations.WritePrimitive(0, linePrimitive);

					// Modify the successive Point primitives
					for (int i = 1; i < positionCount; i++) {
						var pointPrimitive = previousLocations.ReadPoint(i);
						pointPrimitive.Vertex = pointPrimitive.Vertex.SetPosition(Projectile.oldPos[i] + halfSize);
						pointPrimitive.Vertex = pointPrimitive.Vertex.SetColor(Color.White);
						previousLocations.WritePrimitive(i, pointPrimitive);
					}

					// Render it
					PrimitiveUploader.Render(previousLocations);
				}
			} else if (Mode == AI_ShowLocation_LerpColor) {
				// Similar to AI_ShowLocations, except the color is lerped
				int elapsedTicks = MAX_TIMELEFT - Projectile.timeLeft;

				if (elapsedTicks >= 1) {
					previousLocations.Transform = transform;

					int toDraw = Math.Min(elapsedTicks, ProjectileID.Sets.TrailCacheLength[Projectile.type]);
					Vector2 halfSize = Projectile.Size / 2f;
					float lerpStep = 1f / toDraw;
					float lerp = lerpStep;

					// Modify the first Line primitive
					var linePrimitive = previousLocations.ReadLine(0);
					linePrimitive.Start = linePrimitive.Start.SetColor(Color.Red);
					linePrimitive.End = linePrimitive.End.SetPosition(Projectile.oldPos[0] + halfSize);
					linePrimitive.End = linePrimitive.End.SetColor(Color.Lerp(Color.Red, Color.Green, lerp));

					lerp += lerpStep;

					// Modify the successive Point primitives
					for (int i = 1; i < toDraw; i++, lerp += lerpStep) {
						var pointPrimitive = previousLocations.ReadPoint(i);
						pointPrimitive.Vertex = pointPrimitive.Vertex.SetPosition(Projectile.oldPos[i] + halfSize);
						pointPrimitive.Vertex = pointPrimitive.Vertex.SetColor(Color.Lerp(Color.Red, Color.Green, lerp));
					}

					// Render it
					PrimitiveUploader.Render(previousLocations);
				}
			}

			// Restart the SpriteBatch with the original parameters
			PrimitiveUploader.RestartPreviousSpriteBatch();

			return true;
		}
	}
}

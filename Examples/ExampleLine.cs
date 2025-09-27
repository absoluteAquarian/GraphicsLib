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

		// Static builders shared by all projectile instances
		// These fields will be used to make the per-projectile instances in PreDraw
		private static PrimitiveBuilder<VertexPositionColor> hitbox;
		private static PrimitiveBuilder<VertexPositionColor> velocityLine;
		private static PrimitiveBuilder<VertexPositionColor> previousLocations;

		// Per-projectile clones of the builders
		private PrimitiveBuilder<VertexPositionColor> hitboxInstanced;
		private PrimitiveBuilder<VertexPositionColor> velocityLineInstanced;
		private PrimitiveBuilder<VertexPositionColor> previousLocationsInstanced;

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
				positions: stackalloc Vector2[1 + POSITION_HISTORY],  // Positions to overwrite later; the current position + the previous positions from Projectile.oldPos[]
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
			Matrix transform = PrimitiveBuilder.GetOnScreenOrigin(Projectile.Center);

			// Draw a box representing the projectile's hitbox

			hitboxInstanced ??= hitbox.Clone();
			hitboxInstanced.Transform = transform;
			PrimitiveUploader.Render(hitboxInstanced);

			if (Mode == AI_ShowVelocity) {
				// Draws a single line from the projectile's center to where it will be a few ticks in the future

				velocityLineInstanced ??= velocityLine.Clone();
				velocityLineInstanced.Transform = transform;

				// Modify the only Line primitive

				var reader = velocityLineInstanced.GetReader();
				var primitive = reader.GetLineReference(0);
				primitive.End.Position = new Vector3(Projectile.velocity * 10, 0f);

				// Render it

				PrimitiveUploader.Render(velocityLineInstanced);
			} else if (Mode == AI_ShowLocations) {
				// Draws a series of connected lines based on where the projectile has been

				RenderPreviousLocations(transform, _ => Color.White);
			} else if (Mode == AI_ShowLocation_LerpColor) {
				// AI_ShowLocations, except the color is lerped

				RenderPreviousLocations(transform, lerp => Color.Lerp(Color.Red, Color.Green, lerp));
			}

			// Restart the SpriteBatch with the original parameters

			PrimitiveUploader.RestartPreviousSpriteBatch();

			return true;
		}

		private void RenderPreviousLocations(Matrix transform, Func<float, Color> getVertexColor) {
			int elapsedTicks = MAX_TIMELEFT - Projectile.timeLeft;
			int maxTicks = ProjectileID.Sets.TrailCacheLength[Projectile.type];

			if (elapsedTicks >= 1) {
				previousLocationsInstanced ??= previousLocations.Clone();
				previousLocationsInstanced.Transform = transform;

				int positionCount = Math.Min(elapsedTicks, maxTicks);
				float lerpStep = 1f / positionCount;
				float lerp = lerpStep;

				// Modify the first Line primitive

				var reader = previousLocationsInstanced.GetReader();
				var linePrimitive = reader.GetLineReference(0);
				linePrimitive.Start.Color = Color.Red;
				// The transform will make (0, 0) in primitive coordinates refer to the projectile's center
				// Hence, each point using Projectile.oldPos[] needs to account for that
				linePrimitive.End.Position = new Vector3(Projectile.oldPos[0] - Projectile.position, 0f);
				linePrimitive.End.Color = getVertexColor(lerp);

				lerp += lerpStep;

				// Modify the successive Point primitives

				Vector3 lastPosition = default;

				for (int i = 1; i < positionCount; i++, lerp += lerpStep) {
					lastPosition = new Vector3(Projectile.oldPos[i] - Projectile.position, 0f);

					var pointPrimitive = reader.GetPointReference(i);
					pointPrimitive.Vertex.Position = lastPosition;
					pointPrimitive.Vertex.Color = getVertexColor(lerp);
				}

				// Points that shouldn't be drawn yet are moved to the last position so that no "bleed" occurs toward the uninitialized points

				previousLocationsInstanced.FillPrimitive(
					primitive: GraphicsLib.Drawing.Point.Create(new VertexPositionColor(lastPosition, Color.Transparent)),
					start: positionCount
				);

				// Render it

				PrimitiveUploader.Render(previousLocationsInstanced);
			}
		}
	}
}

using GraphicsLib.ColorMath;
using GraphicsLib.Drawing;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace GraphicsLib.Examples;

public class ExampleThickPolygonItem : ModItem {
	public override string Texture => "GraphicsLib/Examples/ExampleThickPolygon";

	public override void SetDefaults() {
		Item.DefaultToMagicWeapon(
			projType: ModContent.ProjectileType<ExampleThickPolygonProjectile>(),
			singleShotTime: 35,
			shotVelocity: 0f,
			hasAutoReuse: true
		);

		Item.damage = 5;
		Item.knockBack = 1.8f;
		Item.crit = 20;
		Item.mana = 40;
		Item.holdStyle = ItemHoldStyleID.HoldLamp;
		Item.useStyle = ItemUseStyleID.RaiseLamp;
		Item.UseSound = SoundID.Item8;
	}

	public override bool CanUseItem(Player player) => player.ownedProjectileCounts[Item.shoot] < 1;
}

public class ExampleThickPolygonProjectile : ModProjectile {
	// This is a more complex example than ExampleLine using a polygon and matrix manipulation

	public override string Texture => "GraphicsLib/Examples/ExampleThickPolygon";

	// Static builder shared by all projectile instances
	// This field will be used to make the per-projectile instances in PreDraw
	private static PrimitiveBuilder<VertexPositionColor> pentagon;
	
	// Per-projectile clone of the builder
	private PrimitiveBuilder<VertexPositionColor> pentagonInstanced;
	// Variables for manipulating the builder
	private const float SCALE_OSCILLATION_SPEED = 0.75f;  // Oscillations per second
	private const float ROTATION_ANGULAR_VELOCITY = 0.85f;  // Revolutions per second
	private const float HUE_ANGULAR_VELOCITY = 360f * ROTATION_ANGULAR_VELOCITY;  // Degrees per second

	public override void SetStaticDefaults() {
		// Goal:  Pentagon using evenly-spread RGB colors that rotates (make rotation happen with the Transform matrix)

		Vector2 baseVector = -Vector2.UnitY * 60;  // ~4 tiles away from the player's center
		const float ANGLE = 72.0f * float.Pi / 180;  // 72° as radians

		pentagon = SimpleShapes.ThickPolygon(
			positions: [
				baseVector,
				baseVector.RotatedBy(ANGLE),
				baseVector.RotatedBy(ANGLE * 2),
				baseVector.RotatedBy(ANGLE * 3),
				baseVector.RotatedBy(ANGLE * 4)
			],
			color: Color.Transparent,  // Will be overwritten
			thickness: 12f
		);
	}

	public override void SetDefaults() {
		Projectile.width = 52 * 2;
		Projectile.height = 52 * 2;
		Projectile.aiStyle = -1;
		Projectile.tileCollide = false;
		Projectile.friendly = true;
		Projectile.penetrate = -1;
		Projectile.timeLeft = 3 * 60;
		Projectile.usesIDStaticNPCImmunity = true;
		Projectile.idStaticNPCHitCooldown = 4;
	}

	public int Counter {
		get => (int)Projectile.ai[0];
		set => Projectile.ai[0] = value;
	}

	public override void AI() {
		Player owner = Main.player[Projectile.owner];
		owner.heldProj = Projectile.whoAmI;

		Projectile.Center = owner.MountedCenter;

		Counter++;
	}

	public override bool PreDraw(ref Color lightColor) {
		// The SpriteBatch must have ended before using primitives
		PrimitiveUploader.EndCurrentSpriteBatch();

		// This Matrix will move the primitives to the correct locations on-screen
		Matrix transform = PrimitiveBuilder.GetOnScreenOrigin(Projectile.Center);

		// Apply a rotation matrix

		float rotation = Counter * ROTATION_ANGULAR_VELOCITY / 60 * MathHelper.TwoPi;
		rotation = MathHelper.WrapAngle(rotation);

		transform = Matrix.CreateRotationZ(rotation) * transform;

		// Apply a scaling matrix

		float scaleSin = float.Sin(Counter * SCALE_OSCILLATION_SPEED / 60 * MathHelper.TwoPi);
		float scale = 1.0f + scaleSin * 0.15f;

		transform = Matrix.CreateScale(xScale: scale, yScale: scale, zScale: 1.0f) * transform;

		// Draws a pentagon that changes colors, rotates and scale

		pentagonInstanced ??= pentagon.Clone();
		pentagonInstanced.Transform = transform;

		// Modify the Point primitives

		var reader = pentagonInstanced.GetReader();

		for (int i = 0; i < 5; i++) {
			var outerPoint = reader.GetPointReference(i);
			var innerPoint = reader.GetPointReference(i + 5);

			HSVA hsv = HSVA.Red;
			hsv.H += Counter * HUE_ANGULAR_VELOCITY / 60;
			hsv.H += i * 72;  // Offset each point by 72 degrees

			Color color = HSVA.ToColor(hsv);

			outerPoint.Vertex.Color = color;
		//	innerPoint.Vertex.Color = Color.Transparent;
		}

		// Render it

		PrimitiveUploader.Render(pentagonInstanced);

		// Restart the SpriteBatch with the original parameters

		PrimitiveUploader.RestartPreviousSpriteBatch();

		return false;  // Don't draw the sprite
	}
}

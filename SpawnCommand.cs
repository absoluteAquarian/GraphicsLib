using GraphicsLib.Examples;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace GraphicsLib {
	public class SpawnCommand : ModCommand {
		public override string Command => "spawnobj";

		public override CommandType Type => CommandType.Chat;

		public override string Usage => "[c/ff6a00:Usage: /spawnobj <example type>]";

		public override string Description => "Spawns an example from GraphicsLib";

		public override void Action(CommandCaller caller, string input, string[] args) {
			if(args.Length < 1) {
				caller.Reply("Expected a positive integer argument.", Color.Red);
				return;
			}

			if(args.Length > 1) {
				caller.Reply("Too many arguments specified.", Color.Red);
				return;
			}

			if(!uint.TryParse(args[0], out uint type)) {
				caller.Reply("Invalid argument", Color.Red);
				return;
			}

			int spawned;
			switch(type) {
				case 0:
					//Example Line: Velocity
					spawned = Projectile.NewProjectile(new EntitySource_DebugCommand(),
						caller.Player.Center - new Vector2(0, 80),
						Main.rand.NextVector2Unit() * 7f,
						ModContent.ProjectileType<ExampleLine>(),
						0,
						0,
						caller.Player.whoAmI,
						ai0: ExampleLine.AI_ShowVelocity);

					caller.Reply("Spawned: Example Line - Velocity");
					break;
				case 1:
					//Example Line: Previous Postions
					spawned = Projectile.NewProjectile(new EntitySource_DebugCommand(),
						caller.Player.Center - new Vector2(0, 80),
						Main.rand.NextVector2Unit() * 7f,
						ModContent.ProjectileType<ExampleLine>(),
						0,
						0,
						caller.Player.whoAmI,
						ai0: ExampleLine.AI_ShowLocations);

					caller.Reply("Spawned: Example Line - Old Postions");
					break;
				case 2:
					//Example Line: Previous Postions, Lerped Color
					spawned = Projectile.NewProjectile(new EntitySource_DebugCommand(),
						caller.Player.Center - new Vector2(0, 80),
						Main.rand.NextVector2Unit() * 7f,
						ModContent.ProjectileType<ExampleLine>(),
						0,
						0,
						caller.Player.whoAmI,
						ai0: ExampleLine.AI_ShowLocation_LerpColor);

					caller.Reply("Spawned: Example Line - Old Postions, Lerped Color");
					break;
				case 3:
					//Example Scale Mesh: Scale Vertically
					spawned = Projectile.NewProjectile(new EntitySource_DebugCommand(),
						caller.Player.Center - new Vector2(0, 80),
						Main.rand.NextVector2Unit() * 7f,
						ModContent.ProjectileType<ExampleScaleMesh>(),
						0,
						0,
						caller.Player.whoAmI,
						ai0: ExampleScaleMesh.AI_ScaleVertically);

					caller.Reply("Spawned: Example Mesh - Scale Vertically");
					break;
				case 4:
					//Example Scale Mesh: Scale Horizontally
					spawned = Projectile.NewProjectile(new EntitySource_DebugCommand(),
						caller.Player.Center - new Vector2(0, 80),
						Main.rand.NextVector2Unit() * 7f,
						ModContent.ProjectileType<ExampleScaleMesh>(),
						0,
						0,
						caller.Player.whoAmI,
						ai0: ExampleScaleMesh.AI_ScaleHorizontally);

					caller.Reply("Spawned: Example Mesh - Scale Horizontally");
					break;
				case 5:
					//Example Scale Mesh: Rotate, then Scale
					spawned = Projectile.NewProjectile(new EntitySource_DebugCommand(),
						caller.Player.Center - new Vector2(0, 80),
						Main.rand.NextVector2Unit() * 7f,
						ModContent.ProjectileType<ExampleScaleMesh>(),
						0,
						0,
						caller.Player.whoAmI,
						ai0: ExampleScaleMesh.AI_RotateThenScale);

					caller.Reply("Spawned: Example Mesh - Scale with Initial Rotation");
					break;
				default:
					caller.Reply("Unknown example type requested", Color.Red);
					return;
			}

			if(Main.netMode == NetmodeID.MultiplayerClient)
				NetMessage.SendData(MessageID.SyncProjectile, number: spawned);
		}
	}
}

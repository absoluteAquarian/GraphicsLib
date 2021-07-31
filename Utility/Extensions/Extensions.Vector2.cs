using Microsoft.Xna.Framework;
using Terraria;

namespace GraphicsLib.Utility.Extensions{
	public static partial class Extensions{
		public static Vector3 ScreenCoord(this Vector2 vector){
			//"vector" is a point on the screen... given the zoom is 1x
			//Let's correct that
			Vector2 screenCenter = new Vector2(Main.screenWidth / 2f, Main.screenHeight / 2f);
			Vector2 diff = vector - screenCenter;
			diff *= Main.GameZoomTarget;
			vector = screenCenter + diff;

			return new Vector3(-1 + vector.X / Main.screenWidth * 2, (-1 + vector.Y / Main.screenHeight * 2f) * -1, 0);
		}
	}
}

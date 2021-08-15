using Microsoft.Xna.Framework;
using System;
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

		/// <summary>
		/// Get the angle between two vectors
		/// </summary>
		public static float AngleBetween(this Vector2 source, Vector2 other){
			float srcAngle = source.ToRotation();
			float otherAngle = other.ToRotation();

			if(srcAngle < 0)
				srcAngle += MathHelper.TwoPi;
			if(otherAngle < 0)
				otherAngle += MathHelper.TwoPi;

			float diff = Math.Abs(srcAngle - otherAngle);
			if(diff > MathHelper.Pi)
				diff = MathHelper.TwoPi - diff;

			return diff;
		}

		/// <summary>
		/// Converts a vector into a unit vector pointing in the same direction
		/// </summary>
		public static Vector2 Unit(this Vector2 vector)
			=> vector / vector.Length();

		/// <summary>
		/// Projects <paramref name="orig"/> onto <paramref name="axis"/>
		/// </summary>
		/// <param name="orig">The original vector</param>
		/// <param name="axis">The axis vector</param>
		public static Vector2 ProjectOnto(this Vector2 orig, Vector2 axis)
			=> orig.Length() * (float)Math.Cos(orig.AngleBetween(axis)) * axis.Unit();
	}
}

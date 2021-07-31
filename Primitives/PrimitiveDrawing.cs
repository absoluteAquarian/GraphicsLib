﻿using GraphicsLib.Utility.Extensions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;

namespace GraphicsLib.Primitives{
	public static class PrimitiveDrawing{
		internal static BasicEffect simpleVertexEffect;
		internal static void Init(GraphicsDevice device){
			simpleVertexEffect = new BasicEffect(device){
				VertexColorEnabled = true
			};
		}

		public static void DrawLineStrip(Vector2[] points, Color color){
			if(points is null)
				throw new ArgumentNullException("points");

			if(points.Length < 2)
				throw new ArgumentException("Too few points provided to draw a line");

			PrimitivePacket packet = new PrimitivePacket(PrimitiveType.LineStrip);

			packet.AddDraw(ToPrimitive(points[0], color), ToPrimitive(points[1], color));
			for(int i = 2; i < points.Length; i++)
				packet.AddDraw(ToPrimitive(points[i], color));

			SubmitPacket(packet);
		}

		public static void DrawLineStrip(Vector2[] points, Color start, Color end){
			if(points is null)
				throw new ArgumentNullException("points");

			if(points.Length < 2)
				throw new ArgumentException("Too few points provided to draw a line");

			float lerpStep = 1f / (points.Length - 1);

			PrimitivePacket packet = new PrimitivePacket(PrimitiveType.LineStrip);

			packet.AddDraw(ToPrimitive(points[0], Color.Lerp(start, end, 0)), ToPrimitive(points[1], Color.Lerp(start, end, lerpStep)));
			for(int i = 2; i < points.Length; i++)
				packet.AddDraw(ToPrimitive(points[i], Color.Lerp(start, end, lerpStep * i)));

			SubmitPacket(packet);
		}

		public static void DrawLineStrip(Vector2[] points, Color[] colors){
			if(points is null)
				throw new ArgumentNullException("points");
			if(colors is null)
				throw new ArgumentNullException("colors");

			if(colors.Length != points.Length)
				throw new ArgumentException("Length of colors array must match length of points array", "colors");

			if(points.Length < 2)
				throw new ArgumentException("Too few points provided to draw a line");

			PrimitivePacket packet = new PrimitivePacket(PrimitiveType.LineStrip);

			packet.AddDraw(ToPrimitive(points[0], colors[0]), ToPrimitive(points[1], colors[0]));
			for(int i = 2; i < points.Length; i++)
				packet.AddDraw(ToPrimitive(points[i], colors[i]));

			SubmitPacket(packet);
		}

		public static void DrawLineList(Vector2[] points, Color color){
			if(points is null)
				throw new ArgumentNullException("points");

			if(points.Length % 2 != 0)
				throw new ArgumentException("Length of points array must be a multiple of 2", "points");

			//Nothing to draw, so don't
			if(points.Length < 2)
				throw new ArgumentException("Too few points provided to draw a line");

			PrimitivePacket packet = new PrimitivePacket(PrimitiveType.LineList);
			for(int i = 0; i < points.Length; i += 2)
				packet.AddDraw(ToPrimitive(points[i], color), ToPrimitive(points[i + 1], color));

			SubmitPacket(packet);
		}

		public static void DrawLineList(Vector2[] points, Color[] colors){
			if(points is null)
				throw new ArgumentNullException("points");
			if(colors is null)
				throw new ArgumentNullException("colors");

			if(points.Length % 2 != 0)
				throw new ArgumentException("Length of points array must be a multiple of 2", "points");
			if(colors.Length != points.Length)
				throw new ArgumentException("Length of colors array must match length of points array", "colors");

			//Nothing to draw, so don't
			if(points.Length < 2)
				return;

			PrimitivePacket packet = new PrimitivePacket(PrimitiveType.LineList);
			for(int i = 0; i < points.Length; i += 2)
				packet.AddDraw(ToPrimitive(points[i], colors[i]), ToPrimitive(points[i + 1], colors[i + 1]));

			SubmitPacket(packet);
		}

		public static void DrawHollowRectangle(Vector2 coordTL, Vector2 coordBR, Color colorTL, Color colorTR, Color colorBL, Color colorBR){
			Vector2 tr = new Vector2(coordBR.X, coordTL.Y);
			Vector2 bl = new Vector2(coordTL.X, coordBR.Y);

			PrimitivePacket packet = new PrimitivePacket(PrimitiveType.LineStrip);

			packet.AddDraw(ToPrimitive(coordTL, colorTL), ToPrimitive(tr, colorTR));
			packet.AddDraw(ToPrimitive(tr, colorTR),      ToPrimitive(coordBR, colorBR));
			packet.AddDraw(ToPrimitive(coordBR, colorBR), ToPrimitive(bl, colorBL));
			packet.AddDraw(ToPrimitive(bl, colorBL),      ToPrimitive(coordTL, colorTL));

			SubmitPacket(packet);
		}

		public static void DrawFilledRectangle(Vector2 coordTL, Vector2 coordBR, Color colorTL, Color colorTR, Color colorBL, Color colorBR){
			Vector2 tr = new Vector2(coordBR.X, coordTL.Y);
			Vector2 bl = new Vector2(coordTL.X, coordBR.Y);

			PrimitivePacket packet = new PrimitivePacket(PrimitiveType.TriangleList);

			packet.AddDraw(ToPrimitive(coordTL, colorTL), ToPrimitive(tr, colorTR), ToPrimitive(bl, colorBL));
			packet.AddDraw(ToPrimitive(bl, colorBL), ToPrimitive(tr, colorTR), ToPrimitive(coordBR, colorBR));

			SubmitPacket(packet);
		}

		public static void DrawHollowCircle(Vector2 center, float radius, Color color){
			PrimitivePacket packet = new PrimitivePacket(PrimitiveType.LineStrip);

			Vector2 rotate = Vector2.UnitX * radius;
			packet.AddDraw(ToPrimitive(rotate + center, color), ToPrimitive(rotate.RotatedBy(MathHelper.TwoPi / 360f) + center, color));
			for(int i = 2; i < 360; i++)
				packet.AddDraw(ToPrimitive(rotate.RotatedBy(MathHelper.TwoPi / 306f * i), color));

			SubmitPacket(packet);
		}

		public static void DrawFilledCircle(Vector2 center, float radius, Color color){
			PrimitivePacket packet = new PrimitivePacket(PrimitiveType.TriangleList);

			Vector2 rotate = Vector2.UnitX * radius;
			packet.AddDraw(ToPrimitive(rotate + center, color),
				ToPrimitive(rotate.RotatedBy(MathHelper.TwoPi / 360f) + center, color),
				ToPrimitive(center, color));
			for(int i = 1; i < 360; i++){
				packet.AddDraw(ToPrimitive(rotate.RotatedBy(MathHelper.TwoPi / 360 * i) + center, color),
					ToPrimitive(rotate.RotatedBy(MathHelper.TwoPi / 360f * (i + 1)) + center, color),
					ToPrimitive(center, color));
			}

			SubmitPacket(packet);
		}

		public static void DrawFilledCircle(Vector2 center, float radius, Color color, Color edge){
			PrimitivePacket packet = new PrimitivePacket(PrimitiveType.TriangleList);

			Vector2 rotate = Vector2.UnitX * radius;
			packet.AddDraw(ToPrimitive(rotate + center, edge),
				ToPrimitive(rotate.RotatedBy(MathHelper.TwoPi / 360f) + center, edge),
				ToPrimitive(center, color));
			for(int i = 1; i < 360; i++){
				packet.AddDraw(ToPrimitive(rotate.RotatedBy(MathHelper.TwoPi / 360 * i) + center, edge),
					ToPrimitive(rotate.RotatedBy(MathHelper.TwoPi / 360f * (i + 1)) + center, edge),
					ToPrimitive(center, color));
			}

			SubmitPacket(packet);
		}

		public static void DrawTriangleList(Vector2[] points, Color color){
			if(points is null)
				throw new ArgumentNullException("points");

			if(points.Length % 3 != 0)
				throw new ArgumentException("Length of points array must be a multiple of 3", "points");

			//Nothing to draw, so don't
			if(points.Length < 3)
				throw new ArgumentException("Too few points provided to draw a triangle");

			PrimitivePacket packet = new PrimitivePacket(PrimitiveType.TriangleList);
			for(int i = 0; i < points.Length; i += 3)
				packet.AddDraw(ToPrimitive(points[i], color), ToPrimitive(points[i + 1], color), ToPrimitive(points[i + 2], color));

			SubmitPacket(packet);
		}

		public static void DrawTriangleList(Vector2[] points, Color[] colors){
			if(points is null)
				throw new ArgumentNullException("points");
			if(colors is null)
				throw new ArgumentNullException("colors");

			if(points.Length % 3 != 0)
				throw new ArgumentException("Length of points array must be a multiple of 3", "points");
			if(colors.Length != points.Length)
				throw new ArgumentException("Length of colors array must match length of points array", "colors");

			//Nothing to draw, so don't
			if(points.Length < 3)
				throw new ArgumentException("Too few points provided to draw a triangle");

			PrimitivePacket packet = new PrimitivePacket(PrimitiveType.LineList);
			for(int i = 0; i < points.Length; i += 3)
				packet.AddDraw(ToPrimitive(points[i], colors[i]), ToPrimitive(points[i + 1], colors[i + 1]), ToPrimitive(points[i + 2], colors[i + 2]));

			SubmitPacket(packet);
		}

		public static void SubmitPacket(PrimitivePacket packet){
			if(packet is null)
				throw new ArgumentNullException("packet");

			VertexBuffer buffer = new VertexBuffer(Main.graphics.GraphicsDevice, typeof(VertexPositionColor), packet.draws.Count, BufferUsage.WriteOnly);

			//Calculate the number of primitives that will be drawn
			int count = packet.GetPrimitivesCount();

			//Device must not have a buffer attached for a buffer to be given data
			Main.graphics.GraphicsDevice.SetVertexBuffer(null);
			buffer.SetData(packet.draws.ToArray());

			//Set the buffer
			Main.graphics.GraphicsDevice.SetVertexBuffer(buffer);
			simpleVertexEffect.CurrentTechnique.Passes[0].Apply();

			//Draw the vertices
			Main.graphics.GraphicsDevice.DrawPrimitives(packet.type, 0, count);
		}

		/// <summary>
		/// Converts the riven world <paramref name="worldPos"/> coordinate and <paramref name="color"/> draw color into a <seealso cref="VertexPositionColor"/> that can be used in primitives drawing
		/// </summary>
		/// <param name="worldPos">The absolute world position</param>
		/// <param name="color">The draw color</param>
		public static VertexPositionColor ToPrimitive(Vector2 worldPos, Color color){
			Vector3 pos = (worldPos - Main.screenPosition).ScreenCoord();
			if(Main.LocalPlayer.gravDir == -1)
				pos.Y = -pos.Y;

			return new VertexPositionColor(pos, color);
		}
	}
}

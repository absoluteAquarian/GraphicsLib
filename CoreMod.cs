using GraphicsLib.Meshes;
using GraphicsLib.Primitives;
using GraphicsLib.Utility.Extensions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace GraphicsLib{
	public class CoreMod : Mod{
		private static Dictionary<MethodInfo, string[]> methodArgs;

		internal static int indirectMeshNextID;
		internal static Dictionary<int, Mesh> indirectMeshes;

		public override void Load(){
			if(Main.netMode != NetmodeID.Server)
				PrimitiveDrawing.Init(Main.graphics.GraphicsDevice);

			methodArgs = new Dictionary<MethodInfo, string[]>();
			indirectMeshNextID = 0;
			indirectMeshes = new Dictionary<int, Mesh>();
		}

		public override void Unload(){
			PrimitiveDrawing.simpleVertexEffect?.Dispose();
			PrimitiveDrawing.simpleVertexEffect = null;

			methodArgs = null;

			if(indirectMeshes != null){
				//Release any references
				foreach(var mesh in indirectMeshes.Values){
					mesh.texture = null;
					mesh.mesh = null;
					mesh.shader = null;
				}
			}

			indirectMeshes = null;
		}

		public override object Call(params object[] args){
			if(args is null)
				throw new ArgumentNullException("args");

			if(args.Length < 2)
				throw new ArgumentException("Too few arguments were provided");

			if(!(args[0] is string function))
				throw new ArgumentException("Expected a function name for the first argument");

			void CheckArgsLength(int expected, params string[] argNames){
				if(args.Length != expected)
					throw new ArgumentOutOfRangeException($"Expected {expected} arguments for Mod.Call(\"{function}\", {string.Join(",", argNames)}), got {args.Length} arguments instead");
			}

			void CheckArg<TExpected>(int argSlot, out TExpected validInstance, string additionalArgs = null){
				if(additionalArgs != null)
					additionalArgs = ", " + additionalArgs;

				if(!(args[argSlot] is TExpected expected))
					throw new ArgumentException($"Argument {argSlot} for Mod.Call(\"{function}\"{additionalArgs ?? ""}) must be of type {typeof(TExpected).FullNameUpgraded()}");

				validInstance = expected;
			}

			switch(function){
				case "Draw Connected Lines, Single Color":
					CheckArgsLength(3, GetMethodArgs(typeof(PrimitiveDrawing), "DrawLineStrip", BindingFlags.Public | BindingFlags.Static, typeof(Vector2[]), typeof(Color)));
					CheckArg(1, out Vector2[] points);
					CheckArg(2, out Color color);

					PrimitiveDrawing.DrawLineStrip(points, color);
					return true;
				case "Draw Connected Lines, Lerp Color":
					CheckArgsLength(4, GetMethodArgs(typeof(PrimitiveDrawing), "DrawLineStrip", BindingFlags.Public | BindingFlags.Static, typeof(Vector2[]), typeof(Color), typeof(Color)));
					CheckArg(1, out points);
					CheckArg(2, out Color start);
					CheckArg(3, out Color end);

					PrimitiveDrawing.DrawLineStrip(points, start, end);
					return true;
				case "Draw Connected Lines, Lerp Color per Point":
					CheckArgsLength(3, GetMethodArgs(typeof(PrimitiveDrawing), "DrawLineStrip", BindingFlags.Public | BindingFlags.Static, typeof(Vector2[]), typeof(Color[])));
					CheckArg(1, out points);
					CheckArg(2, out Color[] colors);

					PrimitiveDrawing.DrawLineStrip(points, colors);
					return true;
				case "Draw Lines, Single Color":
					CheckArgsLength(3, GetMethodArgs(typeof(PrimitiveDrawing), "DrawLineList", BindingFlags.Public | BindingFlags.Static, typeof(Vector2[]), typeof(Color)));
					CheckArg(1, out points);
					CheckArg(2, out color);

					PrimitiveDrawing.DrawLineList(points, color);
					return true;
				case "Draw Lines, Lerp Color per Point":
					CheckArgsLength(3, GetMethodArgs(typeof(PrimitiveDrawing), "DrawLineList", BindingFlags.Public | BindingFlags.Static, typeof(Vector2[]), typeof(Color[])));
					CheckArg(1, out points);
					CheckArg(2, out colors);

					PrimitiveDrawing.DrawLineList(points, colors);
					return true;
				case "Draw Hollow Rectangle":
					CheckArgsLength(7, GetMethodArgs(typeof(PrimitiveDrawing), "DrawHollowRectangle", BindingFlags.Public | BindingFlags.Static));
					CheckArg(1, out Vector2 coordTL);
					CheckArg(2, out Vector2 coordBR);
					CheckArg(3, out Color colorTL);
					CheckArg(4, out Color colorTR);
					CheckArg(5, out Color colorBL);
					CheckArg(6, out Color colorBR);

					PrimitiveDrawing.DrawHollowRectangle(coordTL, coordBR, colorTL, colorTR, colorBL, colorBR);
					return true;
				case "Draw Filled Rectangle":
					CheckArgsLength(7, GetMethodArgs(typeof(PrimitiveDrawing), "DrawFilledRectangle", BindingFlags.Public | BindingFlags.Static));
					CheckArg(1, out coordTL);
					CheckArg(2, out coordBR);
					CheckArg(3, out colorTL);
					CheckArg(4, out colorTR);
					CheckArg(5, out colorBL);
					CheckArg(6, out colorBR);

					PrimitiveDrawing.DrawFilledRectangle(coordTL, coordBR, colorTL, colorTR, colorBL, colorBR);
					return true;
				case "Draw Hollow Circle":
					CheckArgsLength(4, GetMethodArgs(typeof(PrimitiveDrawing), "DrawHollowCircle", BindingFlags.Public | BindingFlags.Instance));
					CheckArg(1, out Vector2 center);
					CheckArg(2, out float radius);
					CheckArg(3, out color);

					PrimitiveDrawing.DrawHollowCircle(center, radius, color);
					return true;
				case "Draw Filled Circle, Single Color":
					CheckArgsLength(4, GetMethodArgs(typeof(PrimitiveDrawing), "DrawFilledCircle", BindingFlags.Public | BindingFlags.Instance, typeof(Vector2), typeof(float), typeof(Color)));
					CheckArg(1, out center);
					CheckArg(2, out radius);
					CheckArg(3, out color);

					PrimitiveDrawing.DrawFilledCircle(center, radius, color);
					return true;
				case "Draw Filled Circle, Lerp Color":
					CheckArgsLength(4, GetMethodArgs(typeof(PrimitiveDrawing), "DrawFilledCircle", BindingFlags.Public | BindingFlags.Instance, typeof(Vector2), typeof(float), typeof(Color), typeof(Color)));
					CheckArg(1, out center);
					CheckArg(2, out radius);
					CheckArg(3, out color);
					CheckArg(4, out Color edge);

					PrimitiveDrawing.DrawFilledCircle(center, radius, color, edge);
					return true;
				case "Draw Triangles, Single Color":
					CheckArgsLength(3, GetMethodArgs(typeof(PrimitiveDrawing), "DrawTriangleList", BindingFlags.Public | BindingFlags.Static, typeof(Vector2[]), typeof(Color)));
					CheckArg(1, out points);
					CheckArg(2, out color);

					PrimitiveDrawing.DrawTriangleList(points, color);
					return true;
				case "Draw Triangles, Lerp Color per Point":
					CheckArgsLength(4, GetMethodArgs(typeof(PrimitiveDrawing), "DrawTriangleList", BindingFlags.Public | BindingFlags.Static, typeof(Vector2[]), typeof(Color), typeof(Color)));
					CheckArg(1, out points);
					CheckArg(2, out colors);

					PrimitiveDrawing.DrawTriangleList(points, colors);
					return true;
				case "Draw Mesh, Single Color":
					CheckArgsLength(6, GetMethodArgs(typeof(Mesh), ".ctor", BindingFlags.Public | BindingFlags.Instance, typeof(Texture2D), typeof(Vector2[]), typeof(Vector2[]), typeof(Color), typeof(Effect)));
					CheckArg(1, out Texture2D texture);
					CheckArg(2, out Vector2[] positions);
					CheckArg(3, out Vector2[] textureCoordinates);
					CheckArg(4, out color);
					CheckArg(5, out Effect shader);

					Mesh mesh = new Mesh(texture, positions, textureCoordinates, color, shader);
					mesh.Draw();

					return mesh.ID;
				case "Draw Mesh, Lerp Color per Vertex":
					CheckArgsLength(6, GetMethodArgs(typeof(Mesh), ".ctor", BindingFlags.Public | BindingFlags.Instance, typeof(Texture2D), typeof(Vector2[]), typeof(Vector2[]), typeof(Color), typeof(Effect)));
					CheckArg(1, out texture);
					CheckArg(2, out positions);
					CheckArg(3, out textureCoordinates);
					CheckArg(4, out colors);
					CheckArg(5, out shader);
					
					mesh = new Mesh(texture, positions, textureCoordinates, colors, shader);
					mesh.Draw();

					return mesh.ID;
				case "Draw Mesh Indirect":
					CheckArgsLength(1, "int id");
					CheckArg(1, out int id);

					if(!indirectMeshes.TryGetValue(id, out mesh))
						throw new ArgumentException($"ID {id} does not refer to a valid cached Mesh instance");

					mesh.Draw();
					return true;
				case "Modify Mesh":
					//Array field will need an additional argument specifying the slot to modify
					string meshArg;
					try{
						CheckArgsLength(3, "int id", "string field", "object value");
						CheckArg(1, out id);
						CheckArg(2, out meshArg);
					}catch{
						if(args.Length >= 3){
							CheckArg(1, out id);
							CheckArg(2, out meshArg);

							if(!(meshArg == "position" || meshArg == "texCoord" || meshArg == "color"))
								throw;

							CheckArgsLength(4, "int id", "string arrayField", "object value", "int slot");
						}else
							throw;
					}

					if(!indirectMeshes.TryGetValue(id, out mesh))
						throw new ArgumentException($"ID {id} does not refer to a valid cached Mesh instance");

					void CheckSlot(out int slot){
						CheckArg(3, out slot);

						if(slot < 0 || slot > mesh.mesh.Length)
							throw new IndexOutOfRangeException($"Array slot requested ({slot}) was outside the bounds of the mesh's vertex array ({mesh.mesh.Length})");
					}

					switch(meshArg){
						case "position":
							CheckArg(2, out Vector2 position, "int id, \"position\", Vector2 position");
							CheckSlot(out int slot);

							mesh.mesh[slot].Position = new Vector3(position, 0);
							return true;
						case "texCoord":
							CheckArg(2, out Vector2 coordinate, "int id, \"coordinate\", Vector2 coordinate");
							CheckSlot(out slot);

							if(coordinate.X < 0 || coordinate.X > 1 || coordinate.Y < 0 || coordinate.Y > 1)
								throw new ArgumentOutOfRangeException($"Texture coordinate had invalid values (X: {coordinate.X}, Y: {coordinate.Y}).  Expected values are between 0 and 1, inclusive.");

							mesh.mesh[slot].TextureCoordinate = coordinate;
							return true;
						case "color":
							CheckArg(2, out color, "int id, \"color\", Color color");
							CheckSlot(out slot);

							mesh.mesh[slot].Color = color;
							return true;
						case "shader":
							CheckArg(2, out shader, "int id, \"shader\", Effect shader");

							mesh.shader = shader;
							return true;
						case "texture":
							CheckArg(2, out texture, "int id, \"texture\", Texture2D texture");

							mesh.texture = texture;
							return true;
						default:
							throw new ArgumentException($"Unknown mesh member requested: {meshArg}");
					}
				default:
					throw new ArgumentException($"Function \"{function}\" is not defined by GraphicsLib");
			}

			// TODO: people would like an easy Zenith trail implementation... find it in the 1.4 source and implement it in GraphicsLib
		}

		/// <summary>
		/// Gets a method's parameters
		/// </summary>
		/// <param name="type">The type</param>
		/// <param name="method">The name of the method</param>
		/// <param name="flags">The flags used when searching for the method</param>
		/// <param name="argTypes">(Optional) The types of the method arguments.  For methods with no overloads, this can be ignored.</param>
		private static string[] GetMethodArgs(Type type, string method, BindingFlags flags, params Type[] argTypes){
			MethodInfo methodInfo = null;
			if(argTypes.Length == 0)
				methodInfo = type.GetMethod(method, flags);
			if(methodInfo is null)
				methodInfo = type.GetMethod(method, flags, null, argTypes, null);

			//If the method is still null here, it couldn't be found
			if(methodInfo is null)
				throw new ArgumentException($"Could not find method \"{method}\" in type {type.FullNameUpgraded()}");

			if(methodArgs.TryGetValue(methodInfo, out var args))
				return args;

			var namedArgs = methodInfo.GetParameters().Select(p => $"{p.ParameterType.FullNameUpgraded()} {p.Name}").ToArray();

			return methodArgs[methodInfo] = namedArgs;
		}
	}
}
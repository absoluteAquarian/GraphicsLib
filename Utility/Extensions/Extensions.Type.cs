using System;
using System.Collections.Generic;
using System.Text;

namespace GraphicsLib.Utility.Extensions{
	public static partial class Extensions{
		private static readonly Dictionary<Type, string> dealias = new Dictionary<Type, string>(){
			[typeof(bool)] = "bool",
			[typeof(sbyte)] = "sbyte",
			[typeof(short)] = "short",
			[typeof(int)] = "int",
			[typeof(long)] = "long",
			[typeof(byte)] = "byte",
			[typeof(ushort)] = "ushort",
			[typeof(uint)] = "uint",
			[typeof(ulong)] = "ulong",
			[typeof(float)] = "float",
			[typeof(double)] = "double",
			[typeof(decimal)] = "decimal",
			[typeof(char)] = "char",
			[typeof(string)] = "string",
			[typeof(IntPtr)] = "IntPtr",
			[typeof(UIntPtr)] = "UIntPtr",
			[typeof(object)] = "object"
		};

		public static string FullNameUpgraded(this Type type){
			if(type.IsGenericTypeDefinition)
				throw new ArgumentException("Type argument was a type definition");

			if(!type.IsGenericType)
				return dealias.TryGetValue(type, out string name) ? name : type.Name;

			StringBuilder sb;
			const int size = 20;

			void GetSubTypesString(){
				var subTypes = type.GetGenericArguments();

				sb.Append(subTypes[0].FullNameUpgraded());
				for(int i = 1; i < subTypes.Length; i++){
					sb.Append(", ");
					sb.Append(subTypes[i].FullNameUpgraded());
				}
			}

			//Special treatment for Nullable<T> and ValueTuple<>
			if(type.IsGenericType){
				var genDef = type.GetGenericTypeDefinition();

				if(genDef == typeof(Nullable<>))
					return type.GetGenericArguments()[0].FullNameUpgraded() + "?";
				else if(genDef == typeof(ValueTuple<>)){
					sb = new StringBuilder(size);

					sb.Append('(');
					GetSubTypesString();
					sb.Append(')');

					return sb.ToString();
				}
			}

			sb = new StringBuilder(size);

			sb.Append(type.Name.Substring(0, type.Name.IndexOf('`')));
			sb.Append('<');
			GetSubTypesString();
			sb.Append('>');

			return sb.ToString();
		}
	}
}

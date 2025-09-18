// This code was borrowed from the MonoGame: https://github.com/MonoGame/MonoGame

using System.IO;
using System.Numerics;

namespace DigitalRiseModel.Converter
{
	internal static class AssimpUtility
	{
		public static Microsoft.Xna.Framework.Matrix ToXna(this Matrix4x4 matrix)
		{
			var result = Microsoft.Xna.Framework.Matrix.Identity;

			result.M11 = matrix.M11;
			result.M12 = matrix.M12;
			result.M13 = matrix.M13;
			result.M14 = matrix.M14;

			result.M21 = matrix.M21;
			result.M22 = matrix.M22;
			result.M23 = matrix.M23;
			result.M24 = matrix.M24;

			result.M31 = matrix.M31;
			result.M32 = matrix.M32;
			result.M33 = matrix.M33;
			result.M34 = matrix.M34;

			result.M41 = matrix.M41;
			result.M42 = matrix.M42;
			result.M43 = matrix.M43;
			result.M44 = matrix.M44;

			result = Microsoft.Xna.Framework.Matrix.Transpose(result);

			return result;
		}

		public static Microsoft.Xna.Framework.Vector2 ToXnaVector2(this Vector3 v) => new Vector2(v.X, v.Y);
		public static Microsoft.Xna.Framework.Vector3 ToXna(this Vector3 v) => new Vector3(v.X, v.Y, v.Z);
		public static Microsoft.Xna.Framework.Vector4 ToXna(this Vector4 v) => new Vector4(v.X, v.Y, v.Z, v.W);
		public static Microsoft.Xna.Framework.Color ToXnaColor(this Vector4 v) => new Microsoft.Xna.Framework.Color(v.X, v.Y, v.Z, v.W);

		public static Microsoft.Xna.Framework.Quaternion ToXna(this Quaternion v) => new Quaternion(v.X, v.Y, v.Z, v.W);

		public static void Write(this BinaryWriter writer, Microsoft.Xna.Framework.Vector2 v)
		{
			writer.Write(v.X);
			writer.Write(v.Y);
		}

		public static void Write(this BinaryWriter writer, Microsoft.Xna.Framework.Vector3 v)
		{
			writer.Write(v.X);
			writer.Write(v.Y);
			writer.Write(v.Z);
		}

		public static void Write(this BinaryWriter writer, Microsoft.Xna.Framework.Vector4 v)
		{
			writer.Write(v.X);
			writer.Write(v.Y);
			writer.Write(v.Z);
			writer.Write(v.W);
		}

		public static void Write(this BinaryWriter writer, Microsoft.Xna.Framework.Color c)
		{
			writer.Write(c.PackedValue);
		}
	}
}

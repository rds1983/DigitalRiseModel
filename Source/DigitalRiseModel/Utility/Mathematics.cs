using Microsoft.Xna.Framework;
using System;

namespace DigitalRiseModel.Utility
{
	internal static class Mathematics
	{
		public static Matrix CreateTransform(Vector3 translation, Vector3 scale, Quaternion rotation)
		{
			var result = Matrix.CreateFromQuaternion(rotation);
			result.Translation = translation;

			result.M11 *= scale.X;
			result.M21 *= scale.X;
			result.M31 *= scale.X;
			result.M12 *= scale.Y;
			result.M22 *= scale.Y;
			result.M32 *= scale.Y;
			result.M13 *= scale.Z;
			result.M23 *= scale.Z;
			result.M33 *= scale.Z;

			return result;
		}

		public static BoundingBox Transform(this BoundingBox source, ref Matrix matrix)
		{
			Vector3.Transform(ref source.Min, ref matrix, out Vector3 v1);
			Vector3.Transform(ref source.Max, ref matrix, out Vector3 v2);

			var min = new Vector3(Math.Min(v1.X, v2.X), Math.Min(v1.Y, v2.Y), Math.Min(v1.Z, v2.Z));
			var max = new Vector3(Math.Max(v1.X, v2.X), Math.Max(v1.Y, v2.Y), Math.Max(v1.Z, v2.Z));

			return new BoundingBox(min, max);
		}
	}
}

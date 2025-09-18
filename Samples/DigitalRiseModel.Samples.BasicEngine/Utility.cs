using Microsoft.Xna.Framework;
using System;

namespace DigitalRiseModel.Samples.BasicEngine
{
	internal static class Utility
	{
		/// <summary>
		/// The value for which all absolute numbers smaller than are considered equal to zero.
		/// </summary>
		public const float ZeroTolerance = 1e-6f;

		/// <summary>
		/// Compares two floating point numbers based on an epsilon zero tolerance.
		/// </summary>
		/// <param name="left">The first number to compare.</param>
		/// <param name="right">The second number to compare.</param>
		/// <param name="epsilon">The epsilon value to use for zero tolerance.</param>
		/// <returns><c>true</c> if <paramref name="left"/> is within epsilon of <paramref name="right"/>; otherwise, <c>false</c>.</returns>
		public static bool EpsilonEquals(this float left, float right, float epsilon = ZeroTolerance)
		{
			return Math.Abs(left - right) <= epsilon;
		}

		public static float ClampDegree(this float deg)
		{
			var isNegative = deg < 0;
			deg = Math.Abs(deg);
			deg = deg % 360;
			if (isNegative)
			{
				deg = 360 - deg;
			}

			return deg;
		}

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
	}
}

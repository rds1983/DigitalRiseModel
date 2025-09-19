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

		public static bool EpsilonEquals(this Vector2 a, Vector2 b, float epsilon = ZeroTolerance)
		{
			return a.X.EpsilonEquals(b.X, epsilon) &&
				a.Y.EpsilonEquals(b.Y, epsilon);
		}

		public static bool EpsilonEquals(this Vector3 a, Vector3 b, float epsilon = ZeroTolerance)
		{
			return a.X.EpsilonEquals(b.X, epsilon) &&
				a.Y.EpsilonEquals(b.Y, epsilon) &&
				a.Z.EpsilonEquals(b.Z, epsilon);
		}

		public static bool EpsilonEquals(this Vector4 a, Vector4 b, float epsilon = ZeroTolerance)
		{
			return a.X.EpsilonEquals(b.X, epsilon) &&
				a.Y.EpsilonEquals(b.Y, epsilon) &&
				a.Z.EpsilonEquals(b.Z, epsilon) &&
				a.W.EpsilonEquals(b.W, epsilon);
		}

		public static bool EpsilonEquals(this Quaternion a, Quaternion b, float epsilon = ZeroTolerance)
		{
			return a.X.EpsilonEquals(b.X, epsilon) &&
				a.Y.EpsilonEquals(b.Y, epsilon) &&
				a.Z.EpsilonEquals(b.Z, epsilon) &&
				a.W.EpsilonEquals(b.W, epsilon);
		}

		public static bool IsZero(this float a)
		{
			return a.EpsilonEquals(0.0f);
		}

		public static bool IsZero(this Vector2 a)
		{
			return a.EpsilonEquals(Vector2.Zero);
		}

		public static bool IsZero(this Vector3 a)
		{
			return a.EpsilonEquals(Vector3.Zero);
		}

		public static bool IsZero(this Vector4 a)
		{
			return a.EpsilonEquals(Vector4.Zero);
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

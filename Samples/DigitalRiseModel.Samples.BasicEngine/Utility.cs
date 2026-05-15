using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
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

		/// <summary>
		/// Compares two 2D vectors based on an epsilon zero tolerance.
		/// </summary>
		/// <param name="a">The first vector to compare.</param>
		/// <param name="b">The second vector to compare.</param>
		/// <param name="epsilon">The epsilon value to use for zero tolerance.</param>
		/// <returns><c>true</c> if both vectors are within epsilon of each other; otherwise, <c>false</c>.</returns>
		public static bool EpsilonEquals(this Vector2 a, Vector2 b, float epsilon = ZeroTolerance)
		{
			return a.X.EpsilonEquals(b.X, epsilon) &&
				a.Y.EpsilonEquals(b.Y, epsilon);
		}

		/// <summary>
		/// Compares two 3D vectors based on an epsilon zero tolerance.
		/// </summary>
		/// <param name="a">The first vector to compare.</param>
		/// <param name="b">The second vector to compare.</param>
		/// <param name="epsilon">The epsilon value to use for zero tolerance.</param>
		/// <returns><c>true</c> if both vectors are within epsilon of each other; otherwise, <c>false</c>.</returns>
		public static bool EpsilonEquals(this Vector3 a, Vector3 b, float epsilon = ZeroTolerance)
		{
			return a.X.EpsilonEquals(b.X, epsilon) &&
				a.Y.EpsilonEquals(b.Y, epsilon) &&
				a.Z.EpsilonEquals(b.Z, epsilon);
		}

		/// <summary>
		/// Compares two 4D vectors based on an epsilon zero tolerance.
		/// </summary>
		/// <param name="a">The first vector to compare.</param>
		/// <param name="b">The second vector to compare.</param>
		/// <param name="epsilon">The epsilon value to use for zero tolerance.</param>
		/// <returns><c>true</c> if both vectors are within epsilon of each other; otherwise, <c>false</c>.</returns>
		public static bool EpsilonEquals(this Vector4 a, Vector4 b, float epsilon = ZeroTolerance)
		{
			return a.X.EpsilonEquals(b.X, epsilon) &&
				a.Y.EpsilonEquals(b.Y, epsilon) &&
				a.Z.EpsilonEquals(b.Z, epsilon) &&
				a.W.EpsilonEquals(b.W, epsilon);
		}

		/// <summary>
		/// Compares two quaternions based on an epsilon zero tolerance.
		/// </summary>
		/// <param name="a">The first quaternion to compare.</param>
		/// <param name="b">The second quaternion to compare.</param>
		/// <param name="epsilon">The epsilon value to use for zero tolerance.</param>
		/// <returns><c>true</c> if both quaternions are within epsilon of each other; otherwise, <c>false</c>.</returns>
		public static bool EpsilonEquals(this Quaternion a, Quaternion b, float epsilon = ZeroTolerance)
		{
			return a.X.EpsilonEquals(b.X, epsilon) &&
				a.Y.EpsilonEquals(b.Y, epsilon) &&
				a.Z.EpsilonEquals(b.Z, epsilon) &&
				a.W.EpsilonEquals(b.W, epsilon);
		}

		/// <summary>
		/// Determines whether a floating point number is effectively zero using epsilon comparison.
		/// </summary>
		/// <param name="a">The value to check.</param>
		/// <returns><c>true</c> if the value is within the zero tolerance; otherwise, <c>false</c>.</returns>
		public static bool IsZero(this float a)
		{
			return a.EpsilonEquals(0.0f);
		}

		/// <summary>
		/// Determines whether a 2D vector is effectively zero using epsilon comparison.
		/// </summary>
		/// <param name="a">The vector to check.</param>
		/// <returns><c>true</c> if the vector magnitude is within the zero tolerance; otherwise, <c>false</c>.</returns>
		public static bool IsZero(this Vector2 a)
		{
			return a.EpsilonEquals(Vector2.Zero);
		}

		/// <summary>
		/// Determines whether a 3D vector is effectively zero using epsilon comparison.
		/// </summary>
		/// <param name="a">The vector to check.</param>
		/// <returns><c>true</c> if the vector magnitude is within the zero tolerance; otherwise, <c>false</c>.</returns>
		public static bool IsZero(this Vector3 a)
		{
			return a.EpsilonEquals(Vector3.Zero);
		}

		/// <summary>
		/// Determines whether a 4D vector is effectively zero using epsilon comparison.
		/// </summary>
		/// <param name="a">The vector to check.</param>
		/// <returns><c>true</c> if the vector magnitude is within the zero tolerance; otherwise, <c>false</c>.</returns>
		public static bool IsZero(this Vector4 a)
		{
			return a.EpsilonEquals(Vector4.Zero);
		}

		/// <summary>
		/// Clamps an angle in degrees to the range [0, 360).
		/// Negative angles are wrapped to their positive equivalent.
		/// </summary>
		/// <param name="deg">The angle in degrees to clamp.</param>
		/// <returns>The clamped angle in the range [0, 360).</returns>
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

		/// <summary>
		/// Transforms a bounding box by the specified transformation matrix.
		/// Recalculates the min/max bounds to fit the transformed box properly.
		/// </summary>
		/// <param name="source">The bounding box to transform.</param>
		/// <param name="matrix">The transformation matrix to apply.</param>
		/// <returns>A new bounding box with transformed coordinates.</returns>
		public static BoundingBox Transform(this BoundingBox source, ref Matrix matrix)
		{
			Vector3.Transform(ref source.Min, ref matrix, out Vector3 v1);
			Vector3.Transform(ref source.Max, ref matrix, out Vector3 v2);

			var min = new Vector3(Math.Min(v1.X, v2.X), Math.Min(v1.Y, v2.Y), Math.Min(v1.Z, v2.Z));
			var max = new Vector3(Math.Max(v1.X, v2.X), Math.Max(v1.Y, v2.Y), Math.Max(v1.Z, v2.Z));

			return new BoundingBox(min, max);
		}

		/// <summary>
		/// Calculates the size (scale) of a bounding box as a 3D vector.
		/// </summary>
		/// <param name="box">The bounding box to measure.</param>
		/// <returns>A vector representing the width, height, and depth of the box.</returns>
		public static Vector3 ToScale(this BoundingBox box)
		{
			return new Vector3(box.Max.X - box.Min.X,
				box.Max.Y - box.Min.Y,
				box.Max.Z - box.Min.Z);
		}

		/// <summary>
		/// Renders a mesh part to the graphics device.
		/// Sets up vertex/index buffers and issues the appropriate draw call.
		/// </summary>
		/// <param name="mesh">The mesh part to draw.</param>
		/// <param name="graphicsDevice">The graphics device to draw with.</param>
		/// <exception cref="ArgumentNullException">Thrown if graphicsDevice is null.</exception>
		public static void Draw(this DrMeshPart mesh, GraphicsDevice graphicsDevice)
		{
			if (graphicsDevice == null)
			{
				throw new ArgumentNullException(nameof(graphicsDevice));
			}

			graphicsDevice.SetVertexBuffer(mesh.VertexBuffer);
			if (mesh.IndexBuffer == null)
			{
				graphicsDevice.DrawPrimitives(mesh.PrimitiveType, mesh.VertexOffset, mesh.PrimitiveCount);
			}
			else
			{

				graphicsDevice.Indices = mesh.IndexBuffer;

#if MONOGAME
				graphicsDevice.DrawIndexedPrimitives(mesh.PrimitiveType, mesh.VertexOffset, mesh.StartIndex, mesh.PrimitiveCount);
#else
				graphicsDevice.DrawIndexedPrimitives(mesh.PrimitiveType, mesh.VertexOffset, 0, mesh.NumVertices, mesh.StartIndex, mesh.PrimitiveCount);
#endif
			}
		}
	}
}

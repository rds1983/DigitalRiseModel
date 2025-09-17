using DigitalRiseModel.Storage;
using DigitalRiseModel.Vertices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DigitalRiseModel
{
	internal static class Utility
	{
		/// <summary>
		/// Safely disposes the object.
		/// </summary>
		/// <typeparam name="T">The type of the object.</typeparam>
		/// <param name="obj">The object to dispose. Can be <see langword="null"/>.</param>
		/// <remarks>
		/// The method calls <see cref="IDisposable.Dispose"/> if the <paramref name="obj"/> is not null
		/// and implements the interface <see cref="IDisposable"/>.
		/// </remarks>
		public static void SafeDispose<T>(this T obj) where T : class
		{
			var disposable = obj as IDisposable;
			if (disposable != null)
			{
				disposable.Dispose();
			}
		}

		public static int CalculateStride(this IEnumerable<VertexElementContent> elements)
		{
			var result = 0;

			foreach (var channel in elements)
			{
				result += channel.Format.GetSize();
			}

			return result;
		}

		public static int GetSize(this VertexElementFormat elementFormat)
		{
			switch (elementFormat)
			{
				case VertexElementFormat.Single:
					return 4;
				case VertexElementFormat.Vector2:
					return 8;
				case VertexElementFormat.Vector3:
					return 12;
				case VertexElementFormat.Vector4:
					return 16;
				case VertexElementFormat.Color:
					return 4;
				case VertexElementFormat.Byte4:
					return 4;
				case VertexElementFormat.Short2:
					return 4;
				case VertexElementFormat.Short4:
					return 8;
				case VertexElementFormat.NormalizedShort2:
					return 4;
				case VertexElementFormat.NormalizedShort4:
					return 8;
				case VertexElementFormat.HalfVector2:
					return 4;
				case VertexElementFormat.HalfVector4:
					return 8;
			}

			throw new Exception($"Unknown vertex element format {elementFormat}");
		}

		/// <summary>
		/// Gets the number of primitives for the given vertex/index buffer and primitive type.
		/// </summary>
		/// <param name="vertexBuffer">The vertex buffer.</param>
		/// <param name="count"></param>
		/// <returns>The number of primitives in the given vertex and index buffer.</returns>
		public static int GetPrimitiveCount(this PrimitiveType primitiveType, int count)
		{
			switch (primitiveType)
			{
				case PrimitiveType.LineList:
					return count / 2;
				case PrimitiveType.LineStrip:
					return count - 1;
				case PrimitiveType.TriangleList:
					return count / 3;
				case PrimitiveType.TriangleStrip:
					return count - 2;
			}

			throw new NotSupportedException("Unknown primitive type.");
		}

		public static int GetSize(this IndexElementSize indexType)
		{
			switch (indexType)
			{
				case IndexElementSize.SixteenBits:
					return 2;
				case IndexElementSize.ThirtyTwoBits:
					return 4;
			}

			throw new Exception($"Unknown index buffer type {indexType}");
		}

		public static VertexBuffer CreateVertexBuffer<T>(this T[] vertices, GraphicsDevice device) where T : struct, IVertexType
		{
			var result = new VertexBuffer(device, new T().VertexDeclaration, vertices.Length, BufferUsage.None);
			result.SetData(vertices);

			return result;
		}

		public static VertexBuffer CreateVertexBuffer(this Vector3[] vertices, GraphicsDevice device)
		{
			var result = new VertexBuffer(device, VertexPosition.VertexDeclaration, vertices.Length, BufferUsage.None);
			result.SetData(vertices);

			return result;
		}

		public static IndexBuffer CreateIndexBuffer(this ushort[] indices, GraphicsDevice device)
		{
			var result = new IndexBuffer(device, IndexElementSize.SixteenBits, indices.Length, BufferUsage.None);
			result.SetData(indices);

			return result;
		}

		public static IndexBuffer CreateIndexBuffer(this int[] indices, GraphicsDevice device)
		{
			var result = new IndexBuffer(device, IndexElementSize.ThirtyTwoBits, indices.Length, BufferUsage.None);
			result.SetData(indices);

			return result;
		}

		public static IEnumerable<Vector3> GetPositions(this VertexPositionNormalTexture[] vertices) => (from v in vertices select v.Position);
		public static IEnumerable<Vector3> GetPositions(this VertexPositionTexture[] vertices) => (from v in vertices select v.Position);
		public static IEnumerable<Vector3> GetPositions(this VertexPositionNormal[] vertices) => (from v in vertices select v.Position);
		public static IEnumerable<Vector3> GetPositions(this VertexPosition[] vertices) => (from v in vertices select v.Position);

		public static BoundingBox BuildBoundingBox(this VertexPositionNormalTexture[] vertices) => BoundingBox.CreateFromPoints(vertices.GetPositions());
		public static BoundingBox BuildBoundingBox(this VertexPositionTexture[] vertices) => BoundingBox.CreateFromPoints(vertices.GetPositions());
		public static BoundingBox BuildBoundingBox(this VertexPositionNormal[] vertices) => BoundingBox.CreateFromPoints(vertices.GetPositions());
		public static BoundingBox BuildBoundingBox(this VertexPosition[] vertices) => BoundingBox.CreateFromPoints(vertices.GetPositions());
		public static BoundingBox BuildBoundingBox(this Vector3[] vertices) => BoundingBox.CreateFromPoints(vertices);

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

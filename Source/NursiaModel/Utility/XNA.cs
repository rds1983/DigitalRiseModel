using NursiaModel.Vertices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NursiaModel.Utility
{
	internal static class XNA
	{
		/// <summary>
		/// Gets the number of primitives for the given vertex/index buffer and primitive type.
		/// </summary>
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
	}
}

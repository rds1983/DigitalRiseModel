using DigitalRiseModel.Utility;
using DigitalRiseModel.Vertices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace DigitalRiseModel
{
	public class DrMeshPart : DrDisposable
	{
		public DrMesh Mesh { get; internal set; }
		public PrimitiveType PrimitiveType { get; }
		public int PrimitiveCount { get; }

		public VertexBuffer VertexBuffer { get; }
		public int VertexOffset { get; }
		public int NumVertices { get; }

		public IndexBuffer IndexBuffer { get; }
		public int StartIndex { get; }

		public BoundingBox BoundingBox { get; set; }

		public bool HasNormals { get; }
		public VertexElementFormat? TangentsFormat { get; }

		public DrMaterial Material { get; set; }
		public DrSkin Skin { get; set; }

		public object Tag { get; set; }

		private DrMeshPart(PrimitiveType primitiveType, int primitiveCount, VertexBuffer vertexBuffer, int vertexOffset, int numVertices,
			IndexBuffer indexBuffer, int startIndex, BoundingBox boundingBox, bool hasNormals, VertexElementFormat? tangentsFormat)
		{
			PrimitiveType = primitiveType;
			PrimitiveCount = primitiveCount;
			VertexBuffer = vertexBuffer;
			VertexOffset = vertexOffset;
			NumVertices = numVertices;
			IndexBuffer = indexBuffer;
			StartIndex = startIndex;
			BoundingBox = boundingBox;
			HasNormals = hasNormals;
			TangentsFormat = tangentsFormat;
		}

		public DrMeshPart(VertexBuffer vertexBuffer, IndexBuffer indexBuffer, BoundingBox boundingBox, PrimitiveType primitiveType = PrimitiveType.TriangleList, int? numVertices = null, int? primitiveCount = null, int vertexOffset = 0, int startIndex = 0)
		{
			if (indexBuffer == null && primitiveCount == null)
			{
				throw new ArgumentNullException("if indexBuffer is null, then primitiveCount must be set explicitly.");
			}

			PrimitiveType = primitiveType;
			PrimitiveCount = primitiveCount ?? primitiveType.GetPrimitiveCount(indexBuffer.IndexCount - startIndex);

			VertexBuffer = vertexBuffer;
			VertexOffset = vertexOffset;
			NumVertices = numVertices ?? VertexBuffer.VertexCount;

			IndexBuffer = indexBuffer;
			StartIndex = startIndex;

			BoundingBox = boundingBox;

			HasNormals = VertexBuffer.VertexDeclaration.FindElement(VertexElementUsage.Normal) != null;

			var tangents = VertexBuffer.VertexDeclaration.FindElement(VertexElementUsage.Tangent);
			TangentsFormat = tangents != null ? tangents.Value.VertexElementFormat : (VertexElementFormat?)null;
		}

		public DrMeshPart(GraphicsDevice graphicsDevice, VertexPositionNormalTexture[] vertices, ushort[] indices, PrimitiveType primitiveType = PrimitiveType.TriangleList) :
			this(vertices.CreateVertexBuffer(graphicsDevice), indices.CreateIndexBuffer(graphicsDevice), vertices.BuildBoundingBox(), primitiveType)
		{
		}

		public DrMeshPart(GraphicsDevice graphicsDevice, VertexPositionNormalTexture[] vertices, int[] indices, PrimitiveType primitiveType = PrimitiveType.TriangleList) :
			this(vertices.CreateVertexBuffer(graphicsDevice), indices.CreateIndexBuffer(graphicsDevice), vertices.BuildBoundingBox(), primitiveType)
		{
		}

		public DrMeshPart(GraphicsDevice graphicsDevice, VertexPositionTexture[] vertices, ushort[] indices, PrimitiveType primitiveType = PrimitiveType.TriangleList) :
			this(vertices.CreateVertexBuffer(graphicsDevice), indices.CreateIndexBuffer(graphicsDevice), vertices.BuildBoundingBox(), primitiveType)
		{
		}

		public DrMeshPart(GraphicsDevice graphicsDevice, VertexPositionTexture[] vertices, int[] indices, PrimitiveType primitiveType = PrimitiveType.TriangleList) :
			this(vertices.CreateVertexBuffer(graphicsDevice), indices.CreateIndexBuffer(graphicsDevice), vertices.BuildBoundingBox(), primitiveType)
		{
		}

		public DrMeshPart(GraphicsDevice graphicsDevice, VertexPositionNormal[] vertices, ushort[] indices, PrimitiveType primitiveType = PrimitiveType.TriangleList) :
			this(vertices.CreateVertexBuffer(graphicsDevice), indices.CreateIndexBuffer(graphicsDevice), vertices.BuildBoundingBox(), primitiveType)
		{
		}

		public DrMeshPart(GraphicsDevice graphicsDevice, VertexPositionNormal[] vertices, int[] indices, PrimitiveType primitiveType = PrimitiveType.TriangleList) :
			this(vertices.CreateVertexBuffer(graphicsDevice), indices.CreateIndexBuffer(graphicsDevice), vertices.BuildBoundingBox(), primitiveType)
		{
		}

		public DrMeshPart(GraphicsDevice graphicsDevice, VertexPosition[] vertices, ushort[] indices, PrimitiveType primitiveType = PrimitiveType.TriangleList) :
			this(vertices.CreateVertexBuffer(graphicsDevice), indices.CreateIndexBuffer(graphicsDevice), vertices.BuildBoundingBox(), primitiveType)
		{
		}

		public DrMeshPart(GraphicsDevice graphicsDevice, VertexPosition[] vertices, int[] indices, PrimitiveType primitiveType = PrimitiveType.TriangleList) :
			this(vertices.CreateVertexBuffer(graphicsDevice), indices.CreateIndexBuffer(graphicsDevice), vertices.BuildBoundingBox(), primitiveType)
		{
		}

		public DrMeshPart(GraphicsDevice graphicsDevice, Vector3[] vertices, ushort[] indices, PrimitiveType primitiveType = PrimitiveType.TriangleList) :
			this(vertices.CreateVertexBuffer(graphicsDevice), indices.CreateIndexBuffer(graphicsDevice), vertices.BuildBoundingBox(), primitiveType)
		{
		}

		public DrMeshPart(GraphicsDevice graphicsDevice, Vector3[] vertices, int[] indices, PrimitiveType primitiveType = PrimitiveType.TriangleList) :
			this(vertices.CreateVertexBuffer(graphicsDevice), indices.CreateIndexBuffer(graphicsDevice), vertices.BuildBoundingBox(), primitiveType)
		{
		}

		public override void Dispose(bool disposing)
		{
			if (disposing)
			{
				VertexBuffer?.Dispose();
				IndexBuffer?.Dispose();
			}
		}

		public DrMeshPart Clone()
		{
			return new DrMeshPart(PrimitiveType, PrimitiveCount, VertexBuffer, VertexOffset, NumVertices, IndexBuffer, StartIndex, BoundingBox, HasNormals, TangentsFormat)
			{
				Material = Material?.Clone(),
				Tag = Tag
			};
		}
	}
}

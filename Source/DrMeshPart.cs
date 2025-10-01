using DigitalRiseModel.Utility;
using DigitalRiseModel.Vertices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Linq;

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

		public DrMaterial Material { get; set; }
		public DrSkin Skin { get; set; }

		public object Tag { get; set; }

		private DrMeshPart(PrimitiveType primitiveType, int primitiveCount, VertexBuffer vertexBuffer, int vertexOffset, int numVertices, IndexBuffer indexBuffer, int startIndex, BoundingBox boundingBox, bool hasNormals)
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
			HasNormals = (from el in VertexBuffer.VertexDeclaration.GetVertexElements() where el.VertexElementUsage == VertexElementUsage.Normal select el).Count() > 0;
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

		public void Draw(GraphicsDevice graphicsDevice)
		{
			if (graphicsDevice == null)
			{
				throw new ArgumentNullException(nameof(graphicsDevice));
			}

			graphicsDevice.SetVertexBuffer(VertexBuffer);
			if (IndexBuffer == null)
			{
				graphicsDevice.DrawPrimitives(PrimitiveType, VertexOffset, PrimitiveCount);
			}
			else
			{
				graphicsDevice.Indices = IndexBuffer;

#if MONOGAME
				graphicsDevice.DrawIndexedPrimitives(PrimitiveType, VertexOffset, StartIndex, PrimitiveCount);
#else
				graphicsDevice.DrawIndexedPrimitives(PrimitiveType, VertexOffset, 0, NumVertices, StartIndex, PrimitiveCount);
#endif
			}
		}

		public DrMeshPart Clone()
		{
			return new DrMeshPart(PrimitiveType, PrimitiveCount, VertexBuffer, VertexOffset, NumVertices, IndexBuffer, StartIndex, BoundingBox, HasNormals)
			{
				Material = Material?.Clone(),
				Tag = Tag
			};
		}
	}
}

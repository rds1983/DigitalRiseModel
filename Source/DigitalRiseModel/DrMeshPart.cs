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
		public int StartVertex { get; }
		public int NumVertices { get; }

		public IndexBuffer IndexBuffer { get; }
		public int StartIndex { get; }

		public BoundingBox BoundingBox { get; }

		public bool HasNormals { get; }

		public DrMaterial Material { get; set; }

		public object Tag { get; set; }

		public DrMeshPart(VertexBuffer vertexBuffer, IndexBuffer indexBuffer, BoundingBox boundingBox, PrimitiveType primitiveType = PrimitiveType.TriangleList, int? numVertices = null, int? primitiveCount = null, int startVertex = 0, int startIndex = 0)
		{
			if (indexBuffer == null && primitiveCount == null)
			{
				throw new ArgumentNullException("if indexBuffer is null, then primitiveCount must be set explicitly.");
			}

			PrimitiveType = primitiveType;
			PrimitiveCount = primitiveCount ?? primitiveType.GetPrimitiveCount(indexBuffer.IndexCount - startIndex);

			VertexBuffer = vertexBuffer;
			StartVertex = startVertex;
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
				VertexBuffer.SafeDispose();
				IndexBuffer.SafeDispose();
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
				graphicsDevice.DrawPrimitives(PrimitiveType, StartVertex, PrimitiveCount);
			}
			else
			{
				graphicsDevice.Indices = IndexBuffer;

#if MONOGAME
				graphicsDevice.DrawIndexedPrimitives(PrimitiveType, StartVertex, StartIndex, PrimitiveCount);
#else
				graphicsDevice.DrawIndexedPrimitives(PrimitiveType, StartVertex, 0, NumVertices, StartIndex, PrimitiveCount);
#endif
			}
		}
	}
}

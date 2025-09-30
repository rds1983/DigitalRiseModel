using NursiaModel.Utility;
using NursiaModel.Vertices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Linq;

namespace NursiaModel
{
	public class NrmMeshPart : NrmDisposable
	{
		public NrmMesh Mesh { get; internal set; }
		public PrimitiveType PrimitiveType { get; }
		public int PrimitiveCount { get; }

		public VertexBuffer VertexBuffer { get; }
		public int VertexOffset { get; }
		public int NumVertices { get; }

		public IndexBuffer IndexBuffer { get; }
		public int StartIndex { get; }

		public BoundingBox BoundingBox { get; set; }

		public bool HasNormals { get; }

		public NrmMaterial Material { get; set; }
		public NrmSkin Skin { get; set; }

		public object Tag { get; set; }

		private NrmMeshPart(PrimitiveType primitiveType, int primitiveCount, VertexBuffer vertexBuffer, int vertexOffset, int numVertices, IndexBuffer indexBuffer, int startIndex, BoundingBox boundingBox, bool hasNormals)
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

		public NrmMeshPart(VertexBuffer vertexBuffer, IndexBuffer indexBuffer, BoundingBox boundingBox, PrimitiveType primitiveType = PrimitiveType.TriangleList, int? numVertices = null, int? primitiveCount = null, int vertexOffset = 0, int startIndex = 0)
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

		public NrmMeshPart(GraphicsDevice graphicsDevice, VertexPositionNormalTexture[] vertices, ushort[] indices, PrimitiveType primitiveType = PrimitiveType.TriangleList) :
			this(vertices.CreateVertexBuffer(graphicsDevice), indices.CreateIndexBuffer(graphicsDevice), vertices.BuildBoundingBox(), primitiveType)
		{
		}

		public NrmMeshPart(GraphicsDevice graphicsDevice, VertexPositionNormalTexture[] vertices, int[] indices, PrimitiveType primitiveType = PrimitiveType.TriangleList) :
			this(vertices.CreateVertexBuffer(graphicsDevice), indices.CreateIndexBuffer(graphicsDevice), vertices.BuildBoundingBox(), primitiveType)
		{
		}

		public NrmMeshPart(GraphicsDevice graphicsDevice, VertexPositionTexture[] vertices, ushort[] indices, PrimitiveType primitiveType = PrimitiveType.TriangleList) :
			this(vertices.CreateVertexBuffer(graphicsDevice), indices.CreateIndexBuffer(graphicsDevice), vertices.BuildBoundingBox(), primitiveType)
		{
		}

		public NrmMeshPart(GraphicsDevice graphicsDevice, VertexPositionTexture[] vertices, int[] indices, PrimitiveType primitiveType = PrimitiveType.TriangleList) :
			this(vertices.CreateVertexBuffer(graphicsDevice), indices.CreateIndexBuffer(graphicsDevice), vertices.BuildBoundingBox(), primitiveType)
		{
		}

		public NrmMeshPart(GraphicsDevice graphicsDevice, VertexPositionNormal[] vertices, ushort[] indices, PrimitiveType primitiveType = PrimitiveType.TriangleList) :
			this(vertices.CreateVertexBuffer(graphicsDevice), indices.CreateIndexBuffer(graphicsDevice), vertices.BuildBoundingBox(), primitiveType)
		{
		}

		public NrmMeshPart(GraphicsDevice graphicsDevice, VertexPositionNormal[] vertices, int[] indices, PrimitiveType primitiveType = PrimitiveType.TriangleList) :
			this(vertices.CreateVertexBuffer(graphicsDevice), indices.CreateIndexBuffer(graphicsDevice), vertices.BuildBoundingBox(), primitiveType)
		{
		}

		public NrmMeshPart(GraphicsDevice graphicsDevice, VertexPosition[] vertices, ushort[] indices, PrimitiveType primitiveType = PrimitiveType.TriangleList) :
			this(vertices.CreateVertexBuffer(graphicsDevice), indices.CreateIndexBuffer(graphicsDevice), vertices.BuildBoundingBox(), primitiveType)
		{
		}

		public NrmMeshPart(GraphicsDevice graphicsDevice, VertexPosition[] vertices, int[] indices, PrimitiveType primitiveType = PrimitiveType.TriangleList) :
			this(vertices.CreateVertexBuffer(graphicsDevice), indices.CreateIndexBuffer(graphicsDevice), vertices.BuildBoundingBox(), primitiveType)
		{
		}

		public NrmMeshPart(GraphicsDevice graphicsDevice, Vector3[] vertices, ushort[] indices, PrimitiveType primitiveType = PrimitiveType.TriangleList) :
			this(vertices.CreateVertexBuffer(graphicsDevice), indices.CreateIndexBuffer(graphicsDevice), vertices.BuildBoundingBox(), primitiveType)
		{
		}

		public NrmMeshPart(GraphicsDevice graphicsDevice, Vector3[] vertices, int[] indices, PrimitiveType primitiveType = PrimitiveType.TriangleList) :
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

		public NrmMeshPart Clone()
		{
			return new NrmMeshPart(PrimitiveType, PrimitiveCount, VertexBuffer, VertexOffset, NumVertices, IndexBuffer, StartIndex, BoundingBox, HasNormals)
			{
				Material = Material?.Clone(),
				Tag = Tag
			};
		}
	}
}

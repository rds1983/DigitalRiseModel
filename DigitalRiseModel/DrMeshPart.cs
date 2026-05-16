using DigitalRiseModel.Utility;
using DigitalRiseModel.Vertices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace DigitalRiseModel
{
	/// <summary>
	/// Represents a part of a mesh with its own vertex and index buffers and material.
	/// </summary>
	public class DrMeshPart : DrDisposable
	{
		/// <summary>
		/// Gets or sets the mesh that this part belongs to.
		/// </summary>
		public DrMesh Mesh { get; internal set; }

		/// <summary>
		/// Gets the type of primitives in this mesh part.
		/// </summary>
		public PrimitiveType PrimitiveType { get; }

		/// <summary>
		/// Gets the number of primitives in this mesh part.
		/// </summary>
		public int PrimitiveCount { get; }

		/// <summary>
		/// Gets the vertex buffer for this mesh part.
		/// </summary>
		public VertexBuffer VertexBuffer { get; }

		/// <summary>
		/// Gets the offset in the vertex buffer where this mesh part's vertices start.
		/// </summary>
		public int VertexOffset { get; }

		/// <summary>
		/// Gets the number of vertices in this mesh part.
		/// </summary>
		public int NumVertices { get; }

		/// <summary>
		/// Gets the index buffer for this mesh part, or null if this mesh part uses non-indexed rendering.
		/// </summary>
		public IndexBuffer IndexBuffer { get; }

		/// <summary>
		/// Gets the offset in the index buffer where this mesh part's indices start.
		/// </summary>
		public int StartIndex { get; }

		/// <summary>
		/// Gets or sets the bounding box of this mesh part.
		/// </summary>
		public BoundingBox BoundingBox { get; set; }

		/// <summary>
		/// Gets a value indicating whether this mesh part has normal vectors.
		/// </summary>
		public bool HasNormals { get; }

		/// <summary>
		/// Gets the format of the tangent vectors in the vertex buffer, or null if there are no tangents.
		/// </summary>
		public VertexElementFormat? TangentsFormat { get; }

		/// <summary>
		/// Gets or sets the material for this mesh part.
		/// </summary>
		public DrMaterial Material { get; set; }

		/// <summary>
		/// Gets or sets the skin for this mesh part, used for skeletal animation.
		/// </summary>
		public DrSkin Skin { get; set; }

		/// <summary>
		/// Gets or sets an arbitrary object associated with this mesh part.
		/// </summary>
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

		/// <summary>
		/// Initializes a new instance of the <see cref="DrMeshPart"/> class from the specified vertex and index buffers.
		/// </summary>
		/// <param name="vertexBuffer">The vertex buffer for this mesh part.</param>
		/// <param name="indexBuffer">The index buffer for this mesh part, or null for non-indexed rendering.</param>
		/// <param name="boundingBox">The bounding box of this mesh part.</param>
		/// <param name="primitiveType">The type of primitives. Default is TriangleList.</param>
		/// <param name="numVertices">The number of vertices, or null to use the vertex buffer's vertex count.</param>
		/// <param name="primitiveCount">The number of primitives, or null to calculate from the index buffer.</param>
		/// <param name="vertexOffset">The offset in the vertex buffer. Default is 0.</param>
		/// <param name="startIndex">The offset in the index buffer. Default is 0.</param>
		/// <exception cref="ArgumentNullException"><paramref name="indexBuffer"/> is null and <paramref name="primitiveCount"/> is not specified.</exception>
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

		/// <summary>
		/// Initializes a new instance of the <see cref="DrMeshPart"/> class from VertexPositionNormalTexture vertices and indices.
		/// </summary>
		/// <param name="graphicsDevice">The graphics device used to create the buffers.</param>
		/// <param name="vertices">The vertices for this mesh part.</param>
		/// <param name="indices">The indices for this mesh part.</param>
		/// <param name="primitiveType">The type of primitives. Default is TriangleList.</param>
		public DrMeshPart(GraphicsDevice graphicsDevice, VertexPositionNormalTexture[] vertices, ushort[] indices, PrimitiveType primitiveType = PrimitiveType.TriangleList) :
			this(vertices.CreateVertexBuffer(graphicsDevice), indices.CreateIndexBuffer(graphicsDevice), vertices.BuildBoundingBox(), primitiveType)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="DrMeshPart"/> class from VertexPositionNormalTexture vertices and indices.
		/// </summary>
		/// <param name="graphicsDevice">The graphics device used to create the buffers.</param>
		/// <param name="vertices">The vertices for this mesh part.</param>
		/// <param name="indices">The indices for this mesh part.</param>
		/// <param name="primitiveType">The type of primitives. Default is TriangleList.</param>
		public DrMeshPart(GraphicsDevice graphicsDevice, VertexPositionNormalTexture[] vertices, int[] indices, PrimitiveType primitiveType = PrimitiveType.TriangleList) :
			this(vertices.CreateVertexBuffer(graphicsDevice), indices.CreateIndexBuffer(graphicsDevice), vertices.BuildBoundingBox(), primitiveType)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="DrMeshPart"/> class from VertexPositionTexture vertices and indices.
		/// </summary>
		/// <param name="graphicsDevice">The graphics device used to create the buffers.</param>
		/// <param name="vertices">The vertices for this mesh part.</param>
		/// <param name="indices">The indices for this mesh part.</param>
		/// <param name="primitiveType">The type of primitives. Default is TriangleList.</param>
		public DrMeshPart(GraphicsDevice graphicsDevice, VertexPositionTexture[] vertices, ushort[] indices, PrimitiveType primitiveType = PrimitiveType.TriangleList) :
			this(vertices.CreateVertexBuffer(graphicsDevice), indices.CreateIndexBuffer(graphicsDevice), vertices.BuildBoundingBox(), primitiveType)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="DrMeshPart"/> class from VertexPositionTexture vertices and indices.
		/// </summary>
		/// <param name="graphicsDevice">The graphics device used to create the buffers.</param>
		/// <param name="vertices">The vertices for this mesh part.</param>
		/// <param name="indices">The indices for this mesh part.</param>
		/// <param name="primitiveType">The type of primitives. Default is TriangleList.</param>
		public DrMeshPart(GraphicsDevice graphicsDevice, VertexPositionTexture[] vertices, int[] indices, PrimitiveType primitiveType = PrimitiveType.TriangleList) :
			this(vertices.CreateVertexBuffer(graphicsDevice), indices.CreateIndexBuffer(graphicsDevice), vertices.BuildBoundingBox(), primitiveType)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="DrMeshPart"/> class from VertexPositionNormal vertices and indices.
		/// </summary>
		/// <param name="graphicsDevice">The graphics device used to create the buffers.</param>
		/// <param name="vertices">The vertices for this mesh part.</param>
		/// <param name="indices">The indices for this mesh part.</param>
		/// <param name="primitiveType">The type of primitives. Default is TriangleList.</param>
		public DrMeshPart(GraphicsDevice graphicsDevice, VertexPositionNormal[] vertices, ushort[] indices, PrimitiveType primitiveType = PrimitiveType.TriangleList) :
			this(vertices.CreateVertexBuffer(graphicsDevice), indices.CreateIndexBuffer(graphicsDevice), vertices.BuildBoundingBox(), primitiveType)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="DrMeshPart"/> class from VertexPositionNormal vertices and indices.
		/// </summary>
		/// <param name="graphicsDevice">The graphics device used to create the buffers.</param>
		/// <param name="vertices">The vertices for this mesh part.</param>
		/// <param name="indices">The indices for this mesh part.</param>
		/// <param name="primitiveType">The type of primitives. Default is TriangleList.</param>
		public DrMeshPart(GraphicsDevice graphicsDevice, VertexPositionNormal[] vertices, int[] indices, PrimitiveType primitiveType = PrimitiveType.TriangleList) :
			this(vertices.CreateVertexBuffer(graphicsDevice), indices.CreateIndexBuffer(graphicsDevice), vertices.BuildBoundingBox(), primitiveType)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="DrMeshPart"/> class from VertexPosition vertices and indices.
		/// </summary>
		/// <param name="graphicsDevice">The graphics device used to create the buffers.</param>
		/// <param name="vertices">The vertices for this mesh part.</param>
		/// <param name="indices">The indices for this mesh part.</param>
		/// <param name="primitiveType">The type of primitives. Default is TriangleList.</param>
		public DrMeshPart(GraphicsDevice graphicsDevice, VertexPosition[] vertices, ushort[] indices, PrimitiveType primitiveType = PrimitiveType.TriangleList) :
			this(vertices.CreateVertexBuffer(graphicsDevice), indices.CreateIndexBuffer(graphicsDevice), vertices.BuildBoundingBox(), primitiveType)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="DrMeshPart"/> class from VertexPosition vertices and indices.
		/// </summary>
		/// <param name="graphicsDevice">The graphics device used to create the buffers.</param>
		/// <param name="vertices">The vertices for this mesh part.</param>
		/// <param name="indices">The indices for this mesh part.</param>
		/// <param name="primitiveType">The type of primitives. Default is TriangleList.</param>
		public DrMeshPart(GraphicsDevice graphicsDevice, VertexPosition[] vertices, int[] indices, PrimitiveType primitiveType = PrimitiveType.TriangleList) :
			this(vertices.CreateVertexBuffer(graphicsDevice), indices.CreateIndexBuffer(graphicsDevice), vertices.BuildBoundingBox(), primitiveType)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="DrMeshPart"/> class from Vector3 vertices and indices.
		/// </summary>
		/// <param name="graphicsDevice">The graphics device used to create the buffers.</param>
		/// <param name="vertices">The vertices for this mesh part.</param>
		/// <param name="indices">The indices for this mesh part.</param>
		/// <param name="primitiveType">The type of primitives. Default is TriangleList.</param>
		public DrMeshPart(GraphicsDevice graphicsDevice, Vector3[] vertices, ushort[] indices, PrimitiveType primitiveType = PrimitiveType.TriangleList) :
			this(vertices.CreateVertexBuffer(graphicsDevice), indices.CreateIndexBuffer(graphicsDevice), vertices.BuildBoundingBox(), primitiveType)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="DrMeshPart"/> class from Vector3 vertices and indices.
		/// </summary>
		/// <param name="graphicsDevice">The graphics device used to create the buffers.</param>
		/// <param name="vertices">The vertices for this mesh part.</param>
		/// <param name="indices">The indices for this mesh part.</param>
		/// <param name="primitiveType">The type of primitives. Default is TriangleList.</param>
		public DrMeshPart(GraphicsDevice graphicsDevice, Vector3[] vertices, int[] indices, PrimitiveType primitiveType = PrimitiveType.TriangleList) :
			this(vertices.CreateVertexBuffer(graphicsDevice), indices.CreateIndexBuffer(graphicsDevice), vertices.BuildBoundingBox(), primitiveType)
		{
		}

		/// <summary>
		/// Releases the unmanaged resources used by this object, and optionally releases the managed resources.
		/// </summary>
		/// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
		public override void Dispose(bool disposing)
		{
			if (disposing)
			{
				VertexBuffer?.Dispose();
				IndexBuffer?.Dispose();
			}
		}

		/// <summary>
		/// Creates a copy of this mesh part with cloned material.
		/// </summary>
		/// <returns>A new <see cref="DrMeshPart"/> instance with the same properties and a cloned material.</returns>
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

using DigitalRiseModel.Vertices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace DigitalRiseModel.Primitives
{
	partial class MeshPrimitives
	{
		/// <summary>
		/// Creates a new meshpart that represents a plane using grid lines.
		/// </summary>
		/// <param name="graphicsDevice">The graphics device.</param>
		/// <param name="size">The size of the grid.</param>
		/// <returns>A new <see cref="DrMeshPart"/> that represents a plane line list.</returns>
		public static DrMeshPart CreatePlaneLinesMeshPart(GraphicsDevice graphicsDevice, int size)
		{
			var vertices = new List<VertexPosition>();
			var indices = new List<ushort>();

			ushort idx = 0;
			for (var x = -size; x <= size; ++x)
			{
				vertices.Add(new VertexPosition
				{
					Position = new Vector3(x, 0, -size)
				});

				vertices.Add(new VertexPosition
				{
					Position = new Vector3(x, 0, size)
				});

				indices.Add(idx);
				++idx;
				indices.Add(idx);
				++idx;
			}

			for (var z = -size; z <= size; ++z)
			{
				vertices.Add(new VertexPosition
				{
					Position = new Vector3(-size, 0, z)
				});

				vertices.Add(new VertexPosition
				{
					Position = new Vector3(size, 0, z)
				});

				indices.Add(idx);
				++idx;
				indices.Add(idx);
				++idx;
			}

			return new DrMeshPart(graphicsDevice, vertices.ToArray(), indices.ToArray(), PrimitiveType.LineList);
		}
	}
}

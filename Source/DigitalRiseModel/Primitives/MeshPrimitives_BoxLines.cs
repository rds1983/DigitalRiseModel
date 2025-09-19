using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DigitalRiseModel.Primitives
{
	partial class MeshPrimitives
	{
		/// <summary>
		/// Creates a new submesh that represents a box using lines.
		/// (The box is centered at the origin. The side length is 1.)
		/// </summary>
		/// <param name="graphicsDevice"></param>
		/// <param name="box"></param>
		/// <returns>A new <see cref="DrSubmesh"/> that represents a box line list.</returns>
		/// <remarks>
		/// If the returned <see cref="DrSubmesh"/> is not going to be modified, then it is better
		/// to call <see cref="GetBoxLines"/> to retrieve a shared <see cref="DrSubmesh"/> instance.
		/// </remarks>
		public static DrSubmesh CreateBoxLinesSubmesh(GraphicsDevice graphicsDevice, BoundingBox box)
		{
			var vertices = new[]
			{
				new Vector3(box.Min.X, box.Min.Y, box.Max.Z),
				new Vector3(box.Max.X, box.Min.Y, box.Max.Z),
				new Vector3(box.Max.X, box.Max.Y, box.Max.Z),
				new Vector3(box.Min.X, box.Max.Y, box.Max.Z),
				new Vector3(box.Min.X, box.Min.Y, box.Min.Z),
				new Vector3(box.Max.X, box.Min.Y, box.Min.Z),
				new Vector3(box.Max.X, box.Max.Y, box.Min.Z),
				new Vector3(box.Min.X, box.Max.Y, box.Min.Z)
			};

			var indices = new ushort[]
			{
				0, 1,
				1, 2,
				2, 3,
				3, 0,

				4, 5,
				5, 6,
				6, 7,
				7, 4,

				0, 4,
				1, 5,
				2, 6,
				3, 7
			};

			return new DrSubmesh(graphicsDevice, vertices, indices, PrimitiveType.LineList);
		}
	}
}

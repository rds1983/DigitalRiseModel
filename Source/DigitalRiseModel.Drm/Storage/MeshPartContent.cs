using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DigitalRiseModel.Storage
{
	internal class MeshPartContent
	{
		public PrimitiveType PrimitiveType { get; set; }

		public int VertexBufferIndex { get; set; }

		public int StartVertex { get; set; }

		public int VertexCount { get; set; }

		public int StartIndex { get; set; }

		public int PrimitiveCount { get; set; }

		public int MaterialIndex { get; set; }

		public BoundingBox BoundingBox { get; set; }
	}
}

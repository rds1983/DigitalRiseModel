using Microsoft.Xna.Framework.Graphics;

namespace DigitalRiseModel.Storage
{
	internal struct VertexElementContent
	{
		public VertexElementUsage Usage { get; set; }

		public VertexElementFormat Format { get; set; }

		public int UsageIndex { get; set; }

		public VertexElementContent(VertexElementUsage usage, VertexElementFormat format, int usageIndex = 0)
		{
			Usage = usage;
			Format = format;
			UsageIndex = usageIndex;
		}

		public override string ToString() => $"{Usage}, {Format}, {UsageIndex}";

		public static bool AreEqual(VertexElementContent a, VertexElementContent b)
		{
			return a.Usage == b.Usage && a.Format == b.Format && a.UsageIndex == b.UsageIndex;
		}
	}
}

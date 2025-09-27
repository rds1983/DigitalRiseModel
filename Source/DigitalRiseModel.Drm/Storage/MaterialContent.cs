using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DigitalRiseModel.Storage
{
	internal class MaterialContent
	{
		public string Name { get; set; }
		public Color DiffuseColor { get; set; }
		public Color SpecularColor { get; set; }
		public float SpecularFactor { get; set; }
		public float SpecularPower { get; set; }

		public string DiffuseTexturePath { get; set; }

		public string NormalTexturePath { get; set; }

		public string SpecularTexturePath { get; set; }
	}
}

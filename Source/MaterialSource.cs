using Microsoft.Xna.Framework;

namespace DigitalRiseModel
{
	internal class MaterialSource
	{
		public string Name { get; set; }
		public Color DiffuseColor { get; set; }
		public Color SpecularColor { get; set; }
		public Color EmissiveColor { get; set; }
		public float Shininess { get; set; }

		public string DiffuseTexture { get; set; }
		public string SpecularTexture { get; set; }
		public string EmissiveTexture { get; set; }
		public string NormalTexture { get; set; }
		public string OcclusionTexture { get; set; }
	}
}

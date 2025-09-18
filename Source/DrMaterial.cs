using Microsoft.Xna.Framework;

namespace DigitalRiseModel
{
	public class DrMaterial
	{
		public string Name { get; set; }
		public Color? DiffuseColor { get; set; }
		public Color? SpecularColor { get; set; }
		public float? SpecularPower { get; set; }
		public string DiffuseTexture { get; set; }
		public string NormalTexture { get; set; }
		public string SpecularTexture { get; set; }
	}
}

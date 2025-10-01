using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DigitalRiseModel
{
	public class DrMaterial
	{
		public string Name { get; set; }
		public Color DiffuseColor { get; set; }
		public Color SpecularColor { get; set; }
		public float SpecularFactor { get; set; }
		public float SpecularPower { get; set; }

		public Texture2D DiffuseTexture { get; set; }

		public Texture2D NormalTexture { get; set; }

		public Texture2D SpecularTexture { get; set; }

		public object Tag { get; set; }

		public DrMaterial Clone()
		{
			return new DrMaterial
			{
				Name = Name,
				DiffuseColor = DiffuseColor,
				SpecularColor = SpecularColor,
				SpecularFactor = SpecularFactor,
				SpecularPower = SpecularPower,
				DiffuseTexture = DiffuseTexture,
				NormalTexture = NormalTexture,
				SpecularTexture = SpecularTexture,
			};
		}
	}
}

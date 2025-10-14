using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DigitalRiseModel
{
	public class DrMaterial : DrDisposable
	{
		public string Name { get; set; }
		public Color DiffuseColor { get; set; } = Color.White;
		public Color SpecularColor { get; set; } = Color.Black;
		public Color EmissiveColor { get; set; } = Color.Black;
		public float Shininess { get; set; }

		public Texture2D DiffuseTexture { get; set; }
		public Texture2D SpecularTexture { get; set; }
		public Texture2D EmissionTexture { get; set; }
		public Texture2D NormalTexture { get; set; }
		public Texture2D OcclusionTexture { get; set; }

		public object Tag { get; set; }

		public override void Dispose(bool disposing)
		{
			if (disposing)
			{
				DiffuseTexture?.Dispose();
				NormalTexture?.Dispose();
				SpecularTexture?.Dispose();
			}
		}

		public DrMaterial Clone()
		{
			return new DrMaterial
			{
				Name = Name,
				DiffuseColor = DiffuseColor,
				SpecularColor = SpecularColor,
				EmissiveColor = EmissiveColor,
				Shininess = Shininess,
				DiffuseTexture = DiffuseTexture,
				SpecularTexture = SpecularTexture,
				EmissionTexture = EmissionTexture,
				NormalTexture = NormalTexture,
				OcclusionTexture = OcclusionTexture
			};
		}
	}
}

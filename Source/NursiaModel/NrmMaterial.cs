using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace NursiaModel
{
	public class NrmMaterial
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
	}
}

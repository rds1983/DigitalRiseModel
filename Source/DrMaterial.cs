using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Text.Json.Serialization;

namespace DigitalRiseModel
{
	public class DrMaterial
	{
		public string Name { get; set; }
		public Color? DiffuseColor { get; set; }
		public Color? SpecularColor { get; set; }
		public float? SpecularPower { get; set; }
		public string DiffuseTexturePath { get; set; }
		public string NormalTexturePath { get; set; }
		public string SpecularTexturePath { get; set; }

		[JsonIgnore]
		public Texture2D DiffuseTexture { get; set; }

		[JsonIgnore]
		public Texture2D NormalTexture { get; set; }

		[JsonIgnore]
		public Texture2D SpecularTexture { get; set; }
	}
}

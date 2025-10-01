using DigitalRiseModel.Samples.BasicEngine;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DigitalRiseModel
{
	public class VisualNode : SceneNode
	{
		public Texture2D Texture { get; set; }
		public Color? Color { get; set; }

		protected Texture2D GetTextureForMeshPart(NrmMeshPart meshpart, Texture2D defaultTexture)
		{
			if (Texture != null)
			{
				return Texture;
			}

			if (meshpart.Material == null || meshpart.Material.DiffuseTexture == null)
			{
				return defaultTexture;
			}

			return meshpart.Material.DiffuseTexture;
		}

		protected Color GetColorForMeshPart(NrmMeshPart meshpart)
		{
			if (Color != null)
			{
				return Color.Value;
			}

			if (meshpart.Material == null)
			{
				return Microsoft.Xna.Framework.Color.White;
			}

			return meshpart.Material.DiffuseColor;
		}
	}
}

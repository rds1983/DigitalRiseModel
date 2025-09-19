using DigitalRiseModel.Samples.BasicEngine;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DigitalRiseModel
{
	public class VisualNode : SceneNode
	{
		public Texture2D Texture { get; set; }
		public Color? Color { get; set; }

		protected Texture2D GetTextureForSubmesh(DrSubmesh submesh, Texture2D defaultTexture)
		{
			if (Texture != null)
			{
				return Texture;
			}

			if (submesh.Material == null || submesh.Material.DiffuseTexture == null)
			{
				return defaultTexture;
			}

			return submesh.Material.DiffuseTexture;
		}

		protected Color GetColorForSubmesh(DrSubmesh submesh)
		{
			if (Color != null)
			{
				return Color.Value;
			}

			if (submesh.Material == null)
			{
				return Microsoft.Xna.Framework.Color.White;
			}

			return submesh.Material.DiffuseColor;
		}
	}
}

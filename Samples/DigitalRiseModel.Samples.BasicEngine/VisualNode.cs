using DigitalRiseModel.Samples.BasicEngine;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DigitalRiseModel
{
	/// <summary>
	/// A scene node that represents a visual object that can be rendered.
	/// Provides helper methods for determining texture and color for mesh parts.
	/// </summary>
	public class VisualNode : SceneNode
	{
		/// <summary>
		/// Gets or sets an override texture for this visual node.
		/// If set, this texture will be used instead of mesh part materials.
		/// </summary>
		public Texture2D Texture { get; set; }

		/// <summary>
		/// Gets or sets an override color for this visual node.
		/// If set, this color will be used instead of mesh part material colors.
		/// </summary>
		public Color? Color { get; set; }

		/// <summary>
		/// Determines which texture should be used for rendering a mesh part.
		/// Priority: Node override > Mesh part material > Default texture
		/// </summary>
		protected Texture2D GetTextureForMeshPart(DrMeshPart meshpart, Texture2D defaultTexture)
		{
			// Use node override texture if available
			if (Texture != null)
			{
				return Texture;
			}

			// Use mesh part material's diffuse texture if available
			if (meshpart.Material == null || meshpart.Material.DiffuseTexture == null)
			{
				return defaultTexture;
			}

			return meshpart.Material.DiffuseTexture;
		}

		/// <summary>
		/// Determines which color should be used for rendering a mesh part.
		/// Priority: Node override > Mesh part material diffuse color > White
		/// </summary>
		protected Color GetColorForMeshPart(DrMeshPart meshpart)
		{
			// Use node override color if available
			if (Color != null)
			{
				return Color.Value;
			}

			// Use mesh part material's diffuse color if available
			if (meshpart.Material == null)
			{
				return Microsoft.Xna.Framework.Color.White;
			}

			return meshpart.Material.DiffuseColor;
		}
	}
}

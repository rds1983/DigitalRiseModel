using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DigitalRiseModel
{
	/// <summary>
	/// Represents a material that defines the appearance of a mesh.
	/// </summary>
	public class DrMaterial : DrDisposable
	{
		/// <summary>
		/// Gets or sets the name of this material.
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// Gets or sets the diffuse color of this material. Default is white.
		/// </summary>
		public Color DiffuseColor { get; set; } = Color.White;

		/// <summary>
		/// Gets or sets the specular color of this material. Default is black.
		/// </summary>
		public Color SpecularColor { get; set; } = Color.Black;

		/// <summary>
		/// Gets or sets the emissive color of this material. Default is black.
		/// </summary>
		public Color EmissiveColor { get; set; } = Color.Black;

		/// <summary>
		/// Gets or sets the shininess value of this material.
		/// </summary>
		public float Shininess { get; set; }

		/// <summary>
		/// Gets or sets the diffuse texture of this material.
		/// </summary>
		public Texture2D DiffuseTexture { get; set; }

		/// <summary>
		/// Gets or sets the specular texture of this material.
		/// </summary>
		public Texture2D SpecularTexture { get; set; }

		/// <summary>
		/// Gets or sets the emissive texture of this material.
		/// </summary>
		public Texture2D EmissiveTexture { get; set; }

		/// <summary>
		/// Gets or sets the normal map texture of this material.
		/// </summary>
		public Texture2D NormalTexture { get; set; }

		/// <summary>
		/// Gets or sets the occlusion texture of this material.
		/// </summary>
		public Texture2D OcclusionTexture { get; set; }

		/// <summary>
		/// Gets or sets an arbitrary object associated with this material.
		/// </summary>
		public object Tag { get; set; }

		/// <summary>
		/// Releases the unmanaged resources used by this object, and optionally releases the managed resources.
		/// </summary>
		/// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
		public override void Dispose(bool disposing)
		{
			if (disposing)
			{
				DiffuseTexture?.Dispose();
				SpecularTexture?.Dispose();
				EmissiveTexture?.Dispose();
				NormalTexture?.Dispose();
				OcclusionTexture?.Dispose();
			}
		}

		/// <summary>
		/// Creates a copy of this material.
		/// </summary>
		/// <returns>A new <see cref="DrMaterial"/> instance with the same properties as this material.</returns>
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
				EmissiveTexture = EmissiveTexture,
				NormalTexture = NormalTexture,
				OcclusionTexture = OcclusionTexture
			};
		}
	}
}

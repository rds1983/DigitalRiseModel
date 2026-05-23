using AssetManagementBase;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace DigitalRiseModel
{
	/// <summary>
	/// Specifies flags for controlling how models are loaded.
	/// </summary>
	[Flags]
	public enum ModelLoadFlags
	{
		/// <summary>
		/// No additional load options.
		/// </summary>
		None = 0,

		/// <summary>
		/// Create vertex and index buffers that support reading data through GetData.
		/// </summary>
		ReadableBuffers = 1 << 0,

		/// <summary>
		/// Automatically add a zero-valued UV channel to meshes that don't have one.
		/// </summary>
		EnsureUVs = 1 << 1,

		/// <summary>
		/// Ignore loading external materials (materials from external files).
		/// </summary>
		IgnoreExternalMaterials = 1 << 2,

		/// <summary>
		/// Ignore loading embedded materials (materials embedded in the model file).
		/// </summary>
		IgnoreEmbeddedMaterials = 1 << 3,

		/// <summary>
		/// Ignore loading all materials (both external and embedded). Equivalent to IgnoreExternalMaterials | IgnoreEmbeddedMaterials.
		/// </summary>
		IgnoreMaterials = IgnoreExternalMaterials | IgnoreEmbeddedMaterials,
	}

	/// <summary>
	/// Internal class that holds settings for model loading.
	/// </summary>
	internal class ModelLoadSettings : IAssetSettings
	{
		/// <summary>
		/// Gets the default model load settings with no flags.
		/// </summary>
		public static readonly ModelLoadSettings Default = new ModelLoadSettings(ModelLoadFlags.None);

		/// <summary>
		/// Gets the flags that control how the model is loaded.
		/// </summary>
		public ModelLoadFlags Flags { get; }

		/// <summary>
		/// Gets the cache key for this settings instance.
		/// </summary>
		private string CacheKey { get; }

		/// <summary>
		/// Gets the buffer usage mode based on the load flags.
		/// </summary>
		public BufferUsage BufferUsage => Flags.HasFlag(ModelLoadFlags.ReadableBuffers) ? BufferUsage.None : BufferUsage.WriteOnly;

		/// <summary>
		/// Initializes a new instance of the <see cref="ModelLoadSettings"/> class.
		/// </summary>
		/// <param name="flags">The load flags to use.</param>
		public ModelLoadSettings(ModelLoadFlags flags)
		{
			Flags = flags;

			CacheKey = flags.ToString();
		}

		/// <summary>
		/// Builds and returns the cache key for these settings.
		/// </summary>
		/// <returns>The cache key as a string.</returns>
		public string BuildKey() => CacheKey;
	}
}

using AssetManagementBase;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace DigitalRiseModel
{
	[Flags]
	public enum ModelLoadFlags
	{
		/// <summary>
		/// No additional load options
		/// </summary>
		None = 0,

		/// <summary>
		/// Ignore loading materials
		/// </summary>
		IgnoreMaterials = 1 << 0,

		/// <summary>
		/// Create vertex/index buffers whose data could be retrieved through GetData
		/// </summary>
		ReadableBuffers = 1 << 1,

		/// <summary>
		/// If a mesh doesn't have uv channel, it'll be added with zero values
		/// </summary>
		EnsureUVs = 1 << 2,
	}

	internal class ModelLoadSettings : IAssetSettings
	{
		public static readonly ModelLoadSettings Default = new ModelLoadSettings(ModelLoadFlags.None);

		public ModelLoadFlags Flags { get; }
		private string CacheKey { get; }

		public BufferUsage BufferUsage => Flags.HasFlag(ModelLoadFlags.ReadableBuffers) ? BufferUsage.None : BufferUsage.WriteOnly;


		public ModelLoadSettings(ModelLoadFlags flags)
		{
			Flags = flags;

			CacheKey = flags.ToString();
		}

		public string BuildKey() => CacheKey;
	}
}

using AssetManagementBase;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace DigitalRiseModel
{
	[Flags]
	public enum ModelLoadFlags
	{
		None = 0,
		IgnoreMaterials = 1 << 0,
		ReadableBuffers = 1 << 1,
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

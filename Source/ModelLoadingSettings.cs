using AssetManagementBase;
using Microsoft.Xna.Framework.Graphics;
using System.Text;

namespace DigitalRiseModel
{
	internal class ModelLoadingSettings : IAssetSettings
	{
		public static readonly ModelLoadingSettings Default = new ModelLoadingSettings(false, TangentsGeneration.None, false);

		public bool IgnoreTextures { get; }
		public TangentsGeneration GenerateTangents { get; }
		public bool ReadableBuffers { get; }
		private string CacheKey { get; }

		public BufferUsage BufferUsage => ReadableBuffers ? BufferUsage.None : BufferUsage.WriteOnly;


		public ModelLoadingSettings(bool ignoreTextures, TangentsGeneration generateTangents, bool readableBuffers)
		{
			IgnoreTextures = ignoreTextures;
			GenerateTangents = generateTangents;
			ReadableBuffers = readableBuffers;

			var sb = new StringBuilder();
			sb.Append(IgnoreTextures);
			sb.Append(",");
			sb.Append(GenerateTangents);
			sb.Append(",");
			sb.Append(ReadableBuffers);

			CacheKey = sb.ToString();
		}

		public string BuildKey() => CacheKey;
	}
}

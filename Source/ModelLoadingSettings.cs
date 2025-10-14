using AssetManagementBase;
using Microsoft.Xna.Framework.Graphics;
using System.Text;

namespace DigitalRiseModel
{
	public enum MaterialsLoadingMode
	{
		/// <summary>
		/// If external ".material" file exist, it'll be preferred over the model materials. Default mode
		/// </summary>
		ExternalMaterial,

		/// <summary>
		/// Ignore external ".material" file even if it exist
		/// </summary>
		IgnoreExternalMaterial,

		/// <summary>
		/// Ignore both external ".material" file and internal model materials
		/// </summary>
		IgnoreEverything
	}

	public enum TangentsGeneration
	{
		None,
		IfDoesntExist,
		Always
	}


	internal class ModelLoadingSettings : IAssetSettings
	{
		public static readonly ModelLoadingSettings Default = new ModelLoadingSettings(MaterialsLoadingMode.ExternalMaterial, TangentsGeneration.None, false);

		public MaterialsLoadingMode MaterialsLoadingMode { get; }
		public TangentsGeneration GenerateTangents { get; }
		public bool ReadableBuffers { get; }
		private string CacheKey { get; }

		public BufferUsage BufferUsage => ReadableBuffers ? BufferUsage.None : BufferUsage.WriteOnly;


		public ModelLoadingSettings(MaterialsLoadingMode materialsLoadingMode, TangentsGeneration generateTangents, bool readableBuffers)
		{
			MaterialsLoadingMode = materialsLoadingMode;
			GenerateTangents = generateTangents;
			ReadableBuffers = readableBuffers;

			var sb = new StringBuilder();
			sb.Append(materialsLoadingMode);
			sb.Append(",");
			sb.Append(GenerateTangents);
			sb.Append(",");
			sb.Append(ReadableBuffers);

			CacheKey = sb.ToString();
		}

		public string BuildKey() => CacheKey;
	}
}

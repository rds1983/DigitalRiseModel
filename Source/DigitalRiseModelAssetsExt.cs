using AssetManagementBase;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json.Linq;
using DigitalRiseModel.G3d;
using System;
using System.IO;

namespace DigitalRiseModel
{
	public enum TangentsGeneration
	{
		None,
		IfDoesntExist,
		Always
	}

	public static class DigitalRiseModelAssetsExt
	{
		private class ModelLoadingSettings : IAssetSettings
		{
			public TangentsGeneration GenerateTangents { get; }
			private string CacheKey { get; }


			public ModelLoadingSettings(TangentsGeneration generateTangents)
			{
				GenerateTangents = generateTangents;

				CacheKey = GenerateTangents.ToString();
			}

			public string BuildKey() => CacheKey;
		}

		private readonly static AssetLoader<DrModel> _gltfLoader = (manager, assetName, settings, tag) =>
		{
			var loader = new GltfLoader();

			var generateTangents = TangentsGeneration.None;
			var modelSettings = (ModelLoadingSettings)settings;
			if (modelSettings != null)
			{
				generateTangents = modelSettings.GenerateTangents;
			}

			var device = (GraphicsDevice)tag;
			return loader.Load(device, manager, assetName, generateTangents);
		};

		private readonly static AssetLoader<DrModel> _g3djLoader = (manager, assetName, settings, tag) =>
		{
			var device = (GraphicsDevice)tag;

			var json = manager.ReadAsString(assetName);
			var obj = JObject.Parse(json);

			return G3dLoader.LoadFromJObject(device, obj, n => manager.LoadTexture2D(device, n));
		};

		public static DrModel LoadGltf(this AssetManager assetManager, GraphicsDevice device, string path, TangentsGeneration generateTangents = TangentsGeneration.None)
		{
			ModelLoadingSettings settings = null;
			if (generateTangents != TangentsGeneration.None)
			{
				settings = new ModelLoadingSettings(generateTangents);
			}

			return assetManager.UseLoader(_gltfLoader, path, settings, tag: device);
		}

		public static DrModel LoadG3dj(this AssetManager assetManager, GraphicsDevice device, string path) =>
			assetManager.UseLoader(_g3djLoader, path, tag: device);

		/// <summary>
		/// Loads a model determining its type on the extension
		/// </summary>
		/// <param name="assetManager"></param>
		/// <param name="device"></param>
		/// <param name="path"></param>
		/// <returns></returns>
		public static DrModel LoadModel(this AssetManager assetManager, GraphicsDevice device, string path, TangentsGeneration generateTangents = TangentsGeneration.None)
		{
			var ext = Path.GetExtension(path).ToLower();

			switch (ext)
			{
				case ".gltf":
				case ".glb":
					return LoadGltf(assetManager, device, path, generateTangents);

				case ".g3dj":
					return LoadG3dj(assetManager, device, path);
			}

			throw new NotSupportedException($"Unknown extension {ext}");
		}
	}
}

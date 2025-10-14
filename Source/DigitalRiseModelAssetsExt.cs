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
			return loader.Load(device, manager, assetName, (ModelLoadingSettings)settings);
		};

		private readonly static AssetLoader<DrModel> _g3djLoader = (manager, assetName, settings, tag) =>
		{
			var device = (GraphicsDevice)tag;

			var json = manager.ReadAsString(assetName);
			var obj = JObject.Parse(json);

			return G3dLoader.LoadFromJObject(device, obj, (ModelLoadingSettings)settings, n => manager.LoadTexture2D(device, n));
		};

		/// <summary>
		/// Loads a model determining its type on the extension
		/// </summary>
		/// <param name="assetManager"></param>
		/// <param name="device"></param>
		/// <param name="path"></param>
		/// <returns></returns>
		public static DrModel LoadModel(this AssetManager assetManager, GraphicsDevice device, string path,
			bool ignoreTextures = false, TangentsGeneration generateTangents = TangentsGeneration.None, bool readableBuffers = false)
		{
			ModelLoadingSettings settings = null;
			if (ignoreTextures == false && generateTangents == TangentsGeneration.None && readableBuffers == false)
			{
				settings = ModelLoadingSettings.Default;
			}
			else
			{
				settings = new ModelLoadingSettings(ignoreTextures, generateTangents, readableBuffers);
			}

			var ext = Path.GetExtension(path).ToLower();

			switch (ext)
			{
				case ".gltf":
				case ".glb":
					return assetManager.UseLoader(_gltfLoader, path, settings, tag: device);

				case ".g3dj":
					return assetManager.UseLoader(_g3djLoader, path, settings, tag: device);
			}

			throw new NotSupportedException($"Unknown extension {ext}");
		}
	}
}

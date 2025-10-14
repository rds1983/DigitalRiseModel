using AssetManagementBase;
using Microsoft.Xna.Framework.Graphics;
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

			return assetManager.UseLoader(_gltfLoader, path, settings, tag: device);
		}
	}
}

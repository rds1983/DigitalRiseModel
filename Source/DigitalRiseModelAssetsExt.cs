using AssetManagementBase;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace DigitalRiseModel
{
	public static class DigitalRiseModelAssetsExt
	{
		private readonly static AssetLoader<DrModel> _gltfLoader = (manager, assetName, settings, tag) =>
		{
			var modelSettings = (ModelLoadSettings)settings;

			var loader = new GltfLoader();
			var device = (GraphicsDevice)tag;
			return loader.Load(device, manager, assetName, (ModelLoadSettings)settings);
		};

		/// <summary>
		/// Loads a model determining its type on the extension
		/// </summary>
		/// <param name="assetManager"></param>
		/// <param name="device"></param>
		/// <param name="path"></param>
		/// <returns></returns>
		public static DrModel LoadModel(this AssetManager assetManager, GraphicsDevice device, string path,
			ModelLoadFlags flags = ModelLoadFlags.None)
		{
			ModelLoadSettings settings = null;
			if (flags == ModelLoadFlags.None)
			{
				settings = ModelLoadSettings.Default;
			}
			else
			{
				settings = new ModelLoadSettings(flags);
			}

			return assetManager.UseLoader(_gltfLoader, path, settings, tag: device);
		}
	}
}

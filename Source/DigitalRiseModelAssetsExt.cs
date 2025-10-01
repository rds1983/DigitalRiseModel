using AssetManagementBase;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json.Linq;
using DigitalRiseModel.G3d;
using System;
using System.IO;

namespace DigitalRiseModel
{
	public static class DigitalRiseModelAssetsExt
	{
		private readonly static AssetLoader<DrModel> _gltfLoader = (manager, assetName, settings, tag) =>
		{
			var loader = new GltfLoader();

			var device = (GraphicsDevice)tag;
			return loader.Load(device, manager, assetName);
		};

		private readonly static AssetLoader<DrModel> _g3djLoader = (manager, assetName, settings, tag) =>
		{
			var device = (GraphicsDevice)tag;

			var json = manager.ReadAsString(assetName);
			var obj = JObject.Parse(json);

			return G3dLoader.LoadFromJObject(device, obj, n => manager.LoadTexture2D(device, n));
		};

		public static DrModel LoadGltf(this AssetManager assetManager, GraphicsDevice device, string path) =>
			assetManager.UseLoader(_gltfLoader, path, tag: device);

		public static DrModel LoadG3dj(this AssetManager assetManager, GraphicsDevice device, string path) =>
			assetManager.UseLoader(_g3djLoader, path, tag: device);

		/// <summary>
		/// Loads a model determining its type on the extension
		/// </summary>
		/// <param name="assetManager"></param>
		/// <param name="device"></param>
		/// <param name="path"></param>
		/// <returns></returns>
		public static DrModel LoadModel(this AssetManager assetManager, GraphicsDevice device, string path)
		{
			var ext = Path.GetExtension(path).ToLower();

			switch(ext)
			{
				case ".gltf":
				case ".glb":
					return LoadGltf(assetManager, device, path);

				case ".g3dj":
					return LoadG3dj(assetManager, device, path);
			}

			throw new NotSupportedException($"Unknown extension {ext}");
		}
	}
}

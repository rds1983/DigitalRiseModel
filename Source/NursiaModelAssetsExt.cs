using AssetManagementBase;
using Microsoft.Xna.Framework.Graphics;

namespace NursiaModel
{
	public static class NursiaModelAssetsExt
	{
		private readonly static AssetLoader<NrmModel> _gltfLoader = (manager, assetName, settings, tag) =>
		{
			var loader = new GltfLoader();

			var device = (GraphicsDevice)tag;
			return loader.Load(device, manager, assetName);
		};

		private readonly static AssetLoader<NrmModel> _g3dLoader = (manager, assetName, settings, tag) =>
		{
			var device = (GraphicsDevice)tag;

			var json = manager.ReadAsString(assetName);

			return G3dLoader.LoadFromJson(device, json, n => manager.LoadTexture2D(device, n));
		};

		public static NrmModel LoadGltf(this AssetManager assetManager, GraphicsDevice device, string path) =>
			assetManager.UseLoader(_gltfLoader, path, tag: device);

		public static NrmModel LoadG3d(this AssetManager assetManager, GraphicsDevice device, string path) =>
			assetManager.UseLoader(_g3dLoader, path, tag: device);
	}
}

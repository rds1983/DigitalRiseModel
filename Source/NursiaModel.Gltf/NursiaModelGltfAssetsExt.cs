using AssetManagementBase;
using Microsoft.Xna.Framework.Graphics;

namespace NursiaModel
{
	public static class NursiaModelGltfAssetsExt
	{
		private readonly static AssetLoader<NrmModel> _gltfLoader = (manager, assetName, settings, tag) =>
		{
			var loader = new GltfLoader();

			var device = (GraphicsDevice)tag;
			return loader.Load(device, manager, assetName);
		};

		public static NrmModel LoadGltf(this AssetManager assetManager, GraphicsDevice device, string path) => 
			assetManager.UseLoader(_gltfLoader, path, tag: device);
	}
}

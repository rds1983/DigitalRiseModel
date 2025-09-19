using AssetManagementBase;
using Microsoft.Xna.Framework.Graphics;

namespace DigitalRiseModel
{
	public static class DigitalRiseModelGltfAssetsExt
	{
		private readonly static AssetLoader<DrModel> _gltfLoader = (manager, assetName, settings, tag) =>
		{
			var loader = new GltfLoader();

			var device = (GraphicsDevice)tag;
			return loader.Load(device, manager, assetName);
		};

		public static DrModel LoadGltf(this AssetManager assetManager, GraphicsDevice device, string path) => 
			assetManager.UseLoader(_gltfLoader, path, tag: device);
	}
}

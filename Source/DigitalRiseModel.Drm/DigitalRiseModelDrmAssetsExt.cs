using AssetManagementBase;
using Microsoft.Xna.Framework.Graphics;

namespace DigitalRiseModel
{
	public static class DigitalRiseModelDrmAssetsExt
	{
		private readonly static AssetLoader<DrModel> _drmLoader = (manager, assetName, settings, tag) =>
		{
			var device = (GraphicsDevice)tag;

			if (assetName.EndsWith(".jdrm", System.StringComparison.OrdinalIgnoreCase))
			{
				var json = manager.ReadAsString(assetName);
				return DrmLoader.CreateFromJson(device, json, s => manager.Open(s), (d, s) => manager.LoadTexture2D(d, s));
			}

			using (var stream = manager.Open(assetName))
			{
				return DrmLoader.CreateFromBinary(device, stream, (d, s) => manager.LoadTexture2D(d, s));
			}
		};

		public static DrModel LoadDrm(this AssetManager assetManager, GraphicsDevice device, string path) =>
			assetManager.UseLoader(_drmLoader, path, tag: device);
	}
}

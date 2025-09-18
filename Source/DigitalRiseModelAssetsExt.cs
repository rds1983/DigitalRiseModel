using AssetManagementBase;
using Microsoft.Xna.Framework.Graphics;

namespace DigitalRiseModel
{
	public static class DigitalRiseModelAssetsExt
	{
		private readonly static AssetLoader<DrModel> _modelLoader = (manager, assetName, settings, tag) =>
		{
			var device = (GraphicsDevice)tag;

			if (assetName.EndsWith("jdrm"))
			{
				var json = manager.ReadAsString(assetName);

				return DrModel.CreateFromJson(device, json,
					name => manager.Open(name),
					(d, name) => manager.LoadTexture2D(device, name));
			}

			using (var stream = manager.Open(assetName))
			{
				return DrModel.CreateFromBinary(device, stream, (d, name) => manager.LoadTexture2D(device, name));
			}
		};

		public static DrModel LoadDrModel(this AssetManager manager, GraphicsDevice graphicsDevice, string path) =>
			manager.UseLoader(_modelLoader, path, tag: graphicsDevice);
	}
}

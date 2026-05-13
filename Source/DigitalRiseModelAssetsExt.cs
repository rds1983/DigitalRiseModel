using AssetManagementBase;
using Microsoft.Xna.Framework.Graphics;

namespace DigitalRiseModel
{
	/// <summary>
	/// Extension methods for the AssetManager to load DigitalRiseModel assets.
	/// </summary>
	public static class DigitalRiseModelAssetsExt
	{
		private static readonly AssetLoader<DrModel> _gltfLoader = (manager, assetName, settings, tag) =>
		{
			var loader = new GltfLoader();
			var device = (GraphicsDevice)tag;
			return loader.Load(device, manager, assetName, (ModelLoadSettings)settings);
		};

		/// <summary>
		/// Loads a 3D model from a file, automatically determining the format from the file extension.
		/// </summary>
		/// <param name="assetManager">The asset manager to use for loading.</param>
		/// <param name="device">The graphics device to use for creating GPU resources.</param>
		/// <param name="path">The path to the model file (typically .gltf or .glb).</param>
		/// <param name="flags">Flags that control how the model is loaded. Default is None.</param>
		/// <returns>The loaded model.</returns>
		public static DrModel LoadModel(this AssetManager assetManager, GraphicsDevice device, string path, ModelLoadFlags flags = ModelLoadFlags.None)
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

using AssetManagementBase;
using DigitalRiseModel.Utility;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;

namespace DigitalRiseModel
{
	public static class DigitalRiseModelAssetsExt
	{
		private readonly static AssetLoader<DrModel> _gltfLoader = (manager, assetName, settings, tag) =>
		{
			var modelSettings = (ModelLoadingSettings)settings;

			var ignoreMaterials = modelSettings.MaterialsLoadingMode == MaterialsLoadingMode.IgnoreEverything;

			Dictionary<string, MaterialSource[]> externalMaterial = null;
			if (modelSettings.MaterialsLoadingMode == MaterialsLoadingMode.ExternalMaterial)
			{
				// Check existance of the external material
				var materialFile = Path.ChangeExtension(assetName, "material");
				if (manager.Exists(materialFile))
				{
					var json = manager.ReadAsString(materialFile);

					externalMaterial = JsonExtensions.DeserializeFromString<Dictionary<string, MaterialSource[]>>(json);

					ignoreMaterials = true;
				}
			}

			var loader = new GltfLoader();
			var device = (GraphicsDevice)tag;
			var result = loader.Load(device, manager, assetName, (ModelLoadingSettings)settings, ignoreMaterials);
			if (externalMaterial != null)
			{
				foreach (var mesh in result.Meshes)
				{
					MaterialSource[] source = null;
					string materialName = null;

					if (mesh.Name != null && externalMaterial.TryGetValue(mesh.Name, out source))
					{
						materialName = mesh.Name;
					} else if (mesh.ParentBone.Name != null && externalMaterial.TryGetValue(mesh.ParentBone.Name, out source))
					{
						materialName = mesh.ParentBone.Name;
					}

					if (source != null)
					{
						for (var i = 0; i < Math.Min(mesh.MeshParts.Count, source.Length); ++i)
						{
							var sourceMaterial = source[i];

							var material = new DrMaterial
							{
								Name = materialName,
								DiffuseColor = sourceMaterial.DiffuseColor,
								SpecularColor = sourceMaterial.SpecularColor,
								EmissiveColor = sourceMaterial.EmissiveColor,
								Shininess = sourceMaterial.Shininess
							};

							if (!string.IsNullOrEmpty(sourceMaterial.DiffuseTexture))
							{
								material.DiffuseTexture = manager.LoadTexture2D(device, sourceMaterial.DiffuseTexture);
							}

							if (!string.IsNullOrEmpty(sourceMaterial.SpecularTexture))
							{
								material.SpecularTexture = manager.LoadTexture2D(device, sourceMaterial.SpecularTexture);
							}

							if (!string.IsNullOrEmpty(sourceMaterial.EmissiveTexture))
							{
								material.EmissionTexture = manager.LoadTexture2D(device, sourceMaterial.EmissiveTexture);
							}

							if (!string.IsNullOrEmpty(sourceMaterial.NormalTexture))
							{
								material.NormalTexture = manager.LoadTexture2D(device, sourceMaterial.NormalTexture);
							}

							if (!string.IsNullOrEmpty(sourceMaterial.OcclusionTexture))
							{
								material.OcclusionTexture = manager.LoadTexture2D(device, sourceMaterial.OcclusionTexture);
							}

							mesh.MeshParts[i].Material = material;
						}
					}
				}
			}

			return result;
		};

		/// <summary>
		/// Loads a model determining its type on the extension
		/// </summary>
		/// <param name="assetManager"></param>
		/// <param name="device"></param>
		/// <param name="path"></param>
		/// <returns></returns>
		public static DrModel LoadModel(this AssetManager assetManager, GraphicsDevice device, string path,
			MaterialsLoadingMode materialsLoadingMode = MaterialsLoadingMode.ExternalMaterial, TangentsGeneration generateTangents = TangentsGeneration.None, bool readableBuffers = false)
		{
			ModelLoadingSettings settings = null;
			if (materialsLoadingMode == MaterialsLoadingMode.ExternalMaterial && generateTangents == TangentsGeneration.None && readableBuffers == false)
			{
				settings = ModelLoadingSettings.Default;
			}
			else
			{
				settings = new ModelLoadingSettings(materialsLoadingMode, generateTangents, readableBuffers);
			}

			return assetManager.UseLoader(_gltfLoader, path, settings, tag: device);
		}
	}
}

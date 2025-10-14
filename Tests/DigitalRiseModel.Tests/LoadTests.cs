using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xna.Framework;
using System;
using System.Linq;

namespace DigitalRiseModel.Tests
{
	[TestClass]
	public sealed class LoadTests
	{
		private static void TestDudeModel(DrModel model, string rootName, int bonesCount, bool readableBuffers)
		{
			Assert.IsNotNull(model.Root);
			Assert.AreEqual(rootName, model.Root.Name);
			Assert.IsNotNull(model.Bones);
			Assert.AreEqual(bonesCount, model.Bones.Length);
			Assert.IsNotNull(model.Meshes);
			Assert.AreEqual(1, model.Meshes.Length);
			Assert.IsNotNull(model.Animations);
			Assert.AreEqual(1, model.Animations.Count);

			var firstPart = model.Meshes[0].MeshParts[0];
			var data = new byte[firstPart.VertexBuffer.VertexCount * firstPart.VertexBuffer.VertexDeclaration.VertexStride];

			if (readableBuffers)
			{
				firstPart.VertexBuffer.GetData(data);
			}
			else
			{
				Assert.ThrowsException<NotSupportedException>(() =>
				{
					firstPart.VertexBuffer.GetData(data);
				});
			}
		}

		[TestMethod]
		[DataRow("dude.gltf")]
		[DataRow("dude.glb")]
		public void LoadDudeModel(string file, string rootName = "RootNode", int bonesCount = 60)
		{
			var manager = Utility.CreateAssetManager();
			var model = manager.LoadModel(TestsEnvironment.GraphicsDevice, file, materialsLoadingMode: MaterialsLoadingMode.IgnoreEverything, readableBuffers: false);
			TestDudeModel(model, rootName, bonesCount, false);

			model = manager.LoadModel(TestsEnvironment.GraphicsDevice, file, materialsLoadingMode: MaterialsLoadingMode.IgnoreEverything, readableBuffers: true);
			TestDudeModel(model, rootName, bonesCount, true);
		}

		[TestMethod]
		public void TestMaterialsLoadingModes()
		{
			var manager = Utility.CreateAssetManager();

			// External material
			var model = manager.LoadModel(TestsEnvironment.GraphicsDevice, @"CesiumMilkTruck.gltf");

			// Check colors
			Assert.AreEqual(3, model.Meshes.Length);
			Assert.AreEqual(3, model.Meshes[0].MeshParts.Count);

			var mat = model.Meshes[0].MeshParts[0].Material;
			Assert.AreEqual(Color.Blue, mat.DiffuseColor);
			Assert.AreEqual(Color.Yellow, mat.EmissiveColor);

			mat = model.Meshes[0].MeshParts[1].Material;
			Assert.AreEqual(new Color(0x12, 0x34, 0x56), mat.SpecularColor);

			mat = model.Meshes[1].MeshParts[0].Material;
			Assert.AreEqual(Color.Red, mat.DiffuseColor);
			Assert.AreEqual(Color.White, mat.SpecularColor);

			// Internal material
			model = manager.LoadModel(TestsEnvironment.GraphicsDevice, @"CesiumMilkTruck.gltf", materialsLoadingMode: MaterialsLoadingMode.IgnoreExternalMaterial);
			mat = model.Meshes[0].MeshParts[0].Material;
			Assert.IsNotNull(mat.DiffuseTexture);
			Assert.AreEqual(2048, mat.DiffuseTexture.Width);

			// No materials
			model = manager.LoadModel(TestsEnvironment.GraphicsDevice, @"CesiumMilkTruck.gltf", materialsLoadingMode: MaterialsLoadingMode.IgnoreEverything);
			mat = model.Meshes[0].MeshParts[0].Material;
			Assert.IsNull(mat.DiffuseTexture);

			// Since different settings produce different cache keys
			// manager should hold 3 models
			var modelCount = (from pair in manager.Cache where pair.Value is DrModel select pair.Key).Count();
			Assert.AreEqual(3, modelCount);
		}
	}
}

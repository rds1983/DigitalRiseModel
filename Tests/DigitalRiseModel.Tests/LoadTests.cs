using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

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
		[DataRow("dude.g3dj", "_Root")]
		public void LoadDudeModel(string file, string rootName = "RootNode", int bonesCount = 60)
		{
			var manager = Utility.CreateAssetManager();
			var model = manager.LoadModel(TestsEnvironment.GraphicsDevice, file, ignoreTextures: true, readableBuffers: false);
			TestDudeModel(model, rootName, bonesCount, false);

			model = manager.LoadModel(TestsEnvironment.GraphicsDevice, file, ignoreTextures: true, readableBuffers: true);
			TestDudeModel(model, rootName, bonesCount, true);

		}
	}
}

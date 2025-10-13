using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xna.Framework.Graphics;

namespace DigitalRiseModel.Tests
{
	[TestClass]
	public sealed class TangentFramesTests
	{
		[TestMethod]
		public void TestTangentFrames()
		{
			var manager = Utility.CreateAssetManager();

			// Firstly load model as it is
			var sourceModel = manager.LoadModel(TestsEnvironment.GraphicsDevice, "AlphaDeadTree.glb");

			// Now load same model forcing tangents generation
			// Since settings are part of cache key, those two models will be loaded/stored separately
			var generatedModel = manager.LoadModel(TestsEnvironment.GraphicsDevice, "AlphaDeadTree.glb", TangentsGeneration.Always);

			// Now compare original and generated tangents and binormals
			for(var i = 0; i < sourceModel.Meshes.Length; ++i)
			{
				var sourceMesh = sourceModel.Meshes[i];
				var generatedMesh = generatedModel.Meshes[i];

				for(var j = 0; j < sourceMesh.MeshParts.Count; ++j)
				{
					var sourceData = sourceMesh.MeshParts[j].VertexBuffer.To2DArray();
					var destData = generatedMesh.MeshParts[j].VertexBuffer.To2DArray();

					Utility.CompareData(sourceData, destData);
				}
			}
		}

		[TestMethod]
		public void TestTangentFramesGenerated()
		{
			var manager = Utility.CreateAssetManager();

			var model = manager.LoadModel(TestsEnvironment.GraphicsDevice, "dude.glb", TangentsGeneration.IfDoesntExist);

			// Make sure it has tangents and binormals
			for (var i = 0; i < model.Meshes.Length; ++i)
			{
				var mesh = model.Meshes[i];

				for (var j = 0; j < mesh.MeshParts.Count; ++j)
				{
					var part = mesh.MeshParts[j];

					part.VertexBuffer.VertexDeclaration.EnsureElement(VertexElementUsage.Tangent);
					part.VertexBuffer.VertexDeclaration.EnsureElement(VertexElementUsage.Binormal);
				}
			}
		}
	}
}

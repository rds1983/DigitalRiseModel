using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xna.Framework.Graphics;

namespace DigitalRiseModel.Tests
{
	[TestClass]
	public sealed class TangentFramesTests
	{
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

					var ve = part.VertexBuffer.VertexDeclaration.EnsureElement(VertexElementUsage.Tangent);

					Assert.AreEqual(VertexElementFormat.Vector4, ve.VertexElementFormat);
				}
			}
		}
	}
}
